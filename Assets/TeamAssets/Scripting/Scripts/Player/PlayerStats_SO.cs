using UnityEngine;
// Use the CreateAssetMenu attribute to allow creating instances of this ScriptableObject from the Unity Editor.
[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats", order = 1)]
public class PlayerStats_SO : ScriptableObject
{
    [Header("Player Defaults")]
    public float m_fPlayerRunSpeed;
    public float m_fPlayerWalkSpeed;
    public float m_fPlayerAcceleration;
    public float m_fPlayerJumpForce;
    public float m_fHealth;
	[Space (20)]

    [Header("Forgiveness Mechanics")]
    public float m_fCoyoteTime;
    public bool m_bJumpBuffering;
    public float m_fInputTolerance;
	[Space(20)]

	[Header("Air & Drag")]
	public float m_fGroundDrag;
	public float m_fAirDrag;
	public float m_fGroundMultiplier;
	public float m_fAirMultiplier;
	[Space(20)]

	[Header("Wallrun")]
	public float m_fWallrunGravity;
	public float m_fWalljumpForce;
	public float wallDistance = 0.5f;
	public float minimumJumpHeight = 1.5f;
}