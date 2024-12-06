using X975.Protocol;
using Albion.Network;
using System.Collections.Generic;
using System.Reflection;

namespace X975.Radar.Packets.Handlers
{
    public class ChangeClusterEvent : BaseOperation
    {
        byte[] offsets = Init.PacketOffsets.ChangeCluster;

        public ChangeClusterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            LocationId = parameters[offsets[0]] as string;
            Type = parameters.ContainsKey(offsets[1]) ? parameters[offsets[1]] as string : "NULL";
        }

        public string LocationId { get; }

        public string Type { get;}
    }
}
