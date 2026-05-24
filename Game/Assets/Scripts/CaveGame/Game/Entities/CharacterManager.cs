using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common;
using CaveTogether.Generation;
using CaveTogether.Generation.Features;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game.Entities
{
    public class CharacterManager : MonoBehaviour
    {
        private const string CHARACTER_DATA_PATH = "Data/Characters";

        [SerializeField] private GameObject CharacterPrefab;

        public readonly List<Character> Characters = new();
        public readonly Dictionary<ulong, Character> ClientIdCharacterLookup = new();

        private List<CharacterDataSO> _characterData = new();

        public void Initialize(GameConfig config)
        {
            _characterData = Resources.LoadAll<CharacterDataSO>(CHARACTER_DATA_PATH).ToList();
            SpawnCharacters(config);
        }

        private void SpawnCharacters(GameConfig config)
        {
            var mapManager = FindAnyObjectByType<MapManager>();
            var mapRenderManager = FindAnyObjectByType<MapRenderManager>();
            var gridSpawnPosition = mapManager.Map.GetFeature<SpawnFeature>().PlayerSpawnPosition;
            var spawnPosition = mapRenderManager.GridToWorldPosition(gridSpawnPosition);

            var players = config.Players;

            foreach (var player in players)
            {
                var characterGameObject = Instantiate(CharacterPrefab);
                var character = characterGameObject.GetComponent<Character>();
                var characterData = _characterData.Find(cd => cd.TypeId.Equals(player.CharacterId.ToString()));

                character.Initialize(characterData, player);

                Characters.Add(character);
                ClientIdCharacterLookup.Add(player.OwnerClientId, character);

                character.SetPosition(gridSpawnPosition);
            }
        }

        public Character GetCharacter(ulong ownerClientId)
        {
            if (ClientIdCharacterLookup.TryGetValue(ownerClientId, out Character character))
                return character;
            else
                return null;
        }

        public Character GetClientCharacter()
        {
            ulong clientId = ServiceLocator.Get<SessionManagerService>().LocalClientId;
            return GetCharacter(clientId);
        }
    }
}