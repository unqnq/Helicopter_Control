// One-time tool: converts this project from HDRP to URP (for the Web build).
// Run it from the menu: Tools > Convert HDRP to URP. After it finishes, this file can be deleted.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class HdrpToUrpConverter
{
    const string SettingsFolder = "Assets/Settings/URP";
    const string RendererPath = SettingsFolder + "/URP_Renderer.asset";
    const string PipelinePath = SettingsFolder + "/URP_Asset.asset";

    [MenuItem("Tools/Convert HDRP to URP")]
    public static void Convert()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var log = new List<string>();

        // 1. Pipeline asset + renderer
        var pipeline = CreatePipelineAsset(log);

        // 2. Graphics + every Quality level use the URP asset
        GraphicsSettings.defaultRenderPipeline = pipeline;
        GraphicsSettings.lightsUseLinearIntensity = true;
        GraphicsSettings.lightsUseColorTemperature = true;
        int currentLevel = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipeline;
        }
        QualitySettings.SetQualityLevel(currentLevel, false);
        log.Add("Graphics + " + QualitySettings.names.Length + " quality levels -> URP_Asset");

        // 3. Materials
        ConvertMaterials(log);

        // 4. Prefabs: remove components whose scripts came from HDRP
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            int removed = RemoveMissingScripts(root);
            if (removed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                log.Add("Prefab " + path + ": removed " + removed + " missing HDRP components");
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        // 5. Scenes
        string firstScene = null;
        foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (firstScene == null) firstScene = path;
            ConvertScene(path, log);
        }

        // 6. Remove HDRP-only assets
        string[] toDelete =
        {
            "Assets/Settings/HDRP Balanced.asset",
            "Assets/Settings/HDRP High Fidelity.asset",
            "Assets/Settings/HDRP Performant.asset",
            "Assets/Settings/HDRPDefaultResources",
            "Assets/Settings/SkyandFogSettingsProfile.asset",
            "Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP LIT.shadergraph",
            "Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP UNLIT.shadergraph",
        };
        foreach (var p in toDelete)
            if (AssetDatabase.LoadMainAssetAtPath(p) != null || AssetDatabase.IsValidFolder(p))
                if (AssetDatabase.DeleteAsset(p)) log.Add("Deleted " + p);

        AssetDatabase.SaveAssets();
        Debug.Log("[HDRP->URP] Done:\n- " + string.Join("\n- ", log));
        EditorUtility.DisplayDialog("HDRP -> URP", "Done. See Console for the full list of changes.", "OK");
    }

    static UniversalRenderPipelineAsset CreatePipelineAsset(List<string> log)
    {
        var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder(SettingsFolder))
            AssetDatabase.CreateFolder("Assets/Settings", "URP");

        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(rendererData, RendererPath);

        var pipeline = UniversalRenderPipelineAsset.Create(rendererData);
        pipeline.supportsHDR = true;
        pipeline.msaaSampleCount = 4;
        pipeline.shadowDistance = 200f;
        pipeline.shadowCascadeCount = 4;
        AssetDatabase.CreateAsset(pipeline, PipelinePath);
        AssetDatabase.SaveAssets();
        log.Add("Created " + PipelinePath + " + " + RendererPath);
        return pipeline;
    }

    static void ConvertMaterials(List<string> log)
    {
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("TextMesh Pro")) continue;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            string shaderName = mat.shader != null ? mat.shader.name : "";
            bool isHdrp = shaderName.StartsWith("HDRP/") || shaderName.Contains("InternalErrorShader");
            if (!isHdrp) continue;

            // Read the old values straight from the saved data (works even if the HDRP shader is gone)
            var so = new SerializedObject(mat);
            Color baseColor = GetSavedColor(so, "_BaseColor", Color.white);
            float metallic = GetSavedFloat(so, "_Metallic", 0f);
            float smoothness = GetSavedFloat(so, "_Smoothness", 0.5f);
            Color emissive = GetSavedColor(so, "_EmissiveColor", Color.black);

            mat.shader = urpLit;
            mat.SetFloat("_WorkflowMode", 1f); // Metallic
            mat.SetFloat("_Surface", 0f);      // Opaque
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            if (emissive.maxColorComponent > 0.001f)
            {
                mat.SetColor("_EmissionColor", emissive);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(mat);
            log.Add("Material " + Path.GetFileName(path) + " -> URP/Lit");
        }
    }

    static Color GetSavedColor(SerializedObject so, string name, Color fallback)
    {
        var arr = so.FindProperty("m_SavedProperties.m_Colors");
        for (int i = 0; arr != null && i < arr.arraySize; i++)
        {
            var e = arr.GetArrayElementAtIndex(i);
            if (e.FindPropertyRelative("first").stringValue == name)
                return e.FindPropertyRelative("second").colorValue;
        }
        return fallback;
    }

    static float GetSavedFloat(SerializedObject so, string name, float fallback)
    {
        var arr = so.FindProperty("m_SavedProperties.m_Floats");
        for (int i = 0; arr != null && i < arr.arraySize; i++)
        {
            var e = arr.GetArrayElementAtIndex(i);
            if (e.FindPropertyRelative("first").stringValue == name)
                return e.FindPropertyRelative("second").floatValue;
        }
        return fallback;
    }

    static int RemoveMissingScripts(GameObject root)
    {
        int removed = 0;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        return removed;
    }

    static void ConvertScene(string path, List<string> log)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        int removed = 0;
        Light sun = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            removed += RemoveMissingScripts(root);

            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                // HDRP uses physical units (lux). URP uses simple multipliers (~1).
                if (light.type == LightType.Directional)
                {
                    light.intensity = 1.5f;
                    if (sun == null) sun = light;
                }
                else
                {
                    log.Add("  Check light intensity manually: " + light.name + " in " + path);
                }
            }

            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
            {
                // HDRP camera showed the sky; in URP that is the Skybox clear mode
                cam.clearFlags = CameraClearFlags.Skybox;
                if (cam.GetComponent<UniversalAdditionalCameraData>() == null)
                    cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
        }

        // Environment: HDRP sky came from its default Volume profile. URP uses a skybox material.
        RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.65f, 0.80f);
        RenderSettings.ambientEquatorColor = new Color(0.42f, 0.45f, 0.50f);
        RenderSettings.ambientGroundColor = new Color(0.22f, 0.20f, 0.18f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.Add("Scene " + path + ": removed " + removed + " HDRP components, sun=1.5, skybox + ambient set");
    }
}
