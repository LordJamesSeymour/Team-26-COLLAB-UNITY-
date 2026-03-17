using UnityEngine;

namespace Group26.Player.Movement
{
	public class RailEntryTrigger : MonoBehaviour
	{
		[SerializeField] private RailSpline railSpline;

		private void OnTriggerEnter(Collider other)
		{
			PlayerController controller =
				other.GetComponentInParent<PlayerController>();

			if (controller == null || railSpline == null)
				return;

			controller.EnterRail(railSpline);
		}
	}
}