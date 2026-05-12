using UnityEngine;
using Group26.Player.Movement;
using Group26.Player.Inputs;

public class PlayerPhysicsDebugProbe : MonoBehaviour
{
	private Rigidbody rb;
	private PlayerController playerController;
	private InputManager inputManager;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		playerController = GetComponent<PlayerController>();
		inputManager = GetComponent<InputManager>();

		Debug.Log("=== PLAYER PHYSICS DEBUG: AWAKE ===");
		PrintState();
	}

	private void Start()
	{
		Debug.Log("=== PLAYER PHYSICS DEBUG: START ===");
		PrintState();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F9))
		{
			Debug.Log("=== PLAYER PHYSICS DEBUG: MANUAL F9 CHECK ===");
			PrintState();
		}
	}

	private void PrintState()
	{
		int playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
		int inputManagers = FindObjectsByType<InputManager>(FindObjectsSortMode.None).Length;
		int rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length;
		int cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None).Length;

		Debug.Log(
			$"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n" +
			$"Time.timeScale: {Time.timeScale}\n" +
			$"Time.fixedDeltaTime: {Time.fixedDeltaTime}\n" +
			$"Physics.simulationMode: {Physics.simulationMode}\n" +
			$"PlayerControllers in scene: {playerControllers}\n" +
			$"InputManagers in scene: {inputManagers}\n" +
			$"Rigidbody count in scene: {rigidbodies}\n" +
			$"Camera count in scene: {cameras}\n" +
			$"RB exists: {rb != null}\n" +
			$"RB velocity: {(rb != null ? rb.linearVelocity.ToString() : "NULL")}\n" +
			$"RB angularVelocity: {(rb != null ? rb.angularVelocity.ToString() : "NULL")}\n" +
			$"RB useGravity: {(rb != null ? rb.useGravity.ToString() : "NULL")}\n" +
			$"RB isKinematic: {(rb != null ? rb.isKinematic.ToString() : "NULL")}\n" +
			$"RB detectCollisions: {(rb != null ? rb.detectCollisions.ToString() : "NULL")}\n" +
			$"RB linearDamping: {(rb != null ? rb.linearDamping.ToString() : "NULL")}\n" +
			$"RB mass: {(rb != null ? rb.mass.ToString() : "NULL")}\n" +
			$"Player state: {(playerController != null ? playerController.state.ToString() : "NULL")}\n" +
			$"Grounded: {(playerController != null ? playerController.m_bIsGrounded.ToString() : "NULL")}\n" +
			$"ActiveGrapple: {(playerController != null ? playerController.m_bActiveGrapple.ToString() : "NULL")}\n" +
			$"ActiveSwing: {(playerController != null ? playerController.m_bActiveSwing.ToString() : "NULL")}\n" +
			$"Dashing: {(playerController != null ? playerController.m_bDashing.ToString() : "NULL")}\n" +
			$"Freeze: {(playerController != null ? playerController.m_bFreeze.ToString() : "NULL")}\n" +
			$"OnRail: {(playerController != null ? playerController.m_bOnRail.ToString() : "NULL")}\n" +
			$"MoveSpeed: {(playerController != null ? playerController.moveSpeed.ToString() : "NULL")}\n" +
			$"MaxYSpeed: {(playerController != null ? playerController.maxYSpeed.ToString() : "NULL")}\n" +
			$"MoveInput: {(inputManager != null ? inputManager.MoveInput.ToString() : "NULL")}\n" +
			$"LookInput: {(inputManager != null ? inputManager.LookInput.ToString() : "NULL")}"
		);
	}
}