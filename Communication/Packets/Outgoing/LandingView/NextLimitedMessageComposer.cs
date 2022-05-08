using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.LandingView
{
    class NextLimitedMessageComposer : ServerPacket
    {
        public NextLimitedMessageComposer()
            : base(ServerPacketHeader.NextLimitedComposer)
        {
            base.WriteInteger(230); // ItemId
            base.WriteInteger(9); // PageId
            base.WriteInteger(100); // Seconds to open page
            base.WriteString("throne"); // Productdata image
        }
    }
}