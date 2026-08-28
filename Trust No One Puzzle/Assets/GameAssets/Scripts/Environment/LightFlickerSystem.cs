using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameAssets.Scripts.Environment
{
    public class LightFlickerSystem : MonoBehaviour
    {
        [Header("Light Sources")]
        [Tooltip("Assign your lights here.")]
        public List<Light> targetLights = new List<Light>();

        [Header("Flicker Settings")]
        public float minFlickerDuration = 0.05f;
        public float maxFlickerDuration = 0.2f;
        
        [Tooltip("The duration the lights stay off during the 'Lights Out' sequence (Room 2 puzzle).")]
        public float lightsOutDuration = 6.5f;

        [Header("Events")]
        [Tooltip("Fires when the lights go completely out.")]
        public UnityEvent OnLightsWentOut;
        [Tooltip("Fires when the lights come back on.")]
        public UnityEvent OnLightsCameBackOn;

        private Coroutine currentRoutine;
        private bool areLightsNormallyOn = true;

        private void Start()
        {
            // Ensure lights start in the correct state
            SetLightsState(areLightsNormallyOn);

            // TODO: Remove default trigger
            TriggerRoom2LightsOutSequence();
        }

        /// <summary>
        /// Triggers a random quick flicker.
        /// </summary>
        public void FlickerOnce()
        {
            if (currentRoutine == null)
            {
                currentRoutine = StartCoroutine(FlickerRoutine(Random.Range(1, 4)));
            }
        }

        /// <summary>
        /// Triggers the specific sequence for Room 2 (Lights flicker, go out for X seconds, lights come back).
        /// </summary>
        public void TriggerRoom2LightsOutSequence()
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            currentRoutine = StartCoroutine(LightsOutSequenceRoutine(lightsOutDuration));
        }

        private IEnumerator FlickerRoutine(int flickers)
        {
            for (int i = 0; i < flickers; i++)
            {
                SetLightsState(false);
                yield return new WaitForSeconds(Random.Range(minFlickerDuration, maxFlickerDuration));
                SetLightsState(true);
                yield return new WaitForSeconds(Random.Range(minFlickerDuration, maxFlickerDuration));
            }

            SetLightsState(areLightsNormallyOn);
            currentRoutine = null;
        }

        private IEnumerator LightsOutSequenceRoutine(float durationOut)
        {
            // Dramatic flicker before going out
            yield return StartCoroutine(FlickerRoutine(3));

            // Lights out completely
            SetLightsState(false);
            OnLightsWentOut?.Invoke();

            // Wait for the duration
            yield return new WaitForSeconds(durationOut);

            // Flicker back on
            yield return StartCoroutine(FlickerRoutine(2));
            SetLightsState(true);
            OnLightsCameBackOn?.Invoke();

            currentRoutine = null;
        }

        /// <summary>
        /// Sets the state of all referenced lights.
        /// </summary>
        private void SetLightsState(bool state)
        {
            foreach (Light l in targetLights)
            {
                if (l != null)
                {
                    l.enabled = state;
                }
            }
        }

        /// <summary>
        /// Manually turn lights on or off without any sequence.
        /// </summary>
        public void SetLights(bool state)
        {
            areLightsNormallyOn = state;
            SetLightsState(state);
        }
    }
}
