using System;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.Minigames
{
    public abstract class MinigameBase : NetworkBehaviour
    {
        public abstract void Initialize(MinigameContext context);
        public event Action<MinigameResult> Completed;
        protected void RaiseCompleted(MinigameResult result) => Completed?.Invoke(result);
    }
}