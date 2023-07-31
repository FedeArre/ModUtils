using UnityEngine;

namespace float_oat.Desktop90
{
    /// <summary>
    /// Provides behavior for the action menu
    /// </summary>
    [RequireComponent(typeof(Canvas), typeof(RectTransform))]
    public class ActionMenuController : MonoBehaviour
    {
        [Tooltip("The RectTransform of the button that toggles this menu")]
        [SerializeField] private RectTransform ActionMenuButtonRectTransform = default;

        private Canvas Canvas;
        private RectTransform MenuRectTransform;

        void Start()
        {
            Canvas = GetComponent<Canvas>();
            if (Canvas == null)
            {
                Debug.LogException(new MissingComponentException("ActionMenuController needs to be attached to a Canvas"), this);
            }
            MenuRectTransform = GetComponent<RectTransform>();
            if (MenuRectTransform == null)
            {
                Debug.LogException(new MissingComponentException("ActionMenuController needs to be attached to a RectTransform"), this);
            }

            if (ActionMenuButtonRectTransform == null)
            {
                Debug.LogException(new MissingReferenceException("ActionMenuController needs a reference to the action menu button"), this);
            }
        }

        void Update()
        {
            if (Canvas.enabled)
            {
                // close the menu when clicked away from the menu and the button
                if (Input.GetMouseButtonDown(0) &&
                    RectTransformUtility.RectangleContainsScreenPoint(MenuRectTransform, Input.mousePosition) == false &&
                    RectTransformUtility.RectangleContainsScreenPoint(ActionMenuButtonRectTransform, Input.mousePosition) == false
                )
                {
                    Hide();
                }
            }
        }

        /// <summary>
        /// Shows the menu if it's hidden, hides the menu if it's showing
        /// </summary>
        public void ToggleShowing()
        {
            Canvas.enabled = !Canvas.enabled;
        }

        /// <summary>
        /// Hides the menu
        /// </summary>
        public void Hide()
        {
            Canvas.enabled = false;
        }
    }
}
