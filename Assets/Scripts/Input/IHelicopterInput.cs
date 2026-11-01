using UnityEngine;

namespace HelicopterSim
{
    public interface IHelicopterInput
    {
        float Collective { get; }
        Vector2 Cyclic { get; } // y = w/s(pitch), x = a/d(roll)
        float Yaw { get; }
    }
}
