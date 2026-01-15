using R3;

namespace Song
{
    public interface IController { }

    public interface ISongController : IController
    {
        void Init();
    }

    public interface IFingeController : IController
    {
        Subject<(FingerInfo, int)> FingerInfoSubject { get; }
        void NotifyFinger(FingerInfo fingerInfo, int lane);
        void Init();
    }

    public interface IAutoFingerController : IController
    {
        Subject<FingerInfo> FingerInfoSubject { get; }
        void NotifyFinger(FingerInfo fingerInfo);
    }

    public interface ITutorialController : IController
    {
        void Init(TutorialData tutorialData);
    }
}