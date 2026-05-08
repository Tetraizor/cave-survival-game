using CaveGame.Common;
using CaveGame.Services;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

namespace CaveGame.Menu.UI
{
    public class HostPanel : MenuPanelBase
    {
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private TMP_InputField _addressInput;
        [SerializeField] private TMP_InputField _portInput;
        [SerializeField] private Button _hostButton;

        [SerializeField] private Button _backButton;

        private void Start()
        {
            _hostButton.onClick.AddListener(AttemptHost);
            _backButton.onClick.AddListener(() => _menuUIController.SwitchToPanel(MenuUIController.MenuPanelType.Main));

            int randomNumber = Random.Range(1000, 10000);
            string randomName = $"Player #{randomNumber}";
            _usernameInput.text = randomName;

            _addressInput.onValueChanged.AddListener((text) => ValidateInput(out _, out _, out _));
            _portInput.onValueChanged.AddListener((text) => ValidateInput(out _, out _, out _));
            _usernameInput.onValueChanged.AddListener((text) => ValidateInput(out _, out _, out _));
        }

        private bool ValidateInput(out string username, out string address, out ushort validatedPort)
        {
            bool isValid = true;

            string portRaw = _portInput.text;
            address = _addressInput.text;
            username = _usernameInput.text;

            if (!ushort.TryParse(portRaw, out validatedPort)) isValid = false;
            if (string.IsNullOrWhiteSpace(address)) isValid = false;
            if (string.IsNullOrWhiteSpace(username)) isValid = false;
            if (NetworkManager.Singleton.IsListening) isValid = false;

            _hostButton.interactable = isValid;
            return isValid;
        }

        private void AttemptHost()
        {
            if (!ValidateInput(out string username, out string address, out ushort port)) return;

            var playerData = new UserConnectionData
            {
                Username = username
            };

            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            ServiceLocator.Get<GameFlowService>().Host(address, port, playerData);

            // For making sure if a connection happened, button is disabled
            ValidateInput(out _, out _, out _);
        }

        private void OnServerStarted()
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            ServiceLocator.Get<GameFlowService>().StartLobby();
        }
    }
}