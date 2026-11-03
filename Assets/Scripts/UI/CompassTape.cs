using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HelicopterSim
{
    // Горизонтальна стрічка компаса: мітки створюються один раз, далі стрічка лише зсувається
    public class CompassTape : MonoBehaviour
    {
        [SerializeField] private RectTransform strip;       // дочірній об'єкт усередині маски (RectMask2D)
        [SerializeField] private Image tickTemplate;        // вимкнений шаблон штриха
        [SerializeField] private TMP_Text labelTemplate;    // вимкнений шаблон підпису
        [SerializeField] private float pixelsPerDegree = 4f;
        [SerializeField] private int tickStep = 15;
        [SerializeField] private int labelStep = 45;

        private static readonly string[] Directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        void Awake()
        {
            // Мітки від -180 до 540 градусів: при будь-якому курсі 0...360 видима частина заповнена
            for (int deg = -180; deg <= 540; deg += tickStep)
            {
                float x = deg * pixelsPerDegree;
                bool isLabel = deg % labelStep == 0;

                Image tick = Instantiate(tickTemplate, strip);
                tick.gameObject.SetActive(true);
                tick.rectTransform.anchoredPosition = new Vector2(x, -14f);
                tick.rectTransform.sizeDelta = new Vector2(2f, isLabel ? 16f : 9f);

                if (isLabel)
                {
                    int wrapped = ((deg % 360) + 360) % 360;
                    TMP_Text label = Instantiate(labelTemplate, strip);
                    label.gameObject.SetActive(true);
                    label.rectTransform.anchoredPosition = new Vector2(x, 9f);
                    label.text = Directions[wrapped / 45];
                }
            }
        }

        public void SetHeading(float heading)
        {
            strip.anchoredPosition = new Vector2(-heading * pixelsPerDegree, strip.anchoredPosition.y);
        }
    }
}
