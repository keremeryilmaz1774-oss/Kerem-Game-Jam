using UnityEngine;

namespace Dogukan
{
    [RequireComponent(typeof(Collider))]
    public sealed class CheckpointTotem : MonoBehaviour
    {
        [SerializeField]
        private Light checkpointLight;

        [SerializeField]
        private bool autoFindChildLight = true;

        [SerializeField]
        private bool autoSetTrigger = true;

        [SerializeField]
        private bool usePlayerTag = true;

        [SerializeField]
        private string playerTag = "Player";

        public bool IsActivated { get; private set; }

        private void Reset()
        {
            if (autoFindChildLight && checkpointLight == null)
            {
                checkpointLight = GetComponentInChildren<Light>(true);
            }

            EnsureTrigger();
        }

        private void Awake()
        {
            if (autoFindChildLight && checkpointLight == null)
            {
                checkpointLight = GetComponentInChildren<Light>(true);
            }

            if (autoSetTrigger)
            {
                EnsureTrigger();
            }

            SetLightState(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsActivated)
            {
                return;
            }

            if (usePlayerTag && !other.CompareTag(playerTag))
            {
                return;
            }

            Activate(other.transform);
        }

        public void Activate(Transform activator)
        {
            IsActivated = true;
            SetLightState(true);

            CheckpointProgress tracker = activator.GetComponentInParent<CheckpointProgress>();
            if (tracker != null)
            {
                tracker.SetCheckpoint(this);
            }
        }

        private void SetLightState(bool active)
        {
            if (checkpointLight != null)
            {
                checkpointLight.enabled = active;
            }
        }

        private void EnsureTrigger()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }
    }
}
