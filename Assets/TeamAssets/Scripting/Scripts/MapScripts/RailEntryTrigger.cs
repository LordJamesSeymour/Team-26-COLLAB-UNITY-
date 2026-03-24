using UnityEngine;

namespace Group26.Player.Movement
{
    public class RailEntryTrigger : MonoBehaviour
    {
        [SerializeField] private RailSpline railSpline;

        public RailSpline RailSpline => railSpline;

        private void Reset()
        {
            AutoAssignFromParent();
        }

        private void Awake()
        {
            AutoAssignFromParent();
        }

        private void OnValidate()
        {
            AutoAssignFromParent();
        }

        public void AutoAssignFromParent()
        {
            if (railSpline == null)
                railSpline = GetComponentInParent<RailSpline>();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryEnter(other);
        }

        private void TryEnter(Collider other)
        {
            AutoAssignFromParent();

            if (railSpline == null)
                return;

            PlayerController controller = other.GetComponentInParent<PlayerController>();
            if (controller == null)
                return;

            if (controller.IsOnRail)
                return;

            controller.EnterRail(railSpline);
        }
    }
}