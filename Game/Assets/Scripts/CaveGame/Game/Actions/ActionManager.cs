using System;
using CaveGame.Common.Enums;
using CaveGame.Game.Entities;
using CaveGame.Services;
using Unity.Netcode;
using UnityEngine;

namespace CaveGame.Game.Actions
{
    public class ActionManager : NetworkBehaviour
    {
        private CharacterManager _characterManager;

        public void Initialize()
        {
            _characterManager = FindAnyObjectByType<CharacterManager>();
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
            var character = _characterManager.GetCharacter(rpcParams.Receive.SenderClientId);
            if (!character.CanDoAction(request.Type))
            {
                Debug.LogError($"[ActionManager] Illegal action! From {rpcParams.Receive.SenderClientId}, for action type {Enum.GetName(typeof(ActionType), request.Type)}");
            }
        }
    }
}