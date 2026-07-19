using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;

public class LightingSetup
{
    public static string Main()
    {
        // --- Volume profile with sky + exposure ---
        const string profilePath = "Assets/Settings/GameSkyProfile.asset";
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        VisualEnvironment visualEnv;
        if (!profile.TryGet(out visualEnv)) visualEnv = profile.Add<VisualEnvironment>(true);
        int skyId = 0;
        try
        {
            foreach (var attr in typeof(PhysicallyBasedSky).GetCustomAttributes(false))
            {
                var at = attr.GetType();
                if (!at.Name.Contains("SkyUniqueID")) continue;
                foreach (var f in at.GetFields())
                    if (f.FieldType == typeof(int)) { skyId = (int)f.GetValue(attr); break; }
                foreach (var p in at.GetProperties())
                    if (p.PropertyType == typeof(int)) { skyId = (int)p.GetValue(attr); break; }
            }
        }
        catch { }
        if (skyId != 0)
        {
            visualEnv.skyType.overrideState = true;
            visualEnv.skyType.value = skyId;
        }

        if (!profile.Has<PhysicallyBasedSky>()) profile.Add<PhysicallyBasedSky>(true);

        Exposure exp;
        if (!profile.TryGet(out exp)) exp = profile.Add<Exposure>(true);
        exp.mode.overrideState = true;
        exp.mode.value = ExposureMode.Fixed;
        exp.fixedExposure.overrideState = true;
        exp.fixedExposure.value = 15f;

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        // --- Global volume in the scene ---
        var volGo = GameObject.Find("Sky and Fog Volume");
        if (volGo == null) volGo = new GameObject("Sky and Fog Volume");
        var vol = volGo.GetComponent<Volume>();
        if (vol == null) vol = volGo.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.sharedProfile = profile;

        // --- Sun ---
        string sunInfo = "no sun";
        var sun = GameObject.Find("Directional Light");
        if (sun != null)
        {
            var light = sun.GetComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            light.intensity = 100000f;   // lux, midday sun
            light.shadows = LightShadows.Soft;
            var hd = sun.GetComponent<HDAdditionalLightData>();
            if (hd == null) hd = sun.AddComponent<HDAdditionalLightData>();
            hd.EnableShadows(true);
            sunInfo = "sun set intensity=" + light.intensity;
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        return "ok skyId=" + skyId + " " + sunInfo;
    }
}
