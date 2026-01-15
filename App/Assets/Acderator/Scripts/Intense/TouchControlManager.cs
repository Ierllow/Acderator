using UnityEngine;
using UnityEngine.EventSystems;

namespace Intense
{
    public class TouchControlManager : SingletonMonoBehaviour<TouchControlManager>
    {
        [SerializeField] private EventSystem eventSystem;

        public bool Enabled => eventSystem.enabled;

        public void SetEventSystemEnabled(bool enabled) => eventSystem.enabled = !enabled;
    }
}