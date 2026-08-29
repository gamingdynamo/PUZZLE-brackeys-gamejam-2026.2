using UnityEngine;
using UnityEngine.Events;

namespace GameAssets.Scripts.Hint
{
    public class Trigger : MonoBehaviour
    {
        [SerializeField] private UnityEvent OnEnter;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnEnter?.Invoke();
            }
        }
    }
}