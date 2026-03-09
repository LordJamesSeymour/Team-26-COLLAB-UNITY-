using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] Transform Orientation;
	[SerializeField] Transform CameraHolder;
	[SerializeField] Transform CameraPos;

	[SerializeField] float sensX; 
	[SerializeField] float sensY;

	float xRotation;
	float yRotation;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		CameraHolder.position = CameraPos.position;
	}

	public void GetInput(Vector2 input)
	{
		yRotation += input.x * sensX;
		xRotation -= input.y * sensY;

		xRotation = Mathf.Clamp(xRotation, -90, 90);

		CameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
		Orientation.rotation = Quaternion.Euler(0, yRotation, 0);
	}
}
