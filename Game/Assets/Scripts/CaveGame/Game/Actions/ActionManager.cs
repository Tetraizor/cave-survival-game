using System;
using System.Collections;
using AYellowpaper.SerializedCollections;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Turn;
using CaveTogether.Generation;
using CaveTogether.Services;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class ActionManager : NetworkBehaviour
    {
        public Action<ulong> ActionStarted;
        public Action<ulong> ActionExecuted;

        private CharacterManager _characterManager;
        private TurnManager _turnManager;
        private MapManager _mapManager;

        [SerializeField] private SerializedDictionary<ActionType, Sprite> _actionIconLookup = new();

        public void Initialize()
        {
            _characterManager = FindAnyObjectByType<CharacterManager>();
            _turnManager = FindAnyObjectByType<TurnManager>();
            _mapManager = FindAnyObjectByType<MapManager>();

            _turnManager.TurnStarted += OnTurnStartedAutoSkip;
        }

        public void Deinitialize()
        {
            _turnManager.TurnStarted -= OnTurnStartedAutoSkip;
        }

        public void RequestAction(Character character, ActionRequest request)
        {
            if (character.OwnerClientId != ServiceLocator.Get<SessionManagerService>().LocalClientId) return;
            SubmitActionServerRpc(request);
        }

        [Rpc(SendTo.Server)]
        private void SubmitActionServerRpc(ActionRequest request, RpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (_turnManager.CurrentTurnOwner != senderId)
            {
                Debug.LogWarning($"[ActionManager] Player {senderId} tried to act out of turn!");
                return;
            }

            var character = _characterManager.GetCharacter(senderId);
            if (!character.CanDoAction(request.Type))
            {
                Debug.LogError($"[ActionManager] Illegal action! From {rpcParams.Receive.SenderClientId}, for action type {Enum.GetName(typeof(ActionType), request.Type)}");
                return;
            }

            GameActionBase actionToExecute = GetActionLogic(request.Type);
            if (actionToExecute == null) throw new Exception($"ActionType {Enum.GetName(typeof(ActionType), request.Type)} does not have an action registered!");

            bool isActionValid = actionToExecute.IsValid(_mapManager.Map, character, request);
            int energyCost = actionToExecute.GetEnergyCost(_mapManager.Map, character, request);

            if (isActionValid && character.Energy >= energyCost)
            {
                ExecuteActionRpc(request, energyCost, senderId);
            }
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ExecuteActionRpc(ActionRequest request, int energyCost, ulong characterId)
        {
            var character = _characterManager.GetCharacter(characterId);
            GameActionBase actionToExecute = GetActionLogic(request.Type);

            character.UseEnergy(energyCost);
            StartCoroutine(RunAction(actionToExecute, request, character, characterId));
        }

        private IEnumerator RunAction(GameActionBase action, ActionRequest request, Character character, ulong characterId)
        {
            ActionStarted?.Invoke(characterId);

            yield return StartCoroutine(action.Execute(_mapManager.Map, character, request));

            if (character.Energy == 0 || character.IsDown)
            {
                if (IsServer) _turnManager.AdvanceTurnRpc();
            }

            ActionExecuted?.Invoke(characterId);
        }

        public GameActionBase GetActionLogic(ActionType type)
        {
            return type switch
            {
                ActionType.Wait => new WaitAction(),
                ActionType.Walk => new WalkAction(),
                ActionType.EndTurn => new EndTurnAction(),
                ActionType.Inspect => new InspectAction(),
                ActionType.Revive => new ReviveAction(),
                ActionType.DebugDown => new DebugDownAction(),
                _ => null
            };
        }

        private void OnTurnStartedAutoSkip(ulong turnOwnerId)
        {
            if (_turnManager.CurrentTurn == 0)
            {
                foreach (var c in _characterManager.Characters)
                    c.ResetEnergy();
            }

            if (!IsServer) return;
            var character = _characterManager.GetCharacter(turnOwnerId);
            if (character == null || !character.IsDown) return;
            StartCoroutine(AutoSkipDownedPlayer(character, turnOwnerId));
        }

        private IEnumerator AutoSkipDownedPlayer(Character character, ulong characterId)
        {
            yield return new WaitForSeconds(1f);
            if (!character.IsDown) yield break;
            ExecuteActionRpc(new ActionRequest { Type = ActionType.EndTurn }, character.Energy, characterId);
        }

        public Sprite GetActionIcon(ActionType type)
        {
            if (_actionIconLookup.TryGetValue(type, out var sprite)) return sprite;
            else return null;
        }
    }
}