using UnityEngine;

namespace CaveTogether.Game.Entities
{
    public class CharacterRenderer : MonoBehaviour
    {
        [SerializeField] public SpriteRenderer _head;
        [SerializeField] public SpriteRenderer _body;
        [SerializeField] public SpriteRenderer _upperArmF;
        [SerializeField] public SpriteRenderer _lowerArmF;
        [SerializeField] public SpriteRenderer _upperArmB;
        [SerializeField] public SpriteRenderer _lowerArmB;
        [SerializeField] public SpriteRenderer _upperLegF;
        [SerializeField] public SpriteRenderer _lowerLegF;
        [SerializeField] public SpriteRenderer _upperLegB;
        [SerializeField] public SpriteRenderer _lowerLegB;
        [SerializeField] public SpriteRenderer _footF;
        [SerializeField] public SpriteRenderer _footB;

        public void Initialize(CharacterDataSO characterData)
        {
            _head.sprite = characterData.SpriteSet.Head;
            _body.sprite = characterData.SpriteSet.Body;
            _upperArmF.sprite = characterData.SpriteSet.UpperArmF;
            _lowerArmF.sprite = characterData.SpriteSet.LowerArmF;
            _upperArmB.sprite = characterData.SpriteSet.UpperArmB;
            _lowerArmB.sprite = characterData.SpriteSet.LowerArmB;
            _upperLegF.sprite = characterData.SpriteSet.UpperLegF;
            _lowerLegF.sprite = characterData.SpriteSet.LowerLegF;
            _upperLegB.sprite = characterData.SpriteSet.UpperLegB;
            _lowerLegB.sprite = characterData.SpriteSet.LowerLegB;
            _footF.sprite = characterData.SpriteSet.FootF;
            _footB.sprite = characterData.SpriteSet.FootB;
        }
    }
}