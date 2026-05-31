using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Game.Actions
{
    public class MapActionSelectorRow : MonoBehaviour
    {
        public Action<ActionRequest> RequestSelected;

        [SerializeField] TextMeshProUGUI _actionNameLabel;
        [SerializeField] TextMeshProUGUI _actionEnergyCostLabel;
        [SerializeField] Image _actionIconImage;
        [SerializeField] Button _actionRowButton;

        private ActionRequest _request;

        public void Initialize(CellActionEntry entry)
        {
            var icon = FindAnyObjectByType<ActionManager>().GetActionIcon(entry.Type);
            _actionRowButton.onClick.AddListener(OnActionRowButtonClicked);

            _actionNameLabel.SetText(entry.Title);
            _actionEnergyCostLabel.SetText(entry.EnergyCost.ToString());
            _actionIconImage.sprite = icon;

            _request = entry.Request ?? new ActionRequest { TargetCell = entry.Position, Type = entry.Type };
        }

        private void OnActionRowButtonClicked()
        {
            RequestSelected?.Invoke(_request);
        }
    }
}