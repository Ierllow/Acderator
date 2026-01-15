using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace Song
{
    public class NotesLineController : MonoBehaviour
    {
        [SerializeField] private GameObject[] laneLights;
        [SerializeField] private GameObject noteLineRoot;

        public float LaneLength => 38.49f;

        public async UniTask Show()
        {
            noteLineRoot.SetActive(true);
            await noteLineRoot.transform.DOMoveZ(0f, 1f).WithCancellation(destroyCancellationToken);
        }

        private List<(GameObject, int)> laneLightTuple;
        public void SetLaneLightActive(int lane, EFingerType fingerType)
        {
            laneLightTuple ??= laneLights.AsValueEnumerable().Select((ll, index) => (ll, index)).ToList();
            foreach (var (ll, index) in laneLightTuple)
            {
                var value = index == lane && fingerType.EnumEquals(EFingerType.Down);
                ll.SetActive(value);
            }
        }

        public void SetLaneLightActiveAll(bool value = false)
        {
            foreach (var laneLight in laneLights.AsValueEnumerable().Where(x => !value ? x.activeSelf : !x.activeSelf)) laneLight.SetActive(value);
        }
    }
}