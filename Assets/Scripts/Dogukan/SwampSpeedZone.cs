using System.Globalization;
using System.Text;
using UnityEngine;

namespace Dogukan
{
    [RequireComponent(typeof(Collider))]
    public sealed class SwampSpeedZone : MonoBehaviour
    {
        private const string LeftSideSlowName = "__SwampLeftSideSlow";
        private const string RightSideSlowName = "__SwampRightSideSlow";

        [SerializeField]
        private bool autoSetTrigger = true;

        [SerializeField]
        private bool restrictByObjectName = true;

        [SerializeField]
        private string requiredObjectName = "batak";

        [SerializeField]
        private bool expandBoxColliderOnAwake = false;

        [SerializeField]
        private Vector3 minimumBoxSize = new Vector3(12f, 3f, 3f);

        [SerializeField]
        [Range(0f, 1f)]
        private float baseSlowPercentage = 0.40f;

        [SerializeField]
        private bool enableSideSlowZones = true;

        [SerializeField]
        [Range(0f, 1f)]
        private float sideSlowPercentage = 0.80f;

        [SerializeField]
        private float sideZoneWidth = 0.75f;

        [SerializeField]
        private float sideZoneDepthPadding = 0f;

        [SerializeField]
        private float sideZoneHeight = 3f;

        public float BaseSlowPercentage => Mathf.Clamp01(baseSlowPercentage);
        private bool IsValidSwampObject =>
            !restrictByObjectName || NormalizeForCompare(gameObject.name).Contains(NormalizeForCompare(requiredObjectName));

        private void Reset()
        {
            if (!IsValidSwampObject)
            {
                DisableSideSlowZones();
                return;
            }

            EnsureTrigger();
            EnsureSideSlowZones();
        }

        private void Awake()
        {
            if (!IsValidSwampObject)
            {
                DisableSideSlowZones();
                return;
            }

            if (autoSetTrigger)
            {
                EnsureTrigger();
            }

            EnsureSideSlowZones();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidSwampObject)
            {
                return;
            }

            SwampSpeedHandler handler = other.GetComponentInParent<SwampSpeedHandler>();
            if (handler != null)
            {
                handler.EnterSwamp(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidSwampObject)
            {
                return;
            }

            SwampSpeedHandler handler = other.GetComponentInParent<SwampSpeedHandler>();
            if (handler != null)
            {
                handler.ExitSwamp(this);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsValidSwampObject)
            {
                return;
            }

            SwampSpeedHandler handler = other.GetComponentInParent<SwampSpeedHandler>();
            if (handler != null)
            {
                handler.EnterSwamp(this);
            }
        }

        private static string NormalizeForCompare(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder(decomposed.Length);
            for (int i = 0; i < decomposed.Length; i++)
            {
                char current = decomposed[i];
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(current);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(current);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void EnsureTrigger()
        {
            Collider targetCollider = GetComponent<Collider>();
            targetCollider.isTrigger = true;

            if (!expandBoxColliderOnAwake)
            {
                return;
            }

            BoxCollider boxCollider = targetCollider as BoxCollider;
            if (boxCollider == null)
            {
                return;
            }

            Vector3 currentSize = boxCollider.size;
            boxCollider.size = new Vector3(
                Mathf.Max(currentSize.x, minimumBoxSize.x),
                Mathf.Max(currentSize.y, minimumBoxSize.y),
                Mathf.Max(currentSize.z, minimumBoxSize.z));
        }

        private void EnsureSideSlowZones()
        {
            if (!enableSideSlowZones)
            {
                DisableSideSlowZones();
                return;
            }

            BoxCollider swampCollider = GetComponent<Collider>() as BoxCollider;
            if (swampCollider == null)
            {
                return;
            }

            Vector3 size = swampCollider.size;
            Vector3 center = swampCollider.center;

            CreateOrUpdateSideSlowZone(
                LeftSideSlowName,
                center + new Vector3(-(size.x * 0.5f + sideZoneWidth * 0.5f), 0f, 0f),
                new Vector3(sideZoneWidth, sideZoneHeight, size.z + sideZoneDepthPadding));

            CreateOrUpdateSideSlowZone(
                RightSideSlowName,
                center + new Vector3(size.x * 0.5f + sideZoneWidth * 0.5f, 0f, 0f),
                new Vector3(sideZoneWidth, sideZoneHeight, size.z + sideZoneDepthPadding));
        }

        private void DisableSideSlowZones()
        {
            Transform left = transform.Find(LeftSideSlowName);
            if (left != null)
            {
                left.gameObject.SetActive(false);
            }

            Transform right = transform.Find(RightSideSlowName);
            if (right != null)
            {
                right.gameObject.SetActive(false);
            }
        }

        private void CreateOrUpdateSideSlowZone(string zoneName, Vector3 localPosition, Vector3 localSize)
        {
            Transform zoneTransform = transform.Find(zoneName);
            GameObject zoneObject;

            if (zoneTransform == null)
            {
                zoneObject = new GameObject(zoneName);
                zoneObject.transform.SetParent(transform, false);
            }
            else
            {
                zoneObject = zoneTransform.gameObject;
            }

            zoneObject.SetActive(true);
            zoneObject.transform.localPosition = localPosition;
            zoneObject.transform.localRotation = Quaternion.identity;
            zoneObject.transform.localScale = Vector3.one;

            BoxCollider zoneCollider = zoneObject.GetComponent<BoxCollider>();
            if (zoneCollider == null)
            {
                zoneCollider = zoneObject.AddComponent<BoxCollider>();
            }

            zoneCollider.isTrigger = true;
            zoneCollider.size = localSize;
            zoneCollider.center = Vector3.zero;

            SwampSideSlowZone sideZone = zoneObject.GetComponent<SwampSideSlowZone>();
            if (sideZone == null)
            {
                sideZone = zoneObject.AddComponent<SwampSideSlowZone>();
            }

            sideZone.SetSlowPercentage(sideSlowPercentage);
        }
    }
}
