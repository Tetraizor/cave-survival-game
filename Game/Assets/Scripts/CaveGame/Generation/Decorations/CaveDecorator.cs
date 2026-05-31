using System;
using UnityEngine;

namespace CaveTogether.Generation.Decorations
{
    public class CaveDecorator : MonoBehaviour, ICellDecorator
    {
        [SerializeField] private GameObject _stalagmitePrefab;
        [SerializeField] private Sprite[] _sprites;

        public void DecorateCell(Vector2Int pos, MapData map, Transform parent)
        {
            int stalagmiteAmount = UnityEngine.Random.Range(0, 4);
            var startPosition = FindAnyObjectByType<MapRenderManager>().GridToWorldPosition(pos);

            for (int i = 0; i < stalagmiteAmount; i++)
            {
                var exitDecorationGO = Instantiate(_stalagmitePrefab, parent);

                exitDecorationGO.transform.localScale = Vector3.one * UnityEngine.Random.Range(.7f, 1.3f);
                exitDecorationGO.GetComponent<SpriteRenderer>().sprite = _sprites[UnityEngine.Random.Range(0, _sprites.Length)];
                exitDecorationGO.GetComponent<SpriteRenderer>().flipX = UnityEngine.Random.Range(0.0f, 1.0f) > .5f;

                var position = startPosition + new Vector3(
                    UnityEngine.Random.Range(.4f, 1.6f),
                    0,
                    UnityEngine.Random.Range(.4f, 1.6f)
                );

                exitDecorationGO.transform.position = position;
            }

        }
    }
}