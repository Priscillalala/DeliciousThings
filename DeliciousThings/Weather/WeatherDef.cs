namespace DeliciousThings.Weather;

public abstract class WeatherDef : Delicious.INetworkedObjectPrefabProvider
{
    public bool enabled;
    public IEnumerable<string> targetSceneNames;
    public float weight;
    public GameObject weatherPrefab;
    public string selectionToken;

    public virtual IEnumerable<GameObject> NetworkedObjectPrefabs => [weatherPrefab];
}