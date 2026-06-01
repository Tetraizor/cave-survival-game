using UnityEngine;

namespace CaveTogether.Generation
{
    public class MapManager : MonoBehaviour
    {
        public const int MAP_WIDTH = 20;
        public const int MAP_HEIGHT = 20;

        public MapData Map { get; private set; } = new MapData(MAP_WIDTH, MAP_HEIGHT);

        public MapGenerator Generator { get; private set; }
        [SerializeField] private MapRenderManager _renderManager;

        public void Initialize(string seed)
        {
            Generator = new MapGenerator(Map, seed);
            _renderManager.Initialize(this);
            Generator.GenerateMap();
        }
    }
}