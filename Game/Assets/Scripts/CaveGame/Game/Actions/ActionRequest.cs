using CaveTogether.Common.Enums;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class ActionRequest : INetworkSerializable
    {
        public ActionType Type;
        public Vector2Int TargetCell;
        public byte ItemSlot;
        public byte ItemActionIndex;
        public ulong TargetCharacterId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref TargetCell);
            serializer.SerializeValue(ref ItemSlot);
            serializer.SerializeValue(ref ItemActionIndex);
            serializer.SerializeValue(ref TargetCharacterId);
        }
    }
}