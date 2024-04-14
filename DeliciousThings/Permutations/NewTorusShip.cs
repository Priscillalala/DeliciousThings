using HG;
using IvyLibrary;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public class NewTorusShip : PermutationDef, Delicious.IStaticContent
{
    const string GATE_NAME = "BlockedByTorusShip";

    public NewTorusShip() : base()
    {
        const string SECTION = "Siren`s Call";
        const string NAME = "New Torus Ship";
        enabled = Delicious.PermutationsConfig.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, NAME), true).Value;
        targetSceneName = "shipgraveyard";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var shipgraveyardGroundNodeNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/shipgraveyard/shipgraveyardGroundNodeNodegraph.asset");
        var shipgraveyardAirNodeNodegraph = Addressables.LoadAssetAsync<NodeGraph>("RoR2/Base/shipgraveyard/shipgraveyardAirNodeNodegraph.asset");
        
        List<NodeGraph.NodeIndex> dest = [];

        yield return shipgraveyardGroundNodeNodegraph;
        NodeGraph groundNodegraph = shipgraveyardGroundNodeNodegraph.Result;
        groundNodegraph.blockMap.GetItemsInSphere(new Vector3(-15, 3, 14), 10, dest);
        AssignNodesToGate(groundNodegraph, dest, GATE_NAME);
        dest.Clear();

        yield return shipgraveyardAirNodeNodegraph;
        NodeGraph airNodegraph = shipgraveyardAirNodeNodegraph.Result;
        airNodegraph.blockMap.GetItemsInSphere(new Vector3(-15, 15, 11), 15, dest);
        airNodegraph.blockMap.GetItemsInSphere(new Vector3(18, 15, 59), 15, dest);
        AssignNodesToGate(airNodegraph, dest, GATE_NAME);
    }

    public override void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController)
    {
        GameObject TorusShipRound = Object.Instantiate(Addressable<GameObject>("RoR2/Base/shipgraveyard/TorusShip Round.prefab"));
        TorusShipRound.AddComponent<GateStateSetter>().gateToDisableWhenEnabled = GATE_NAME;
        TorusShipRound.SetActive(false);
        TorusShipRound.transform.localPosition = new Vector3(2, 5, 41);
        TorusShipRound.transform.localEulerAngles = new Vector3(340, 300, 230);
        if (rootObjects.TryGetValue("HOLDER: Environment", out GameObject environmentHolder) && environmentHolder.transform.TryFind("TO BE CONVERTED!!!!!!!!!!/HOLDER: Main Rocks", out Transform mainRocks))
        {
            TorusShipRound.AddComponent(out SetSceneObjectsActive setSceneObjectsActive);
            setSceneObjectsActive.objectsToDeactivate.Add(mainRocks.GetChild(27).gameObject);
        }
        ArrayUtils.ArrayAppend(ref toggleGroupController.toggleGroups, new GameObjectToggleGroup
        {
            objects = [TorusShipRound],
            minEnabled = 0,
            maxEnabled = 1,
        });
    }
}
