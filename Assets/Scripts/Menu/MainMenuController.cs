using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HelicopterSim
{
    public class MainMenuController : MonoBehaviour
    {
        private const string TuningPrefKey = "menu_tuningMode";

        [SerializeField] private HelicopterCardSpawner spawner;
        [SerializeField] private Button flyButton;
        [SerializeField] private Toggle tuningToggle;
        [SerializeField] private string flightSceneName = "FlightTest";

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            flyButton.interactable = FlightSession.SelectedHelicopter != null;
            flyButton.onClick.AddListener(Fly);
            spawner.HelicopterSelected += OnHelicopterSelected;

            if (tuningToggle != null)
            {
                tuningToggle.isOn = PlayerPrefs.GetInt(TuningPrefKey, 0) == 1;
                tuningToggle.onValueChanged.AddListener(OnTuningToggled);
            }
        }

        void OnDestroy()
        {
            if (spawner != null) spawner.HelicopterSelected -= OnHelicopterSelected;
        }

        private void OnHelicopterSelected(GameObject prefab) => flyButton.interactable = true;

        private void OnTuningToggled(bool isOn) => PlayerPrefs.SetInt(TuningPrefKey, isOn ? 1 : 0);

        private void Fly()
        {
            if (FlightSession.SelectedHelicopter == null) return;
            FlightSession.Mode = tuningToggle != null && tuningToggle.isOn ? FlightMode.Tuning : FlightMode.Flight;
            SceneManager.LoadScene(flightSceneName);
        }

        public void Quit() => Application.Quit();
    }
}