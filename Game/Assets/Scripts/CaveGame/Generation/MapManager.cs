using UnityEngine;

namespace CaveGame.Generation
{
    public class MapManager : MonoBehaviour
    {
        public const int MAP_WIDTH = 24;
        public const int MAP_HEIGHT = 24;

        public MapData Map { get; private set; } = new MapData(MAP_WIDTH, MAP_HEIGHT);

        public MapGenerator Generator { get; private set; }
        [SerializeField] private MapRenderManager _renderManager;

        public void Initialize(string seed)
        {
            Generator = new MapGenerator(Map, seed);
            _renderManager.Initialize(this);
            Generator.GenerateMap();

            Debug.Log("MapManager initialization complete!");
        }
    }
}