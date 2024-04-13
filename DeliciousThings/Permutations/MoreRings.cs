using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class MoreRings : PermutationDef
{
    public MoreRings() : base()
    {
        const string SECTION = "Wetland Aspect";
        const string NAME = "More Ruin Rings";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "foggyswamp";
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("HOLDER: Tree Trunks w Collision", out GameObject treeTrunksHolder))
        {
            return;
        }
        if (!treeTrunksHolder.transform.TryFind("FSTreeTrunkLongCollision (1)/FSRuinRingCollision", out Transform FSRuinRingCollision))
        {
            return;
        }
        GameObject ruinRing1 = Object.Instantiate(FSRuinRingCollision.gameObject, new Vector3(-5, 0, 26), Quaternion.Euler(5, 0, 90));
        ruinRing1.SetActive(false);
        ruinRing1.transform.localScale = Vector3.one * 22f;

        GameObject ruinRing2 = Object.Instantiate(FSRuinRingCollision.gameObject, new Vector3(-39, 65, -82), Quaternion.Euler(0, 0, 90));
        ruinRing2.SetActive(false);
        ruinRing2.transform.localScale = Vector3.one * 22f;

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [ruinRing1, ruinRing2],
            minEnabled = 0,
            maxEnabled = 2,
        });
    }
}
