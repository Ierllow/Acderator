using System;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intense.UI
{
    public class ToggleEx : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI toggleText;

        public Toggle Toggle => toggle;
        public bool IsOn => toggle.isOn;

        public IUniTaskAsyncEnumerable<bool> OnValueChangedAsAsyncEnumerable => toggle.OnValueChangedAsAsyncEnumerable(destroyCancellationToken);
        public UniTask SetToggleTextAsAsyncEnumerableForEachAsync(Func<bool, string> predicate) => OnValueChangedAsAsyncEnumerable.ForEachAsync((isOn) => SetToggleText(predicate(isOn)), destroyCancellationToken);

        public void SetToggleText(string text) => toggleText.SetTextFormat("{0}", text);
    }
}