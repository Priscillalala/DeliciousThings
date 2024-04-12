using HG;
using IvyLibrary;
using RoR2.Navigation;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class SecondAqueduct : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "SecondaryAqueduct";

    public SecondAqueduct() : base()
    {
        const string SECTION = "Abandoned Aqueduct";
        const string NAME = "Second Aqueduct";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "goolake";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var goolakeGroundNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/goolake/goolakeGroundNodegraph.asset");
        var goolakeAirNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/goolake/goolakeAirNodegraph.asset");

        yield return goolakeGroundNodegraph;
        NodeGraph groundNodegraph = goolakeGroundNodegraph.Result;
        List<NodeGraph.NodeIndex> dest = [];
        Bounds bounds = new Bounds
        {
            min = new Vector3(210, -95, -260),
            max = new Vector3(230, -30, -224),
        };
        groundNodegraph.blockMap.GetItemsInBounds(bounds, dest);
        AssignNodesToGate(groundNodegraph, dest, GATE_NAME);

        yield return goolakeAirNodegraph;
        NodeGraph airNodegraph = goolakeAirNodegraph.Result;
        dest.Clear();
        airNodegraph.blockMap.GetItemsInBounds(bounds, dest);
        AssignNodesToGate(airNodegraph, dest, GATE_NAME);
    }


    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("HOLDER: GameplaySpace", out GameObject gameplaySpaceHolder))
        {
            return;
        }
        if (!gameplaySpaceHolder.transform.TryFind("Terrain/mdlGlDam/mdlGlAqueductPartial", out Transform mdlGlAqueductPartial))
        {
            return;
        }
        GameObject toggleableAqueduct = new GameObject("SecondaryAqueductToggle");
        toggleableAqueduct.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        toggleableAqueduct.SetActive(false);
        toggleableAqueduct.AddComponent(out SetSceneObjectsActive setSceneObjectsActive);
        toggleableAqueduct.transform.localEulerAngles = new Vector3(0, 18.7f, 180f);
        Transform mdlGlAqueductPartial2 = Object.Instantiate(mdlGlAqueductPartial.gameObject, toggleableAqueduct.transform).transform;
        mdlGlAqueductPartial2.localPosition = new Vector3(-264.7861f, 94.7f, -158.4718f);
        mdlGlAqueductPartial2.localScale = new Vector3(10f, 14.29f, 8f);
        if (mdlGlAqueductPartial2.TryFind("GooWaterfall/FoamOverParticles", out Transform FoamOverParticles))
        {
            FoamOverParticles.localScale = new Vector3(0.8f, 1f, 0.5f);
        }
        if (mdlGlAqueductPartial2.TryFind("GooWaterfall/SplashZone, Directional", out Transform SplashZoneDirectional))
        {
            SplashZoneDirectional.localPosition = SplashZoneDirectional.localPosition with { y = 2.5f };
        }
        if (rootObjects.TryGetValue("HOLDER: Warning Flags", out GameObject warningFlagsHolder))
        {
            setSceneObjectsActive.objectsToDeactivate.Add(warningFlagsHolder.transform.Find("GLWarningFlag")?.gameObject);
            setSceneObjectsActive.objectsToDeactivate.Add(warningFlagsHolder.transform.Find("GLWarningFlag (2)")?.gameObject);
        }
        if (gameplaySpaceHolder.transform.TryFind("Gates/BridgeOverGooOff", out Transform BridgeOverGooOff))
        {
            setSceneObjectsActive.objectsToActivate.Add(BridgeOverGooOff.gameObject);
        }
        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups[0].objects, toggleableAqueduct);
    }
}
