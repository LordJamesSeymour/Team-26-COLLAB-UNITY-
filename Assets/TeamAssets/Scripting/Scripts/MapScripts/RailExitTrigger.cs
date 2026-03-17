using UnityEngine;

namespace Group26.Player.Movement
{
    public class RailExitTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            TryExit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryExit(other);
        }

        private void TryExit(Collider other)
        {
            PlayerController controller = other.GetComponentInParent<PlayerController>();
            if (controller == null)
                return;

            if (!controller.IsOnRail)
                return;

            controller.ForceExitRail(false);
        }
    }
}