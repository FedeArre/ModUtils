using System.Collections;
using UnityEngine;

namespace float_oat.Desktop90
{
    /// <summary>
    /// Handles open and closing behavior of a window
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class WindowController : MonoBehaviour
    {
        [Header("Sound")]
        [SerializeField] private AudioClip OnOpenAudioClip = default;
        [SerializeField] private AudioClip OnCloseAudioClip = default;

        [Header("Fade Animation")]
        public bool EnableFadeInAndFadeOutAnimation = true;
        public float FadeOutTime = 0.1f;
        public float FadeInTime = 0.1f;

        [Header("Collapsing")]
        [SerializeField] private RectTransform HideOnCollapse = default;
        [SerializeField] private float CollapsedHeight = 32;

        private float ExpandedHeight;
        private bool IsCollapsed = false;

        private RectTransform RectTransform;
        private AudioSource AudioSource;
        private CanvasGroup CanvasGroup;

        void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            if (RectTransform == null)
            {
                Debug.LogException(new MissingComponentException("Window needs a RectTransform"), this);
            }

            AudioSource = GetComponent<AudioSource>();
            if (AudioSource == null && (OnOpenAudioClip != null || OnCloseAudioClip != null))
            {
                Debug.LogException(new MissingComponentException("Window needs an AudioSource to play AudioClips"), this);
            }

            CanvasGroup = GetComponent<CanvasGroup>();
            if (CanvasGroup == null && EnableFadeInAndFadeOutAnimation)
            {
                Debug.LogException(new MissingComponentException("Window needs a CanvasGroup to do fade in and fade out animations"), this);
            }
        }

        /// <summary>
        /// Close the window by disabiling the canvas
        /// </summary>
        public void Close()
        {
            if (OnCloseAudioClip != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(OnCloseAudioClip);
            }

            if (CanvasGroup != null && EnableFadeInAndFadeOutAnimation && FadeOutTime > 0f)
            {
                StartCoroutine(FadeOutAnimation());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator FadeOutAnimation()
        {
            float elapsedTime = 0f;
            while (elapsedTime < FadeOutTime)
            {
                CanvasGroup.alpha = 1f - (elapsedTime / FadeOutTime);
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            CanvasGroup.alpha = 1f;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Open the window by enabling the canvas
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            if (CanvasGroup != null && EnableFadeInAndFadeOutAnimation && FadeInTime > 0f)
            {
                StartCoroutine(FadeInAnimation());
            }

            if (OnOpenAudioClip != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(OnOpenAudioClip);
            }

            BringToFront();
        }

        private IEnumerator FadeInAnimation()
        {
            float elapsedTime = 0f;
            while (elapsedTime < FadeInTime)
            {
                CanvasGroup.alpha = elapsedTime / FadeInTime;
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            CanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Collapses the window content if it is expanded
        /// </summary>
        public void Collapse()
        {
            if (!IsCollapsed)
            {
                if (HideOnCollapse == null)
                {
                    Debug.LogException(new MissingReferenceException("HideOnCollapse field required to collapse window"), this);
                    return;
                }
                ExpandedHeight = RectTransform.sizeDelta.y;
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, CollapsedHeight);
                HideOnCollapse.gameObject.SetActive(false);

                IsCollapsed = true;
            }
        }

        /// <summary>
        /// Expands the window content is it is collapsed
        /// </summary>
        public void Expand()
        {
            if (IsCollapsed)
            {
                HideOnCollapse.gameObject.SetActive(true);
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, ExpandedHeight);

                IsCollapsed = false;
            }

            BringToFront();
        }

        /// <summary>
        /// Puts the window in front of any sibling windows
        /// </summary>
        public void BringToFront()
        {
            transform.SetSiblingIndex(transform.parent.childCount - 1);
        }

        /// <summary>
        /// Puts the window behind any sibling windows
        /// </summary>
        public void SendToBack()
        {
            transform.SetSiblingIndex(0);
        }
    }
}
