using TMPro;
using UnityEngine;

namespace Intense.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshProUGUIEx : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI text;
        [SerializeField] protected int lineSpacing;

        public virtual TextMeshProUGUI Text => text;

        protected virtual void Awake() => text.lineSpacing = lineSpacing;
    }
}