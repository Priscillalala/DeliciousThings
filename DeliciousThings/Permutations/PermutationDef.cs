using RoR2.Navigation;
using System.Security.Cryptography;
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

    protected static void EnsureNodesInBounds(NodeGraph nodeGraph, List<NodeGraph.NodeIndex> nodes, Bounds bounds)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (!nodeGraph.GetNodePosition(nodes[i], out Vector3 position) || !bounds.Contains(position))
            {
                nodes.RemoveAt(i);
            }
        }
    }
    #endregion
}