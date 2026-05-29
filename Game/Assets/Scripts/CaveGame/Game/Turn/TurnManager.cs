using System;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common;
using CaveTogether.Game.UI;
using CaveTogether.Services;
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

        public List<ulong> TurnOrder { get; private set; } = new();

        private readonly Dictionary<ulong, string> _playerNames = new();
        private GameNotificationUI _notifications;

        public void Initialize(GameConfig config)
        {
            TurnOrder = config.Players.Select(p => p.OwnerClientId).ToList();
            foreach (var p in config.Players)
                _playerNames[p.OwnerClientId] = p.Username.ToString();

            _notifications = ServiceLocator.Get<GameNotificationUI>();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void StartRoundRpc()
        {
            CurrentTurn = 0;
            CurrentRound++;
            CurrentTurnOwner = TurnOrder[0];

            string firstName = _playerNames.TryGetValue(CurrentTurnOwner, out var n) ? n : "???";
            _notifications.Push($"— Round {CurrentRound} —");
            _notifications.Push($"{firstName}'s turn");
            TurnStarted?.Invoke(CurrentTurnOwner);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void AdvanceTurnRpc()
        {
            CurrentTurn++;

            if (CurrentTurn == TurnOrder.Count)
            {
                RoundEnded?.Invoke(CurrentRound);
                CurrentTurn = 0;
                CurrentTurnOwner = ulong.MaxValue;
            }
            else
            {
                int currentPlayerIndex = TurnOrder.IndexOf(CurrentTurnOwner);
                CurrentTurnOwner = TurnOrder[(currentPlayerIndex + 1) % TurnOrder.Count];

                string name = _playerNames.TryGetValue(CurrentTurnOwner, out var n) ? n : "???";
                _notifications.Push($"{name}'s turn");
                TurnStarted?.Invoke(CurrentTurnOwner);
            }
        }
    }
}