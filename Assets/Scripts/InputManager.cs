using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PathOfView.GameLogic
{
    public class InputManager : MonoBehaviour
    {
        private GameInput _gameInput;

        private bool isDragging = false;

        private void Awake()
        {
            _gameInput = new GameInput();
            _gameInput.Player.Tap.performed += ctx => OnTapHandler(ctx);
            _gameInput.Player.Drag.performed += ctx => OnDragHandler(ctx);
            _gameInput.Player.LongTouch.performed += ctx => OnLongTouchHandler(ctx);
            _gameInput.Player.Drag.started += ctx => isDragging = true;
            _gameInput.Player.Drag.canceled += ctx => isDragging = false;
            _gameInput.Player.Touch.canceled += ctx => OnTouchHandler(ctx);
        }

        private void OnDestroy()
        {
            _gameInput.Player.Tap.performed -= ctx => OnTapHandler(ctx);
            _gameInput.Player.Drag.performed -= ctx => OnDragHandler(ctx);
            _gameInput.Player.LongTouch.performed -= ctx => OnLongTouchHandler(ctx);
            _gameInput.Player.Drag.started -= ctx => isDragging = true;
            _gameInput.Player.Drag.canceled -= ctx => isDragging = false;
            _gameInput.Player.Touch.canceled -= ctx => OnTouchHandler(ctx);
        }

        private void OnEnable()
        {
            _gameInput.Enable();
        }

        private void OnDisable()
        {
            _gameInput.Disable();
        }



        public event Action<Vector2> OnTap;
        public event Action<Vector2> OnDrag;
        public event Action<Vector2> OnLongTouch;
        public event Action OnTouchLift;

        private void OnTouchHandler(InputAction.CallbackContext ctx)
        {
            OnTouchLift?.Invoke();
        }

        private void OnTapHandler(InputAction.CallbackContext ctx)
        {
            Vector2 tapPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector2 screenPosition = Camera.main.ScreenToWorldPoint(tapPosition);
            OnTap?.Invoke(tapPosition);
        }

        private void OnDragHandler(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<Vector2>().x == Single.PositiveInfinity || 
                ctx.ReadValue<Vector2>().y == Single.PositiveInfinity ||
                ctx.ReadValue<Vector2>().x == Single.NegativeInfinity ||
                ctx.ReadValue<Vector2>().y == Single.NegativeInfinity)
            {
                return;
            }
            OnDrag?.Invoke(ctx.ReadValue<Vector2>());
        }

        private void OnLongTouchHandler(InputAction.CallbackContext ctx)
        {
            if (!isDragging)
            {
                Vector2 tapPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                Vector2 screenPosition = Camera.main.ScreenToWorldPoint(tapPosition);
                OnLongTouch?.Invoke(screenPosition);
            }
        }
    }
}