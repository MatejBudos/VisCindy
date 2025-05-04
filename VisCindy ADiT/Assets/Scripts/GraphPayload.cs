using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;


// Helper class because Unity’s JsonUtility doesn't handle Dictionary serialization directly
[System.Serializable]
public class Wrapper
{
    public List<NodeEdgePair> nodes = new List<NodeEdgePair>();

    public Wrapper(Dictionary<string, List<string>> dict)
    {
        foreach (var kvp in dict)
        {
            nodes.Add(new NodeEdgePair { node_id = kvp.Key, edges = kvp.Value });
        }
    }
}

[System.Serializable]
public class NodeEdgePair
{
    public string node_id;
    public List<string> edges;
}

