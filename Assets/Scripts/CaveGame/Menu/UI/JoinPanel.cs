using UnityEngine;
using UnityEngine.UI;

namespace CaveGame.Menu.UI
{
    public class JoinPanel : MenuPanelBase
    {
        [SerializeField] private Button _backButton;

        private void Start()
        {
            _backButton.onClick.AddListener(() => _menuUIController.SwitchToPanel(MenuUIController.MenuPanelType.Main));
        }
    }
}