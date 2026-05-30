using UnityEngine;

namespace Song
{
    public sealed class LaneDetector
    {
        private const float PosYMin = -4.5f;
        private const float PosYMax = -1.2f;

        private static readonly (float Min, float Max)[] laneRanges =
        {
            (-6.8f, -2.8f),
            (-2.8f,  0.0f),
            ( 0.0f,  2.8f),
            ( 2.8f,  6.8f),
        };

        private Camera camera;
        private float planeDistance;

        public void Init(Camera camera, Transform hitPlane)
        {
            this.camera = camera;
            planeDistance = Vector3.Distance(camera.transform.position, hitPlane.position);
        }

        public Vector3 GetWorldPosition(Vector2 screenPosition) => camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, planeDistance));

        public bool TryGetLane(float positionX, float positionY, out int lane)
        {
            if (positionY < PosYMin || positionY > PosYMax)
            {
                lane = -1;
                return false;
            }
            for (var i = 0; i < laneRanges.Length; i++)
            {
                var (min, max) = laneRanges[i];
                if (positionX >= min && positionX <= max)
                {
                    lane = i;
                    return true;
                }
            }
            lane = -1;
            return false;
        }
    }
}