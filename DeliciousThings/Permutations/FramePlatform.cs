using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class FramePlatform : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "RuinFramePlatform";

    public FramePlatform() : base()
    {
        const string SECTION = "Wetland Aspect";
        const string NAME = "Outset Frame Platform";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "foggyswamp";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var foggyswampGroundNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/foggyswamp/foggyswampGroundNodesNodegraph.asset");
        var foggyswampAirNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/foggyswamp/foggyswampAirNodesNodegraph.asset");

        List<NodeGraph.NodeIndex> dest = [];
        Bounds[] bounds =
        [
            new Bounds
            {
                min = new Vector3(84, -155, -341),
                max = new Vector3(149, -111, -301),
            },
            new Bounds
            {
                min = new Vector3(128, -142, -340),
                max = new Vector3(209, -89, -284),
            },
        ];

        yield return foggyswampGroundNodesNodegraph;
        NodeGraph groundNodegraph = foggyswampGroundNodesNodegraph.Result;
        foreach (Bounds b in bounds)
        {
            groundNodegraph.blockMap.GetItemsInBounds(b, dest);
            EnsureNodesInBounds(groundNodegraph, dest, b);
            AssignNodesToGate(groundNodegraph, dest, GATE_NAME);
            dest.Clear();
        }

        yield return foggyswampAirNodesNodegraph;
        NodeGraph airNodegraph = foggyswampAirNodesNodegraph.Result;
        foreach (Bounds b in bounds)
        {
            airNodegraph.blockMap.GetItemsInBounds(b, dest);
            EnsureNodesInBounds(airNodegraph, dest, b);
            AssignNodesToGate(airNodegraph, dest, GATE_NAME);
            dest.Clear();
        }
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("HOLDER: Ruin Pieces", out GameObject ruinPiecesHolder))
        {
            return;
        }
        if (!ruinPiecesHolder.transform.TryFind("FSGiantRuinFrameCollision", out Transform FSGiantRuinFrameCollision))
        {
            return;
        }
        GameObject disabled = new GameObject("FSGiantRuinFrameCollisionDisabled");
        disabled.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        disabled.SetActive(false);
        if (rootObjects.TryGetValue("HOLDER: Foliage", out GameObject foliageHolder))
        {
            disabled.AddComponent(out SetSceneObjectsActive setSceneObjectsActive);

            Bounds[] bounds =
            [
                new Bounds
                {
                    min = new Vector3(84, -150, -341),
                    max = new Vector3(149, -111, -301),
                },
                new Bounds
                {
                    min = new Vector3(128, -152, -340),
                    max = new Vector3(209, -89, -284),
                },
            ];

            foreach (Transform foliageCategory in foliageHolder.transform)
            {
                foreach (Transform foliage in foliageCategory)
                {
                    foreach (Bounds b in bounds)
                    {
                        if (b.Contains(foliage.position))
                        {
                            setSceneObjectsActive.objectsToDeactivate.Add(foliage.gameObject);
                            break;
                        }
                    }
                }
            }
        }

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [FSGiantRuinFrameCollision.gameObject, disabled],
            minEnabled = 1,
            maxEnabled = 1,
        });
    }
}
