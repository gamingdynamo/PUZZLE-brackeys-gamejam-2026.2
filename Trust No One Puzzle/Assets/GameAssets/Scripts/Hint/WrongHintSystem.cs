using UnityEngine;

namespace GameAssets.Scripts.Hint
{
    public class WrongHintSystem : MonoBehaviour
    {
        [SerializeField] private Hint hintPrefab;
        [SerializeField] private Transform hintParent;
        [SerializeField] private AudioSource wrongHintSource;

        [SerializeField] private string[] hintTexts;
        
        public void SpawnHint(int index)
        {
            var hint = Instantiate(hintPrefab, hintParent);
            hint.Text = hintTexts[index];
            wrongHintSource.Play();
        }
    }
}
