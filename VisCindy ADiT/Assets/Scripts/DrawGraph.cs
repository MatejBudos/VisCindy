using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


[System.Serializable]
public class QueryPayload
{
    public string query;
}
public class DrawGraph : MonoBehaviour, ISingleton
{
    public GameObject graphPrefab;
    private int _counter = 0;
    private int _counterEdges = 0;
    private Queue<GameObject> linePool;
    private Queue<GameObject> spherePool;
    private static readonly CookieContainer CookieContainer = new CookieContainer();
    private Stack<Command> undo = new Stack<Command>();
    private Stack<Command> redo = new Stack<Command>();
    private string firstSelectedKey = null;
    private string secondSelectedKey = null;
    public bool isAddEdgeMode = false;
    public bool isRemoveEdgeMode = false;
    public bool isRemoveNodeMode = false;
    private HttpClientHandler _handler = new HttpClientHandler
    {
        CookieContainer = CookieContainer,
        UseCookies = true
    };
    public string apiUrl = "http://127.0.0.1:5000/api/";
    private Dictionary<string, NodeObject> _nodesDictionary = new Dictionary<string, NodeObject>();
    
    [SerializeField] private TMP_Dropdown layoutDropdown;
    [SerializeField] private TMP_Dropdown getGraphDropdown;

    private bool _loadedFlag = true;
    private string _responseData1;

    private List<GameObject> activeNodes = new List<GameObject>();
    private List<GameObject> activeEdges = new List<GameObject>();

