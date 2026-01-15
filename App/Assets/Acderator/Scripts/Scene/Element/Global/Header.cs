using Element.UI;
using Intense;
using Intense.UI;
using R3;
using UnityEngine;

namespace Element
{
    public class Header : MonoBehaviour
    {
        [SerializeField] private CommonButton menuButton;

        private void Start()
        {
            var context = new MenuPopupContext { NegativeCallback = async (sceneType) => await SceneManager.Instance.ChangeSceneAsync(sceneType) };
            menuButton.OnTapButtonAsObservable.SubscribeLock(new(true), __ => PopupManager.Instance.OpenPopup(context)).RegisterTo(destroyCancellationToken);
        }

        public void SetHeaderActive(bool active) => gameObject.SetActive(active);
    }
}