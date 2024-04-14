using IvyLibrary;
using UnityEngine.Rendering.PostProcessing;

namespace DeliciousThings.Weather;

public class FogWarm : WeatherDef
{
    public FogWarm() : base()
    {
        enabled = true;
        targetSceneNames = ["ancientloft", "wispgraveyard"];
        weight = 1.0f;
        selectionToken = "DELICIOUSTHINGS_WEATHER_FOG";
        weatherPrefab = Ivyl.CreatePrefab("Weather, Fog Warm");
        weatherPrefab.layer = LayerIndex.postProcess.intVal;
        weatherPrefab.AddComponent<NetworkIdentity>();
        weatherPrefab.AddComponent<GlobalVisionLimiter>().visionDistance = 45f;
        weatherPrefab.AddComponent<IncreaseSpawnDistance>().value = 20f;
        PostProcessProfile ppFoggyWarm = ScriptableObject.CreateInstance<PostProcessProfile>();
        ppFoggyWarm.name = "ppFoggyWarm";
        RampFog rampFog = ppFoggyWarm.AddSettings<RampFog>();
        rampFog.fogIntensity.Override(1f);
        rampFog.fogPower.Override(0.8f);
        rampFog.fogZero.Override(0.002f);
        rampFog.fogOne.Override(0.02f);
        rampFog.skyboxStrength.Override(0.05f);
        ColorGrading colorGrading = ppFoggyWarm.AddSettings<ColorGrading>();
        colorGrading.postExposure.Override(-0.1f);
        colorGrading.contrast.Override(100f);
        colorGrading.temperature.Override(80f);
        colorGrading.colorFilter.Override(new Color32(255, 83, 143, 255));
        Bloom bloom = ppFoggyWarm.AddSettings<Bloom>();
        bloom.intensity.Override(2f);
        bloom.threshold.Override(0f);
        PostProcessVolume volume = weatherPrefab.AddComponent<PostProcessVolume>();
        volume.sharedProfile = ppFoggyWarm;
        volume.isGlobal = true;
        volume.priority = 1000;
    }
}