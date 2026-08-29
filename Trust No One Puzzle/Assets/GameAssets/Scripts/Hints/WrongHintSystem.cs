using UnityEngine;

namespace GameAssets.Scripts.Hints
{
    public class WrongHintSystem : MonoBehaviour
    {
        [SerializeField] private Hint hintPrefab;
        [SerializeField] private Transform hintParent;
        [SerializeField] private AudioSource hintSource;
        [SerializeField] private float destroyDelay;

        [SerializeField] private string[] hints;
        
        public void SpawnHint(int index)
        {
            var hint = Instantiate(hintPrefab, hintParent);
            hint.HintText = hints[index];
            hintSource.Play();
            Destroy(hint.gameObject, destroyDelay);
        }
    }
}