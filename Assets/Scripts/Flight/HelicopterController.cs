using UnityEngine;

namespace HelicopterSim
{
    [RequireComponent(typeof(Rigidbody))]
    public class HelicopterController : MonoBehaviour
    {
        private Rigidbody rb;
        private IHelicopterInput input;

        [Header("Lift")]
        [SerializeField] private float collectiveAuthority = 0.5f;

        [Header("Torque")]
        [SerializeField] float pitchTorque = 2000;
        [SerializeField] float rollTorque = 550;
        [SerializeField] float yawTorque = 1400;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<IHelicopterInput>();

            if (input == null)
            {
                Debug.LogError("HelicopterController: на об'єкті немає компонента з IHelicopterInput", this);
                enabled = false;
            }
        }

        void FixedUpdate()
        {
            ApplyLift();
            ApplyTorque();
        }

        void ApplyLift()
        {
            float hoverLift = rb.mass * Physics.gravity.magnitude;
            float lift = hoverLift * (1 + input.Collective * collectiveAuthority);
            rb.AddForce(transform.up * lift);
        }

        void ApplyTorque()
        {
            Vector3 torque = new Vector3(input.Cyclic.y * pitchTorque,
                                input.Yaw * yawTorque,
                                -input.Cyclic.x * rollTorque);
            rb.AddRelativeTorque(torque);
        }
    }
}
