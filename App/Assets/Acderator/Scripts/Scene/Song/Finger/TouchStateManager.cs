using System.Collections.Generic;
using UnityEngine;

namespace Song
{
    internal sealed class TouchStateManager
    {
        public readonly struct TouchState
        {
            public Vector2 StartScreenPosition { get; }
            public int Lane { get; }

            public TouchState(Vector2 startScreenPosition, int lane)
            {
                StartScreenPosition = startScreenPosition;
                Lane = lane;
            }

            public TouchState WithLane(int lane) => new(StartScreenPosition, lane);
        }

        private readonly Dictionary<int, TouchState> dict = new();

        public bool TryGet(int pointerId, out TouchState state) => dict.TryGetValue(pointerId, out state);

        public void Set(int pointerId, TouchState state) => dict[pointerId] = state;

        public void Remove(int pointerId) => dict.Remove(pointerId);

        public void Clear() => dict.Clear();
    }
}
