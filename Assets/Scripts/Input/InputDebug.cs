//скрипт для тестування зчитування значень

using UnityEngine;

namespace HelicopterSim
{
    public class InputDebug : MonoBehaviour
    {
        private IHelicopterInput input;
        private Rigidbody rb;

        [Header("UI")]
        [SerializeField] private int textSize = 26;
        [SerializeField] private Color textColor = Color.white;
        private GUIStyle labelStyle;

        void Awake()
        {
            input = GetComponent<IHelicopterInput>();
            if (input == null)
                Debug.LogError("InputDebug: на об'єкті немає компонента з IHelicopterInput", this);

            rb = GetComponent<Rigidbody>();
        }

        void OnGUI()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = textSize;
                labelStyle.normal.textColor = textColor;
            }

            if (input == null) return;
            if (rb == null) return;
            GUILayout.BeginArea(new Rect(20, 20, 700, 500));

            GUILayout.Label($"Collective: {input.Collective * 100f:F0}% (hover ≈ 63%)", labelStyle);
            // GUILayout.Label($"Cyclic: {input.Cyclic}", labelStyle);
            // GUILayout.Label($"Yaw: {input.Yaw}", labelStyle);
            GUILayout.Space(10);

            Vector3 v = rb.linearVelocity;
            float horizontalSpeed = new Vector3(v.x, 0f, v.z).magnitude;


            GUILayout.Label($"Speed: {v.magnitude:F1} m/s ({v.magnitude * 3.6f:F0} km/h)", labelStyle);
            GUILayout.Label($"Vertical: {v.y:F1} m/s", labelStyle);
            GUILayout.Label($"Horizontal: {horizontalSpeed:F1} m/s", labelStyle);
            GUILayout.Label($"Height: {transform.position.y:F1} m", labelStyle);

            GUILayout.EndArea();
        }
    }
}