using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.Networking;

[System.Serializable]
public class QueryPayload
{
    public string query;
}

public class DrawGraph : MonoBehaviour
{
    public GameObject graphPrefab;
    private int counter = 0;
    private static CookieContainer CookieContainer = new CookieContainer();
    private HttpClientHandler handler = new HttpClientHandler
    {
        CookieContainer = CookieContainer,
        UseCookies = true
    };
    public string apiUrl = "http://127.0.0.1:5000/api/";
    private Dictionary<string, NodeObject> _nodesDictionary = new Dictionary<string, NodeObject>();

    private bool _loadedFlag = true;
    private string responseData1;

    private const string SPHERE_POOL_KEY = "Nodes";
    private const string LINE_POOL_KEY = "Lines";

    private void Start()
    {
        // StartCoroutine(SendToLayouterApi(""));
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
       var jozo =  StartCoroutine(GetGraphData());
        Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
        foreach (var node in ReadingJson.ReadJson(responseData1))
        {
            Debug.Log("vrojor " + counter + " " + node.Key);
            _nodesDictionary.Add(counter.ToString(), node.Value);
            forAdd.Add(counter.ToString(), node.Value);
            counter++;
        }

        _loadedFlag = true;
        VisualizeGraph(forAdd);
    }

    private IEnumerator GetGraphData()
    {
        using (HttpClient client = new HttpClient(new HttpClientHandler
               {
                   CookieContainer = CookieContainer,
                   UseCookies = true
               }))
        {
            // First request to /graph/1
            var response1 = client.GetAsync(apiUrl + "graph/1");
            yield return response1; // Wait for the response

            if (response1.Result.IsSuccessStatusCode)
            {
                responseData1 = response1.Result.Content.ReadAsStringAsync().ToString();
                yield return responseData1; // Wait for the data
                Debug.Log(response1);
            }
            else
            {
                Console.WriteLine($"Error: {response1.Result.StatusCode} - {response1.Result.ReasonPhrase}");
            }

            // Second request to /layouter/grid (if needed) - uncomment if you need this
            // var response2 = client.GetAsync(apiUrl + "layouter/grid");
            // yield return response2;

            // if (response2.Result.IsSuccessStatusCode)
            // {
            //     var responseData2 = response2.Result.Content.ReadAsStringAsync();
            //     yield return responseData2;
            //     // Use responseData2 here
            // }
            // else
            // {
            //     Console.WriteLine($"Error: {response2.Result.StatusCode} - {response2.Result.ReasonPhrase}");
            // }
        }
    }


