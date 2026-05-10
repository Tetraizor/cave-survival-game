using System.Collections.Generic;
using System.Linq;
using CaveGame.Common;
using UnityEngine;

namespace CaveGame.Game.Entities
{
    public class CharacterManager : MonoBehaviour
    {
        private const string CHARACTER_DATA_PATH = "Data/Characters";

        [SerializeField] private GameObject CharacterPrefab;

        public readonly List<Character> Characters = new();
        public readonly Dictionary<ulong, Character> ClientIdCharacterLookup;

        private List<CharacterDataSO> _characterData = new();

        public void Initialize(GameConfig config)
        {
            _characterData = Resources.LoadAll<CharacterDataSO>(CHARACTER_DATA_PATH).ToList();
            SpawnCharacters(config);
        }

        private void SpawnCharacters(GameConfig config)
        {
            var players = config.Players;

            foreach (var player in players)
            {
                var characterGameObject = Instantiate(CharacterPrefab);
                var character = characterGameObject.GetComponent<Character>();
                var characterData = _characterData.Find(cd => cd.TypeId.Equals(player.CharacterId));

                character.Initialize(characterData);

                Characters.Add(character);
                ClientIdCharacterLookup.Add(player.OwnerClientId, character);
            }
        }

        public Character GetCharacter(ulong ownerClientId)
        {
            if (ClientIdCharacterLookup.TryGetValue(ownerClientId, out Character character))
                return character;
            else
                return null;
        }
    }
}