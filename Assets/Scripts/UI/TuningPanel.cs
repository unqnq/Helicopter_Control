using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HelicopterSim
{
    // Панель тюнінгу: слайдери для Lift, Torque, Drag. Зміни діють одразу, Save зберігає в PlayerPrefs.
    // Компонент стоїть на Canvas, а не на самій панелі: так Tab працює й тоді, коли панель схована.
    public class TuningPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private RectTransform rowsContainer;
        [SerializeField] private TuningSliderRow rowTemplate;    // вимкнений шаблон рядка
        [SerializeField] private TMP_Text headerTemplate;        // вимкнений шаблон заголовка групи
        [SerializeField] private Button saveButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private float rangeMultiplier = 3f;    // слайдер: від 0 до заводське значення x 3

        private HelicopterController controller;
        private string helicopterId;
        private TuningData defaults;
        private TuningData current;
        private float statusHideTime;

        private readonly List<(TuningSliderRow row, Func<TuningData, float> get)> rows = new List<(TuningSliderRow, Func<TuningData, float>)>();

        public void Bind(HelicopterController target, string id, TuningData factoryDefaults)
        {
            controller = target;
            helicopterId = id;
            defaults = factoryDefaults.Clone();
            current = controller.GetTuning();   // поточні значення (можуть бути вже завантажені збережені)

            if (titleText != null) titleText.text = "TUNING  ·  " + id.ToUpper();
            if (statusText != null) statusText.text = "";

            BuildRows();

            saveButton.onClick.AddListener(Save);
            resetButton.onClick.AddListener(ResetToDefaults);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            panelRoot.SetActive(true);
        }

        private void BuildRows()
        {
            AddHeader("LIFT");
            AddRow("Max Lift Ratio", d => d.maxLiftRatio, (d, v) => d.maxLiftRatio = v, 1f, Mathf.Max(2.5f, defaults.maxLiftRatio * 1.5f));
            AddRow("Vertical Damping", d => d.verticalDamping, (d, v) => d.verticalDamping = v);

            AddHeader("TORQUE");
            AddRow("Pitch", d => d.pitchTorque, (d, v) => d.pitchTorque = v);
            AddRow("Roll", d => d.rollTorque, (d, v) => d.rollTorque = v);
            AddRow("Yaw", d => d.yawTorque, (d, v) => d.yawTorque = v);

            AddHeader("DRAG");
            AddRow("Side (X)", d => d.drag.x, (d, v) => d.drag.x = v);
            AddRow("Vertical (Y)", d => d.drag.y, (d, v) => d.drag.y = v);
            AddRow("Forward (Z)", d => d.drag.z, (d, v) => d.drag.z = v);

            AddHeader("ANGULAR DRAG");
            AddRow("Pitch (X)", d => d.angularDrag.x, (d, v) => d.angularDrag.x = v);
            AddRow("Yaw (Y)", d => d.angularDrag.y, (d, v) => d.angularDrag.y = v);
            AddRow("Roll (Z)", d => d.angularDrag.z, (d, v) => d.angularDrag.z = v);
        }

        private void AddHeader(string title)
        {
            TMP_Text header = Instantiate(headerTemplate, rowsContainer);
            header.gameObject.SetActive(true);
            header.text = title;
        }

        // get / set - як прочитати й як записати цей параметр у TuningData
        private void AddRow(string title, Func<TuningData, float> get, Action<TuningData, float> set, float min = 0f, float max = -1f)
        {
            if (max < 0f) max = Mathf.Max(get(defaults) * rangeMultiplier, 1f);

            TuningSliderRow row = Instantiate(rowTemplate, rowsContainer);
            row.gameObject.SetActive(true);
            row.Setup(title, min, max, get(current), value =>
            {
                set(current, value);
                controller.ApplyTuning(current);   // зміна діє одразу, прямо в польоті
            });
            rows.Add((row, get));
        }

        private void Save()
        {
            TuningStorage.Save(helicopterId, current);
            ShowStatus("Saved");
            ClearSelection();
        }

        private void ResetToDefaults()
        {
            current = defaults.Clone();
            controller.ApplyTuning(current);
            TuningStorage.Delete(helicopterId);

            foreach (var (row, get) in rows)
                row.SetValue(get(current));

            ShowStatus("Reset to default");
            ClearSelection();
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
                panelRoot.SetActive(!panelRoot.activeSelf);

            // Слайдер у фокусі реагує на A/D (крутить значення). Тому, щойно кнопку миші відпущено,
            // знімаємо фокус - і клавіатура знову керує тільки гвинтокрилом
            if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
                ClearSelection();

            if (statusText != null && statusText.text.Length > 0 && Time.unscaledTime > statusHideTime)
                statusText.text = "";
        }

        private void ShowStatus(string message)
        {
            if (statusText == null) return;
            statusText.text = message;
            statusHideTime = Time.unscaledTime + 1.5f;
        }

        private static void ClearSelection()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
