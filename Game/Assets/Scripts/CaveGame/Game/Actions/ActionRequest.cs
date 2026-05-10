using CaveGame.Common.Enums;
using Unity.Netcode;
using UnityEngine;

namespace CaveGame.Game.Actions
{
    public class ActionRequest : INetworkSerializable
    {
        public ActionType Type;
        public Vector2Int TargetCell;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref TargetCell);
        }
    }
}