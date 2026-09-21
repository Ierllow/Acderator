#nullable enable

using UnityEngine;

namespace Song
{
    public enum FingerJudgeRequestType { Normal, Swipe, HoldCross }

    public sealed class FingerJudgeRequest
    {
        public FingerJudgeRequestType RequestType { get; }
        public FingerType FingerType { get; }
        public int Lane { get; }
        public bool AllowMiss { get; }
        public int PointerId { get; }
        public int Frame { get; }

        public bool IsCurrentFrame() => Frame == Time.frameCount;

        private FingerJudgeRequest(FingerJudgeRequestType requestType, FingerType fingerType, int pointerId, int lane, bool allowMiss)
        {
            RequestType = requestType;
            FingerType = fingerType;
            Lane = lane;
            AllowMiss = allowMiss;
            PointerId = pointerId;
            Frame = Time.frameCount;
        }

        public static FingerJudgeRequest Down(int pointerId, int lane) => new(FingerJudgeRequestType.Normal, FingerType.Down, pointerId, lane, allowMiss: false);

        public static FingerJudgeRequest Up(int pointerId, int lane) => new(FingerJudgeRequestType.Normal, FingerType.Up, pointerId, lane, allowMiss: true);

        public static FingerJudgeRequest Swipe(int pointerId, int lane) => new(FingerJudgeRequestType.Swipe, FingerType.Up, pointerId, lane, allowMiss: false);

        public static FingerJudgeRequest HoldCross(int pointerId, int lane) => new(FingerJudgeRequestType.HoldCross, FingerType.Up, pointerId, lane, allowMiss: true);
    }
}