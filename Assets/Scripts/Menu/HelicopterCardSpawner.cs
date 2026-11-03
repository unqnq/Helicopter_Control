using System;
using System.Collections.Generic;
using UnityEngine;

namespace HelicopterSim
{
    // Створює картки для всіх гвинтокрилів зі списку, ставить їхні моделі в сцену меню
    // й запам'ятовує вибір у FlightSession
    public class HelicopterCardSpawner : MonoBehaviour
    {
        // Один гвинтокрил у меню: префаб (для польоту) + де поставити його модель у меню
        [Serializable]
        private class MenuSlot
        {
            public GameObject prefab;
            public Vector3 position;
            public Vector3 rotation;   // кути Ейлера, як у Transform

            public MenuSlot(Vector3 position, Vector3 rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }
        }

        [SerializeField] private HelicopterCard cardPrefab;
        [SerializeField] private Transform container;

        public event Action<GameObject> HelicopterSelected;
        private HelicopterCard selectedCard;

        // Координати взяті з моделей, які стояли в сцені MainMenu (порядок: Scout, Utility, Transport)
        [SerializeField]
        private MenuSlot[] slots =
        {
            new MenuSlot(new Vector3(-22.4f, 0.2f, 458.9f), new Vector3(0f, 100f, 0f)),
            new MenuSlot(new Vector3(4.1f, 0.2f, 456.8f), new Vector3(0f, 100f, 0f)),
            new MenuSlot(new Vector3(28.8f, 0.2f, 456.8f), new Vector3(0f, 100f, 0f)),
        };

        private readonly List<HelicopterCard> cards = new List<HelicopterCard>();
        private readonly List<GameObject> menuModels = new List<GameObject>();

        void Start()
        {
            if (cardPrefab == null)
            {
                Debug.LogError("HelicopterCardSpawner: не задано префаб картки", this);
                return;
            }

            Transform parent = container != null ? container : transform;

            foreach (MenuSlot slot in slots)
            {
                if (slot.prefab == null) continue;

                GameObject model = SpawnMenuModel(slot);
                menuModels.Add(model);

                HelicopterCard card = Instantiate(cardPrefab, parent);
                card.name = "Card_" + slot.prefab.name;
                card.Setup(slot.prefab, model);

                // Повернулись з польоту в меню - підсвічуємо гвинтокрил, обраний минулого разу
                if (FlightSession.SelectedHelicopter == slot.prefab)
                    OnHelicopterChosen(card);
                card.Chosen += OnHelicopterChosen;
                cards.Add(card);
            }
        }

        // Модель у меню - лише для показу: без фізики, вводу й HUD
        private GameObject SpawnMenuModel(MenuSlot slot)
        {
            GameObject model = Instantiate(slot.prefab, slot.position, Quaternion.Euler(slot.rotation));
            model.name = slot.prefab.name + "_Menu";

            // Вимикаємо всі скрипти (контролер, ввід, HUD, RotorVisual)
            foreach (MonoBehaviour behaviour in model.GetComponents<MonoBehaviour>())
                behaviour.enabled = false;

            // Фізика не рухає модель
            Rigidbody rb = model.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            return model;
        }

        void OnDestroy()
        {
            foreach (HelicopterCard card in cards)
                if (card != null) card.Chosen -= OnHelicopterChosen;
        }

        private void OnHelicopterChosen(HelicopterCard card)
        {
            if (selectedCard != null) selectedCard.SetSelected(false);
            selectedCard = card;
            card.SetSelected(true);

            FlightSession.SelectedHelicopter = card.Prefab;
            HelicopterSelected?.Invoke(card.Prefab);
        }
    }
}
