using UnityEngine;

namespace Group26.Editor.Camera
{
    public class EditorCamera : MonoBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] private float lookSensitivityX = 0.1f;
        [SerializeField] private float lookSensitivityY = 0.1f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private float xInput;
        private float yInput;

        private float yaw;
        private float pitch;

        private bool isHoldingClick;
        private bool wasHoldingClick;

        private void Start()
        {
            Vector3 startRotation = transform.rotation.eulerAngles;

            yaw = startRotation.y;
            pitch = startRotation.x;

            if (pitch > 180f)
                pitch -= 360f;
        }

        private void Update()
        {
            if (isHoldingClick != wasHoldingClick)
            {
                wasHoldingClick = isHoldingClick;

                Cursor.lockState = isHoldingClick ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isHoldingClick;
            }

            if (!isHoldingClick)
                return;

            yaw += xInput * lookSensitivityX;
            pitch -= yInput * lookSensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        public void SetLookInput(float x, float y)
        {
            xInput = x;
            yInput = y;
        }

        public void SetClickState(bool clicked)
        {
            isHoldingClick = clicked;
        }
    }
}