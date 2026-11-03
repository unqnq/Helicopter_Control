using UnityEngine;


namespace HelicopterSim
{
    [RequireComponent(typeof(Rigidbody))]
    public class HelicopterTelemetry : MonoBehaviour
    {
        private Rigidbody rb;
        private IHelicopterInput input;
        private HelicopterController controller;

        [SerializeField] private float groundCheckRange = 1000f; // наскільки далеко вниз шукаємо землю для HeightAboveGround

        public float Speed { get; private set; }
        public float VerticalSpeed { get; private set; }
        public float Pitch { get; private set; }

        public float Roll { get; private set; }
        public float Direction { get; private set; }
        public float HeightAboveGround { get; private set; } //висота під гелікоптером
        public float Height { get; private set; } //відносно поверхні(нуль по координатам)
        public float Collective { get; private set; }
        public float HoverCollective { get; private set; } // положення важеля для зависання

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<IHelicopterInput>();
            controller = GetComponent<HelicopterController>();
            if (controller != null && controller.MaxLiftRatio > 0f)
            {
                HoverCollective = 1f / controller.MaxLiftRatio;
            }
        }

        void LateUpdate()
        {
            Vector3 velocity = rb.linearVelocity;
            Speed = velocity.magnitude * 3.6f; // * 3.6f щоб перевести з м/с в км/год
            VerticalSpeed = velocity.y;
            Direction = transform.eulerAngles.y;

            // forward.y показує, наскільки ніс дивиться вгору, Asin переводить це в кут. Ніс угору дає плюс
            Pitch = Mathf.Asin(Mathf.Clamp(transform.forward.y, -1f, 1f)) * Mathf.Rad2Deg;

            //порівнюємо, наскільки опустився правий борт (right.y) відносно того, наскільки «вертикально» стоїть щогла (up.y). Atan2 дає кут з правильним знаком у всьому діапазоні
            Roll = Mathf.Atan2(-transform.right.y, transform.up.y) * Mathf.Rad2Deg;

            Height = transform.position.y;
            HeightAboveGround = -1f;

            if (controller != null)
            {
                Vector3 origin = transform.position + Vector3.up * 0.5f;
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckRange, controller.GroundMask))
                    HeightAboveGround = Mathf.Max(0f, hit.distance - 0.5f);   // полози можуть трохи "просісти" в землю - без від'ємних значень
            }

            Collective = input != null ? input.Collective : 0f;

            // Рахуємо щокадру: у режимі тюнінгу Max Lift Ratio може змінитися
            if (controller != null && controller.MaxLiftRatio > 0f)
                HoverCollective = 1f / controller.MaxLiftRatio;
        }
    }
}