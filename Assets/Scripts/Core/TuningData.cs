using System;
using UnityEngine;

namespace HelicopterSim
{
    // Параметри фізики, які можна налаштовувати в режимі тюнінгу й зберігати між запусками
    [Serializable]
    public class TuningData
    {
        public float maxLiftRatio;
        public float verticalDamping;
        public float pitchTorque;
        public float rollTorque;
        public float yawTorque;
        public Vector3 drag;          // x = вбік, y = вертикаль, z = вперед
        public Vector3 angularDrag;   // x = pitch, y = yaw, z = roll

        public TuningData Clone() => (TuningData)MemberwiseClone();
    }
}
