using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class RootBridge : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "RootBridge";

    public RootBridge() : base()
    {
        const string SECTION = "Sundered Grove";
        const string NAME = "Root Bridge";
        enabled = false;//Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "rootjungle";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var rootjungleGroundNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/rootjungle/rootjungleGroundNodesNodegraph.asset");
        var rootjungleAirNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/rootjungle/rootjungleAirNodesNodegraph.asset");

        List<NodeGraph.NodeIndex> dest = [];

        yield return rootjungleGroundNodesNodegraph;
        NodeGraph groundNodegraph = rootjungleGroundNodesNodegraph.Result;
        groundNodegraph.blockMap.GetItemsInSphere(new Vector3(63, 30, 90), 30f, dest);
        AssignNodesToGate(groundNodegraph, dest, GATE_NAME);
        dest.Clear();

        yield return rootjungleAirNodesNodegraph;
        NodeGraph airNodegraph = rootjungleAirNodesNodegraph.Result;
        airNodegraph.blockMap.GetItemsInSphere(new Vector3(63, 30, 90), 30f, dest);
        AssignNodesToGate(airNodegraph, dest, GATE_NAME);
    }

    public override SceneObjectToggleGroup FindToggleGroupController(Scene scene, IDictionary<string, GameObject> rootObjects)
    {
        if (rootObjects.TryGetValue("SceneInfo", out GameObject sceneInfo))
        {
            return sceneInfo.transform.Find("SceneObjectToggleGroup")?.GetComponent<SceneObjectToggleGroup>();
        }
        return null;
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJTriangle.prefab"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJTwistedTreeBig.prefab"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJTriangleRoot.prefab"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJRoot1.prefab"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJRoot2.prefab"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJRoot3.prefab"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJRoot4.prefab"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJRootBridgeRandom.fbx"));
        Object.Instantiate(Addressable<GameObject>("RoR2/Base/rootjungle/RJTwistedTree.fbx"));
        return;
        if (!rootObjects.TryGetValue("HOLDER: Props", out GameObject propsHolder))
        {
            return;
        }
        if (!propsHolder.transform.TryFind("GROUP: Mushrooms/RJShroomBig", out Transform RJShroomBig))
        {
            return;
        }
        GameObject disabled = new GameObject("RJShroomBigDisabled");
        disabled.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        disabled.SetActive(false);
        if (rootObjects.TryGetValue("HOLDER: Foliage", out GameObject foliageHolder) && foliageHolder.transform.TryFind("GROUP: ShroomFoliage", out Transform shroomFoliageGroup))
        {
            disabled.AddComponent(out SetSceneObjectsActive setSceneObjectsActive);

            foreach (Transform shroomFoliage in shroomFoliageGroup)
            {
                if ((shroomFoliage.position - new Vector3(63, 30, 90)).sqrMagnitude <= 1600f)
                {
                    setSceneObjectsActive.objectsToDeactivate.Add(shroomFoliage.gameObject);
                }
            }
        }

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [RJShroomBig.gameObject, disabled],
            minEnabled = 1,
            maxEnabled = 1,
        });
    }
}
