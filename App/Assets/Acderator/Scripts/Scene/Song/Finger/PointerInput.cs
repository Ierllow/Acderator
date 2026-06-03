using System;
using UnityEngine;

namespace Song
{
    public sealed class PointerInput
    {
        private const int MousePointerId = -1;

        private Action<int, Vector2> fingerDown;
        private Action<int, Vector2> fingerUpdate;
        private Action<int, Vector2> fingerUp;

        public void SetCallbacks(Action<int, Vector2> fingerDown, Action<int, Vector2> fingerUpdate, Action<int, Vector2> fingerUp)
        {
            this.fingerDown = fingerDown;
            this.fingerUpdate = fingerUpdate;
            this.fingerUp = fingerUp;
        }

        public void Update()
        {
            var touchCount = Input.touchCount;
            if (Input.touchSupported && touchCount > 0)
            {
                UpdateTouches(touchCount);
                return;
            }

            if (Application.isEditor) UpdateMouse();
        }

        private void UpdateTouches(int touchCount)
        {
            for (var i = 0; i < touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        fingerDown(touch.fingerId, touch.position);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        fingerUpdate(touch.fingerId, touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        fingerUp(touch.fingerId, touch.position);
                        break;
                }
            }
        }

        private void UpdateMouse()
        {
            var screenPosition = (Vector2)Input.mousePosition;
            if (Input.GetMouseButtonDown(0)) fingerDown(MousePointerId, screenPosition);
            else if (Input.GetMouseButton(0)) fingerUpdate(MousePointerId, screenPosition);
            else if (Input.GetMouseButtonUp(0)) fingerUp(MousePointerId, screenPosition);
        }
    }
}