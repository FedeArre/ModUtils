using UnityEngine;
using UnityEngine.UI;

namespace float_oat.Desktop90
{
    /// <summary>
    /// Button UI element for Desktop 90 UI. Extends the Unity button by giving behavior for chaning the text color when disabled, and shifting the content when pressed.
    /// </summary>
    public class D90Button : Button
    {
        [Header("Text coloring")]
        [Tooltip("Graphic element such as text or image to change color when the button is disabled")]
        [SerializeField] private Graphic GraphicToColor = default;
        [SerializeField] private Color EnabledGraphicColor = Color.black;
        [SerializeField] private Color DisabledGraphicColor = Color.gray;

        [Header("Press-in content")]
        [Tooltip("The UI within the button to shift when the button is pressed down")]
        [SerializeField] private RectTransform PressInContent = default;
        [SerializeField] private Vector2 DistanceToPressIn = new Vector2(1f, -1f);

        private Vector2 restingPosition;

        protected override void Start()
        {
            base.Start();
            if (PressInContent != null)
            {
                restingPosition = PressInContent.anchoredPosition;
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (GraphicToColor != null)
            {
                switch (state)
                {
                    case SelectionState.Disabled:
                        GraphicToColor.color = DisabledGraphicColor;
                        break;
                    default:
                        GraphicToColor.color = EnabledGraphicColor;
                        break;
                }
            }

            if (PressInContent != null)
            {
                switch (state)
                {
                    case SelectionState.Pressed:
                        PressInContent.anchoredPosition = restingPosition + DistanceToPressIn;
                        break;
                    default:
                        PressInContent.anchoredPosition = restingPosition;
                        break;
                }
            }
        }
    }
}
