using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class MoreRuins : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "BlockedByRuinsPath";

    public MoreRuins() : base()
    {
        const string SECTION = "Distant Roost";
        const string NAME = "More Ruins";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "blackbeach";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var blackbeachGroundNodes = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/blackbeach/blackbeachGroundNodes.asset");
        var blackbeachAirNodes = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/blackbeach/blackbeachAirNodes.asset");

        List<NodeGraph.NodeIndex> dest = [];

        yield return blackbeachGroundNodes;
        NodeGraph groundNodegraph = blackbeachGroundNodes.Result;
        groundNodegraph.blockMap.GetItemsInSphere(new Vector3(103, -159, -78), 5f, dest);
        AssignNodesToGate(groundNodegraph, dest, GATE_NAME);
        dest.Clear();

        yield return blackbeachAirNodes;
        NodeGraph airNodegraph = blackbeachAirNodes.Result;
        airNodegraph.blockMap.GetItemsInSphere(new Vector3(123, -135, -75), 10f, dest);
        AssignNodesToGate(airNodegraph, dest, GATE_NAME);
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("GAMEPLAY SPACE", out GameObject gameplaySpace))
        {
            return;
        }
        if (!gameplaySpace.transform.TryFind("BbRuinGate_LOD0 (2)", out Transform BbRuinGate))
        {
            return;
        }
        GameObject moreRuins = new GameObject("ToggleableRuins");
        moreRuins.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        moreRuins.SetActive(false);
        moreRuins.transform.SetParent(gameplaySpace.transform, false);
        GameObject ruinGate = Object.Instantiate(BbRuinGate.gameObject, moreRuins.transform);
        ruinGate.transform.localPosition = new Vector3(130f, -135f, -74.5f);

        Addressable("RoR2/Base/blackbeach/BbRuinStep1_LOD0.fbx", out Mesh BbRuinStep1Mesh);
        if (gameplaySpace.transform.TryFind("BbRuinStep1_LOD0 (1)", out Transform BbRuinStep1))
        {
            GameObject step1 = Object.Instantiate(BbRuinStep1.gameObject, moreRuins.transform);
            step1.GetComponent<MeshFilter>().mesh = BbRuinStep1Mesh;
            step1.transform.localPosition = new Vector3(96f, -155.5f, -64f);
            step1.transform.localEulerAngles = new Vector3(270, 240, 0);

            GameObject step2 = Object.Instantiate(BbRuinStep1.gameObject, moreRuins.transform);
            step2.GetComponent<MeshFilter>().mesh = BbRuinStep1Mesh;
            step2.transform.localPosition = new Vector3(108f, -151.5f, -63f);
            step2.transform.localEulerAngles = new Vector3(270, 255, 0);
        }

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [moreRuins],
            minEnabled = 0,
            maxEnabled = 1,
        });
    }
}
