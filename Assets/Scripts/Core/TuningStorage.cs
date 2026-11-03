using UnityEngine;

namespace HelicopterSim
{
    // Збереження тюнінгу в PlayerPrefs: один JSON-рядок на кожен гвинтокрил
    public static class TuningStorage
    {
        private const string KeyPrefix = "tuning_";

        public static void Save(string helicopterId, TuningData data)
        {
            PlayerPrefs.SetString(KeyPrefix + helicopterId, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        // Збережені значення накладаються поверх заводських: якщо в TuningData колись
        // з'явиться нове поле, у старому збереженні його немає - лишиться заводське значення
        public static bool TryLoad(string helicopterId, TuningData defaults, out TuningData result)
        {
            result = null;
            string key = KeyPrefix + helicopterId;
            if (!PlayerPrefs.HasKey(key)) return false;

            result = defaults.Clone();
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), result);
            return true;
        }

        public static void Delete(string helicopterId)
        {
            PlayerPrefs.DeleteKey(KeyPrefix + helicopterId);
            PlayerPrefs.Save();
        }
    }
}