    private const string SPHERE_POOL_KEY = "Nodes";
    private const string LINE_POOL_KEY = "Lines";
    
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
        ResetGraph();
        StartCoroutine(ProcessGraphData());
    }

    public void CreateGraphLayout()
    {
        ResetGraph();
        StartCoroutine(ProcessGraphDataLayout(layoutDropdown.options[layoutDropdown.value].text));
    }

    private IEnumerator ProcessGraphDataLayout(string layout)
    {
        yield return StartCoroutine(LayoutGraph(layout));

        Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
        foreach (var node in ReadingJson.ReadJson(_responseData1))
        {
            _nodesDictionary.Add(node.Key, node.Value);
            forAdd.Add(node.Key, node.Value);
            _counter++;
        }

        _loadedFlag = true;
        VisualizeGraph(forAdd);
    }

    private IEnumerator ProcessGraphData()
    {
        yield return StartCoroutine(GetGraphData());

        Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
        foreach (var node in ReadingJson.ReadJson(_responseData1))
        {
            _nodesDictionary.Add(node.Key, node.Value);
            forAdd.Add(node.Key, node.Value);
            _counter++;
        }

        _loadedFlag = true;
        VisualizeGraph(forAdd);
    }

    private IEnumerator GetGraphData()
    {
        using (HttpClient client = new HttpClient(new HttpClientHandler()))
        {

            Debug.Log(getGraphDropdown.options[getGraphDropdown.value].text);
            var response1 = client.GetAsync(apiUrl + "graph/" + getGraphDropdown.options[getGraphDropdown.value].text);
            yield return response1;

            if (response1.Result.IsSuccessStatusCode)
            {
                _responseData1 = response1.Result.Content.ReadAsStringAsync().Result;
                yield return _responseData1;
            }
            else
            {
                Console.WriteLine($"Error: {response1.Result.StatusCode} - {response1.Result.ReasonPhrase}");
            }

        }
    }

    private void VisualizeGraph(Dictionary<string, NodeObject> forAdd)
    {
        Queue<GameObject> spherePool = ObjectPool.SharedInstance.poolDictionary[SPHERE_POOL_KEY];
        Queue<GameObject> linePool = ObjectPool.SharedInstance.poolDictionary[LINE_POOL_KEY];

        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            GameObject sphere = spherePool.Dequeue();
            if (sphere != null)
            {
                sphere.SetActive(true);
                sphere.name = node.Key;
                sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
                sphere.transform.SetParent(graphPrefab.transform);
                VertexSelector vertexSelector = sphere.AddComponent<VertexSelector>();
                vertexSelector.drawGraph = this;
                _nodesDictionary[node.Key].UInode = sphere;
                activeNodes.Add(sphere);
            }
            else
            {
                Debug.LogWarning("Not enough spheres in the pool!");
            }
        }

        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            int enumerator = 0;
            foreach (string targetNode in node.Value.edges)
            {
                if (linePool.Count > 0)
                {
                    GameObject edge = linePool.Dequeue();
                    edge.SetActive(true);
                    edge.transform.SetParent(graphPrefab.transform);

                    // Reset Line Renderer positions:
                    LineRenderer lr = edge.GetComponent<LineRenderer>();
                    lr.positionCount = 0;
                    lr.positionCount = 2;

                    if (!_nodesDictionary[node.Key].UIedges.ContainsKey(targetNode))
                    {
                        _nodesDictionary[node.Key].UIedges.Add(targetNode, edge);
                        activeEdges.Add(edge);
                    }
                }
                else
                {
                    Debug.LogWarning("Not enough lines in the pool!");
                }
                enumerator++;                
            }
            _counterEdges += enumerator;
        }
    }

    public IEnumerator LayoutGraph(string layoutType)
    {
        using (HttpClient client = new HttpClient(new HttpClientHandler
        {
            CookieContainer = CookieContainer,
            UseCookies = true
        }))
        {
            var response1 = client.GetAsync(apiUrl + "layouter/" + layoutType);
            yield return response1;

            if (response1.Result.IsSuccessStatusCode)
            {
                _responseData1 = response1.Result.Content.ReadAsStringAsync().Result;
                yield return _responseData1;
            }
            else
            {
                Console.WriteLine($"Error: {response1.Result.StatusCode} - {response1.Result.ReasonPhrase}");
            }

        }
    }

    public void ResetGraph()
    {
        _nodesDictionary = new Dictionary<string, NodeObject>();
        foreach (GameObject node in activeNodes)
        {
            ObjectPool.SharedInstance.ReturnObject(node, SPHERE_POOL_KEY);
        }
        activeNodes.Clear();

        foreach (GameObject edge in activeEdges)
        {
            ObjectPool.SharedInstance.ReturnObject(edge, LINE_POOL_KEY);
        }
        activeEdges.Clear();
    }

    public void AddNode()
    {        
        GameObject sphere = ObjectPool.SharedInstance.GetObject(SPHERE_POOL_KEY);
        if (sphere != null)
        {
            sphere.SetActive(true);            
            sphere.name = _counter.ToString();
            sphere.transform.position = new Vector3(0,0,0);
            sphere.transform.SetParent(graphPrefab.transform);
            VertexSelector vertexSelector = sphere.AddComponent<VertexSelector>();
            vertexSelector.drawGraph = this;
            _nodesDictionary.Add(_counter.ToString(), new NodeObject(_counter.ToString(),0,0,0));
            _nodesDictionary[_counter.ToString()].UInode = sphere;            
            activeNodes.Add(sphere);
            redo.Clear();
            undo.Push(new Command(sphere, "addNode","","",_counter.ToString()));
            _counter++;
        }
        else
        {
            Debug.LogWarning("Not enough spheres in the pool!");
        }
    }

    public void AddEdge()
    {
        firstSelectedKey = null;
        secondSelectedKey = null;
        isAddEdgeMode = true;
    }

    public void RemoveEdge()
    {
        firstSelectedKey = null;
        secondSelectedKey = null;
        isRemoveEdgeMode = true;
        
    }

    public void RemoveNode()
    {
        firstSelectedKey = null;
        secondSelectedKey = null;
        isRemoveNodeMode = true;
    }

    public void OnVertexSelected(string vertexKey)
    {
        if (firstSelectedKey == null)
        {
            firstSelectedKey = vertexKey;
            Debug.Log($"First vertex selected: {firstSelectedKey}");
            if (isRemoveNodeMode)
            {
                DisableNode(firstSelectedKey);
                isRemoveNodeMode = false;
            }
        }
        else if (secondSelectedKey == null && firstSelectedKey != vertexKey)
        {
            secondSelectedKey = vertexKey;
            Debug.Log($"Second vertex selected: {secondSelectedKey}");

            if (isAddEdgeMode)
            {
                CreateEdge(firstSelectedKey, secondSelectedKey);
                isAddEdgeMode = false;
            } else if (isRemoveEdgeMode)
            {
                DisableEdge(firstSelectedKey, secondSelectedKey);
                isRemoveEdgeMode = false;
            }                        
        }
        else
        {
            Debug.LogWarning("Invalid selection. Please select a different second vertex.");
        }
    }

    private void DisableEdge(string fromNode, string toNode)
    {
        if(_nodesDictionary.ContainsKey(fromNode) && _nodesDictionary[fromNode].UIedges.ContainsKey(toNode))
        {
            Debug.Log("Edge between" + fromNode + " " + toNode +"succesfully disabled");
            GameObject edge = _nodesDictionary[fromNode].UIedges[toNode];            
            SetVisibilityEdge(edge,false);
            undo.Push(new Command(edge,"deleteRelationship",fromNode,toNode));
            redo.Clear();
        } else if (_nodesDictionary.ContainsKey(toNode) && _nodesDictionary[toNode].UIedges.ContainsKey(fromNode))
        {
            Debug.Log("Edge between" + fromNode + " " + toNode + "succesfully disabled");
            GameObject edge = _nodesDictionary[toNode].UIedges[fromNode];
            SetVisibilityEdge(edge, false);
            undo.Push(new Command(edge, "deleteRelationship",toNode,fromNode));
            redo.Clear();
        }
    }

    private void DisableNode(string nodeKey)
    {
        if (_nodesDictionary.ContainsKey(nodeKey))
        {
            GameObject node = _nodesDictionary[nodeKey].UInode;
            SetVisibilityNode(node, false);
            foreach (KeyValuePair<string,NodeObject> vrchol in _nodesDictionary)
            {
                foreach (KeyValuePair<string, GameObject> edge in _nodesDictionary[vrchol.Key].UIedges)
                {
                    if(vrchol.Key.Equals(nodeKey) || edge.Key.Equals(nodeKey))
                    {
                        SetVisibilityEdge(edge.Value, false);
                    }                    
                }
            }
            Command command = new Command(node, "deleteNode");
            command.nodeName = nodeKey;
            undo.Push(command);
            redo.Clear();
        }
    }

    // Function to create the edge between two vertices
    private void CreateEdge(string fromNode, string toNode)
    {
        if (_nodesDictionary.ContainsKey(fromNode) && _nodesDictionary.ContainsKey(toNode) && !fromNode.Equals(toNode) && !_nodesDictionary[fromNode].UIedges.ContainsKey(toNode) && !_nodesDictionary[toNode].UIedges.ContainsKey(fromNode))
        {
            GameObject edge = ObjectPool.SharedInstance.GetObject(LINE_POOL_KEY);
            if (edge != null)
            {
                edge.SetActive(true);
                edge.transform.SetParent(graphPrefab.transform);

                LineRenderer lr = edge.GetComponent<LineRenderer>();
                lr.SetPosition(0, _nodesDictionary[fromNode].UInode.transform.position);
                lr.SetPosition(1, _nodesDictionary[toNode].UInode.transform.position);

                // Add the edge to the dictionary and activeEdges list
                if (!_nodesDictionary[fromNode].UIedges.ContainsKey(toNode))
                {
                    _nodesDictionary[fromNode].UIedges.Add(toNode, edge);
                    _nodesDictionary[fromNode].edges.Add(toNode);
                    _counterEdges++;
                    _nodesDictionary[fromNode].edges_id.Add(_counterEdges.ToString());
                    activeEdges.Add(edge);
                    Debug.Log($"Edge created between {fromNode} and {toNode}");

                    redo.Clear();
                    undo.Push(new Command(edge, "addRelationship",fromNode,toNode));

                    // Reset Line Renderer positions:
                    LineRenderer lrenderer = edge.GetComponent<LineRenderer>();
                    lrenderer.positionCount = 0;
                    lrenderer.positionCount = 2;
                }
                else
                {
                    Debug.Log("Edge already exists between these nodes!");
                    ObjectPool.SharedInstance.ReturnObject(edge, LINE_POOL_KEY); // Return to pool
                }
            }
            else
            {
                Debug.Log("Not enough lines in the pool!");
            }
        }
        else
        {
            Debug.Log($"Invalid nodes: {fromNode} or {toNode} not found in the graph!");
        }
    }

    public void SetVisibilityNode(GameObject sphere, bool active)
    {
        sphere.SetActive(active);        
    }

    public void SetVisibilityEdge(GameObject edge, bool active)
    {
        edge.SetActive(active);
    }


    public void Undo()
    {
        if (undo.Count != 0)
        {
            Command aktualCommand = undo.Pop();
            if (aktualCommand.command.Equals("addNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, false);
                redo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("addRelationship"))
            {
                SetVisibilityNode(aktualCommand.gameObject, false);
                redo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("deleteNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, true);
                foreach (KeyValuePair<string, NodeObject> vrchol in _nodesDictionary)
                {
                    foreach (KeyValuePair<string, GameObject> edge in _nodesDictionary[vrchol.Key].UIedges)
                    {
                        if (vrchol.Key.Equals(aktualCommand.nodeName) || edge.Key.Equals(aktualCommand.nodeName))
                        {
                            SetVisibilityEdge(edge.Value, true);
                        }
                    }
                }
                redo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("deleteRelationship"))
            {
                redo.Push(aktualCommand);
                SetVisibilityEdge(aktualCommand.gameObject,true);
            }
        }
    }

    public void Redo()
    {
        if(redo.Count != 0)
        {
            Command aktualCommand = redo.Pop();
            if (aktualCommand.command.Equals("addNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, true);
                undo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("addRelationship"))
            {
                SetVisibilityEdge(aktualCommand.gameObject, true);
                undo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("deleteNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, false);
                foreach (KeyValuePair<string, NodeObject> vrchol in _nodesDictionary)
                {
                    foreach (KeyValuePair<string, GameObject> edge in _nodesDictionary[vrchol.Key].UIedges)
                    {
                        if (vrchol.Key.Equals(aktualCommand.nodeName) || edge.Key.Equals(aktualCommand.nodeName))
                        {
                            SetVisibilityEdge(edge.Value, false);
                        }
                    }
                }
                undo.Push(aktualCommand);
            }
            else if (aktualCommand.command.Equals("deleteRelationship"))
            {
                undo.Push(aktualCommand);
                SetVisibilityEdge(aktualCommand.gameObject, false);
            }
        }                
    }

    public void CreatJsonFromBuffer()
    {
        JObject sendToDB = new JObject(new JProperty("changes",new JArray()));
        List<Command> commands = new List<Command>();
            
        foreach (var command in undo)
        {
            commands.Add(command);
        }
        //iterate over undo stack which contain new changes and them add to json 
        for (int i = commands.Count-1; i >= 0; i--)
        {            
            Debug.Log(commands[i].command + " " + commands[i].nodeName + " " + commands[i].fromNode + " " + commands[i].toNode + " " + commands[i].gameObject.transform.position.x + " " + commands[i].gameObject.transform.position.y);            
            //if change is addNode is enough to add him just to JSON no backround proceses are needed
            if (commands[i].command.Equals("addNode"))
            {
                string selectedGraphId = getGraphDropdown.options[getGraphDropdown.value].text;
                //create json file   
                JObject objekt = new JObject(
                    new JProperty("actionType", commands[i].command),                    
                    new JProperty("properties",
                        new JObject(
                            new JProperty("graphId", selectedGraphId),
                            new JProperty("x", commands[i].gameObject.transform.position.x),
                            new JProperty("y", commands[i].gameObject.transform.position.y),
                            new JProperty("z", commands[i].gameObject.transform.position.z)
                            )));                
                ((JArray)sendToDB["changes"]).Add(objekt);
            }
            //if change is AddEdge adding to json is enought too 
            else if (commands[i].command.Equals("addRelationship"))
            {
                //find out id of edge for adding                
                int enumerator = 0;
                string edge_id = "";
                foreach(string edge in _nodesDictionary[commands[i].fromNode].edges)
                {
                    if (edge.Equals(commands[i].toNode))
                    {
                        edge_id = _nodesDictionary[commands[i].fromNode].edges_id[enumerator];
                        break;
                    }
                    enumerator++;
                }
                //create json file                
                JObject objekt = new JObject(
                    new JProperty("actionType", commands[i].command),
                    new JProperty("properties",
                        new JObject(
                            new JProperty("fromNodeId", commands[i].fromNode),
                            new JProperty("toNodeId", commands[i].toNode))));
                ((JArray)sendToDB["changes"]).Add(objekt);
            }
            //if command is RemoveNode we must add JSON node for removing and all edges which go to or from him,
            //than we must do some backround processes like remove node and again all his edges which go to or from him
            else if (commands[i].command.Equals("deleteNode"))
            {
                //AddNode for delete to json                
                JObject objekt = new JObject(
                    new JProperty("actionType", commands[i].command),
                    new JProperty("properties",
                        new JObject(
                            new JProperty("nodeId", commands[i].nodeName))));
                ((JArray)sendToDB["changes"]).Add(objekt);
                //destroy all edge gameobject which start from our node
                foreach(KeyValuePair<string,GameObject> edge in _nodesDictionary[commands[i].nodeName].UIedges)
                {
                    Destroy(edge.Value);
                }
                int enumerator = 0;
                //Add all edges to json, which starts from our node
                //foreach (string edge_id in _nodesDictionary[commands[i].nodeName].edges_id)
                //{
                //    //create json file
                //    objekt = new JObject(
                //    new JProperty("actionType", "deleteRelationship"),
                //    new JProperty("properties",
                //        new JObject(
                //            //new JProperty("graphId",edge_id),
                //            new JProperty("fromNodeId", commands[i].nodeName),
                //            new JProperty("toNodeId", _nodesDictionary[commands[i].nodeName].edges[enumerator]))));

                //    ((JArray)sendToDB["changes"]).Add(objekt);
                //    enumerator++;
                //}
                //Add all edges to json, which come to our node
                foreach (KeyValuePair<string, NodeObject> node in _nodesDictionary)
                {
                    if (!node.Key.Equals(commands[i].nodeName))
                    {
                        enumerator = 0;
                        foreach (KeyValuePair<string, GameObject> edge in node.Value.UIedges)
                        {
                            if (edge.Key.Equals(commands[i].nodeName))
                            {
                                //create json file
                                //objekt = new JObject(
                                //    new JProperty("actionType", "deleteRelationship"),
                                //    new JProperty("properties",
                                //        new JObject(
                                //            //new JProperty("graphId",node.Value.edges_id[enumerator]),
                                //            new JProperty("fromNodeId", node.Key),
                                //            new JProperty("toNodeId", edge.Key))));

                                //((JArray)sendToDB["changes"]).Add(objekt);
                                
                                //Destroy part
                                Destroy(edge.Value);
                                node.Value.UIedges.Remove(edge.Key);
                                node.Value.edges.Remove(edge.Key);
                                node.Value.edges_id.Remove(node.Value.edges_id[enumerator]);
                                break;
                            }
                            enumerator++;
                        }
                    }                  
                }
                Destroy(_nodesDictionary[commands[i].nodeName].UInode);
                _nodesDictionary.Remove(commands[i].nodeName);
            }
            //if condition is RemoveEdge than we add her to json and do some neccessary backround process for removing edge from our list
            else if (commands[i].command.Equals("deleteRelationship"))
            {
                //find out id of edge for adding                
                int enumerator = 0;
                string edge_id = "";
                foreach (string edge in _nodesDictionary[commands[i].fromNode].edges)
                {
                    if (edge.Equals(commands[i].toNode))
                    {
                        edge_id = _nodesDictionary[commands[i].fromNode].edges_id[enumerator];
                    }
                    enumerator++;
                }
                //create json file
                JObject objekt = new JObject(
                    new JProperty("actionType", commands[i].command),
                    new JProperty("properties",
                        new JObject(
                            new JProperty("fromNodeId", commands[i].fromNode),
                            new JProperty("toNodeId", commands[i].toNode))));

                ((JArray)sendToDB["changes"]).Add(objekt);
                
                //remove edge from our list
                Destroy(_nodesDictionary[commands[i].fromNode].UIedges[commands[i].toNode]);
                _nodesDictionary[commands[i].fromNode].UIedges.Remove(commands[i].toNode);
                enumerator = 0;
                foreach(string edge in _nodesDictionary[commands[i].fromNode].edges)
                {
                    if (edge.Equals(commands[i].toNode))
                    {
                        _nodesDictionary[commands[i].fromNode].edges.Remove(commands[i].toNode);
                        _nodesDictionary[commands[i].fromNode].edges_id.Remove(_nodesDictionary[commands[i].fromNode].edges_id[enumerator]);
                        break;
                    }
                    enumerator++;
                }
            }
        }
        //clear our last changes and commit them to DB
        undo.Clear();
        //iterate over redo due reason when was add some node or edge they were backroundly created and only visibility
        //dynamicly change
        commands = new List<Command>();    
        foreach(var command in redo)
        {
            commands.Add(command);            
        }
        for(int i = commands.Count - 1; i >= 0; i--)
        {
            if (commands[i].command.Equals("addNode"))
            {
                Destroy(_nodesDictionary[commands[i].nodeName].UInode);
                _nodesDictionary.Remove(commands[i].nodeName);
            }
            else if (commands[i].command.Equals("addRelationship"))
            {
                Destroy(_nodesDictionary[commands[i].fromNode].UIedges[commands[i].toNode]);
                _nodesDictionary[commands[i].fromNode].UIedges.Remove(commands[i].toNode);
                int edge_index = _nodesDictionary[commands[i].fromNode].edges.IndexOf(commands[i].toNode);
                _nodesDictionary[commands[i].fromNode].edges.Remove(commands[i].toNode);
                _nodesDictionary[commands[i].fromNode].edges_id.Remove(_nodesDictionary[commands[i].fromNode].edges_id[edge_index]);
            }
        }
        redo.Clear();
        Debug.Log(sendToDB.ToString());
        sendJson(sendToDB);
    }

    private async void sendJson(JObject sendToDB)
    {
        await PostJsonAsync(apiUrl+ "update_graph", sendToDB);
    }

    private async Task PostJsonAsync(string url, JObject sendToDB)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Serialize JObject na JSON string
                string json = sendToDB.ToString();

                // Log JSON pred odoslan�m
                Debug.Log("Preparing to send JSON: " + json);

                // Pripravte obsah pre POST request
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Odo�lite POST request
                HttpResponseMessage response = await client.PostAsync(url, content);

                // Spracovanie odpovede
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    Debug.Log("Response: " + responseData);
                }
                else
                {
                    // Ak server vr�ti chybu, logujte jej detaily
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    Debug.LogError($"Response content: {errorResponse}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Log HTTP chyby
                Debug.LogError("HTTP Request Exception: " + httpEx.Message);
            }
            catch (JsonSerializationException jsonEx)
            {
                // Log chyby pri serializ�cii JSON-u
                Debug.LogError("JSON Serialization Exception: " + jsonEx.Message);
            }
            catch (Exception e)
            {
                // Log v�eobecn� v�nimky
                Debug.LogError("Exception: " + e.Message);
            }
        }
    }

}