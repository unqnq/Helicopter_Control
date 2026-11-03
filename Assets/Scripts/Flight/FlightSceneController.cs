using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

namespace HelicopterSim
{
    public class FlightSceneController : MonoBehaviour
    {
        [SerializeField] private GameObject defaultPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private string menuSceneName = "MainMenu";
        [SerializeField] private CinemachineCamera flightCamera;
        [SerializeField] private FlightHUD hud;
        [SerializeField] private TuningPanel tuningPanel;
        [SerializeField] private FlightMode defaultMode = FlightMode.Tuning;   // якщо сцену запущено напряму, без меню

        public GameObject Helicopter { get; private set; }

        void Awake()
        {
            GameObject prefab = FlightSession.SelectedHelicopter != null ? FlightSession.SelectedHelicopter : defaultPrefab;

            if (prefab == null)
            {
                Debug.LogError("FlightSceneController: немає префаба гвинтокрила", this);
                return;
            }

            Transform point = spawnPoint != null ? spawnPoint : transform;
            Helicopter = Instantiate(prefab, point.position, point.rotation);
            Helicopter.name = prefab.name;

            if (flightCamera != null)
            {
                flightCamera.Follow = Helicopter.transform;
                flightCamera.LookAt = Helicopter.transform;

                // Відстань залежить від розміру ротора: від Transport камера далі, ніж від Scout
                CinemachineFollow follow = flightCamera.GetComponent<CinemachineFollow>();
                float bladeLength = HelicopterMetrics.GetBladeLength(Helicopter);
                if (follow != null && bladeLength > 0f)
                    follow.FollowOffset = new Vector3(0f, bladeLength * 0.8f + 2f, -(bladeLength * 3f + 4f));
            }

            // Збережений тюнінг (якщо є) застосовується в обох режимах
            HelicopterController controller = Helicopter.GetComponent<HelicopterController>();
            HelicopterController prefabController = prefab.GetComponent<HelicopterController>();
            TuningData factoryTuning = prefabController != null ? prefabController.GetTuning() : null;
            if (controller != null && factoryTuning != null && TuningStorage.TryLoad(prefab.name, factoryTuning, out TuningData saved))
                controller.ApplyTuning(saved);

            FlightMode mode = FlightSession.SelectedHelicopter != null ? FlightSession.Mode : defaultMode;

            if (hud != null)
                hud.Bind(Helicopter.GetComponent<HelicopterTelemetry>(), prefab.name, mode);

            if (tuningPanel != null)
            {
                if (mode == FlightMode.Tuning && controller != null && factoryTuning != null)
                    tuningPanel.Bind(controller, prefab.name, factoryTuning);
                else
                    tuningPanel.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(menuSceneName);
            }
        }
    }
}