    private IEnumerator SendToLayouterApi(string jsonResponse)
    {
        // using (UnityWebRequest request = new UnityWebRequest(apiUrl + "layouter", "POST"))
        // {
            // byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonResponse);
            // request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            // request.downloadHandler = new DownloadHandlerBuffer();
            // request.SetRequestHeader("Content-Type", "application/json");
            //
            // yield return request.SendWebRequest();

            // if (request.result == UnityWebRequest.Result.Success)
            if(true)
            {
                // Debug.Log("Layouter Response: " + request.downloadHandler.text);
                // string json = request.downloadHandler.text;
                string json = @"{'nodes': {'0': {'coords': [0.0, 0.0, -1.0]}, '1': {'coords': [-0.08329374786056633, 0.35529838654534873, -0.9310344827586207]}, '2': {'coords': [-0.5063093161172892, 0.02209015855253898, -0.8620689655172413]}, '3': {'coords': [-0.31069274324896534, -0.523886380454168, -0.7931034482758621]}, '4': {'coords': [0.2797908580243922, -0.630350166655862, -0.7241379310344828]}, '5': {'coords': [0.7254162933651299, -0.21099836379677062, -0.6551724137931034]}, '6': {'coords': [0.699739315581481, 0.4083213987348161, -0.5862068965517242]}, '7': {'coords': [0.23206549502418847, 0.8237760384645273, -0.5172413793103448]}, '8': {'coords': [-0.39739651702461737, 0.8007026662519795, -0.4482758620689655]}, '9': {'coords': [-0.8523185273545365, 0.3601066373103553, -0.3793103448275862]}, '10': {'coords': [-0.9104791953145361, -0.2733381109356729, -0.31034482758620685]}, '11': {'coords': [-0.5494269586138836, -0.7999162741723369, -0.24137931034482762]}, '12': {'coords': [0.06438444136199092, -0.9829181693600875, -0.1724137931034483]}, '13': {'coords': [0.6604871388109371, -0.7436768072802238, -0.10344827586206895]}, '14': {'coords': [0.9819976950614893, -0.18571878271146813, -0.03448275862068961]}, '15': {'coords': [0.890701802104536, 0.4532783240853673, 0.034482758620689724]}, '16': {'coords': [0.4229715346459225, 0.9002186040626122, 0.10344827586206895]}, '17': {'coords': [-0.2226621637593039, 0.9595285533936477, 0.17241379310344818]}, '18': {'coords': [-0.7633582847383878, 0.5991829083499643, 0.24137931034482762]}, '19': {'coords': [-0.9503046915317326, -0.024639018715566587, 0.31034482758620685]}, '20': {'coords': [-0.6856025149479726, -0.6213476110872178, 0.3793103448275863]}, '21': {'coords': [-0.08856321567008796, -0.8894972222084222, 0.4482758620689655]}, '22': {'coords': [0.5306193093817442, -0.6714942323210029, 0.5172413793103448]}, '23': {'coords': [0.8068256814230862, -0.07344245523788479, 0.5862068965517242]}, '24': {'coords': [0.5374927987573378, 0.5308960345383288, 0.6551724137931034]}, '25': {'coords': [-0.11087172069947687, 0.6806847422898769, 0.7241379310344827]}, '26': {'coords': [-0.5761767461370427, 0.19750260135974568, 0.7931034482758621]}, '27': {'coords': [-0.2878708771777861, -0.4170940622508614, 0.8620689655172413]}, '28': {'coords': [0.3397272208465569, -0.13326742786691093, 0.9310344827586208]}, '29': {'coords': [0.0, -0.0, 1.0]}}, 'edges': {'0': {'start': [0.0, 0.0, -1.0], 'end': [-0.5494269586138836, -0.7999162741723369, -0.24137931034482762]}, '1': {'start': [-0.31069274324896534, -0.523886380454168, -0.7931034482758621], 'end': [-0.9104791953145361, -0.2733381109356729, -0.31034482758620685]}, '2': {'start': [-0.31069274324896534, -0.523886380454168, -0.7931034482758621], 'end': [0.06438444136199092, -0.9829181693600875, -0.1724137931034483]}, '3': {'start': [-0.31069274324896534, -0.523886380454168, -0.7931034482758621], 'end': [-0.11087172069947687, 0.6806847422898769, 0.7241379310344827]}, '4': {'start': [0.2797908580243922, -0.630350166655862, -0.7241379310344828], 'end': [0.5374927987573378, 0.5308960345383288, 0.6551724137931034]}, '5': {'start': [0.2797908580243922, -0.630350166655862, -0.7241379310344828], 'end': [-0.5761767461370427, 0.19750260135974568, 0.7931034482758621]}, '6': {'start': [-0.5063093161172892, 0.02209015855253898, -0.8620689655172413], 'end': [0.7254162933651299, -0.21099836379677062, -0.6551724137931034]}, '7': {'start': [0.7254162933651299, -0.21099836379677062, -0.6551724137931034], 'end': [-0.6856025149479726, -0.6213476110872178, 0.3793103448275863]}, '8': {'start': [0.7254162933651299, -0.21099836379677062, -0.6551724137931034], 'end': [0.8068256814230862, -0.07344245523788479, 0.5862068965517242]}, '9': {'start': [0.699739315581481, 0.4083213987348161, -0.5862068965517242], 'end': [-0.7633582847383878, 0.5991829083499643, 0.24137931034482762]}, '10': {'start': [0.699739315581481, 0.4083213987348161, -0.5862068965517242], 'end': [0.5374927987573378, 0.5308960345383288, 0.6551724137931034]}, '11': {'start': [0.7254162933651299, -0.21099836379677062, -0.6551724137931034], 'end': [0.23206549502418847, 0.8237760384645273, -0.5172413793103448]}, '12': {'start': [0.23206549502418847, 0.8237760384645273, -0.5172413793103448], 'end': [-0.6856025149479726, -0.6213476110872178, 0.3793103448275863]}, '13': {'start': [0.7254162933651299, -0.21099836379677062, -0.6551724137931034], 'end': [-0.39739651702461737, 0.8007026662519795, -0.4482758620689655]}, '14': {'start': [-0.08329374786056633, 0.35529838654534873, -0.9310344827586207], 'end': [-0.8523185273545365, 0.3601066373103553, -0.3793103448275862]}, '15': {'start': [-0.8523185273545365, 0.3601066373103553, -0.3793103448275862], 'end': [-0.6856025149479726, -0.6213476110872178, 0.3793103448275863]}, '16': {'start': [-0.8523185273545365, 0.3601066373103553, -0.3793103448275862], 'end': [-0.5761767461370427, 0.19750260135974568, 0.7931034482758621]}, '17': {'start': [0.23206549502418847, 0.8237760384645273, -0.5172413793103448], 'end': [-0.9104791953145361, -0.2733381109356729, -0.31034482758620685]}, '18': {'start': [0.0, 0.0, -1.0], 'end': [-0.5494269586138836, -0.7999162741723369, -0.24137931034482762]}, '19': {'start': [-0.5494269586138836, -0.7999162741723369, -0.24137931034482762], 'end': [0.4229715346459225, 0.9002186040626122, 0.10344827586206895]}, '20': {'start': [-0.31069274324896534, -0.523886380454168, -0.7931034482758621], 'end': [0.06438444136199092, -0.9829181693600875, -0.1724137931034483]}, '21': {'start': [-0.5494269586138836, -0.7999162741723369, -0.24137931034482762], 'end': [0.06438444136199092, -0.9829181693600875, -0.1724137931034483]}, '22': {'start': [-0.5494269586138836, -0.7999162741723369, -0.24137931034482762], 'end': [0.6604871388109371, -0.7436768072802238, -0.10344827586206895]}, '23': {'start': [0.7254162933651299, -0.21099836379677062, -0.6551724137931034], 'end': [0.9819976950614893, -0.18571878271146813, -0.03448275862068961]}, '24': {'start': [-0.8523185273545365, 0.3601066373103553, -0.3793103448275862], 'end': [0.9819976950614893, -0.18571878271146813, -0.03448275862068961]}, '25': {'start': [0.2797908580243922, -0.630350166655862, -0.7241379310344828], 'end': [0.4229715346459225, 0.9002186040626122, 0.10344827586206895]}, '26': {'start': [0.4229715346459225, 0.9002186040626122, 0.10344827586206895], 'end': [0.8068256814230862, -0.07344245523788479, 0.5862068965517242]}, '27': {'start': [0.890701802104536, 0.4532783240853673, 0.034482758620689724], 'end': [-0.2226621637593039, 0.9595285533936477, 0.17241379310344818]}, '28': {'start': [-0.31069274324896534, -0.523886380454168, -0.7931034482758621], 'end': [-0.9503046915317326, -0.024639018715566587, 0.31034482758620685]}, '29': {'start': [-0.9104791953145361, -0.2733381109356729, -0.31034482758620685], 'end': [-0.6856025149479726, -0.6213476110872178, 0.3793103448275863]}, '30': {'start': [-0.6856025149479726, -0.6213476110872178, 0.3793103448275863], 'end': [-0.5761767461370427, 0.19750260135974568, 0.7931034482758621]}, '31': {'start': [0.0, 0.0, -1.0], 'end': [-0.08856321567008796, -0.8894972222084222, 0.4482758620689655]}, '32': {'start': [0.06438444136199092, -0.9829181693600875, -0.1724137931034483], 'end': [0.5306193093817442, -0.6714942323210029, 0.5172413793103448]}, '33': {'start': [0.9819976950614893, -0.18571878271146813, -0.03448275862068961], 'end': [0.8068256814230862, -0.07344245523788479, 0.5862068965517242]}, '34': {'start': [0.4229715346459225, 0.9002186040626122, 0.10344827586206895], 'end': [0.8068256814230862, -0.07344245523788479, 0.5862068965517242]}, '35': {'start': [0.8068256814230862, -0.07344245523788479, 0.5862068965517242], 'end': [0.3397272208465569, -0.13326742786691093, 0.9310344827586208]}, '36': {'start': [-0.11087172069947687, 0.6806847422898769, 0.7241379310344827], 'end': [0.3397272208465569, -0.13326742786691093, 0.9310344827586208]}, '37': {'start': [-0.11087172069947687, 0.6806847422898769, 0.7241379310344827], 'end': [-0.5761767461370427, 0.19750260135974568, 0.7931034482758621]}, '38': {'start': [-0.5063093161172892, 0.02209015855253898, -0.8620689655172413], 'end': [-0.2878708771777861, -0.4170940622508614, 0.8620689655172413]}, '39': {'start': [-0.31069274324896534, -0.523886380454168, -0.7931034482758621], 'end': [0.3397272208465569, -0.13326742786691093, 0.9310344827586208]}, '40': {'start': [-0.2226621637593039, 0.9595285533936477, 0.17241379310344818], 'end': [0.3397272208465569, -0.13326742786691093, 0.9310344827586208]}, '41': {'start': [0.0, 0.0, -1.0], 'end': [0.0, -0.0, 1.0]}, '42': {'start': [-0.8523185273545365, 0.3601066373103553, -0.3793103448275862], 'end': [0.0, -0.0, 1.0]}, '43': {'start': [0.06438444136199092, -0.9829181693600875, -0.1724137931034483], 'end': [0.0, -0.0, 1.0]}, '44': {'start': [-0.6856025149479726, -0.6213476110872178, 0.3793103448275863], 'end': [0.0, -0.0, 1.0]}}}";
                Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
                foreach (var node in ReadingJson.ReadJson(json))
                {
                    Debug.Log("vrojor " + counter + " " + node.Key);
                    _nodesDictionary.Add(counter.ToString(), node.Value);
                    forAdd.Add(counter.ToString(), node.Value);
                    counter++;
                }

                _loadedFlag = true;
                VisualizeGraph(forAdd);
            }
            else
            {
                // Debug.LogError("Layouter API Error: " + request.error);
            }
        // }
        yield return true;
    }

