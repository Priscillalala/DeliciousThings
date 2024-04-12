using RoR2.Navigation;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public abstract class PermutationDef
{
    public bool enabled;
    public string targetSceneName;

    public virtual SceneObjectToggleGroup FindToggleGroupController(Scene scene, IDictionary<string, GameObject> rootObjects)
    {
        if (rootObjects.TryGetValue("SceneInfo", out GameObject sceneInfo))
        {
            return sceneInfo.transform.Find("ToggleGroupController")?.GetComponent<SceneObjectToggleGroup>();
        }
        return null;
    }

    public abstract void Apply(Scene scene, IDictionary<string, GameObject> rootObjects, SceneObjectToggleGroup toggleGroupController);

    #region util
    protected static void Addressable<TObject>(object key, out TObject asset) => asset = Addressables.LoadAssetAsync<TObject>(key).WaitForCompletion();

    protected static TObject Addressable<TObject>(object key) => Addressables.LoadAssetAsync<TObject>(key).WaitForCompletion();

    protected static void AssignNodesToGate(NodeGraph nodeGraph, List<NodeGraph.NodeIndex> nodes, string gateName)
    {
        byte gateIndex = nodeGraph.RegisterGateName(gateName);
        foreach (NodeGraph.NodeIndex nodeIndex in nodes)
        {
            nodeGraph.nodes[nodeIndex.nodeIndex].gateIndex = gateIndex;
            foreach (NodeGraph.LinkIndex linkIndex in nodeGraph.GetActiveNodeLinks(nodeIndex))
            {
                nodeGraph.links[linkIndex.linkIndex].gateIndex = gateIndex;
            }
        }
    }
    #endregion
}