namespace DeliciousThings;

public class IncreaseSpawnDistance : MonoBehaviour
{
    public float value;

    public void OnEnable()
    {
        On.RoR2.DirectorCore.GetMonsterSpawnDistance += DirectorCore_GetMonsterSpawnDistance;
    }

    public void OnDisable()
    {
        On.RoR2.DirectorCore.GetMonsterSpawnDistance -= DirectorCore_GetMonsterSpawnDistance;
    }

    private void DirectorCore_GetMonsterSpawnDistance(On.RoR2.DirectorCore.orig_GetMonsterSpawnDistance orig, DirectorCore.MonsterSpawnDistance input, out float minimumDistance, out float maximumDistance)
    {
        orig(input, out minimumDistance, out maximumDistance);
        maximumDistance += value;
        maximumDistance += value;
    }
}
