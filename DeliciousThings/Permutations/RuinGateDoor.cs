using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class RuinGateDoor : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "BlockedByRuinGateDoor";

    public RuinGateDoor() : base()
    {
        const string SECTION = "Distant Roost";
        const string NAME = "Ruin Gate Door";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "blackbeach2";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var blackbeach2GroundNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/blackbeach2/blackbeach2GroundNodesNodegraph.asset");
        var blackbeach2AirNodesNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/blackbeach2/blackbeach2AirNodesNodegraph.asset");

        List<NodeGraph.NodeIndex> dest = [];

        yield return blackbeach2GroundNodesNodegraph;
        NodeGraph groundNodegraph = blackbeach2GroundNodesNodegraph.Result;
        groundNodegraph.blockMap.GetItemsInSphere(new Vector3(-8, 33, -156), 10f, dest);
        AssignNodesToGate(groundNodegraph, dest, GATE_NAME);
        dest.Clear();

        yield return blackbeach2AirNodesNodegraph;
        NodeGraph airNodegraph = blackbeach2AirNodesNodegraph.Result;
        airNodegraph.blockMap.GetItemsInSphere(new Vector3(-5, 44, -163), 15f, dest);
        AssignNodesToGate(airNodegraph, dest, GATE_NAME);
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        if (!rootObjects.TryGetValue("HOLDER: Ruins", out GameObject ruinsHolder))
        {
            return;
        }
        if (!ruinsHolder.transform.TryFind("GROUP: Gates", out Transform gatesGroup))
        {
            return;
        }
        GameObject ruinGateDoor = new GameObject("RuinGateDoor");
        ruinGateDoor.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        ruinGateDoor.SetActive(false);
        ruinGateDoor.transform.parent = gatesGroup.Find("BbRuinArch_LOD0");
        ruinGateDoor.transform.localPosition = Vector3.zero;
        ruinGateDoor.transform.localEulerAngles = Vector3.zero;
        ruinGateDoor.transform.localScale = new Vector3(0.09f, 0.17f, 0.17f);
        GameObject door1 = Object.Instantiate(gatesGroup.Find("DownstairsGate, Open/BbRuinGateDoor1_LOD0").gameObject, ruinGateDoor.transform);
        door1.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        door1.transform.localEulerAngles = Vector3.zero;
        door1.transform.localScale = door1.transform.localScale with { z = -door1.transform.localScale.z };
        GameObject door2 = Object.Instantiate(gatesGroup.Find("DownstairsGate, Open/BbRuinGateDoor2_LOD0").gameObject, ruinGateDoor.transform);
        door2.transform.localPosition = new Vector3(0f, -0.75f, 0f);
        door2.transform.localEulerAngles = Vector3.zero;
        door2.transform.localScale = door2.transform.localScale with { z = -door2.transform.localScale.z };

        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [ruinGateDoor],
            minEnabled = 0,
            maxEnabled = 1,
        });
    }
}
