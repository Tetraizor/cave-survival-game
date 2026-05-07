using CaveGame.Common.Enums;
using UnityEngine;

namespace CaveGame.Generation
{
    [CreateAssetMenu(fileName = "CellBaseData", menuName = "Generation/CellBaseData", order = 1)]
    public class CellBaseData : ScriptableObject
    {
        [SerializeField] private Direction _openDirections;
        public Direction OpenDirections => _openDirections;

        [SerializeField] private GameObject _prefab;
        public GameObject Prefab => _prefab;
    }
}