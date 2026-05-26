using CaveTogether.Game;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using CaveTogether.Services;
using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.DebugUtils
{
    public class GameDebugUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _debugPanel;

        [Header("Player Actions")]
        [SerializeField] private Button _downPlayerButton;
        [SerializeField] private Button _healPlayerButton;
        [SerializeField] private Button _refreshEnergyButton;

        [Header("Map Actions")]
        [SerializeField] private Button _revealExitButton;
        [SerializeField] private Button _revealWholeMapButton;

        private GameDebugManager _debugManager;
        private InputService _inputService;

        private void Start()
        {
            _inputService = ServiceLocator.Get<InputService>();
            _inputService.DebugPanelToggled += OnDebugPanelToggled;

            _debugManager = FindAnyObjectByType<GameDebugManager>();

            _downPlayerButton.onClick.AddListener(() => _debugManager.DebugDownServerRpc());
            _healPlayerButton.onClick.AddListener(() => _debugManager.DebugHealServerRpc());
            _refreshEnergyButton.onClick.AddListener(() => _debugManager.DebugRefreshEnergyServerRpc());
            _revealExitButton.onClick.AddListener(() => _debugManager.RevealExit());
            _revealWholeMapButton.onClick.AddListener(() => _debugManager.RevealWholeMap());
        }

        private void OnDestroy()
        {
            if (_inputService != null)
                _inputService.DebugPanelToggled -= OnDebugPanelToggled;
        }

        private void OnDebugPanelToggled() => _debugPanel.SetActive(!_debugPanel.activeSelf);
    }
}
