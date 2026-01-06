using UnityEngine;

namespace CaveGame.Generation
{
    public class MapData
    {
        public CellData[,] Cells { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public MapData(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new CellData[height, width];
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
    }
}