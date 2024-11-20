using UnityEngine;
using System.Collections.Generic;

public class DrawGraph : MonoBehaviour
{
    public GameObject graphPrefab;
    public GameObject spherePrefab;
    public GameObject linePrefab;

    public ReadingJson forRead = new ReadingJson();
    public Dictionary<string, NodeObject> nodesDictionary = new Dictionary<string, NodeObject>();

    void Start()
    {
        nodesDictionary = forRead.readJson("C:/Users/Admin/Downloads/VisCindy/examples/graph_data.json");

        // Create spheres
        foreach (KeyValuePair<string, NodeObject> node in nodesDictionary)
        {
            GameObject sphere = Instantiate(spherePrefab, graphPrefab.transform);
            sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
            nodesDictionary[node.Key].UInode = sphere;
        }

        // Create edges
        foreach (KeyValuePair<string, NodeObject> node in nodesDictionary)
        {
            foreach (string targetNode in node.Value.edges)
            {
                GameObject edge = Instantiate(linePrefab, gameObject.transform);
                nodesDictionary[node.Key].UIedges.Add(targetNode, edge);
            }
        }
    }
    void Update()
    {
        foreach(KeyValuePair<string,NodeObject> node in nodesDictionary)
        {
            foreach(KeyValuePair<string,GameObject> edge in node.Value.UIedges)
            {
                LineRenderer lineRenderer = edge.Value.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, node.Value.UInode.transform.position);
                lineRenderer.SetPosition(1, nodesDictionary[edge.Key].UInode.transform.position);
            }
        }    
    }
}
