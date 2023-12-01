using UnityEngine;

namespace Match3._Project.Scripts.GridSystem
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Gem : MonoBehaviour {
        public GemType type;

        public void SetType(GemType gemType) {
            type = gemType;
            GetComponent<SpriteRenderer>().sprite = gemType.sprite;
        }
        
        public GemType GetType() => type;
    }
}