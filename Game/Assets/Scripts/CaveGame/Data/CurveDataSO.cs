using CaveGame.Common.Enums;
using UnityEngine;

namespace CaveGame.Data
{
    [CreateAssetMenu(fileName = "Curve", menuName = "Data/Curve", order = 1)]
    public class CurveDataSO : ScriptableObject
    {
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private ConstantCurveType _type;

        public AnimationCurve Curve { get => _curve; }
        public ConstantCurveType Type { get => _type; }
    }
}