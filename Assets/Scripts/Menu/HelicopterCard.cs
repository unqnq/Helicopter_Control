using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HelicopterSim
{
    public class HelicopterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text massText;
        [SerializeField] private TMP_Text rotorText;
        [SerializeField] private Button chooseButton;
        [SerializeField] private Image background;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(255f, 255f, 255f, 0.5f);
        [SerializeField] private Color selectedColor = new Color(0.55f, 1f, 0.42f, 0.35f);

        public event Action<HelicopterCard> Chosen;
        public GameObject Prefab { get; private set; }

        private RotorVisual rotor;
        private bool isSelected;

        public void Setup(GameObject prefab, GameObject menuModel)
        {
            Prefab = prefab;
            rotor = menuModel != null ? menuModel.GetComponent<RotorVisual>() : null;

            float mass = HelicopterMetrics.GetMass(prefab);
            float bladeLength = HelicopterMetrics.GetBladeLength(prefab);
            nameText.text = prefab.name;
            massText.text = $"Mass: {mass:F0} kg";
            rotorText.text = $"Blade length: {bladeLength:F1} m";

            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => Chosen?.Invoke(this));

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (background != null) background.color = selected ? selectedColor : normalColor;
            SetRotorSpinning(selected);
        }

        public void OnPointerEnter(PointerEventData eventData) => SetRotorSpinning(true);

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected) SetRotorSpinning(false);
        }

        private void SetRotorSpinning(bool spinning)
        {
            if (rotor != null) rotor.enabled = spinning;
        }

        private void Reset()
        {
            nameText = FindChild<TMP_Text>("NameText");
            massText = FindChild<TMP_Text>("MassText");
            rotorText = FindChild<TMP_Text>("RotorText");
            background = FindChild<Image>("Panel");
            chooseButton = GetComponentInChildren<Button>(true);
        }

        private T FindChild<T>(string childName) where T : Component
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }
    }
}