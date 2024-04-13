using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class LowerStaircase : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "LowerSpiralStaircase";

    public LowerStaircase() : base()
    {
        const string SECTION = "Scorched Acres";
        const string NAME = "Lower Spiral Staircase";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "wispgraveyard";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var wispgraveyardGroundNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/wispgraveyard/wispgraveyardGroundNodesNodegraph.asset");
        var wispgraveyardAirNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/wispgraveyard/wispgraveyardAirNodesNodegraph.asset");

        List<NodeGraph.NodeIndex> dest = [];
        Bounds[] outerBounds =
        [
            new Bounds
            {
                min = new Vector3(-91, 42, 186),
                max = new Vector3(-63, 75, 261),
            },
            new Bounds
            {
                min = new Vector3(126, 31, 190),
                max = new Vector3(158, 75, 291),
            },
        ];
        Bounds[] groundBounds =
        [
            new Bounds
            {
                min = new Vector3(-128, 10, 208),
                max = new Vector3(158, 46, 340),
            },
            .. outerBounds
        ];
        Bounds[] airBounds =
        [
            new Bounds
            {
                min = new Vector3(-128, 20, 208),
                max = new Vector3(158, 56, 340),
            },
             .. outerBounds
        ];

        yield return wispgraveyardGroundNodesNodegraph;
        NodeGraph groundNodegraph = wispgraveyardGroundNodesNodegraph.Result;
        foreach (Bounds b in groundBounds)
        {
            groundNodegraph.blockMap.GetItemsInBounds(b, dest);
            EnsureNodesInBounds(groundNodegraph, dest, b);
            AssignNodesToGate(groundNodegraph, dest, GATE_NAME);
            dest.Clear();
        }

        yield return wispgraveyardAirNodesNodegraph;
        NodeGraph airNodegraph = wispgraveyardAirNodesNodegraph.Result;
        foreach (Bounds b in airBounds)
        {
            airNodegraph.blockMap.GetItemsInBounds(b, dest);
            EnsureNodesInBounds(airNodegraph, dest, b);
            AssignNodesToGate(airNodegraph, dest, GATE_NAME);
            dest.Clear();
        }
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("ENTIRE SCENE HOLDER", out GameObject entireSceneHolder))
        {
            return;
        }
        if (!entireSceneHolder.transform.TryFind("HOLDER: Temple Pieces", out Transform templePiecesHolder))
        {
            return;
        }
        Transform WPPlatform2 = templePiecesHolder.GetChild(3);
        if (!WPPlatform2)
        {
            return;
        }
        GameObject disabled = new GameObject("FSGiantRuinFrameCollisionDisabled");
        disabled.transform.position = new Vector3(12, 61, 230);
        disabled.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        disabled.SetActive(false);
        disabled.AddComponent(out SetSceneObjectsActive setSceneObjectsActive);
        setSceneObjectsActive.objectsToDeactivate.Add(WPPlatform2.Find("WPPlatform2SpiralStairsCW")?.gameObject);
        setSceneObjectsActive.objectsToDeactivate.Add(WPPlatform2.Find("WPPlatform2SpiralStairsCCW")?.gameObject);
        setSceneObjectsActive.objectsToDeactivate.Add(WPPlatform2.Find("WPPlanter1Long (4)")?.gameObject);
        setSceneObjectsActive.objectsToDeactivate.Add(WPPlatform2.Find("WPPlanter1Long (3)")?.gameObject);
        setSceneObjectsActive.objectsToDeactivate.Add(WPPlatform2.Find("WPPlanter1KnockedOver")?.gameObject);
        setSceneObjectsActive.objectsToDeactivate.Add(WPPlatform2.Find("WPPlanter1LongKnockedOver")?.gameObject);
        setSceneObjectsActive.objectsToDeactivate.Add(WPPlatform2.Find("WPPlanter1WithBonsaiTree (9)")?.gameObject);
        Transform barrier1 = WPPlatform2.Cast<Transform>().Where(x => x.name == "WPPlatform2BarrierHigh").ElementAtOrDefault(2);
        if (barrier1)
        {
            setSceneObjectsActive.objectsToDeactivate.Add(barrier1.gameObject);
        }
        Transform barrier2 = WPPlatform2.Cast<Transform>().Where(x => x.name == "WPPlatform2BarrierHigh (1)").ElementAtOrDefault(2);
        if (barrier2)
        {
            setSceneObjectsActive.objectsToDeactivate.Add(barrier2.gameObject);
        }

        Addressable("RoR2/Base/wispgraveyard/WPPlatform2BarrierHigh.prefab", out GameObject WPPlatform2BarrierHigh);
        GameObject barrier1Disabled = Object.Instantiate(WPPlatform2BarrierHigh, disabled.transform);
        barrier1Disabled.transform.localPosition = new Vector3(-8.7f, -36.5f, -11f);
        barrier1Disabled.transform.localEulerAngles = new Vector3(270, 267, 0);
        GameObject barrier2Disabled = Object.Instantiate(WPPlatform2BarrierHigh, disabled.transform);
        barrier2Disabled.transform.localPosition = new Vector3(-8f, -36.5f, -5f);
        barrier2Disabled.transform.localEulerAngles = new Vector3(270, 280, 0);
        GameObject planterWithTreeDisabled = Object.Instantiate(Addressable<GameObject>("RoR2/Base/wispgraveyard/WPPlanter1WithBonsaiTree.prefab"), disabled.transform);
        planterWithTreeDisabled.transform.localPosition = new Vector3(-75f, -14.4f, -26f);
        planterWithTreeDisabled.transform.localEulerAngles = new Vector3(0, 200, 0);
        GameObject planterLongKnockedOverDisabled = Object.Instantiate(Addressable<GameObject>("RoR2/Base/wispgraveyard/WPPlanter1LongKnockedOver.prefab"), disabled.transform);
        planterLongKnockedOverDisabled.transform.localPosition = new Vector3(-76f, -14.9f, -32f);
        planterLongKnockedOverDisabled.transform.localEulerAngles = new Vector3(0, 25, 0);

        GameObject occlusionColling = new GameObject("DisableOcclusionCulling");
        occlusionColling.transform.SetParent(disabled.transform);
        occlusionColling.transform.localPosition = new Vector3(0, 0, 0);
        occlusionColling.AddComponent<DisableOcclusionNearby>().radius = 120;

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [disabled],
            minEnabled = 0,
            maxEnabled = 1,
        });
    }
}
