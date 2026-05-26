using CaveTogether.Common;
using CaveTogether.Game;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using CaveTogether.Generation.Features;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.DebugUtils
{
    public class GameDebugManager : NetworkBehaviour
    {
        private bool _isCheatsEnabled;
        private CharacterManager _characterManager;
        private MapManager _mapManager;
        private ExplorationManager _explorationManager;

        public void Initialize(GameConfig config)
        {
            _isCheatsEnabled = config.IsCheatsEnabled;
            _characterManager = FindAnyObjectByType<CharacterManager>();
            _mapManager = FindAnyObjectByType<MapManager>();
            _explorationManager = FindAnyObjectByType<ExplorationManager>();
        }

        // ── Character cheats ──────────────────────────────────────────

        [Rpc(SendTo.Server)]
        public void DebugDownServerRpc(RpcParams rpcParams = default)
        {
            if (!_isCheatsEnabled) return;
            ApplyDebugDownRpc(rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ApplyDebugDownRpc(ulong clientId)
        {
            var c = _characterManager.GetCharacter(clientId);
            if (c != null) c.TakeDamage(c.Health);
        }

        [Rpc(SendTo.Server)]
        public void DebugHealServerRpc(RpcParams rpcParams = default)
        {
            if (!_isCheatsEnabled) return;
            ApplyDebugHealRpc(rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ApplyDebugHealRpc(ulong clientId)
        {
            var c = _characterManager.GetCharacter(clientId);
            if (c != null) c.Heal(c.MaxHealth);
        }

        [Rpc(SendTo.Server)]
        public void DebugRefreshEnergyServerRpc(RpcParams rpcParams = default)
        {
            if (!_isCheatsEnabled) return;
            ApplyDebugRefreshEnergyRpc(rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ApplyDebugRefreshEnergyRpc(ulong clientId)
        {
            var c = _characterManager.GetCharacter(clientId);
            if (c != null) c.ResetEnergy();
        }

        // ── Map cheats ────────────────────────────────────────────────

        [Rpc(SendTo.Server)]
        public void RevealExitServerRpc()
        {
            if (!_isCheatsEnabled) return;
            ApplyRevealExitRpc();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ApplyRevealExitRpc()
        {
            var exit = _mapManager.Map.GetFeature<ExitFeature>();
            if (exit != null) _explorationManager.RevealFromPosition(exit.ExitPosition);
        }

        [Rpc(SendTo.Server)]
        public void RevealWholeMapServerRpc()
        {
            if (!_isCheatsEnabled) return;
            ApplyRevealWholeMapRpc();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ApplyRevealWholeMapRpc()
        {
            for (int y = 0; y < _mapManager.Map.Height; y++)
                for (int x = 0; x < _mapManager.Map.Width; x++)
                    if (!_mapManager.Map.GetCellRef(x, y).IsEmpty)
                        _explorationManager.RevealFromPosition(new Vector2Int(x, y));
        }
    }
}