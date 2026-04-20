using Group26.Player.Movement;
using System;
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

			if (collisionObject.CompareTag("Player"))
			{
				PlayerController playerController = collisionObject.GetComponent<PlayerController>();

				if (playerController == null)
				{
					Debug.LogError(collisionObject.name + " does not have an attached PlayerController");
				}
				else
				{
					TriggerPlayerPointBoost(playerController);
				}

				if (m_logPlayerEntry)
					Debug.Log("Player collided with " + name);
			}

			if (m_logEntry)
			{
				Debug.Log(collisionObject.name + " collided with " + name + ". " + collisionObject.name + " has the tag: " + collisionObject.tag);
			}
		}

		private void TriggerPlayerPointBoost(PlayerController playerController)
		{
			Type playerType = playerController.GetType();
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			FieldInfo grappleField = playerType.GetField("grappleScript", flags);
			if (grappleField != null)
				grappleField.SetValue(playerController, this);

			MethodInfo assignMethod = playerType.GetMethod("AssignGrapple", flags);

			if (assignMethod != null)
			{
				assignMethod.Invoke(playerController, new object[] { this });
				PointBoost?.Invoke();
				return;
			}

			MethodInfo pointBoostMethod = playerType.GetMethod("PointBoost", flags);

			if (pointBoostMethod != null)
			{
				pointBoostMethod.Invoke(playerController, null);
				return;
			}

			Debug.LogWarning(playerController.name + " has no AssignGrapple or PointBoost method available.");
		}
	}
}