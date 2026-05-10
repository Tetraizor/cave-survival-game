using CaveTogether.Common.Enums;
using CaveTogether.Constants;
using UnityEngine;

namespace CaveTogether.Generation
{
    [CreateAssetMenu(fileName = "CellBaseData", menuName = "Cave Game/Generation/CellBaseData", order = 1)]
    public class CellBaseData : ScriptableObject
    {
        [SerializeField] private Direction _openDirections;
        public Direction OpenDirections => _openDirections;

        [SerializeField] private GameObject _prefab;
        public GameObject Prefab => _prefab;

        [SerializeField][Min(0f)] private float _baseWeight = 1f;
        public float BaseWeight => _baseWeight;

        [SerializeField] private AnimationCurve _spawnChanceMultOverEdgeDistance;
        [SerializeField] private AnimationCurve _mapFullnessMultiplier;

        public float GetCalculatedWeight(float distanceToEdgeNormalized, float mapFillPercentage)
        {
            float distanceMultiplier = (_spawnChanceMultOverEdgeDistance == null || _spawnChanceMultOverEdgeDistance.length == 0)
                ? 1f
                : _spawnChanceMultOverEdgeDistance.Evaluate(distanceToEdgeNormalized);

            float mapFillMultiplier = (_mapFullnessMultiplier == null || _mapFullnessMultiplier.length == 0)
                ? 1.0f
                : _mapFullnessMultiplier.Evaluate(mapFillPercentage);

            return _baseWeight * distanceMultiplier;
        }
    }
}