using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace float_oat.Desktop90
{
    /// <summary>
    /// A StandaloneInputModule with the functionality of changing the cursor image when hovering over different UI elements
    /// </summary>
    public class CustomCursorStandaloneInputModule : StandaloneInputModule
    {
        [Header("Cursor textures")]
        [SerializeField] private Texture2D NormalCursor = default;

        [Tooltip("Cursor to show when hovering over an InputField")]
        [SerializeField] private Texture2D TextInputCursor = default;

        [Tooltip("Cursor to show when hovering over a non-interactable selectable object")]
        [SerializeField] private Texture2D DisabledCursor = default;

        [Header("Cursor settings")]
        [SerializeField] private CursorMode CursorMode = CursorMode.ForceSoftware;

        private bool CursorIsNotDefault = false;

        protected override void Start()
        {
            base.Start();
            ChangeCursor(NormalCursor);
        }

        protected void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject(kMouseLeftId))
            {
                Selectable hoveredSelectable = GameObjectUnderPointer(kMouseLeftId).GetComponent<Selectable>();
                if (hoveredSelectable != null)
                {
                    if (DisabledCursor != null && hoveredSelectable.interactable == false)
                    {
                        ChangeCursor(DisabledCursor);
                        CursorIsNotDefault = true;
                    }
                    else if (TextInputCursor != null && hoveredSelectable as InputField != null)
                    {
                        ChangeCursor(TextInputCursor);
                        CursorIsNotDefault = true;
                    }
                }
                else
                {
                    ChangeCursor(NormalCursor);
                    CursorIsNotDefault = false;
                }
            }
            else if (CursorIsNotDefault)
            {
                ChangeCursor(NormalCursor);
                CursorIsNotDefault = false;
            }
        }

        private void ChangeCursor(Texture2D cursorTexture)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode);
        }

        private GameObject GameObjectUnderPointer(int pointerId)
        {
            var lastPointer = GetLastPointerEventData(pointerId);
            if (lastPointer != null)
            {
                return lastPointer.pointerCurrentRaycast.gameObject;
            }
            return null;
        }
    }
}
