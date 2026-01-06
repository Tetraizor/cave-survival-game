using UnityEngine;

namespace CaveGame.Generation
{
    public class MapManager : MonoBehaviour
    {
        public const int MAP_WIDTH = 32;
        public const int MAP_HEIGHT = 32;

        public const int CELL_SIZE = 2;

        public MapData Map { get; private set; } = new MapData(MAP_WIDTH, MAP_HEIGHT);

        public MapGenerator Generator { get; private set; }
        [SerializeField] private MapRenderManager _renderManager;

        private void Start()
        {
            Debug.Log("MapManager is initializing...");

            Generator = new MapGenerator(Map);

            _renderManager.Initialize(this);

            Generator.GenerateMap();

            Debug.Log("MapManager initialization complete!");
        }
    }
}