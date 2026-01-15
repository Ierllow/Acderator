using UnityEngine;

namespace Song
{
    public class FrameRateController : IController
    {
        private float lastUpdateTime = 1f / Application.targetFrameRate;
        private readonly float updateInterval;

        public bool ShouldUpdate()
        {
            var currentTime = Time.time;
            if (currentTime - lastUpdateTime < updateInterval) return false;

            lastUpdateTime = currentTime;
            return true;
        }
    }
}