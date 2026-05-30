using R3;

namespace Song
{
    public interface IController { }

    public interface ISongController : IController
    {
        void Init();
    }

    public interface IFingerController : IController
    {
        Observable<FingerInfo> JudgmentAsObservable { get; }
        void Init();
        void UpdateInput();
        void Judge(float currentSec);
    }
}