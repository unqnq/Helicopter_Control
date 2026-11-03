using TMPro;
using UnityEngine;

namespace HelicopterSim
{
    // Показує дані з HelicopterTelemetry. Сам нічого не рахує.
    public class FlightHUD : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private TMP_Text heightAboveGroundText;
        [SerializeField] private TMP_Text heightText;
        [SerializeField] private TMP_Text verticalSpeedText;
        [SerializeField] private TMP_Text collectiveText;
        [SerializeField] private TMP_Text directionText;
        [SerializeField] private TMP_Text rollText;
        [SerializeField] private TMP_Text pitchText;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text hintText;

        [Header("Collective bar")]
        [SerializeField] private RectTransform collectiveFill;   // anchor = низ бару, висота = положення важеля
        [SerializeField] private RectTransform hoverMark;        // лінія висіння

        [Header("Attitude")]
        [SerializeField] private RectTransform helicopterIcon;
        [SerializeField] private RectTransform pitchMarkerLeft;
        [SerializeField] private RectTransform pitchMarkerRight;
        [SerializeField] private float pitchPixelsPerDegree = 2f;
        [SerializeField] private float pitchLimit = 30f;

        [Header("Compass")]
        [SerializeField] private CompassTape compass;

        private HelicopterTelemetry telemetry;
        private float barHeight;

        public void Bind(HelicopterTelemetry source, string helicopterName, FlightMode mode)
        {
            telemetry = source;
            if (telemetry == null)
                Debug.LogError("FlightHUD: на гвинтокрилі немає HelicopterTelemetry", this);

            if (modeText != null) modeText.text = $"{mode.ToString().ToUpper()}  ·  {helicopterName.ToUpper()}";
            if (hintText != null) hintText.text = mode == FlightMode.Tuning ? "TAB  TUNING PANEL     ESC  MENU" : "ESC  MENU";

            if (collectiveFill != null && collectiveFill.parent is RectTransform bar)
                barHeight = bar.rect.height;
        }

        void LateUpdate()
        {
            if (telemetry == null) return;

            // SetText з форматом не створює новий рядок у пам'яті щокадру.
            // У TextMeshPro {0:0} = без знаків після коми, {0:1} = один знак
            speedText?.SetText("{0:0}", telemetry.Speed);
            heightText?.SetText("ASL {0:0}", telemetry.Height);

            if (heightAboveGroundText != null)
            {
                if (telemetry.HeightAboveGround >= 0f) heightAboveGroundText.SetText("GND {0:0}", telemetry.HeightAboveGround);
                else heightAboveGroundText.SetText("GND ---");
            }

            if (verticalSpeedText != null)
            {
                if (telemetry.VerticalSpeed >= 0f) verticalSpeedText.SetText("VS +{0:1}", telemetry.VerticalSpeed);
                else verticalSpeedText.SetText("VS {0:1}", telemetry.VerticalSpeed);
            }

            collectiveText?.SetText("{0:0}%", telemetry.Collective * 100f);
            directionText?.SetText("{0:0}°", telemetry.Direction);
            rollText?.SetText("ROLL {0:0}°", telemetry.Roll);
            pitchText?.SetText("PITCH {0:0}°", telemetry.Pitch);

            if (collectiveFill != null)
                collectiveFill.sizeDelta = new Vector2(collectiveFill.sizeDelta.x, telemetry.Collective * barHeight);
            if (hoverMark != null)
                hoverMark.anchoredPosition = new Vector2(hoverMark.anchoredPosition.x, telemetry.HoverCollective * barHeight);

            // Крен вправо = іконка обертається за годинниковою (в UI це від'ємний кут по Z)
            if (helicopterIcon != null)
                helicopterIcon.localEulerAngles = new Vector3(0f, 0f, -telemetry.Roll);

            float pitchY = Mathf.Clamp(telemetry.Pitch, -pitchLimit, pitchLimit) * pitchPixelsPerDegree;
            if (pitchMarkerLeft != null) pitchMarkerLeft.anchoredPosition = new Vector2(pitchMarkerLeft.anchoredPosition.x, pitchY);
            if (pitchMarkerRight != null) pitchMarkerRight.anchoredPosition = new Vector2(pitchMarkerRight.anchoredPosition.x, pitchY);

            compass?.SetHeading(telemetry.Direction);
        }
    }
}
