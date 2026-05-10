using System;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common;
using Unity.Netcode;

namespace CaveTogether.Game.Turn
{
    public class TurnManager : NetworkBehaviour
    {
        public ulong CurrentTurnOwner { get; private set; } = 0;

        public int CurrentRound { get; private set; } = 0;
        public int CurrentTurn { get; private set; } = 0;

        public event Action<ulong> TurnStarted;
        public event Action<int> RoundEnded;

        private List<ulong> _turnOrder = new();

        public void Initialize(GameConfig config)
        {
            _turnOrder = config.Players.Select(p => p.OwnerClientId).ToList();
            StartRoundRpc();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void StartRoundRpc()
        {
            CurrentTurn = 0;
            CurrentRound++;
            CurrentTurnOwner = _turnOrder[0];

            TurnStarted?.Invoke(CurrentTurnOwner);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void AdvanceTurnRpc()
        {
            CurrentTurn++;
            if (CurrentTurn == _turnOrder.Count)
            {
                RoundEnded?.Invoke(CurrentRound);
            }
            else
            {
                int currentPlayerIndex = _turnOrder.IndexOf(CurrentTurnOwner);
                CurrentTurnOwner = _turnOrder[(currentPlayerIndex + 1) % _turnOrder.Count];

                TurnStarted?.Invoke(CurrentTurnOwner);
            }
        }
    }
}