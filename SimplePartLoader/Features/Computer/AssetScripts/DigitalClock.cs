using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace float_oat.Desktop90
{
    /// <summary>
    /// Controls a Text UI element to show the current system time
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class DigitalClock : MonoBehaviour
    {
        [SerializeField] private bool Use24HourTime = false;
        [SerializeField] private float SecondsBetweenUpdates = 10f;

        private Text Text;

        void Start()
        {
            Text = GetComponent<Text>();
            if (Text == null)
            {
                Debug.LogException(new MissingComponentException("DigitalClock requires Text component"), this);
            }
            StartCoroutine(UpdateTimeAtInterval());
        }

        private IEnumerator UpdateTimeAtInterval()
        {
            while (true)
            {
                DateTime time = DateTime.Now;
                string hour = (Use24HourTime ? time.Hour : time.Hour % 12).ToString();
                string minute = time.Minute.ToString().PadLeft(2, '0');

                Text.text = hour + ":" + minute;

                yield return new WaitForSeconds(SecondsBetweenUpdates);
            }
        }
    }
}
