using UnityEngine;

namespace GameAssets.Scripts.Environment
{
    public class MirrorReflectionSystem : MonoBehaviour
    {
        [Tooltip("Assign CharacterController here.")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform mirrorCam;
        
        private void Update()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            var mirrorPos = new Vector3(transform.position.x, player.position.y, transform.position.z);
            var startPos = player.position - mirrorPos;
            var angle = Vector3.SignedAngle(startPos, transform.forward, Vector3.up);

            mirrorCam.localEulerAngles = new Vector3(0, angle, 0);
        }
    }
}
