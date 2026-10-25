#nullable enable

using UnityEngine;

namespace Song
{
    public sealed class SongTimeCalculator
    {
        private const float NotStartedSec = -9999f;

        private float leadInSec;
        private float leadInStartRealtime;
        private float lastSec = NotStartedSec;
        private bool isLeadInStarted;

        public void StartLeadIn(float leadInSec)
        {
            this.leadInSec = leadInSec;
            leadInStartRealtime = Time.realtimeSinceStartup;
            lastSec = -this.leadInSec;
            isLeadInStarted = true;
        }

        public float GetSec(float requestedSec, SongState currentState)
        {
            lastSec = currentState switch
            {
                SongState.Ready when isLeadInStarted => Time.realtimeSinceStartup - leadInStartRealtime - leadInSec,
                SongState.Playing => requestedSec > 0 ? requestedSec : 0f,
                SongState.None => NotStartedSec,
                _ => lastSec,
            };
            return lastSec;
        }
    }
}