using System;
using UnityEngine;

namespace GameAssets.Scripts.Entities
{
    public class AnimatorController : MonoBehaviour
    {
        [SerializeField] private float animationSwitchSpeed;
        
        private Animator _animator;
        private HashProperties _hashProperties;

        public float HorizontalValue { get; set; }
        public float VerticalValue { get; set; }

        private void OnEnable()
        {
            _animator = GetComponent<Animator>();
            _hashProperties = new HashProperties();
        }

        private void Update()
        {
            AnimationHandler();
        }

        private void AnimationHandler()
        {
            _animator.SetFloat(_hashProperties.Horizontal, 
                Mathf.Lerp(_animator.GetFloat(_hashProperties.Horizontal), HorizontalValue, animationSwitchSpeed * Time.deltaTime));
            
            _animator.SetFloat(_hashProperties.Vertical, 
                Mathf.Lerp(_animator.GetFloat(_hashProperties.Vertical), VerticalValue, animationSwitchSpeed * Time.deltaTime));
        }
    }

    public class HashProperties
    {
        public readonly int Horizontal = Animator.StringToHash("MotionHorizontal");
        public readonly int Vertical = Animator.StringToHash("MotionVertical");
    }
}