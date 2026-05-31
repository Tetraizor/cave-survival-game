using System.Collections;
using CaveTogether.Game.CellEffects;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using UnityEngine;

namespace CaveTogether.Game
{
    public class CellEffectManager : MonoBehaviour
    {
        private MapManager _mapManager;

        private void Start() => _mapManager = FindAnyObjectByType<MapManager>();

        public IEnumerator TriggerEffects(CellEffectTrigger trigger, Vector2Int pos, Character character)
        {
            if (_mapManager == null) yield break;

            foreach (var provider in _mapManager.Generator.GetEffectProviders())
                foreach (var cellEffect in provider.GetEffectsForCell(pos))
                    if (cellEffect.Trigger == trigger)
                        yield return character.StartCoroutine(cellEffect.Effect.Apply(character));
        }
    }
}
