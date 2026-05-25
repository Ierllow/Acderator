using UnityEngine;

namespace Song
{
    public sealed class SongTimeCalculator
    {
        private float offset;
        private float pauseTime;

        public void SetOffset(float offset) => this.offset = offset;

        public float GetSec(float requestedSec, ESongState currentState)
        {
            if (currentState is ESongState.None or ESongState.Stop) pauseTime += Time.deltaTime;
            return requestedSec <= 0 ? Time.timeSinceLevelLoad - pauseTime - offset : requestedSec;
        }
    }
}