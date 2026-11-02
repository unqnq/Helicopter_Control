using UnityEngine;

namespace HelicopterSim
{
    [RequireComponent(typeof(Rigidbody))]
    public class HelicopterController : MonoBehaviour
    {
        private Rigidbody rb;
        private IHelicopterInput input;
        [SerializeField] private float collectiveAuthority = 0.5f;

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
        }

        void ApplyLift()
        {
            float hoverLift = rb.mass * Physics.gravity.magnitude;
            float lift = hoverLift * (1 + input.Collective * collectiveAuthority);
            rb.AddForce(transform.up * lift);
        }
    }
}
