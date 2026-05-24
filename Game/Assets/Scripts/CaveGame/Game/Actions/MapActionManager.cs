using System.Collections.Generic;
using System.Linq;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Turn;
using CaveTogether.Generation;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.Assertions;

namespace CaveTogether.Game.Actions
{
    public class MapActionManager : MonoBehaviour
    {
        [SerializeField] private GameObject _actionSelectorPrefab;

        private Character _clientCharacter;
        private TurnManager _turnManager;
        private MapManager _mapManager;
        private ActionManager _actionManager;
        private MapRenderManager _mapRenderManager;

        private bool _actionHintsEnabled;

        private Dictionary<Vector2Int, ActionSelector> _instantiatedSelectors = new();

        public void Initialize()
        {
            _mapManager = FindAnyObjectByType<MapManager>();
            _actionManager = FindAnyObjectByType<ActionManager>();
            _mapRenderManager = FindAnyObjectByType<MapRenderManager>();
            _turnManager = FindAnyObjectByType<TurnManager>();

            _turnManager.TurnStarted += OnTurnStarted;

            _clientCharacter = FindAnyObjectByType<CharacterManager>().GetClientCharacter();

            Assert.IsNotNull(_clientCharacter);
            Assert.IsNotNull(_actionSelectorPrefab);
        }

        public void Deinitialize()
        {
            _turnManager.TurnStarted -= OnTurnStarted;

            foreach (var selector in _instantiatedSelectors)
            {
                if (selector.Value.gameObject)
                    Destroy(selector.Value.gameObject);
            }

            _instantiatedSelectors = new();
        }

        private void OnDestroy() => Deinitialize();

        private void ToggleActionHints(bool state)
        {
            if (_actionHintsEnabled == state) return;
            _actionHintsEnabled = state;

            int width = _mapManager.Map.Width;
            int height = _mapManager.Map.Height;

            var possibleActionTypes = _clientCharacter.PossibleActionTypes;
            var possibleActions = possibleActionTypes
                .Select(at => _actionManager.GetActionLogic(at))
                .Where(a => a.UIType == ActionUIType.ContextualCell);

            var actionRequest = new ActionRequest();

            if (state)
            {
                // TODO: Check only explored cells
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int validActionCount = 0;
                        foreach (var action in possibleActions)
                        {
                            actionRequest.TargetCell = new Vector2Int(x, y);
                            actionRequest.Type = action.Type;

                            if (action.IsValid(_mapManager.Map, _clientCharacter, actionRequest)
                                && action.GetEnergyCost(_mapManager.Map, _clientCharacter, actionRequest) <= _clientCharacter.Energy)
                            {
                                validActionCount++;
                            }
                        }

                        if (validActionCount > 0)
                        {
                            var selectorGO = Instantiate(_actionSelectorPrefab);
                            selectorGO.transform.position = _mapRenderManager.GridToWorldPosition(new Vector2Int(x, y));
                            var selector = selectorGO.GetComponent<ActionSelector>();
                            selector.Initialize(validActionCount); // TODO: Fix count

                            _instantiatedSelectors.Add(new Vector2Int(x, y), selector);
                        }
                    }
                }
            }
            else
            {
                foreach (var selector in _instantiatedSelectors)
                {
                    Destroy(selector.Value.gameObject);
                }

                _instantiatedSelectors = new();
            }
        }

        private void OnTurnStarted(ulong turnOwnerId)
        {

            ToggleActionHints(turnOwnerId == _clientCharacter.OwnerClientId);
        }
    }
}