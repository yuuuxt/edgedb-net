﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public class Terminate : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.Terminate;

        protected override void BuildPacket(PacketWriter writer, EdgeDBTcpClient client) { } // no data
    }
}
