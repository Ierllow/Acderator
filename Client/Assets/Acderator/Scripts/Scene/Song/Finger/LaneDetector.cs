#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Song
{
    public sealed class LaneDetector
    {
        private Camera camera = default!;
        private Transform hitPlane = default!;
        private IReadOnlyList<Transform> laneTargets = default!;
        private float planeDistance;

        public void Init(Camera camera, Transform hitPlane, IReadOnlyList<Transform> laneTargets)
        {
            this.camera = camera;
            this.hitPlane = hitPlane;
            this.laneTargets = laneTargets;
            planeDistance = Vector3.Distance(camera.transform.position, hitPlane.position);
        }

        public bool TryGetLane(Vector2 screenPosition, out int lane)
        {
            lane = -1;
            if (!CanDetect()) return false;

            var localPosition = hitPlane.InverseTransformPoint(GetWorldPosition(screenPosition));
            var bestDistance = float.MaxValue;
            for (var i = 0; i < laneTargets.Count; i++)
            {
                var target = laneTargets[i];
                if (target == null) continue;

                var targetPosition = hitPlane.InverseTransformPoint(target.position);
                var distance = Mathf.Abs(targetPosition.x - localPosition.x);
                if (distance >= bestDistance) continue;

                bestDistance = distance;
                lane = i;
            }

            return lane >= 0 && bestDistance <= GetLaneHalfWidth(lane);
        }

        private Vector3 GetWorldPosition(Vector2 screenPosition) => camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, planeDistance));

        private bool CanDetect() => camera != null && hitPlane != null && laneTargets is { Count: > 0 };

        private float GetLaneHalfWidth(int lane)
        {
            if (laneTargets.Count <= 1) return float.MaxValue;

            var laneX = GetLaneLocalX(lane);
            var distance = float.MaxValue;
            if (lane > 0 && laneTargets[lane - 1] != null) distance = Mathf.Min(distance, Mathf.Abs(laneX - GetLaneLocalX(lane - 1)));
            if (lane < laneTargets.Count - 1 && laneTargets[lane + 1] != null) distance = Mathf.Min(distance, Mathf.Abs(laneX - GetLaneLocalX(lane + 1)));
            return distance < float.MaxValue ? distance * 0.5f : float.MaxValue;
        }

        private float GetLaneLocalX(int lane) => hitPlane.InverseTransformPoint(laneTargets[lane].position).x;
    }
}