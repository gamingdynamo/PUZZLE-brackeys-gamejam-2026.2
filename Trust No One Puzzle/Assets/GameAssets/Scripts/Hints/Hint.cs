using TMPro;
using UnityEngine;

namespace GameAssets.Scripts.Hints
{
    public class Hint : MonoBehaviour
    {
        [SerializeField] private TMP_Text hintText;

        public string HintText
        {
            get => hintText.text;
            set => hintText.text = value;
        }
    }
}
