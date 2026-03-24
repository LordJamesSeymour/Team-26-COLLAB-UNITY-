using Group26.Player.Movement;
using System;
using UnityEngine;

namespace Group26.Player.Utility
{
    public class GrapplePointScript : MonoBehaviour
    {
        //Events
        public event Action PointBoost;

        [Header("Debug")]
        [SerializeField] private bool m_logEntry = false;
        [SerializeField] private bool m_logPlayerEntry = false;


        private void OnTriggerEnter(Collider collision)
        {
            GameObject collisionParent = collision.transform.parent.gameObject;

            if (collisionParent.CompareTag("Player"))
            {
                //Sets the reference in the collisionParent to this object
                PlayerController playerController = collisionParent.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogError(collisionParent.name + " does not have an attached player controller");
                }
                else
                {
                    playerController.AssignGrapple(this);
                }
                PointBoost?.Invoke();

                //debug logging
                if (m_logPlayerEntry)
                {
                    Debug.Log("Player collided with + " + this.name);
                }
            }
            
            if (m_logEntry)
            {
                Debug.Log(collisionParent.name + " collided with " + this.name + ". " + collisionParent.name + " has the tag: " + collisionParent.tag);
            }
        }
    }
}

