#nullable enable

using UnityEngine;

namespace Song
{
    public enum FingerJudgeRequestType { Normal, Swipe, HoldCross }

    public sealed class FingerJudgeRequest
    {
        private const int PointerIdNone = int.MinValue;

        public FingerJudgeRequestType RequestType { get; }
        public EFingerType FingerType { get; }
        public int Lane { get; }
        public bool AllowMiss { get; }
        public int PointerId { get; }
        public int Frame { get; }

        public bool IsCurrentFrame() => Frame == Time.frameCount;

        private FingerJudgeRequest(FingerJudgeRequestType requestType, EFingerType fingerType, int lane, bool allowMiss, int pointerId = PointerIdNone)
        {
            RequestType = requestType;
            FingerType = fingerType;
            Lane = lane;
            AllowMiss = allowMiss;
            PointerId = pointerId;
            Frame = Time.frameCount;
        }

        public static FingerJudgeRequest Down(int lane) => new(FingerJudgeRequestType.Normal, EFingerType.Down, lane, allowMiss: false);

        public static FingerJudgeRequest Up(int lane) => new(FingerJudgeRequestType.Normal, EFingerType.Up, lane, allowMiss: true);

        public static FingerJudgeRequest Swipe(int lane) => new(FingerJudgeRequestType.Swipe, EFingerType.Up, lane, allowMiss: false);

        public static FingerJudgeRequest HoldCross(int pointerId, int lane) => new(FingerJudgeRequestType.HoldCross, EFingerType.Up, lane, allowMiss: true, pointerId: pointerId);
    }
}
