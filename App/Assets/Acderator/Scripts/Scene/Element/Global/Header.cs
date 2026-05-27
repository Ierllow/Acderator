using Element.UI;
using Intense;
using Intense.UI;
using R3;
using UnityEngine;
using Zenject;

namespace Element
{
    public class Header : MonoBehaviour
    {
        [SerializeField] private CommonButton menuButton;

        [Inject] private readonly PopupManager popupManager;
        [Inject] private readonly SceneManager sceneManager;

        private void Start()
        {
            var context = new MenuPopupContext { NegativeCallback = async (sceneType) => await sceneManager.ChangeSceneAsync(sceneType) };
            menuButton.OnTapButtonAsObservable.SubscribeLock(__ => popupManager.OpenPopup(context)).RegisterTo(destroyCancellationToken);
        }

        public void SetHeaderActive(bool active) => gameObject.SetActive(active);
    }
}