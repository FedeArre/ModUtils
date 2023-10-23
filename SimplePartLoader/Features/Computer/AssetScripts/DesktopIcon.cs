using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace float_oat.Desktop90
{
    /// <summary>
    /// Provides behavior for a double-clickable and draggable UI element
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DesktopIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
    {
        [Header("Background graphic")]
        [Tooltip("Graphic element to color to show when the icon is selected")]
        [SerializeField] private Graphic TargetGraphic = default;
        [SerializeField] private Color NormalColor = new Color(0f, 0f, 0f, 0f);
        [SerializeField] private Color SingleClickedColor = Color.blue;

        [Header("Interaction")]
        [Tooltip("When enabled, the icon can be moved by clicking and dragging it")]
        [SerializeField] internal bool Draggable = true;
        [Tooltip("When turned off, the action is invoked after a single click")]
        [SerializeField] internal bool RequiresDoubleClick = true;
        [SerializeField] internal UnityEvent OnDoubleClick = default;

        private bool isSelected = false;
        private RectTransform RectTransform;
        private Vector3 mouseOffset;

        void Start()
        {
            RectTransform = GetComponent<RectTransform>();
            if (RectTransform == null)
            {
                Debug.LogException(new MissingComponentException("Desktop icon must have a RectTransform"), this);
            }
        }

        void Update()
        {
            if (isSelected)
            {
                // unselect when clicked away
                if (Input.GetMouseButtonDown(0) && RectTransformUtility.RectangleContainsScreenPoint(RectTransform, Input.mousePosition) == false)
                {
                    Unselect();
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            mouseOffset = RectTransform.position - Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Draggable)
            {
                Unselect();
                RectTransform.position = Input.mousePosition + mouseOffset;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // select the icon, invoke action if already selected
            if (isSelected || !RequiresDoubleClick)
            {
                OnDoubleClick.Invoke();
                Unselect();
            }
            else
            {
                Select();
            }
        }

        private void Select()
        {
            isSelected = true;
            if (TargetGraphic != null)
            {
                TargetGraphic.color = SingleClickedColor;
            }
        }

        private void Unselect()
        {
            isSelected = false;
            if (TargetGraphic != null)
            {
                TargetGraphic.color = NormalColor;
            }
        }
    }
}
