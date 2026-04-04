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

        [Inject] private PopupManager popupManager;
        [SerializeField] private CommonButton menuButton;

        private void Start()
        {
            var context = new MenuPopupContext { NegativeCallback = async (sceneType) => await sceneManager.ChangeSceneAsync(sceneType) };
            menuButton.OnTapButtonAsObservable.SubscribeLock(new(true), __ => popupManager.OpenPopup(context)).RegisterTo(destroyCancellationToken);
        }

        public void SetHeaderActive(bool active) => gameObject.SetActive(active);
    }
}
