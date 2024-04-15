using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class IslandPlatform : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "IslandPlatform";

    public IslandPlatform() : base()
    {
        const string SECTION = "Aphelion Sanctuary";
        const string NAME = "Island Platform";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "ancientloft";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var ancientloft_GroundNodeGraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/DLC1/ancientloft/ancientloft_GroundNodeGraph.asset");
        var ancientloft_AirNodeGraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/DLC1/ancientloft/ancientloft_AirNodeGraph.asset");

        List<NodeGraph.NodeIndex> dest = [];
        Bounds[] bounds =
        [
            new Bounds
            {
                min = new Vector3(174, 14, -107),
                max = new Vector3(284, 105, 150),
            },
            new Bounds
            {
                min = new Vector3(136, 7, 31),
                max = new Vector3(218, 56, 110),
            },
        ];

        yield return ancientloft_GroundNodeGraph;
        NodeGraph groundNodegraph = ancientloft_GroundNodeGraph.Result;
        foreach (Bounds b in bounds)
        {
            groundNodegraph.blockMap.GetItemsInBounds(b, dest);
            AssignNodesToGate(groundNodegraph, dest, GATE_NAME);
            dest.Clear();
        }

        yield return ancientloft_AirNodeGraph;
        NodeGraph airNodegraph = ancientloft_AirNodeGraph.Result;
        foreach (Bounds b in bounds)
        {
            airNodegraph.blockMap.GetItemsInBounds(b, dest);
            AssignNodesToGate(airNodegraph, dest, GATE_NAME);
            dest.Clear();
        }
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("HOLDER: Terrain", out GameObject terrainHolder))
        {
            return;
        }
        if (!terrainHolder.transform.TryFind("TerrainPlatform (1)", out Transform TerrainPlatform))
        {
            return;
        }
        GameObject disabled = new GameObject("TerrainPlatformDisabled");
        disabled.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        disabled.SetActive(false);
        disabled.AddComponent(out SetSceneObjectsActive setSceneObjectsActive);
        setSceneObjectsActive.objectsToDeactivate.Add(terrainHolder.transform.Find("mdlAncientLoft_FBridge (2)")?.gameObject);

        GameObject bridgeEdge = new GameObject("Edging the bridge");
        bridgeEdge.transform.SetParent(disabled.transform, false);

        Addressable("RoR2/Base/wispgraveyard/WPPlanter2WithBonsaiTree.prefab", out GameObject WPPlanter2WithBonsaiTree);
        GameObject bonsai1 = Object.Instantiate(WPPlanter2WithBonsaiTree, disabled.transform);
        bonsai1.transform.localPosition = new Vector3(182f, 18.8f, -15f);
        bonsai1.transform.eulerAngles = new Vector3(0, 160, 0);
        GameObject bonsai2 = Object.Instantiate(WPPlanter2WithBonsaiTree, disabled.transform);
        bonsai2.transform.localPosition = new Vector3(182f, 18.8f, -42f);
        bonsai2.transform.eulerAngles = new Vector3(0, 213, 0);

        GameObject pillar = Object.Instantiate(Addressable<GameObject>("RoR2/DLC1/ancientloft/AncientLoft_PillarHalfIntactSimpler.prefab"), disabled.transform);
        pillar.transform.localPosition = new Vector3(195, -10, 39);

        GameObject largeFountain = Object.Instantiate(Addressable<GameObject>("RoR2/DLC1/ancientloft/AncientLoft_FountainLG.prefab"), disabled.transform);
        largeFountain.transform.localPosition = new Vector3(172f, 20.34f, -28.3f);
        largeFountain.transform.eulerAngles = new Vector3(0, 270, 0);

        Addressable("RoR2/DLC1/ancientloft/AncientLoft_SculptureSM.prefab", out GameObject AncientLoft_SculptureSM);
        GameObject sculpture1 = Object.Instantiate(AncientLoft_SculptureSM, disabled.transform);
        sculpture1.transform.localPosition = new Vector3(173, 22, -36);
        GameObject sculpture2 = Object.Instantiate(AncientLoft_SculptureSM, disabled.transform);
        sculpture2.transform.localPosition = new Vector3(173.5f, 22f, -21f);

        GameObject spinningRocks = Object.Instantiate(Addressable<GameObject>("RoR2/DLC1/ancientloft/AncientLoft_SpnningRocks.prefab"), disabled.transform);
        spinningRocks.transform.localPosition = new Vector3(190, 25, -45);
        spinningRocks.transform.eulerAngles = new Vector3(0, 40, 0);

        Bounds[] bounds =
        [
            new Bounds
            {
                min = new Vector3(157, 14, -107),
                max = new Vector3(284, 105, 150),
            },
            new Bounds
            {
                min = new Vector3(134, 6, 4),
                max = new Vector3(218, 56, 110),
            },
        ];

        if (rootObjects.TryGetValue("HOLDER: Ruins", out GameObject ruinsHolder))
        {
            Object.Instantiate(ruinsHolder.transform.Find("AncientLoft_Rubbles/Rubble_One").gameObject, bridgeEdge.transform, true);
            Object.Instantiate(ruinsHolder.transform.Find("AncientLoft_Rubbles/Rubble_Two").gameObject, bridgeEdge.transform, true);

            foreach (Transform ruinsTransform in ruinsHolder.transform)
            {
                foreach (Bounds b in bounds)
                {
                    if (b.Contains(ruinsTransform.position))
                    {
                        setSceneObjectsActive.objectsToDeactivate.Add(ruinsTransform.gameObject);
                    }
                }
            }
        }

        if (rootObjects.TryGetValue("HOLDER: Foliage", out GameObject foliageHolder))
        {
            Addressable("RoR2/Base/blackbeach/mdlBBBoulderMediumRound1.fbx", out Mesh mdlBBBoulderMediumRound1);
            Object.Instantiate(foliageHolder.transform.Find("mdlBBBoulderMediumRound1 (14)").gameObject, bridgeEdge.transform, true)
                .GetComponent<MeshFilter>().sharedMesh = mdlBBBoulderMediumRound1;
            Object.Instantiate(foliageHolder.transform.Find("mdlBBBoulderMediumRound1 (15)").gameObject, bridgeEdge.transform, true)
                .GetComponent<MeshFilter>().sharedMesh = mdlBBBoulderMediumRound1;
            Object.Instantiate(foliageHolder.transform.Find("mdlBBBoulderMediumRound1 (16)").gameObject, bridgeEdge.transform, true)
                .GetComponent<MeshFilter>().sharedMesh = mdlBBBoulderMediumRound1;

            foreach (Transform foilageTransform in foliageHolder.transform)
            {
                foreach (Bounds b in bounds)
                {
                    if (b.Contains(foilageTransform.position))
                    {
                        setSceneObjectsActive.objectsToDeactivate.Add(foilageTransform.gameObject);
                    }
                }
            }
        }

        bridgeEdge.transform.localPosition = new Vector3(262f, -29.5f, -94f);
        bridgeEdge.transform.eulerAngles = new Vector3(0, 90, 0);

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [TerrainPlatform.gameObject, disabled],
            minEnabled = 1,
            maxEnabled = 1,
        });
    }
}