using UnityEngine;

namespace HelicopterSim
{
    // Характеристики гвинтокрила, які рахуються з його моделі (працює і з префабом, і з об'єктом у сцені)
    public static class HelicopterMetrics
    {
        private const string MainRotorName = "MainRotor";

        // Довжина лопасті = відстань від осі ротора до кінчика лопасті (радіус ротора), у метрах
        // Рахується з мешів усередині MainRotor, тож не залежить від того, як побудовані лопасті
        public static float GetBladeLength(GameObject helicopter)
        {
            Transform rotor = FindDeep(helicopter.transform, MainRotorName);
            if (rotor == null) return 0f;

            float maxRadius = 0f;
            foreach (MeshFilter meshFilter in rotor.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh == null) continue;

                // 8 кутів bounding box меша -> у систему координат ротора -> відстань від осі в площині XZ
                Bounds bounds = mesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 sign = new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f);
                    Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, sign);
                    Vector3 inRotor = rotor.InverseTransformPoint(meshFilter.transform.TransformPoint(corner));
                    maxRadius = Mathf.Max(maxRadius, new Vector2(inRotor.x, inRotor.z).magnitude);
                }
            }
            return maxRadius;
        }

        public static float GetMass(GameObject helicopter)
        {
            Rigidbody rb = helicopter.GetComponent<Rigidbody>();
            return rb != null ? rb.mass : 0f;
        }

        // Пошук дочірнього об'єкта за назвою на будь-якій глибині ієрархії
        private static Transform FindDeep(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName) return child;
                Transform found = FindDeep(child, childName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
