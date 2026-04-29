using UnityEngine;

namespace Dogukan
{
    public sealed class SwampSideSlowZone : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float slowPercentage = 0.80f;

        public void SetSlowPercentage(float value)
        {
            slowPercentage = Mathf.Clamp01(value);
        }

        private void OnTriggerEnter(Collider other)
        {
            SwampSpeedHandler handler = other.GetComponentInParent<SwampSpeedHandler>();
            if (handler != null)
            {
                handler.EnterCustomSlowZone(this, slowPercentage);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            SwampSpeedHandler handler = other.GetComponentInParent<SwampSpeedHandler>();
            if (handler != null)
            {
                handler.ExitCustomSlowZone(this);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            SwampSpeedHandler handler = other.GetComponentInParent<SwampSpeedHandler>();
            if (handler != null)
            {
                handler.EnterCustomSlowZone(this, slowPercentage);
            }
        }
    }
}
