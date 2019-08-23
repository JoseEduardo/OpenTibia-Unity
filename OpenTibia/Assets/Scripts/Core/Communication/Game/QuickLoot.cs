﻿namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseUpdateLootContainers(Internal.ByteArray message) {
            byte unknown = message.ReadUnsignedByte();
            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                var type = message.ReadEnum<ObjectCategory>();
                ushort object_id = message.ReadUnsignedShort();
            }
        }
    }
}