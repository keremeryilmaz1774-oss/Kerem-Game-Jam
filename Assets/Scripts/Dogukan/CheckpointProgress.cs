using UnityEngine;

namespace Dogukan
{
    public sealed class CheckpointProgress : MonoBehaviour
    {
        public CheckpointTotem CurrentCheckpoint { get; private set; }

        public Vector3 CurrentCheckpointPosition
        {
            get
            {
                return CurrentCheckpoint == null ? transform.position : CurrentCheckpoint.transform.position;
            }
        }

        public void SetCheckpoint(CheckpointTotem checkpoint)
        {
            CurrentCheckpoint = checkpoint;
        }
    }
}
