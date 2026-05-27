using System;
using UnityEngine;

namespace CaveTogether.Minigames
{
    public abstract class MinigameBase : MonoBehaviour
    {
        public abstract void Initialize(MinigameContext context);
        public event Action<MinigameResult> Completed;
        protected void RaiseCompleted(MinigameResult result) => Completed?.Invoke(result);
    }
}