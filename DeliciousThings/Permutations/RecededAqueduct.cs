using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class RecededAqueduct : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "BlockedByEel";

    public RecededAqueduct() : base()
    {
        const string SECTION = "Abandoned Aqueduct";
        const string NAME = "Receded Aqueduct";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "goolake";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var goolakeAirNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/goolake/goolakeAirNodegraph.asset");

        yield return goolakeAirNodegraph;
        NodeGraph airNodegraph = goolakeAirNodegraph.Result;
        List<NodeGraph.NodeIndex> dest = [];
        airNodegraph.blockMap.GetItemsInSphere(new Vector3(151, -102, 98), 10f, dest);
        AssignNodesToGate(airNodegraph, dest, GATE_NAME);
        airNodegraph.SetGateState(GATE_NAME, true);
    }

    public GameObject goolakeBoulderPrefab;

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!goolakeBoulderPrefab)
        {
            goolakeBoulderPrefab = Ivyl.ClonePrefab(Addressable<GameObject>("RoR2/Base/golemplains2/BBBoulderMediumRound1.prefab"), "GlBoulderApproximation");
            goolakeBoulderPrefab.GetComponent<MeshRenderer>().sharedMaterial = Addressable<Material>("RoR2/Base/goolake/matGoolakeRocks.mat");
        }
        if (!rootObjects.TryGetValue("HOLDER: GameplaySpace", out GameObject gameplaySpaceHolder))
        {
            return;
        }
        if (!gameplaySpaceHolder.transform.TryFind("Terrain/mdlGlDam/mdlGlAqueductPartial", out Transform mdlGlAqueductPartial))
        {
            return;
        }
        Transform mdlGlAqueductReceded = Object.Instantiate(mdlGlAqueductPartial.gameObject, mdlGlAqueductPartial.parent).transform;
        mdlGlAqueductReceded.localPosition = new Vector3(-0.89f, 1.2f, -0.53f);
        mdlGlAqueductReceded.localEulerAngles = new Vector3(0f, 2f, 0f);
        mdlGlAqueductReceded.localScale = new Vector3(1.225f, 1.263564f, 0.9f);
        mdlGlAqueductReceded.gameObject.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        mdlGlAqueductReceded.gameObject.SetActive(false);
        mdlGlAqueductReceded.gameObject.AddComponent(out SetSceneObjectsActive setSceneObjectsActive);
        if (mdlGlAqueductReceded.TryFind("GooWaterfall", out Transform GooWaterfall))
        {
            GooWaterfall.gameObject.SetActive(false);
        }
        if (rootObjects.TryGetValue("HOLDER: Misc Props", out GameObject miscPropsHolder))
        {
            if (miscPropsHolder.transform.TryFind("Props", out Transform props))
            {
                Transform eelSkeleton = props.AllChildren().FirstOrDefault(x => !x.gameObject.activeSelf && x.gameObject.name == "EelSkeleton");
                if (eelSkeleton)
                {
                    Transform archEelSkeleton = Object.Instantiate(eelSkeleton.gameObject, eelSkeleton.parent).transform;
                    archEelSkeleton.localPosition = new Vector3(19.4f, -27f, -78f);
                    archEelSkeleton.localEulerAngles = new Vector3(53f, 78f, 339f);
                    archEelSkeleton.localScale = Vector3.one * 14.4f;
                    archEelSkeleton.gameObject.SetActive(false);
                    setSceneObjectsActive.objectsToActivate.Add(archEelSkeleton.gameObject);

                    Transform eelSkeletonTail = Object.Instantiate(eelSkeleton.gameObject, eelSkeleton.parent).transform;
                    eelSkeletonTail.localPosition = new Vector3(-79f, -87f, -128f);
                    eelSkeletonTail.localEulerAngles = new Vector3(25f, 226f, 173f);
                    eelSkeletonTail.localScale = Vector3.one * 9f;
                    eelSkeletonTail.gameObject.SetActive(false);
                    setSceneObjectsActive.objectsToActivate.Add(eelSkeletonTail.gameObject);
                }
            }
        }
        Addressable("RoR2/Base/goolake/spmGlBamboo1Large.spm", out GameObject spmGlBamboo1Large);
        Addressable("RoR2/Base/goolake/spmGlBamboo1.spm", out GameObject spmGlBamboo1);
        Addressable("RoR2/Base/goolake/spmGlCactusBush1.spm", out GameObject spmGlCactusBush1);
        GameObject[] propInstances =
        [
            Object.Instantiate(goolakeBoulderPrefab, new Vector3(171, -115, 96), Quaternion.Euler(340f, 0f, 0f)),
            Object.Instantiate(goolakeBoulderPrefab, new Vector3(155, -112, 117), Quaternion.Euler(340f, 40f, 0f)),
            Object.Instantiate(spmGlBamboo1Large, new Vector3(163, -114, 127), Quaternion.Euler(5f, 20f, 0f)),
            Object.Instantiate(spmGlBamboo1, new Vector3(169, -114, 124), Quaternion.Euler(0f, 0f, 350f)),
            Object.Instantiate(spmGlBamboo1, new Vector3(180, -117, 107), Quaternion.Euler(5f, 30f, 340f)),
            Object.Instantiate(spmGlCactusBush1, new Vector3(177, -114, 116), Quaternion.Euler(0f, 0f, 330f)),
            Object.Instantiate(spmGlCactusBush1, new Vector3(180.5f, -116, 112), Quaternion.Euler(30f, 80f, 5f)),
        ];
        propInstances[0].transform.localScale = Vector3.one * 5.5f;
        propInstances[1].transform.localScale = Vector3.one * 2.5f;
        propInstances[2].transform.localScale = Vector3.one * 1.5f;
        propInstances[3].transform.localScale = Vector3.one * 1.5f;
        propInstances[4].transform.localScale = new Vector3(1.2f, 1.6f, 1.2f);
        propInstances[5].transform.localScale = Vector3.one * 1.5f;
        propInstances[6].transform.localScale = Vector3.one * 0.8f;
        foreach (GameObject prop in propInstances)
        {
            prop.gameObject.SetActive(false);
        }
        setSceneObjectsActive.objectsToActivate.AddRange(propInstances);

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects =
            [
                mdlGlAqueductPartial.gameObject,
                mdlGlAqueductReceded.gameObject,
            ],
            minEnabled = 1,
            maxEnabled = 1,
        });
    }
}
