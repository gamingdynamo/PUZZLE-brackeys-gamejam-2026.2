using System;
using UnityEngine;

namespace GameAssets.Scripts.Entities
{
    public class AnimatorController : MonoBehaviour
    {
        [SerializeField] private float animationSwitchSpeed;
        
        private Animator _animator;
        private HashProperties _hashProperties;

        public float MotionValue { get; set; }

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
            _animator.SetFloat(_hashProperties.MotionValue, 
                Mathf.Lerp(_animator.GetFloat(_hashProperties.MotionValue), MotionValue, animationSwitchSpeed * Time.deltaTime));
        }
    }

    public class HashProperties
    {
        public readonly int MotionValue = Animator.StringToHash("Motion");
    }
}