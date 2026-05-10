using System;
using System.Collections.Generic;
using CaveTogether.Generation.Features;
using UnityEngine;

namespace CaveTogether.Generation
{
    public class MapData
    {
        public CellData[,] Cells { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Dictionary<Type, IMapFeature> _features = new();

        public MapData(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new CellData[height, width];
        }

        public void AddFeature<T>(T feature) where T : class, IMapFeature
        {
            _features[typeof(T)] = feature;
        }

        public T GetFeature<T>() where T : class, IMapFeature
        {
            if (_features.TryGetValue(typeof(T), out var feature))
                return feature as T;

            return null;
        }

        public ref CellData GetCellRef(Vector2Int position) => ref GetCellRef(position.x, position.y);
        public ref CellData GetCellRef(int x, int y)
        {
            return ref Cells[y, x];
        }

        public CellData this[int row, int col]
        {
            get
            {
                return Cells[row, col];
            }
            set
            {
                Cells[row, col] = value;
            }
        }

        public bool IsInsideBounds(Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < Width && position.y < Height;
        }
    }
}