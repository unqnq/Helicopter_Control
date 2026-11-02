using UnityEngine;

namespace HelicopterSim
{
    public class HelicopterInput : MonoBehaviour, IHelicopterInput
    {

        [SerializeField] private float collectiveRate = 0.5f; // швидкість руху важеля: одиниць за секунду (0.5 = повний хід за 2 с)
        public float Collective { get; private set; }
        public Vector2 Cyclic { get; private set; } // y = w/s(pitch), x = a/d(roll)
        public float Yaw { get; private set; }

        private HelicopterControls controls;

        void Awake()
        {
            controls = new HelicopterControls();
        }

        void OnEnable()
        {
            controls.Helicopter.Enable();
        }

        void OnDisable()
        {
            controls.Helicopter.Disable();
        }

        void OnDestroy()
        {
            controls.Dispose();
        }

        void Update()
        {
            float collectiveInput = controls.Helicopter.Collective.ReadValue<float>();
            Collective = Mathf.Clamp01(Collective + collectiveInput * collectiveRate * Time.deltaTime);
            Cyclic = controls.Helicopter.Cyclic.ReadValue<Vector2>();
            Yaw = controls.Helicopter.Yaw.ReadValue<float>();
        }
    }
}
