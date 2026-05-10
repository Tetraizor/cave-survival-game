using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Menu.UI
{
    public class SettingsPanel : MenuPanelBase
    {
        [SerializeField] private Button _backButton;

        private void Start()
        {
            _backButton.onClick.AddListener(() => _menuUIController.SwitchToPanel(MenuUIController.MenuPanelType.Main));
        }
    }
}