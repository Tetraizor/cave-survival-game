using UnityEngine;

namespace CaveGame.Menu.UI
{
    public abstract class MenuPanelBase : MonoBehaviour
    {
        protected MenuUIController _menuUIController;

        public void Initialize(MenuUIController controller)
        {
            _menuUIController = controller;
        }
    }
}
