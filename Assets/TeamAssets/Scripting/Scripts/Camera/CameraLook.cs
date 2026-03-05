using UnityEngine;
using UnityEngine.EventSystems;

public class CameraLook : MonoBehaviour
{
	[SerializeField] Transform PlayerTransform;

	[SerializeField] private float sensX;
	[SerializeField] private float sensY;

	Camera cam;

	float mouseX;
	float mouseY;

	float multiplier = 0.01f;

	float xRotation;
	float yRotation;

	private void Awake()
	{
		cam = GetComponentInChildren<Camera>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
		PlayerTransform.rotation = Quaternion.Euler(0, yRotation, 0);
	}

	public void PlayerInput(Vector2 InputV2)
	{
		mouseX = InputV2.x;
		mouseY = InputV2.y;

		yRotation += mouseX * sensX * multiplier;
		xRotation -= mouseY * sensY * multiplier;

		xRotation = Mathf.Clamp(xRotation, -90f, 90f);
	}
}