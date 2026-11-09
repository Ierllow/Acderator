#nullable enable

using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Song
{
    public class NotesLineController : MonoBehaviour
    {
        [SerializeField] private GameObject[] laneLights = default!;
        [SerializeField] private GameObject noteLineRoot = default!;

        public float LaneLength => 38.49f;

        public async UniTask Show()
        {
            noteLineRoot.SetActive(true);
            await noteLineRoot.transform.DOMoveZ(0f, 1f).WithCancellation(destroyCancellationToken);
        }

        public void SetLaneLightActive(int lane, FingerType fingerType) => laneLights[lane].SetActive(fingerType == FingerType.Down);

        public void SetLaneLightActiveAll(bool value = false)
        {
            foreach (var laneLight in laneLights)
            {
                if (laneLight.activeSelf != value) laneLight.SetActive(value);
            }
        }
    }
}