using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

public class ReadingJson
{
    // Start is called before the first frame update
    public Dictionary<string,NodeObject> readJson(string path)
    {
        Dictionary<string, NodeObject> nodesDictionary = new Dictionary<string, NodeObject>();
        
        // Read the JSON file content as a string
        string jsonContent = File.ReadAllText(path);

        // Deserialize JSON to a JObject for easier manipulation
        var data = JsonConvert.DeserializeObject<JObject>(jsonContent);

        // Check if "nodes" exists
        if (data.ContainsKey("nodes"))
        {
            // Access the "nodes" object
            var nodes = data["nodes"] as JObject;
            if (nodes != null)
            {
                // Iterate over each key-value pair in "nodes"
                foreach (var node in nodes)
                {
                    string nodeId = node.Key; // Get the node ID
                    var nodeData = node.Value as JObject;
                    // Check if "coords" exists in this node
                    if (nodeData != null && nodeData.ContainsKey("coords"))
                    {
                        // Access and print the "coords" array
                        var coords = nodeData["coords"] as JArray;
                        if (coords != null)
                        {
                            nodesDictionary.Add(nodeId, new NodeObject(nodeId, (float)coords[0], (float)coords[1], (float)coords[2]));
                            Debug.Log($"Node {nodeId} Coords: {string.Join(", ", coords)}");
                        }
                    }
                }
            }
        }

        if (data.ContainsKey("edges"))
        {
            var edges = data["edges"] as JObject;
            if (edges != null)
            {
                // Iterate over each key-value pair in "edges"
                foreach (var edge in edges)
                {
                    string edge_id = edge.Key;
                    var start = edge.Value["start"] as JArray;
                    var end = edge.Value["end"] as JArray;
                    string nodeStart = " ";
                    string nodeEnd = " ";
                    foreach (KeyValuePair<string,NodeObject> node in nodesDictionary)
                    {
                        if((float) start[0] ==node.Value.x && (float) start[1] == node.Value.y && (float) start[2] == node.Value.z)
                        {
                            nodeStart = node.Key;
                        } else if ((float) end[0] == node.Value.x && (float) end[1] == node.Value.y && (float) end[2] == node.Value.z)
                        {
                            nodeEnd = node.Key;
                        }
                    }
                    if(!nodeStart.Equals(" ") && !nodeEnd.Equals(" "))
                    {
                        nodesDictionary[nodeStart].edges.Add(nodeEnd);
                    }
                    Debug.Log($"edge {edge_id} from {string.Join(", ", start)} to {string.Join(", ",end)}");
                }
            }
        }
        return nodesDictionary;
    }        
}
