using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Menu.UI
{
    public class MainPanel : MenuPanelBase
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            _hostButton.onClick.AddListener(() => _menuUIController.SwitchToPanel(MenuUIController.MenuPanelType.Host));
            _joinButton.onClick.AddListener(() => _menuUIController.SwitchToPanel(MenuUIController.MenuPanelType.Join));
            _settingsButton.onClick.AddListener(() => _menuUIController.SwitchToPanel(MenuUIController.MenuPanelType.Settings));
            _quitButton.onClick.AddListener(() => Application.Quit());
        }
    }
}