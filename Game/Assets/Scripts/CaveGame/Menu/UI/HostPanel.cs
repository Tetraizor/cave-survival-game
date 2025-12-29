using CaveGame.Game;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CaveGame.Menu.UI
{
    public class HostPanel : MenuPanelBase
    {
        [SerializeField] private TMP_InputField _addressInput;
        [SerializeField] private TMP_InputField _portInput;
        [SerializeField] private Button _hostButton;

        [SerializeField] private Button _backButton;

        private void Start()
        {
            _hostButton.onClick.AddListener(AttemptHost);
            _backButton.onClick.AddListener(() => _menuUIController.SwitchToPanel(MenuUIController.MenuPanelType.Main));

            _addressInput.onValueChanged.AddListener((text) => ValidateInput(out _, out _));
            _portInput.onValueChanged.AddListener((text) => ValidateInput(out _, out _));
        }

        private bool ValidateInput(out string address, out ushort validatedPort)
        {
            bool isValid = true;

            string portRaw = _portInput.text;
            address = _addressInput.text;

            if (!ushort.TryParse(portRaw, out validatedPort)) isValid = false;
            if (string.IsNullOrWhiteSpace(address)) isValid = false;
            if (NetworkManager.Singleton.IsListening) isValid = false;

            _hostButton.interactable = isValid;
            return isValid;
        }

        private void AttemptHost()
        {
            if (ValidateInput(out string address, out ushort port))
                MainMenuController.Instance.Host(address, port);

            // For making sure if a connection happened, button is disabled
            ValidateInput(out _, out _);
        }
    }
}