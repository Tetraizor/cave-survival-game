using System;
using Unity.Collections;
using Unity.Netcode;

namespace CaveTogether.Common
{
    [Serializable]
    public struct UserConnectionData : INetworkSerializable
    {
        public string Username;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var fixedUsername = new FixedString32Bytes(Username ?? "");
            serializer.SerializeValue(ref fixedUsername);
            if (serializer.IsReader) Username = fixedUsername.ToString();
        }
    }
}