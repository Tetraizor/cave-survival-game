using CaveTogether.Common;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] private PlayerStatusPanel[] _playerPanels;

        public void Initialize(GameConfig config)
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
    }
}