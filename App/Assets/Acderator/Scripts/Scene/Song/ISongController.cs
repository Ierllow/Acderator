namespace Song
{
    public interface IController { }

    public interface ISongController : IController
    {
        void Init();
    }

    public interface IFingerController : IController
    {
        void Init();
        void UpdateInput();
    }
}