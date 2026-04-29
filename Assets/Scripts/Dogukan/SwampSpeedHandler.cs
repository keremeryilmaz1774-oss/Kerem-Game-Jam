using Invector.vCharacterController;
using System.Collections.Generic;
using UnityEngine;

namespace Dogukan
{
    public sealed class SwampSpeedHandler : MonoBehaviour
    {
        private const float SourceStaleTime = 0.25f;

        [SerializeField]
        private vThirdPersonController controller;

        private SpeedSnapshot baseFreeSpeed;
        private SpeedSnapshot baseStrafeSpeed;
        private readonly Dictionary<Object, SlowSourceState> activeSlowSources = new Dictionary<Object, SlowSourceState>();

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<vThirdPersonController>();
            }

            if (controller == null)
            {
                Debug.LogError("SwampSpeedHandler requires a vThirdPersonController reference.", this);
                enabled = false;
                return;
            }

            baseFreeSpeed = SpeedSnapshot.From(controller.freeSpeed);
            baseStrafeSpeed = SpeedSnapshot.From(controller.strafeSpeed);
            ApplyCurrentSpeed();
        }

        public void EnterSwamp(SwampSpeedZone zone)
        {
            if (zone == null)
            {
                return;
            }

            SetSlowSource(zone, zone.BaseSlowPercentage);
        }

        public void ExitSwamp(SwampSpeedZone zone)
        {
            RemoveSlowSource(zone);
        }

        public void EnterCustomSlowZone(Object source, float slowPercentage)
        {
            SetSlowSource(source, slowPercentage);
        }

        public void ExitCustomSlowZone(Object source)
        {
            RemoveSlowSource(source);
        }

        private void OnDisable()
        {
            activeSlowSources.Clear();
            ApplyCurrentSpeed();
        }

        private void Update()
        {
            if (activeSlowSources.Count == 0)
            {
                return;
            }

            float now = Time.time;
            List<Object> staleSources = null;
            foreach (KeyValuePair<Object, SlowSourceState> entry in activeSlowSources)
            {
                if (now - entry.Value.LastTouchTime <= SourceStaleTime)
                {
                    continue;
                }

                if (staleSources == null)
                {
                    staleSources = new List<Object>();
                }

                staleSources.Add(entry.Key);
            }

            if (staleSources == null)
            {
                return;
            }

            for (int i = 0; i < staleSources.Count; i++)
            {
                activeSlowSources.Remove(staleSources[i]);
            }

            ApplyCurrentSpeed();
        }

        private void ApplyCurrentSpeed()
        {
            float highestSlow = 0f;
            foreach (KeyValuePair<Object, SlowSourceState> source in activeSlowSources)
            {
                highestSlow = Mathf.Max(highestSlow, source.Value.SlowPercentage);
            }

            float multiplier = 1f - highestSlow;
            multiplier = Mathf.Clamp(multiplier, 0f, 1f);

            baseFreeSpeed.ApplyTo(controller.freeSpeed, multiplier);
            baseStrafeSpeed.ApplyTo(controller.strafeSpeed, multiplier);
        }

        private void SetSlowSource(Object source, float slowPercentage)
        {
            if (source == null)
            {
                return;
            }

            activeSlowSources[source] = new SlowSourceState(Mathf.Clamp01(slowPercentage), Time.time);
            ApplyCurrentSpeed();
        }

        private void RemoveSlowSource(Object source)
        {
            if (source == null)
            {
                return;
            }

            activeSlowSources.Remove(source);
            ApplyCurrentSpeed();
        }

        private readonly struct SpeedSnapshot
        {
            public readonly float WalkSpeed;
            public readonly float RunningSpeed;
            public readonly float SprintSpeed;

            public SpeedSnapshot(float walkSpeed, float runningSpeed, float sprintSpeed)
            {
                WalkSpeed = walkSpeed;
                RunningSpeed = runningSpeed;
                SprintSpeed = sprintSpeed;
            }

            public static SpeedSnapshot From(vThirdPersonMotor.vMovementSpeed source)
            {
                return new SpeedSnapshot(source.walkSpeed, source.runningSpeed, source.sprintSpeed);
            }

            public void ApplyTo(vThirdPersonMotor.vMovementSpeed target, float multiplier)
            {
                target.walkSpeed = WalkSpeed * multiplier;
                target.runningSpeed = RunningSpeed * multiplier;
                target.sprintSpeed = SprintSpeed * multiplier;
            }
        }

        private readonly struct SlowSourceState
        {
            public readonly float SlowPercentage;
            public readonly float LastTouchTime;

            public SlowSourceState(float slowPercentage, float lastTouchTime)
            {
                SlowPercentage = slowPercentage;
                LastTouchTime = lastTouchTime;
            }
        }
    }
}
