namespace CaveGame.Generation
{
    public struct CellData
    {
        public int X;
        public int Y;

        public CellBaseData Data;
        public ushort Orientation;

        public bool IsEmpty() => Data == null;
    }
}