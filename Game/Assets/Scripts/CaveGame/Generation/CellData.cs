namespace CaveGame.Generation
{
    public struct CellData
    {
        public int X;
        public int Y;

        public CellBaseData Data;

        /// <summary>
        /// Takes values 0, 1, 2, 3 representing how many times the cell is rotated 90 degrees clockwise
        /// </summary>
        public int Orientation;

        public bool IsEmpty => Data == null;
    }
}