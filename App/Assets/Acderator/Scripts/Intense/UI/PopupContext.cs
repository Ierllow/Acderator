using System;

namespace Intense.UI
{
    public abstract class PopupContext
    {
        public Action NegativeCallback { get; init; }
    }

    public abstract class PopupContext<TPopup> : PopupContext where TPopup : PopupBase { }
}