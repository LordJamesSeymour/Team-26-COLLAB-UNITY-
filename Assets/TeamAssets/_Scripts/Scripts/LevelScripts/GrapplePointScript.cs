using Group26.Player.Movement;
using UnityEngine;

namespace Group26.Player.Utility
{
	public class GrapplePointScript : MonoBehaviour
	{
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
				GrappleBoosting grappleBoost = collisionObject.GetComponent<GrappleBoosting>();
				if (playerController == null)
				{
					Debug.LogError(collisionObject.name + " does not have an attached PlayerController");
				}
				else if (grappleBoost == null)
				{
					Debug.LogError(collisionObject.name + " does not have an attached GrappleBoosting component");
				}
				else
				{
					grappleBoost.InvokeBoost();
				}
			}
		}
	}
}