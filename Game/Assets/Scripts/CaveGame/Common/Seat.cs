using System;
using Unity.Netcode;

namespace CaveGame.Common
{
    public struct Seat : INetworkSerializable, IEquatable<Seat>
    {
        public Seat(ulong clientId)
        {
            ClientID = clientId;
            ConnectionData = new UserConnectionData();
        }

        public ulong ClientID;
        public UserConnectionData ConnectionData;

        public bool IsTaken => ClientID != ulong.MaxValue;

        public bool Equals(Seat other)
        {
            return ClientID == other.ClientID;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientID);
            serializer.SerializeValue(ref ConnectionData);
        }
    }
}