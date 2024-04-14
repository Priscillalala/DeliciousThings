using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Weather;

public static class WeatherManager
{
    public static Dictionary<string, WeightedSelection<WeatherDef>> weatherSelections;

    public static void SetWeatherDefs(WeatherDef[] weatherDefs)
    {
        weatherSelections = [];
        foreach (WeatherDef weatherDef in weatherDefs)
        {
            if (weatherDef.weight > 0f)
            {
                foreach (string scene in weatherDef.targetSceneNames)
                {
                    Delicious.Logger.LogInfo(scene);
                    if (!weatherSelections.TryGetValue(scene, out var selection))
                    {
                        weatherSelections.Add(scene, selection = new WeightedSelection<WeatherDef>());
                    }
                    selection.AddChoice(weatherDef, weatherDef.weight);
                }
            }
        }

        SceneManager.activeSceneChanged += ActiveSceneChanged;
    }

    private static void ActiveSceneChanged(Scene _, Scene newScene)
    {
        if (NetworkServer.active && Run.instance && weatherSelections.TryGetValue(newScene.name, out var weightedSelection))
        {
            WeatherDef weatherDef = weightedSelection.Evaluate(Run.instance.stageRng.nextNormalizedFloat);
            if (weatherDef != null)
            {
                Delicious.Logger.LogInfo($"Current Weather: {weatherDef.GetType().Name}");
                if (!string.IsNullOrEmpty(weatherDef.selectionToken))
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = weatherDef.selectionToken,
                    });
                }
                if (weatherDef.weatherPrefab)
                {
                    Delicious.Logger.LogInfo($"Has prefab");
                    GameObject weatherInstance = Object.Instantiate(weatherDef.weatherPrefab);
                    NetworkServer.Spawn(weatherInstance);
                }
            } 
        }
    }
}