using System;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Turn;
using CaveTogether.Generation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace CaveTogether.Game.Actions
{
    public struct CellActionEntry
    {
        public ActionType Type;
        public string Title;
        public int EnergyCost;
        public Vector2Int Position;
    }

    public class MapActionManager : MonoBehaviour
    {
        [SerializeField] private GameObject _actionSelectorPrefab;
        [SerializeField] private GameObject _mapActionSelectorListPrefab;

        private Character _clientCharacter;
        private TurnManager _turnManager;
        private MapManager _mapManager;
        private ActionManager _actionManager;
        private MapRenderManager _mapRenderManager;

        private bool _actionHintsEnabled;

        private Vector2Int? _openSelectorCell;

        private Dictionary<Vector2Int, ActionSelector> _instantiatedSelectors = new();
        private MapActionSelectorList _mapActionSelectorList;

        public void Initialize()
        {
            _mapManager = FindAnyObjectByType<MapManager>();
            _actionManager = FindAnyObjectByType<ActionManager>();
            _mapRenderManager = FindAnyObjectByType<MapRenderManager>();
            _turnManager = FindAnyObjectByType<TurnManager>();

            _turnManager.TurnStarted += OnTurnStarted;
            _turnManager.RoundEnded += OnRoundEnded;
            _actionManager.ActionExecuted += OnActionExecuted;
            _actionManager.ActionStarted += OnActionStarted;

            _clientCharacter = FindAnyObjectByType<CharacterManager>().GetClientCharacter();

            var cursorManager = FindAnyObjectByType<CursorManager>();
            cursorManager.CellClicked += OnCellClicked;
            cursorManager.CellHoverEnter += OnCellHoverEnter;
            cursorManager.CellHoverExit += OnCellHoverExit;

            Assert.IsNotNull(_clientCharacter);
            Assert.IsNotNull(_actionSelectorPrefab);
        }

        public void Deinitialize()
        {
            _turnManager.TurnStarted -= OnTurnStarted;
            _turnManager.RoundEnded -= OnRoundEnded;
            _actionManager.ActionExecuted -= OnActionExecuted;
            _actionManager.ActionStarted -= OnActionStarted;

            var cursorManager = FindAnyObjectByType<CursorManager>();
            cursorManager.CellClicked -= OnCellClicked;
            cursorManager.CellHoverEnter -= OnCellHoverEnter;
            cursorManager.CellHoverExit -= OnCellHoverExit;

            foreach (var selector in _instantiatedSelectors)
            {
                if (selector.Value != null && selector.Value.gameObject != null)
                    Destroy(selector.Value.gameObject);
            }

            _instantiatedSelectors = new();
            CloseActionSelector();
        }

        private void OnDestroy() => Deinitialize();

        private void ToggleActionHints(bool state)
        {
            if (_actionHintsEnabled == state) return;
            _actionHintsEnabled = state;

            int width = _mapManager.Map.Width;
            int height = _mapManager.Map.Height;

            if (state)
            {
                // TODO: Check only explored cells
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var validActions = GetValidActionEntriesOnCell(x, y);

                        if (validActions.Count > 0)
                        {
                            var selectorGO = Instantiate(_actionSelectorPrefab);
                            selectorGO.transform.position = _mapRenderManager.GridToWorldPosition(new Vector2Int(x, y));
                            var selector = selectorGO.GetComponent<ActionSelector>();
                            selector.Initialize(validActions.Count);

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

        public List<CellActionEntry> GetValidActionEntriesOnCell(int x, int y)
        {
            var possibleActionTypes = _clientCharacter.PossibleActionTypes;
            var possibleActions = possibleActionTypes
                .Select(at => _actionManager.GetActionLogic(at))
                .Where(a => a.UIType == ActionUIType.ContextualCell);

            var actionRequest = new ActionRequest();

            List<CellActionEntry> actions = new();

            foreach (var action in possibleActions)
            {
                actionRequest.TargetCell = new Vector2Int(x, y);
                actionRequest.Type = action.Type;

                int energyCost = action.GetEnergyCost(_mapManager.Map, _clientCharacter, actionRequest);

                if (action.IsValid(_mapManager.Map, _clientCharacter, actionRequest)
                    && energyCost <= _clientCharacter.Energy)
                {
                    actions.Add(new CellActionEntry
                    {
                        EnergyCost = energyCost,
                        Position = new Vector2Int(x, y),
                        Title = action.DisplayName,
                        Type = action.Type
                    });
                }
            }

            return actions;
        }

        public void OpenActionSelectorForCell(int x, int y)
        {
            if (!_mapManager.Map.IsInsideBounds(new Vector2Int(x, y)))
                throw new Exception("[MapActionManager] Selector is not inside of map bounds!");

            _openSelectorCell = new Vector2Int(x, y);

            CloseActionSelector();

            var cellActionEntries = GetValidActionEntriesOnCell(x, y);
            var cellWorldPos = _mapRenderManager.GridToWorldPosition(new Vector2Int(x, y));

            var mapActionSelectorListGO = Instantiate(_mapActionSelectorListPrefab);
            _mapActionSelectorList = mapActionSelectorListGO.GetComponent<MapActionSelectorList>();
            _mapActionSelectorList.Initialize(cellActionEntries, cellWorldPos);
            _mapActionSelectorList.RequestSelected += OnRequestSelected;
        }

        public void CloseActionSelector()
        {
            if (_mapActionSelectorList != null)
            {
                _mapActionSelectorList.RequestSelected -= OnRequestSelected;
                Destroy(_mapActionSelectorList.gameObject);

                _openSelectorCell = null;
                _mapActionSelectorList = null;
            }
        }

        private void RefreshSelectors()
        {
            var previousSelectorCell = _openSelectorCell;
            CloseActionSelector();

            foreach (var selector in _instantiatedSelectors)
                Destroy(selector.Value.gameObject);
            _instantiatedSelectors = new();

            int width = _mapManager.Map.Width;
            int height = _mapManager.Map.Height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var validActions = GetValidActionEntriesOnCell(x, y);
                    if (validActions.Count > 0)
                    {
                        var selectorGO = Instantiate(_actionSelectorPrefab);
                        selectorGO.transform.position = _mapRenderManager.GridToWorldPosition(new Vector2Int(x, y));
                        var selector = selectorGO.GetComponent<ActionSelector>();
                        selector.Initialize(validActions.Count);
                        _instantiatedSelectors.Add(new Vector2Int(x, y), selector);
                    }
                }
            }

            if (previousSelectorCell.HasValue && _instantiatedSelectors.ContainsKey(previousSelectorCell.Value))
                OpenActionSelectorForCell(previousSelectorCell.Value.x, previousSelectorCell.Value.y);
        }

        private void OnActionExecuted(ulong characterId)
        {
            if (!_actionHintsEnabled) return;
            RefreshSelectors();
        }

        private void OnActionStarted(ulong characterId)
        {
            if (!_actionHintsEnabled) return;

            CloseActionSelector();
            foreach (var selector in _instantiatedSelectors)
                Destroy(selector.Value.gameObject);
            _instantiatedSelectors = new();
        }

        private void OnRequestSelected(ActionRequest request)
        {
            _actionManager.RequestAction(_clientCharacter, request);
        }

        private void OnRoundEnded(int round)
        {
            ToggleActionHints(false);
            CloseActionSelector();
        }

        private void OnTurnStarted(ulong turnOwnerId)
        {
            bool isOwnAction = turnOwnerId == _clientCharacter.OwnerClientId;

            ToggleActionHints(isOwnAction);
            if (!isOwnAction) CloseActionSelector();
        }

        private void OnCellHoverExit(Vector2Int cellPosition)
        {
            if (_instantiatedSelectors.TryGetValue(cellPosition, out var selector))
            {
                selector.ToggleHoverState(false);
            }
        }

        private void OnCellHoverEnter(Vector2Int cellPosition)
        {
            if (_instantiatedSelectors.TryGetValue(cellPosition, out var selector))
            {
                selector.ToggleHoverState(true);
            }
        }

        private void OnCellClicked(Vector2Int cellPosition)
        {
            if (!_instantiatedSelectors.ContainsKey(cellPosition)) return;
            OpenActionSelectorForCell(cellPosition.x, cellPosition.y);
        }
    }
}