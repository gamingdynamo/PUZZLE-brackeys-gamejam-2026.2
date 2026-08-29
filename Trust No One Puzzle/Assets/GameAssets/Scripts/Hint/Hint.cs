using TMPro;
using UnityEngine;

namespace GameAssets.Scripts.Hint
{
    public class Hint : MonoBehaviour
    {
        [SerializeField] private TMP_Text hintText;
        public string Text
        {
            get => hintText.text;
            set
            {
                if (value != string.Empty)
                {
                    hintText.text = value;
                }
            }
        }
    }
}