using UnityEngine;

namespace HelicopterSim
{
    public class HelicopterInput : MonoBehaviour, IHelicopterInput
    {
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
            Collective = controls.Helicopter.Collective.ReadValue<float>();
            Cyclic = controls.Helicopter.Cyclic.ReadValue<Vector2>();
            Yaw = controls.Helicopter.Yaw.ReadValue<float>();
        }
    }
}
