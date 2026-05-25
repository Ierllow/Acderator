using R3;
using TMPro;
using UnityEngine;
using Zenject;

namespace Song
{
    public class ComboView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI comboNumText;

        [Inject] private ComboController comboController;

        private void Start() => comboController.CurrentCombo.Subscribe(combo => comboNumText.SetText(combo == 0 ? "" : combo.ToString())).RegisterTo(destroyCancellationToken);
    }
}