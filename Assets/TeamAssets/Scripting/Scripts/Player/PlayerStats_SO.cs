using UnityEngine;
// Use the CreateAssetMenu attribute to allow creating instances of this ScriptableObject from the Unity Editor.
[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats", order = 1)]
public class PlayerStats_SO : ScriptableObject
{
    [Header("Player Defaults")]
    public float m_fPlayerRunSpeed;
    public float m_fPlayerWalkSpeed;
    public float m_fPlayerJumpForce;
    public float m_fHealth;

    [Header("Forgiveness Mechanics")]
    public float m_fCoyoteTime;
    public bool m_bJumpBuffering;
    public float m_fInputTolerance;
}