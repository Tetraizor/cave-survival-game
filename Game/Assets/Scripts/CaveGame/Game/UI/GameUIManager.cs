using System;
using System.Linq;
using CaveTogether.Common;
using CaveTogether.Game.Turn;
using CaveTogether.Services;
using TMPro;
using UnityEngine;

namespace CaveTogether.Game.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] private PlayerStatusPanel[] _playerPanels;

        [SerializeField] private TextMeshProUGUI _turnInfoLabel;
        [SerializeField] private TextMeshProUGUI _currentPlayingPlayerLabel;
        [SerializeField] private TextMeshProUGUI _gameStatusLabel;

        private TurnManager _turnManager;

        public void Initialize(GameConfig config)
        {
            _turnManager = FindAnyObjectByType<TurnManager>();
            _turnManager.RoundEnded += OnRoundEnded;
            _turnManager.TurnStarted += OnTurnStarted;

            SetupPlayerPanels(config);
        }

        private void SetupPlayerPanels(GameConfig config)
        {
            for (int i = 0; i < SessionManagerService.MAX_PLAYERS; i++)
            {
                bool panelActive = i < config.Players.Length;
                _playerPanels[i].gameObject.SetActive(panelActive);

                if (!panelActive)
                {
                    Destroy(_playerPanels[i].gameObject);
                    continue;
                }

                _playerPanels[i].Initialize(config.Players[i]);
            }
        }

        public void Deinitialize()
        {
            _turnManager.RoundEnded -= OnRoundEnded;
            _turnManager.TurnStarted -= OnTurnStarted;
        }

        private void OnTurnStarted(ulong turnOwnerId)
        {
            var seat = ServiceLocator.Get<SessionManagerService>().Seats.ToList().Find(s => s.ClientID == turnOwnerId);
            _currentPlayingPlayerLabel.SetText($"{seat.ConnectionData.Username}'s turn");

            _turnInfoLabel.SetText($"Round: {_turnManager.CurrentRound}, Turn: {_turnManager.CurrentTurn}/{_turnManager.TurnOrder.Count}");
        }

        private void OnRoundEnded(int round)
        {
            _turnInfoLabel.SetText($"Round: {_turnManager.CurrentRound}, Turn: {_turnManager.CurrentTurn}/{_turnManager.TurnOrder.Count}");
            _gameStatusLabel.SetText("Round is now over. Starting a minigame.");
        }
    }
}