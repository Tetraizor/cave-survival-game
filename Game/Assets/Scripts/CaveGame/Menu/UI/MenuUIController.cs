using System.Linq;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Menu.UI
{
    public class MenuUIController : MonoBehaviour
    {
        private const float TRANSITION_TIME = .3f;

        public enum MenuPanelType
        {
            None = -1,
            Main,
            Host,
            Join,
            Settings
        }

        [SerializedDictionary("Panel Type", "Rect")]
        public SerializedDictionary<MenuPanelType, MenuPanelBase> _panels = new();

        private MenuPanelType _activePanel = MenuPanelType.None;
        public MenuPanelType ActivePanel => _activePanel;

        private void Start()
        {
            _panels.Values.ToList().ForEach(value => value.Initialize(this));

            SwitchToPanel(MenuPanelType.Main);
        }

        public void SwitchToPanel(MenuPanelType panelType)
        {
            if (panelType == _activePanel) return;

            MenuPanelBase newActivePanel;
            if (!_panels.TryGetValue(panelType, out newActivePanel))
            {
                Debug.LogError($"Could not find type {panelType}");
                return;
            }

            MenuPanelBase previousActivePanel;
            MenuPanelType previousActivePanelType = _activePanel;

            _activePanel = panelType;

            _panels.TryGetValue(previousActivePanelType, out previousActivePanel);
            if (previousActivePanel != null)
                previousActivePanel.GetComponent<RectTransform>().DOAnchorPosX
                (
                    previousActivePanelType == MenuPanelType.Main ?
                        GetComponent<CanvasScaler>().referenceResolution.x * -1 :
                        GetComponent<CanvasScaler>().referenceResolution.x,
                    TRANSITION_TIME
                );

            if (newActivePanel != null)
                newActivePanel.GetComponent<RectTransform>().DOAnchorPosX
                (
                    0,
                    TRANSITION_TIME
                );
        }
    }
}
