using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Song
{
    public class ComboView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI comboNumText;

        public void UpdateComboNum(int currentCombo) => comboNumText.SetTextFormat("{0}", currentCombo == 0 ? "" : currentCombo.ToString());
    }
}