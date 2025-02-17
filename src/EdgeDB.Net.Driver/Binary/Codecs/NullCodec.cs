namespace EdgeDB.Binary.Codecs
{
    internal sealed class NullCodec
        : ICodec, IArgumentCodec, ICacheableCodec
    {
        public Type ConverterType => typeof(object);

        public bool CanConvert(Type t)
        {
            return true;
        }

        public object? Deserialize(ref PacketReader reader) { return null; }

        public void Serialize(ref PacketWriter writer, object? value)
        {
            writer.Write(0);
        }

        public void SerializeArguments(ref PacketWriter writer, object? value) { }
    }
}
