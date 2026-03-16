using System;
using UnityEngine;

public class GrapplePointScript : MonoBehaviour
{
    //Events
    public event Action PointBoost;

    [Header("Grapple point properties")]
    [SerializeField] private float m_boostForce = 2.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided");
        }
    }
}
