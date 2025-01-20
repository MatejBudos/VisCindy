using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ReadingJson
{
    private const double Tolerance = 0.01;
    public static Dictionary<string,NodeObject> ReadJson(string jsonContent)
    {        
        Dictionary<string, NodeObject> nodesDictionary = new Dictionary<string, NodeObject>();

        var data = JsonConvert.DeserializeObject<JObject>(jsonContent);
              
        if (data["nodes"] is JObject nodes)
        {
            foreach (var node in nodes)
            {
                string nodeId = node.Key;
                if (node.Value is JObject nodeData && nodeData.ContainsKey("coords"))
                {
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
                string edgeID = edge.Key;
                var start = edge.Value?["start"] as JArray;
                var end = edge.Value?["end"] as JArray;
                string nodeStart = " ";
                string nodeEnd = " ";
                foreach (KeyValuePair<string,NodeObject> node in nodesDictionary)
                {
                    if(Math.Abs((float) start?[0] - node.Value.x) < Tolerance && 
                        Math.Abs((float) start?[1] - node.Value.y) < Tolerance && 
                        Math.Abs((float) start?[2] - node.Value.z) < Tolerance)
                    {
                        nodeStart = node.Key;
                    } else if (Math.Abs((float) end?[0] - node.Value.x) < Tolerance && 
                                Math.Abs((float) end?[1] - node.Value.y) < Tolerance && 
                                Math.Abs((float) end?[2] - node.Value.z) < Tolerance)
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
