using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats", order = 1)]
public class PlayerStats_SO : ScriptableObject
{
	[Header("Player Defaults")]
	public float m_fPlayerRunSpeed = 15f;
	public float m_fPlayerWalkSpeed = 5f;
	public float m_fPlayerAcceleration = 10f;
	public float m_fPlayerJumpForce = 10f;
	public float m_fHealth = 100f;

	[Header("Forgiveness Mechanics")]
	public float m_fCoyoteTime = 0.2f;
	public float m_fJumpBufferTime = 0.15f;

	[Header("Air & Drag")]
	public float m_fGroundDrag = 6f;
	public float m_fAirDrag = 2f;
	public float m_fGroundMultiplier = 10f;
	public float m_fAirMultiplier = 2f;

	[Header("Wallrun")]
	public float m_fWallrunGravity = 0.65f;
	public float m_fWalljumpForce = 6f;
	public float wallDistance = 0.6f;
	public float minimumJumpHeight = 1.2f;

	// NEW tuning fields for this approach
	public float m_fWallrunDrag = 0.1f;
	public float m_fWallStickForce = 15f;
	public float m_fWallrunMultiplier = 6;
	public float m_fWallrunVerticalInputScale = 0.3f;
}