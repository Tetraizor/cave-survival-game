using System.Collections.Generic;
using CaveTogether.Game.States;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Services.Matchmaker.Models;

namespace CaveTogether.Game
{
    public class GameStateManager : NetworkBehaviour
    {
        private Dictionary<GameStateType, IGameState> _states = new();
        public IGameState State { get; private set; }

        private void Awake()
        {
            var gameStates = transform.GetComponentsInChildren<IGameState>();
            foreach (var state in gameStates)
            {
                _states.Add(state.Type, state);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer) SwitchStateRpc(GameStateType.Start);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void SwitchStateRpc(GameStateType type)
        {
            if (State != null) State.Exit();

            State = _states[type];
            Debug.Log($"New state is {Enum.GetName(typeof(GameStateType), type)}");

            State.Enter();
        }
    }
}