using UnityEngine;

namespace Group26.Player.Movement
{
	public class RailExitTrigger : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			PlayerController controller = other.GetComponentInParent<PlayerController>();
			if (controller == null)
				return;

			controller.ExitRailAtEnd();
		}
	}
}