using Group26.Player.Movement;
using Mono.Cecil;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Group26.Player.Utility
{
	public class GrapplePointScript : MonoBehaviour
	{
		public event Action PointBoost;

		[Header("Highlight")]
		[SerializeField] private GrappleHighlightTarget m_highlightTarget;
		public GrappleHighlightTarget HighlightTarget => m_highlightTarget;

		[Header("Debug")]
		[SerializeField] private bool m_logEntry = false;
		[SerializeField] private bool m_logPlayerEntry = false;

		private void Reset()
		{
			if (m_highlightTarget == null)
				m_highlightTarget = GetComponentInParent<GrappleHighlightTarget>();
		}

		private void OnValidate()
		{
			if (m_highlightTarget == null)
				m_highlightTarget = GetComponentInParent<GrappleHighlightTarget>();
		}

		private void OnTriggerEnter(Collider collision)
		{
			GameObject collisionObject = null;

			if (collision.attachedRigidbody != null)
				collisionObject = collision.attachedRigidbody.gameObject;
			else
				collisionObject = collision.transform.root.gameObject;

			if (collisionObject == null)
				return;

			if (collisionObject.transform.root.CompareTag("Player"))
			{
				
				PlayerController playerController = collisionObject.GetComponent<PlayerController>();
				GrappleBoosting grappleBoost = collisionObject.GetComponent<GrappleBoosting>();

				if (playerController == null)
				{
					Debug.LogError(collisionObject.name + " does not have an attached PlayerController");
				}
				else
				{
                    StyleSystem styleSystem = playerController.GetComponent<StyleSystem>();
                    styleSystem.AddStyleCombo(500, "Grapple", "Boosted");

                    if (grappleBoost == null)
					{
						Debug.Log("Grapple boost is not valid");
					}
					else
					{
                        grappleBoost.InvokeBoost();
                        Debug.Log("Invoked Booset From GrapplePointScript");
                    }
				}

				if (m_logPlayerEntry)
					Debug.Log("Player collided with " + name);
			}
			else
			{
				Debug.Log("Collided object is not the player");
			}

			if (m_logEntry)
			{
				Debug.Log(collisionObject.name + " collided with " + name + ". " + collisionObject.name + " has the tag: " + collisionObject.tag);
			}
		}

        /*private void OnTriggerExit(Collider other)
        {
            if(other.transform.root.tag == "Player")
			{
				other.transform.root.GetComponent<GrappleBoosting>().ResetGrappleDash();
			}
        }*/

        IEnumerator DelayBoost()
		{
			yield return new WaitForSeconds(2);
		}
	}
}