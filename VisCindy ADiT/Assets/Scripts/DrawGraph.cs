using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DrawGraph : MonoBehaviour
{
    public GameObject graphPrefab;
    public GameObject spherePrefab;
    public GameObject linePrefab;
    public int pregeneratedNodes = 50;
    public int pregeneratedEdges = 50;
    private int counter = 0;

    public string apiUrl = "http://127.0.0.1:5000/api/grid";
    private Dictionary<string, NodeObject> _nodesDictionary = new Dictionary<string, NodeObject>();

    private bool _loadedFlag = true;

    private const string SPHERE_POOL_KEY = "Nodes";
    private const string LINE_POOL_KEY = "Lines";

    void Start()
    {
        // Initialize object pools
        ObjectPool.SharedInstance.CreatePool(spherePrefab, pregeneratedNodes, SPHERE_POOL_KEY);
        ObjectPool.SharedInstance.CreatePool(linePrefab, pregeneratedEdges, LINE_POOL_KEY);
    }

    void Update()
    {
        if (!_loadedFlag) return;

        foreach (KeyValuePair<string, NodeObject> node in _nodesDictionary)
        {
            foreach (KeyValuePair<string, GameObject> edge in node.Value.UIedges)
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
            Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
             foreach(var node in ReadingJson.ReadJson(json))
            {
                Debug.Log("vrojor " + counter);
                _nodesDictionary.Add(counter.ToString(), node.Value);
                forAdd.Add(counter.ToString(), node.Value);
                counter++;
            }

            _loadedFlag = true;
            VisualizeGraph(forAdd);
        }
    }

    private void VisualizeGraph(Dictionary<string, NodeObject> forAdd)
    {
        // Create spheres using object pool
        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            GameObject sphere = ObjectPool.SharedInstance.GetObject(SPHERE_POOL_KEY);
            if (sphere != null)
            {
                sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
                sphere.transform.SetParent(graphPrefab.transform);
                _nodesDictionary[node.Key].UInode = sphere;
            }
        }

        // Create edges using object pool
        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            foreach (string targetNode in node.Value.edges)
            {
                GameObject edge = ObjectPool.SharedInstance.GetObject(LINE_POOL_KEY);
                if (edge != null)
                {
                    edge.transform.SetParent(gameObject.transform);
                    _nodesDictionary[node.Key].UIedges.Add(targetNode, edge);
                }
            }
        }
    }

    public void ResetGraph()
    {
        foreach (KeyValuePair<string, NodeObject> node in _nodesDictionary)
        {
            if (node.Value.UInode != null)
            {
                ObjectPool.SharedInstance.ReturnObject(node.Value.UInode, SPHERE_POOL_KEY);
            }

            foreach (KeyValuePair<string, GameObject> edge in node.Value.UIedges)
            {
                ObjectPool.SharedInstance.ReturnObject(edge.Value, LINE_POOL_KEY);
            }
            node.Value.UIedges.Clear();
        }

        _nodesDictionary.Clear();
        _loadedFlag = false;
    }
}
