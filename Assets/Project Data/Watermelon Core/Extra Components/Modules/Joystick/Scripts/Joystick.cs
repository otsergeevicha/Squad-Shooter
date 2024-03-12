using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Animator))]
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IControlBehavior
    {
        public static Joystick Instance { get; private set; }

        [Header("Joystick")]
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Image handleImage;

        [Space]
        [SerializeField] Color backgroundActiveColor = Color.white;
        [SerializeField] Color backgroundDisableColor = Color.white;

        [SerializeField] Color handleActiveColor = Color.white;
        [SerializeField] Color handleDisableColor = Color.white;

        [Space]
        [SerializeField] float handleRange = 1;
        [SerializeField] float deadZone = 0;

        [Header("Tutorial")]
        [SerializeField] bool useTutorial;
        [SerializeField] GameObject pointerGameObject;

        private RectTransform baseRectTransform;
        private RectTransform backgroundRectTransform;
        private RectTransform handleRectTransform;

        private bool isJoysticTouched;
        public bool IsJoysticTouched => isJoysticTouched;

        private bool canDrag;

        private Canvas canvas;
        private Camera canvasCamera;

        protected Vector2 input = Vector2.zero;

        public Vector3 Input => input;
        public Vector3 FormatInput => new Vector3(input.x, 0, input.y);

        private Vector2 defaultAnchoredPosition;

        private Animator joystickAnimator;
        private bool isTutorialDisplayed;
        private bool hideVisualsActive;

        // Events
        public static OnJoystickTouchedCallback OnJoystickTouched;

        public void Initialise(Canvas canvas)
        {
            this.canvas = canvas;

            Instance = this;

            joystickAnimator = GetComponent<Animator>();

            baseRectTransform = GetComponent<RectTransform>();
            backgroundRectTransform = backgroundImage.rectTransform;
            handleRectTransform = handleImage.rectTransform;

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                canvasCamera = canvas.worldCamera;

            Vector2 center = new Vector2(0.5f, 0.5f);
            backgroundRectTransform.pivot = center;
            handleRectTransform.anchorMin = center;
            handleRectTransform.anchorMax = center;
            handleRectTransform.pivot = center;
            handleRectTransform.anchoredPosition = Vector2.zero;

            isJoysticTouched = false;

            if(useTutorial)
            {
                joystickAnimator.enabled = true;
                isTutorialDisplayed = true;

                pointerGameObject.SetActive(true);
            }
            else
            {
                joystickAnimator.enabled = false;
                isTutorialDisplayed = false;

                pointerGameObject.SetActive(false);
            }

            backgroundImage.color = backgroundDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);
            handleImage.color = handleDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);

            defaultAnchoredPosition = backgroundRectTransform.anchoredPosition;

            // Set joystick as main control behavior
            Control.SetControl(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            canDrag = !WorldSpaceRaycaster.Raycast(eventData);

            if (!canDrag) return;

            if (!isTutorialDisplayed)
            {
                isTutorialDisplayed = true;

                joystickAnimator.enabled = false;
                pointerGameObject.SetActive(false);
            }

            backgroundRectTransform.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);

            backgroundImage.color = backgroundActiveColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);
            handleImage.color = handleActiveColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);

            isJoysticTouched = true;

            OnJoystickTouched?.Invoke();

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isJoysticTouched || !canDrag)
                return;

            Vector2 position = RectTransformUtility.WorldToScreenPoint(canvasCamera, backgroundRectTransform.position);
            Vector2 radius = backgroundRectTransform.sizeDelta / 2;
            input = (eventData.position - position) / (radius * canvas.scaleFactor);
            HandleInput(input.magnitude, input.normalized, radius, canvasCamera);
            handleRectTransform.anchoredPosition = input * radius * handleRange;
        }

        protected void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (magnitude > deadZone)
            {
                if (magnitude > 1)
                    input = normalised;
            }
            else
                input = Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            WorldSpaceRaycaster.OnPointerUp(eventData);

            if (!isJoysticTouched)
                return;

            isJoysticTouched = false;

            ResetInput();
        }

        public void ResetInput()
        {
            isJoysticTouched = false;

            backgroundImage.color = backgroundDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);
            handleImage.color = handleDisableColor.SetAlpha(hideVisualsActive ? 0f : backgroundDisableColor.a);

            backgroundRectTransform.anchoredPosition = defaultAnchoredPosition;

            input = Vector2.zero;
            handleRectTransform.anchoredPosition = Vector2.zero;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRectTransform, screenPosition, canvasCamera, out localPoint))
            {
                Vector2 pivotOffset = baseRectTransform.pivot * baseRectTransform.sizeDelta;
                return localPoint - (backgroundRectTransform.anchorMax * baseRectTransform.sizeDelta) + pivotOffset;
            }
            return Vector2.zero;
        }

        public void EnableControl()
        {
            gameObject.SetActive(true);
        }

        public void DisableControl()
        {
            gameObject.SetActive(false);
            isJoysticTouched = false;

            ResetInput();
        }

        public void HideVisuals()
        {
            hideVisualsActive = true;

            backgroundImage.color = backgroundImage.color.SetAlpha(0f);
            handleImage.color = backgroundImage.color.SetAlpha(0f);
        }

        public void ShowVisuals()
        {
            hideVisualsActive = false;

            backgroundImage.color = backgroundImage.color.SetAlpha(1f);
            handleImage.color = backgroundImage.color.SetAlpha(1f);
        }

        public delegate void OnJoystickTouchedCallback();
    }
}