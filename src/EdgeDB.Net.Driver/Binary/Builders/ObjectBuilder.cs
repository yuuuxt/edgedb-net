using EdgeDB.Binary.Packets;
using EdgeDB.Binary.Codecs;
using EdgeDB.DataTypes;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace EdgeDB
{
    internal sealed class ObjectBuilder
    {
        private static readonly Dictionary<int, Guid> _codecVisitorStateTable = new();
        private static readonly object _visitorLock = new object();

        public static TType? BuildResult<TType>(ILogger logger, ICodec codec, ref Data data)
        {
            // TO INVESTIGATE: since a codec can only be "visited" or "mutated" for
            // one type at a time, we have to ensure that the codec is ready to deserialize
            // TType, we can store the states of the codecs here for building result
            // to achieve this.
            bool wasSkipped = false;
            lock(_visitorLock)
            {
                wasSkipped = _codecVisitorStateTable.TryGetValue(codec.GetHashCode(), out var typeId) && typeId == typeof(TType).GUID;
                if (!wasSkipped)
                {
                    var visitor = new TypeVisitor(logger);

                    visitor.SetTargetType(typeof(TType));

                    visitor.Visit(ref codec);

                    _codecVisitorStateTable[codec.GetHashCode()] = typeof(TType).GUID;
                }
            }

            if(wasSkipped)
            {
                logger.SkippingCodecVisiting(typeof(TType), codec);
            }
            

            if (codec is ObjectCodec objectCodec)
            {
                return (TType?)TypeBuilder.BuildObject(typeof(TType), objectCodec, ref data);
            }

            var value = codec.Deserialize(data.PayloadBuffer);

            return (TType?)ConvertTo(typeof(TType), value);
        }

        public static object? ConvertTo(Type type, object? value)
        {
            if (value is null)
            {
                return ReflectionUtils.GetDefault(type);
            }

            var valueType = value.GetType();

            if (valueType.IsAssignableTo(type))
                return value;

            // check for nullable
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // if the value was null, the above IsAssignableTo check would have returned true,
                // return a convert with the inner generic
                return ConvertTo(type.GenericTypeArguments[0], value);
            }

            // check for enums
            if(value is string str && type.IsEnum)
            {
                return Enum.Parse(type, str);
            }

            // check for arrays or sets
            if ((valueType.IsArray || valueType.IsAssignableTo(typeof(IEnumerable))) && (type.IsArray || type.IsAssignableFrom(typeof(IEnumerable)) || type.IsAssignableTo(typeof(IEnumerable))))
            {
                return ConvertCollection(type, valueType, value);
            }
            
            // check for edgeql types
            //if (TypeBuilder.IsValidObjectType(type) && value is IDictionary<string, object?> dict)
            //    return TypeBuilder.BuildObject(type, dict);

            // check for tuple
            if(value is TransientTuple tuple && type.GetInterface("ITuple") != null)
            {
                if (type.Name.StartsWith("ValueTuple"))
                    return tuple.ToValueTuple();
                else
                    return tuple.ToReferenceTuple();
            }

            // check for F# option
            if(type.IsFSharpOption())
            {
                // convert inner value
                var innerValue = ConvertTo(type.GenericTypeArguments[0], value);
                return Activator.CreateInstance(type, new object?[] { innerValue });
            }

            if (type.IsFSharpValueOption())
            {
                // is the value null?
                if (value is null)
                {
                    return type.GetProperty("ValueNone", BindingFlags.Static | BindingFlags.Public)!.GetValue(null);
                }

                var newValueSome = type.GetMethod("NewValueSome", BindingFlags.Static | BindingFlags.Public)!;
                var innerValue = ConvertTo(type.GenericTypeArguments[0], value);
                return newValueSome.Invoke(null, new object?[] { innerValue });
            }

            try
            {
                return Convert.ChangeType(value, type);
            }
            catch
            {
                try
                {
                    return ReflectionUtils.DynamicCast(value, type);
                }
                catch
                {
                    throw new ArgumentException($"Cannot convert {valueType} to type {type}");
                }
            }
        }

        internal static object? ConvertCollection(Type targetType, Type valueType, object value)
        {
            List<object?> converted = new();
            var strongInnerType = targetType.IsArray ? targetType.GetElementType()! : targetType.GenericTypeArguments.FirstOrDefault();

            foreach (var val in (IEnumerable)value)
            {
                converted.Add(strongInnerType is not null ? ConvertTo(strongInnerType, val) : val);
                
                //if (val is IDictionary<string, object?> raw)
                //{
                //    converted.Add(strongInnerType is not null ? TypeBuilder.BuildObject(strongInnerType, raw) : val);
                //}
                //else
                    

            }

            var arr = Array.CreateInstance(strongInnerType ?? valueType.GenericTypeArguments[0], converted.Count);
            Array.Copy(converted.ToArray(), arr, converted.Count);

            switch (targetType)
            {
                case Type when targetType.Name == typeof(List<>).Name:
                    {
                        var l = typeof(List<>).MakeGenericType(strongInnerType ?? valueType.GenericTypeArguments[0]);
                        return Activator.CreateInstance(l, arr);
                    }
                case Type when targetType.IsArray || targetType.IsAssignableTo(typeof(IEnumerable)):
                    {
                        return arr;
                    }
                default:
                    {
                        if (arr.GetType().IsAssignableTo(targetType))
                            return ReflectionUtils.DynamicCast(arr, targetType);

                        throw new EdgeDBException($"Couldn't convert {valueType} to {targetType}");
                    }
            }
        }
    }
}
