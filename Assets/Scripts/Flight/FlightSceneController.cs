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
