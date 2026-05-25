using System;
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
        }

        public void Deinitialize() { }

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
            actionToExecute.Execute(_mapManager.Map, character, request);

            if (character.Energy == 0 && request.Type != ActionType.EndTurn)
            {
                character.ResetEnergy();
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
                _ => null
            };
        }

        public Sprite GetActionIcon(ActionType type) => _actionIconLookup[type];
    }
}