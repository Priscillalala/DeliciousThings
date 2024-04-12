using RoR2.Navigation;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DeliciousThings.Permutations;

public static class PermutationManager
{
    public static ILookup<string, PermutationDef> stagePermutationsLookup;

    public static void SetPermutationDefs(PermutationDef[] permutationDefs)
    {
        stagePermutationsLookup = permutationDefs.ToLookup(x => x.targetSceneName);

        SceneManager.activeSceneChanged += ActiveSceneChanged;

        /*SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            
        };*/

        On.RoR2.Navigation.NodeGraph.GenerateLinkDebugMesh += NodeGraph_GenerateLinkDebugMesh;
    }

    private static void ActiveSceneChanged(Scene _, Scene newScene)
    {
        if (stagePermutationsLookup.Contains(newScene.name))
        {
            Dictionary<string, GameObject> rootObjects = [];
            foreach (GameObject rootObject in newScene.GetRootGameObjects())
            {
                rootObjects[rootObject.name] = rootObject;
            }
            foreach (PermutationDef permutation in stagePermutationsLookup[newScene.name])
            {
                SceneObjectToggleGroup toggleGroupController = permutation.FindToggleGroupController(newScene, rootObjects);
                permutation.Apply(newScene, rootObjects, toggleGroupController);
            }
        }
    }

    private static Mesh NodeGraph_GenerateLinkDebugMesh(On.RoR2.Navigation.NodeGraph.orig_GenerateLinkDebugMesh orig, NodeGraph self, HullMask hullMask)
    {
        using WireMeshBuilder wireMeshBuilder = new WireMeshBuilder();
        NodeGraph.Link[] array = self.links;
        for (int i = 0; i < array.Length; i++)
        {
            NodeGraph.Link link = array[i];
            if (((uint)link.hullMask & (uint)hullMask) == 0)
            {
                continue;
            }
            Vector3 position = self.nodes[link.nodeIndexA.nodeIndex].position;
            Vector3 position2 = self.nodes[link.nodeIndexB.nodeIndex].position;
            Vector3 val = (position + position2) * 0.5f;
            bool jump = ((uint)link.jumpHullMask & (uint)hullMask) != 0;
            Color color = jump ? Color.cyan : Color.green;
            if (link.gateIndex != 0)
            {
                color = self.openGates[link.gateIndex] ? (jump ? Color.gray : Color.blue) : (jump ? Color.magenta : Color.red);
            }
            if (jump)
            {
                Vector3 apexPos = val;
                apexPos.y = position.y + link.minJumpHeight;
                int num2 = 8;
                Vector3 p = position;
                for (int j = 1; j <= num2; j++)
                {
                    if (j > num2 / 2)
                    {
                        color.a = 0.1f;
                    }
                    Vector3 quadraticCoordinates = self.GetQuadraticCoordinates((float)j / (float)num2, position, apexPos, position2);
                    wireMeshBuilder.AddLine(p, color, quadraticCoordinates, color);
                    p = quadraticCoordinates;
                }
            }
            else
            {
                Color c = color;
                c.a = 0.1f;
                wireMeshBuilder.AddLine(position, color, (position + position2) * 0.5f, c);
            }
        }
        return wireMeshBuilder.GenerateMesh();
    }
}