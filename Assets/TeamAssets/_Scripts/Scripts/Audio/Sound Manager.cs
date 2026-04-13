using System;
using Group26.Player.Movement;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource m_MainAudio;
    [SerializeField] private AudioSource m_WallRunAudio;
    [SerializeField] private AudioSource m_BallAudio;
    [SerializeField] private PlayerController m_PC;

    private void FixedUpdate()
    {
        if (m_PC.m_bIsWallRunning)
        {
            m_WallRunAudio.mute  = false;
        }
        else
        {
            m_WallRunAudio.mute = true;
        }
    }
}
