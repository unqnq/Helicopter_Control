using UnityEngine;

namespace HelicopterSim
{
    public enum FlightMode { Flight, Tuning }

    // Дані, які передаються з меню у сцену польоту
    // Статичні властивості не знищуються під час завантаження іншої сцени
    public static class FlightSession
    {
        public static GameObject SelectedHelicopter { get; set; }
        public static FlightMode Mode { get; set; } = FlightMode.Flight;
    }
}
