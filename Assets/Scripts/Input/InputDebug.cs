//скрипт для тестування зчитування значень

using UnityEngine;

namespace HelicopterSim
{
    public class InputDebug : MonoBehaviour
    {
        private IHelicopterInput input;

        void Awake()
        {
            input = GetComponent<IHelicopterInput>();
            if (input == null)
                Debug.LogError("InputDebug: на об'єкті немає компонента з IHelicopterInput", this);

        }

        void OnGUI()
        {
            if (input == null) return;

            GUI.Label(new Rect(10, 10, 400, 25), $"Collective: {input.Collective}");
            GUI.Label(new Rect(10, 35, 400, 25), $"Cyclic: {input.Cyclic}");
            GUI.Label(new Rect(10, 65, 400, 25), $"Yaw: {input.Yaw}");
        }
    }
}