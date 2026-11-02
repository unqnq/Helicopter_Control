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

        [Header("Drag")]
        [SerializeField] private Vector3 dragCoefficients = new Vector3(2.5f, 24f, 1.1f); // x = вбік, y = вертикаль, z = вперед
        [SerializeField] private Vector3 angularDragCoefficients = new Vector3(2000f, 1750f, 550f); // x = pitch, y = yaw, z = roll

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
            ApplyDrag();
            ApplyAngularDrag();
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

        void ApplyDrag()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            //Квадратичний опір повітря F = −k · v · |v|
            Vector3 localDrag = new Vector3( 
                -dragCoefficients.x * localVelocity.x * Mathf.Abs(localVelocity.x),
                -dragCoefficients.y * localVelocity.y * Mathf.Abs(localVelocity.y),
                -dragCoefficients.z * localVelocity.z * Mathf.Abs(localVelocity.z));

            rb.AddRelativeForce(localDrag);
        }

        void ApplyAngularDrag()
        {
            Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);

            // Лінійний кутовий опір: τ = -c · ω
            Vector3 angularDrag = Vector3.Scale(-angularDragCoefficients, localAngularVelocity);

            rb.AddRelativeTorque(angularDrag);
        }
    }
}
