using System;
using CaveTogether.Common.Enums;
using Unity.Collections;
using Unity.Netcode;

namespace CaveTogether.Common
{
    [Serializable]
    public struct GameConfig : INetworkSerializable
    {
        public FixedString32Bytes Seed;
        public Difficulty Difficulty;
        public PlayerConfig[] Players;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Seed);
            serializer.SerializeValue(ref Difficulty);

            int length = 0;
            if (serializer.IsWriter)
            {
                length = Players != null ? Players.Length : 0;
            }

            serializer.SerializeValue(ref length);

            if (serializer.IsReader)
            {
                Players = new PlayerConfig[length];
            }

            for (int i = 0; i < length; i++)
            {
                serializer.SerializeValue(ref Players[i]);
            }
        }
    }
}