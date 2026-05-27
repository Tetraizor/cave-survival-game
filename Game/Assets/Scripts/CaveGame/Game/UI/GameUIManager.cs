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

        private TurnManager _turnManager;

        public void Initialize(GameConfig config)
        {
            _turnManager = FindAnyObjectByType<TurnManager>();
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
            _turnManager.TurnStarted -= OnTurnStarted;
        }

        private void OnTurnStarted(ulong turnOwnerId)
        {
            var seat = ServiceLocator.Get<SessionManagerService>().Seats.ToList().Find(s => s.ClientID == turnOwnerId);
        }
    }
}