using UnityEngine;

namespace HelicopterSim
{
    public class RotorVisual : MonoBehaviour
    {
        [Header("Rotors")]
        [SerializeField] private Transform mainRotor;
        [SerializeField] private Transform tailRotor;

        [Header("Speed (°/s)")]
        [SerializeField] private float idleSpeed = 360f;
        [SerializeField] private float maxSpeed = 1260f;
        [SerializeField] private float tailSpeedMultiplier = 2f;
        [SerializeField] private float spinUpRate = 500f;

        [Header("Direction")]
        [SerializeField] private bool clockwise = false;

        private IHelicopterInput input;
        private float currentSpeed;

        void Awake()
        {
            input = GetComponent<IHelicopterInput>();
            if (input == null)
            {
                Debug.LogError("RotorVisual: на об'єкті немає компонента з IHelicopterInput", this);
                enabled = false;
                return;
            }

            currentSpeed = idleSpeed;
        }

        void Update()
        {
            float targetSpeed = Mathf.Lerp(idleSpeed, maxSpeed, input.Collective);

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, spinUpRate * Time.deltaTime);

            float direction = clockwise ? 1f : -1f;

            if (mainRotor != null)
                mainRotor.Rotate(0f, direction * currentSpeed * Time.deltaTime, 0f, Space.Self);

            if (tailRotor != null)
                tailRotor.Rotate(currentSpeed * tailSpeedMultiplier * Time.deltaTime, 0f, 0f, Space.Self);
        }
    }
}
