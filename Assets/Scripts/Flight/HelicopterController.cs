using UnityEngine;

namespace HelicopterSim
{
    [RequireComponent(typeof(Rigidbody))]
    public class HelicopterController : MonoBehaviour
    {
        private Rigidbody rb;
        private IHelicopterInput input;

        [Header("Lift")]
        [SerializeField] private float maxLiftRatio = 1.6f;
        [SerializeField] private float verticalDamping = 800;

        [Header("Torque")]
        [SerializeField] private float pitchTorque = 2000;
        [SerializeField] private float rollTorque = 550;
        [SerializeField] private float yawTorque = 1400;

        [Header("Drag")]
        [SerializeField] private Vector3 dragCoefficients = new Vector3(2.5f, 24f, 1.1f); // x = вбік, y = вертикаль, z = вперед
        [SerializeField] private Vector3 angularDragCoefficients = new Vector3(2000f, 1750f, 550f); // x = pitch, y = yaw, z = roll

        [Header("Ground")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.4f;
        public bool IsGrounded { get; private set; }

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
            CheckGround();
            ApplyLift();
            ApplyTorque();
            ApplyDrag();
            ApplyAngularDrag();
        }

        void CheckGround()
        {
            Vector3 originPoint = transform.position + transform.up * 0.3f;
            IsGrounded = Physics.Raycast(originPoint, Vector3.down, groundCheckDistance + 0.3f, groundMask);
            Debug.DrawRay(originPoint, Vector3.down * (groundCheckDistance + 0.3f), IsGrounded ? Color.green : Color.red);
        }

        void ApplyLift()
        {
            float hoverLift = rb.mass * Physics.gravity.magnitude;
            float lift = hoverLift * input.Collective * maxLiftRatio;
            lift -= verticalDamping * Vector3.Dot(rb.linearVelocity, transform.up);
            lift = Mathf.Max(0f, lift);
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
