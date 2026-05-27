using Unity.Netcode;

namespace CaveTogether.Minigames
{
    public struct MinigameResult : INetworkSerializable
    {
        public ulong[] PlayerRanking;
        public int[] HealthDeltas;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int len = PlayerRanking?.Length ?? 0;
            serializer.SerializeValue(ref len);
            if (serializer.IsReader)
            {
                PlayerRanking = new ulong[len];
                HealthDeltas = new int[len];
            }
            for (int i = 0; i < len; i++)
            {
                serializer.SerializeValue(ref PlayerRanking[i]);
                serializer.SerializeValue(ref HealthDeltas[i]);
            }
        }
    }
}