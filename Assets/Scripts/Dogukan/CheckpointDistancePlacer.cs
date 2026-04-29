using System.Collections.Generic;
using UnityEngine;

namespace Dogukan
{
    public sealed class CheckpointDistancePlacer : MonoBehaviour
    {
        [SerializeField]
        private CheckpointTotem checkpointPrefab;

        [SerializeField]
        private List<Transform> pathPoints = new List<Transform>();

        [SerializeField]
        [Min(1f)]
        private float spacing = 20f;

        [SerializeField]
        private bool clearPreviousGenerated = true;

        [SerializeField]
        private bool enableDebugLogs = true;

        [ContextMenu("Generate Checkpoints")]
        private void GenerateCheckpoints()
        {
            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[CheckpointDistancePlacer] Generate started on '{name}'. PathPointCount={pathPoints.Count}, Spacing={spacing}, ClearPrevious={clearPreviousGenerated}.",
                    this);
            }

            if (checkpointPrefab == null)
            {
                Debug.LogWarning("Checkpoint prefab is missing.", this);
                return;
            }

            if (pathPoints.Count < 2)
            {
                Debug.LogWarning("At least 2 path points are required.", this);
                return;
            }

            if (pathPoints[0] == null)
            {
                Debug.LogWarning("[CheckpointDistancePlacer] First path point is null.", this);
                return;
            }

            if (clearPreviousGenerated)
            {
                ClearGeneratedCheckpoints();
            }

            float carryDistance = 0f;
            Vector3 previous = pathPoints[0].position;
            int spawnedCount = 0;
            int skippedNullPoints = 0;

            for (int i = 1; i < pathPoints.Count; i++)
            {
                if (pathPoints[i] == null)
                {
                    skippedNullPoints++;
                    if (enableDebugLogs)
                    {
                        Debug.LogWarning($"[CheckpointDistancePlacer] Path point index {i} is null, skipping segment.", this);
                    }
                    continue;
                }

                Vector3 current = pathPoints[i].position;
                float segmentLength = Vector3.Distance(previous, current);
                if (segmentLength <= Mathf.Epsilon)
                {
                    if (enableDebugLogs)
                    {
                        Debug.LogWarning($"[CheckpointDistancePlacer] Segment {i - 1}->{i} length is ~0, skipping.", this);
                    }
                    previous = current;
                    continue;
                }

                Vector3 segmentDir = (current - previous).normalized;
                float traveled = carryDistance;
                int segmentSpawned = 0;

                while (traveled + spacing <= segmentLength)
                {
                    traveled += spacing;
                    Vector3 spawnPos = previous + segmentDir * traveled;
                    Instantiate(checkpointPrefab, spawnPos, Quaternion.identity, transform);
                    spawnedCount++;
                    segmentSpawned++;
                }

                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[CheckpointDistancePlacer] Segment {i - 1}->{i} length={segmentLength:F2}, carryIn={carryDistance:F2}, spawned={segmentSpawned}, carryOut={segmentLength - traveled:F2}.",
                        this);
                }

                carryDistance = segmentLength - traveled;
                previous = current;
            }

            if (enableDebugLogs)
            {
                if (spawnedCount == 0)
                {
                    Debug.LogWarning(
                        $"[CheckpointDistancePlacer] No checkpoints spawned. Check spacing ({spacing}) versus segment lengths, path point nulls ({skippedNullPoints}), and prefab assignment.",
                        this);
                }
                else
                {
                    Debug.Log($"[CheckpointDistancePlacer] Spawn completed. Spawned={spawnedCount}.", this);
                }
            }
        }

        [ContextMenu("Clear Generated Checkpoints")]
        private void ClearGeneratedCheckpoints()
        {
            int removedCount = 0;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
                removedCount++;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[CheckpointDistancePlacer] Cleared {removedCount} generated checkpoint objects.", this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (pathPoints == null || pathPoints.Count < 2)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 1; i < pathPoints.Count; i++)
            {
                if (pathPoints[i - 1] == null || pathPoints[i] == null)
                {
                    continue;
                }

                Gizmos.DrawLine(pathPoints[i - 1].position, pathPoints[i].position);
            }
        }
    }
}
