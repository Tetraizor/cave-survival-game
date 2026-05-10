using System;
using Unity.Collections;
using Unity.Netcode;

namespace CaveGame.PlayerData
{
    [Serializable]
    public struct PlayerConfig : INetworkSerializable
    {
        public ulong OwnerClientId;
        public FixedString32Bytes Username;
        public FixedString32Bytes CharacterId; // TODO: Temporary variable

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref OwnerClientId);
            serializer.SerializeValue(ref Username);
            serializer.SerializeValue(ref CharacterId);
        }
    }
}