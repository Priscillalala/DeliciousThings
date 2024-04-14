using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class MaulingRocks : PermutationDef
{

    public MaulingRocks() : base()
    {
        const string SECTION = "Sky Meadow";
        const string NAME = "Mauling Rocks Direction";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "skymeadow";
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("HOLDER: Mauling Rocks", out GameObject maulingRocksHolder))
        {
            return;
        }
        GameObject altMaulingRocksHolder = Object.Instantiate(maulingRocksHolder);
        altMaulingRocksHolder.SetActive(false);
        foreach (MaulingRockZoneManager manager in altMaulingRocksHolder.GetComponentsInChildren<MaulingRockZoneManager>())
        {
            (manager.startZonePoint1, manager.endZonePoint1) = (manager.endZonePoint1, manager.startZonePoint1);
            (manager.startZonePoint2, manager.endZonePoint2) = (manager.endZonePoint2, manager.startZonePoint2);
        }
        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [maulingRocksHolder, altMaulingRocksHolder],
            minEnabled = 1,
            maxEnabled = 1,
        });
    }
}
