using IvyLibrary;
using UnityEngine.Rendering.PostProcessing;

namespace DeliciousThings.Weather;

public class Fog : WeatherDef
{
    public Fog() : base()
    {
        Delicious.Logger.LogInfo("Fog awake!");
        enabled = true;
        targetSceneNames = ["golemplains", "golemplains2", "blackbeach", "blackbeach2", "foggyswamp", "sulfurpools", "shipgraveyard", "rootjungle", "skymeadow"];
        weight = 1.0f;
        selectionToken = "DELICIOUSTHINGS_WEATHER_FOG";
        weatherPrefab = Ivyl.CreatePrefab("Weather, Fog");
        weatherPrefab.layer = LayerIndex.postProcess.intVal;
        weatherPrefab.AddComponent<NetworkIdentity>();
        weatherPrefab.AddComponent<GlobalVisionLimiter>().visionDistance = 45f;
        weatherPrefab.AddComponent<IncreaseSpawnDistance>().value = 20f;
        PostProcessProfile ppFoggy = ScriptableObject.CreateInstance<PostProcessProfile>();
        ppFoggy.name = "ppFoggy";
        RampFog rampFog = ppFoggy.AddSettings<RampFog>();
        rampFog.fogIntensity.Override(1f);
        rampFog.fogPower.Override(0.8f);
        rampFog.fogZero.Override(-0.002f);
        rampFog.fogOne.Override(0.02f);
        rampFog.skyboxStrength.Override(0.02f);
        ColorGrading colorGrading = ppFoggy.AddSettings<ColorGrading>();
        //colorGrading.postExposure.Override(-0.1f);
        //colorGrading.contrast.Override(70f);
        colorGrading.temperature.Override(-10f);
        colorGrading.colorFilter.Override(new Color32(95, 99, 74, 255));
        Bloom bloom = ppFoggy.AddSettings<Bloom>();
        bloom.intensity.Override(0.6f);
        bloom.threshold.Override(0f);
        PostProcessVolume volume = weatherPrefab.AddComponent<PostProcessVolume>();
        volume.sharedProfile = ppFoggy;
        volume.isGlobal = true;
        volume.priority = 1000;
        volume.weight = .99f;
    }
}