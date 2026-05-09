using System.Collections.Generic;
using CaveGame.Common.Enums;
using CaveGame.Data;
using UnityEngine;

namespace CaveGame.Constants
{
    public static class Curves
    {
        private const string CurveDataPath = "Data/Curves";

        private static Dictionary<ConstantCurveType, CurveDataSO> _curvesByType = new();

        public static AnimationCurve GetCurveByType(ConstantCurveType type)
        {
            if (!_curvesByType.TryGetValue(type, out var curveDataSO))
            {
                var allCurveData = Resources.LoadAll<CurveDataSO>(CurveDataPath);
                _curvesByType = new();

                foreach (var data in allCurveData)
                {
                    _curvesByType.Add(data.Type, data);
                }
            }

            return _curvesByType[type].Curve;
        }
    }
}