    private void VisualizeGraph(Dictionary<string, NodeObject> forAdd)
    {
        // Get the queues from the pool dictionary
        Queue<GameObject> spherePool = ObjectPool.SharedInstance.poolDictionary[SPHERE_POOL_KEY];
        Queue<GameObject> linePool = ObjectPool.SharedInstance.poolDictionary[LINE_POOL_KEY];
        
        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            GameObject sphere = ObjectPool.SharedInstance.GetObject(SPHERE_POOL_KEY);
            if (sphere != null)
            {
                sphere.SetActive(true);
                sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
                sphere.transform.SetParent(graphPrefab.transform);
                _nodesDictionary[node.Key].UInode = sphere;
            }
            else
            {
                Debug.LogWarning("Not enough spheres in the pool!");
                // Handle the case where the pool is empty (e.g., instantiate a new sphere)
            }
        }

        // Create edges using object pool
        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            foreach (string targetNode in node.Value.edges)
            {
                if (linePool.Count > 0)
                {
                    GameObject edge = linePool.Dequeue();
                    edge.SetActive(true);
                    edge.transform.SetParent(graphPrefab.transform);
                    if (!_nodesDictionary[node.Key].UIedges.ContainsKey(targetNode))
                    {
                        _nodesDictionary[node.Key].UIedges.Add(targetNode, edge);
                    }
                }
                else
                {
                    Debug.LogWarning("Not enough lines in the pool!");
                    // Handle the case where the pool is empty (e.g., instantiate a new line)
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