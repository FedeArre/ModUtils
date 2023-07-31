using UnityEngine;
using UnityEngine.SceneManagement;

namespace float_oat.Desktop90
{
    /// <summary>
    /// Provides behavior for opening and closing the pause menu, and switching between tabs in it
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject[] ContentTabs = default;

        public void OpenPauseMenu()
        {
            gameObject.SetActive(true);
        }

        public void ClosePauseMenu()
        {
            gameObject.SetActive(false);
        }

        public void SwitchContentTab(int contentTab)
        {
            if (ContentTabs == null || ContentTabs.Length < 1)
            {
                Debug.LogException(new System.InvalidOperationException("Pause menu has no content tabs. Drag content into the ContentTabs field"), this);
            }

            if (contentTab >= ContentTabs.Length)
            {
                Debug.LogException(new System.ArgumentOutOfRangeException("Pause menu has no content tab " + contentTab), this);
            }

            for (int i = 0; i < ContentTabs.Length; i++)
            {
                ContentTabs[i].SetActive(i == contentTab);
            }
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
