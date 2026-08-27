using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GameAssets.Scripts.Interaction
{
    /// <summary>
    /// Handles the toolbox open/close interaction for Room 2.
    /// The toolbox has two parts: Bottom (stays) and Cover (rotates open/closed on a hinge).
    ///
    /// Designed to work with the scene hierarchy:
    ///   Tool Box (this script + Collider)
    ///     ├── Bottom
    ///     └── Cover  ← this is the part that rotates
    ///
    /// The cover pivots around its own local X-axis (like a lid hinge on the back edge).
    /// Make sure the Cover's pivot point is at the hinge location in your 3D model or
    /// use an empty parent as the pivot.
    /// </summary>
    public class ToolBoxInteractable : MonoBehaviour, IInteractable
    {
        [Header("Cover Settings")]
        [Tooltip("The cover/lid transform that rotates open and closed.")]
        [SerializeField] private Transform cover;

        [Tooltip("The axis (in Cover's local space) around which the lid rotates. Usually Vector3.right for X-axis hinge.")]
        [SerializeField] private Vector3 hingeAxis = Vector3.right;

        [Tooltip("The angle (degrees) the cover rotates to when fully open. Positive = opens backward.")]
        [SerializeField] private float openAngle = -110f;

        [Tooltip("How long the open/close animation takes in seconds.")]
        [SerializeField] private float animationDuration = 0.6f;

        [Tooltip("Easing curve for the open/close animation.")]
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Lock Settings")]
        [Tooltip("If true, the toolbox starts locked and requires UnlockToolBox() to be called first.")]
        [SerializeField] private bool startsLocked = true;

        [Header("Audio")]
        [Tooltip("AudioSource for playing open/close sounds. If null, no sound plays.")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("Sound played when the toolbox opens.")]
        [SerializeField] private AudioClip openSound;

        [Tooltip("Sound played when the toolbox closes.")]
        [SerializeField] private AudioClip closeSound;

        [Tooltip("Sound played when the player tries to open a locked toolbox.")]
        [SerializeField] private AudioClip lockedSound;

        [Header("Events")]
        [Tooltip("Fired when the toolbox finishes opening.")]
        public UnityEvent OnOpened;

        [Tooltip("Fired when the toolbox finishes closing.")]
        public UnityEvent OnClosed;

        [Tooltip("Fired when the player tries to interact but the toolbox is locked.")]
        public UnityEvent OnLockedAttempt;

        // State
        private bool _isOpen;
        private bool _isLocked;
        private bool _isAnimating;
        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private Coroutine _animationCoroutine;

        // IInteractable
        public string InteractionPrompt
        {
            get
            {
                if (_isLocked) return "Locked [Need Key]";
                if (_isAnimating) return "";
                return _isOpen ? "Press [E] to Close" : "Press [E] to Open";
            }
        }

        public bool CanInteract => !_isAnimating;

        private void Awake()
        {
            _isLocked = startsLocked;

            if (cover == null)
            {
                Debug.LogError($"[ToolBoxInteractable] No cover transform assigned on {gameObject.name}!");
                return;
            }

            // Cache the closed rotation (current rotation in scene)
            _closedRotation = cover.localRotation;

            // Calculate the open rotation by rotating around the hinge axis
            _openRotation = _closedRotation * Quaternion.AngleAxis(openAngle, hingeAxis);
        }

        public void OnInteract()
        {
            if (_isAnimating) return;

            if (_isLocked)
            {
                PlaySound(lockedSound);
                OnLockedAttempt?.Invoke();
                return;
            }


            // Toggle open/close
            if (_isOpen)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Opens the toolbox lid with animation.
        /// </summary>
        public void Open()
        {
            if (_isOpen || _isAnimating || _isLocked) return;

            PlaySound(openSound);
            _animationCoroutine = StartCoroutine(AnimateCover(_closedRotation, _openRotation, true));
        }

        /// <summary>
        /// Closes the toolbox lid with animation.
        /// </summary>
        public void Close()
        {
            if (!_isOpen || _isAnimating) return;

            PlaySound(closeSound);
            _animationCoroutine = StartCoroutine(AnimateCover(_openRotation, _closedRotation, false));
        }

        /// <summary>
        /// Unlocks the toolbox so the player can open it.
        /// Call this from a key pickup event, puzzle completion, etc.
        /// </summary>
        public void UnlockToolBox()
        {
            _isLocked = false;
        }

        /// <summary>
        /// Locks the toolbox (e.g. after the lights-out sequence changes things).
        /// </summary>
        public void LockToolBox()
        {
            _isLocked = true;

            // If currently open, slam it shut
            if (_isOpen)
            {
                ForceClose();
            }
        }

        /// <summary>
        /// Instantly snaps the cover to the closed position without animation.
        /// Useful for puzzle state resets.
        /// </summary>
        public void ForceClose()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            _isAnimating = false;
            _isOpen = false;

            if (cover != null)
                cover.localRotation = _closedRotation;
        }

        /// <summary>
        /// Instantly snaps the cover to the open position without animation.
        /// </summary>
        public void ForceOpen()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            _isAnimating = false;
            _isOpen = true;

            if (cover != null)
                cover.localRotation = _openRotation;
        }

        /// <summary>
        /// Returns true if the toolbox is currently in the open state.
        /// </summary>
        public bool IsOpen => _isOpen;

        /// <summary>
        /// Returns true if the toolbox is currently locked.
        /// </summary>
        public bool IsLocked => _isLocked;

        private IEnumerator AnimateCover(Quaternion from, Quaternion to, bool opening)
        {
            _isAnimating = true;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(Mathf.Clamp01(elapsed / animationDuration));
                cover.localRotation = Quaternion.Slerp(from, to, t);
                yield return null;
            }

            cover.localRotation = to;
            _isOpen = opening;
            _isAnimating = false;
            _animationCoroutine = null;

            if (opening)
                OnOpened?.Invoke();
            else
                OnClosed?.Invoke();
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
