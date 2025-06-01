using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ReadingJson
{

    public static bool inTolerance(JArray edge, float[] values)
    {
        for (int i = 0; i < 3; i++)
        {
            if (Math.Abs((float)edge?[i] - values[i]) > Tolerance)
            {
                return false;
            }
        }
        return true;
    }
    
    private const double Tolerance = 0.1;
    public static Dictionary<string,NodeObject> ReadJson(string jsonContent)
    {        
        Dictionary<string, NodeObject> nodesDictionary = new Dictionary<string, NodeObject>();

        var data = JsonConvert.DeserializeObject<JObject>(jsonContent);
        Debug.Log(data);  
        if (data["nodes"] is JObject nodes)
        {
            foreach (var node in nodes)
            {                
                if (node.Value is JObject nodeData && nodeData.ContainsKey("coords"))
                {
                    string nodeId = nodeData["NeoId"].ToString();
                    if (nodeData["coords"] is JArray coords)
                    {
                        nodesDictionary.Add(nodeId, 
                            new NodeObject(nodeId, (float)coords[0], (float)coords[1], (float)coords[2]));
                    }
                }
            }
        }
                
        if (data["edges"] is JObject edges)
        {
            foreach (var edge in edges)
            {
                string edgeID = edge.Value["NeoId"].ToString() ;
                var start = edge.Value?["start"] as JArray;
                var end = edge.Value?["end"] as JArray;
                string nodeStart = " ";
                string nodeEnd = " ";
                
                foreach (KeyValuePair<string,NodeObject> node in nodesDictionary)
                {
                    float[] nodePosition = new float[] { node.Value.x, node.Value.y, node.Value.z };
                    if(inTolerance(start,nodePosition))
                    {
                        nodeStart = node.Key;
                    } else if (inTolerance(end,nodePosition))
                    {
                        nodeEnd = node.Key;                     
                    }
                }
                if(!nodeStart.Equals(" ") && !nodeEnd.Equals(" "))
                {
                    nodesDictionary[nodeStart].edges.Add(nodeEnd);
                    nodesDictionary[nodeStart].edges_id.Add(edgeID);
                }
            }
        }        
        return nodesDictionary;
    }

    
}
