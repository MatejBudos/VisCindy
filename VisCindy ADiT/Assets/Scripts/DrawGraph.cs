using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;


public class DrawGraph : MonoBehaviour
{
    public GameObject graphPrefab;
    public GameObject spherePrefab;
    public GameObject linePrefab;
    
    public string apiUrl = "http://127.0.0.1:5000/api/grid";
    private Dictionary<string, NodeObject> _nodesDictionary = new Dictionary<string, NodeObject>();

    private bool _loadedFlag = true;
    
    
    void Update()
    {
        if (!_loadedFlag) return;

        foreach(KeyValuePair<string,NodeObject> node in _nodesDictionary)
        {
            foreach(KeyValuePair<string,GameObject> edge in node.Value.UIedges)
            {
                LineRenderer lineRenderer = edge.Value.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, node.Value.UInode.transform.position);
                lineRenderer.SetPosition(1, _nodesDictionary[edge.Key].UInode.transform.position);
                
            }
        }    
    }
    
    public void CreateGraphFromAPI()
    {
        StartCoroutine(GetGraphData());
    }

    IEnumerator GetGraphData()
    {
        using UnityWebRequest www = UnityWebRequest.Get(apiUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error); 

        }
        else
        {
            string json = www.downloadHandler.text;

            _nodesDictionary = ReadingJson.ReadJson(json);
            _loadedFlag = true;
               
            VisualizeGraph();
        }
    }

    private void VisualizeGraph()
    {
        // Create spheres
        foreach (KeyValuePair<string, NodeObject> node in _nodesDictionary)
        {
            GameObject sphere = Instantiate(spherePrefab, graphPrefab.transform);
            sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
            _nodesDictionary[node.Key].UInode = sphere;
        }

        // Create edges
        foreach (KeyValuePair<string, NodeObject> node in _nodesDictionary)
        {
            foreach (string targetNode in node.Value.edges)
            {
                GameObject edge = Instantiate(linePrefab, gameObject.transform);
                _nodesDictionary[node.Key].UIedges.Add(targetNode, edge);
            }
        }
    }
}
