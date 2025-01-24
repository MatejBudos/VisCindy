using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool SharedInstance;
    public GameObject nodePrefab;
    public GameObject linePrefab;
    public int initialNodes = 50; 
    public int initialLines = 50;
    public int threshold = 5;
    public int refillAmount = 3; 
    public GameObject container;
    public Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private const string SpherePoolKey = "Nodes";
    private const string LinePoolKey = "Lines";

    private void Awake()
    {
        SharedInstance = this;
        CreatePool(nodePrefab, initialNodes, SpherePoolKey);
        CreatePool(linePrefab, initialLines, LinePoolKey);
    }
    
    private void Update()
    {
        if (poolDictionary.ContainsKey("Nodes") && poolDictionary["Nodes"].Count < threshold)
        {
            for (int i = 0; i < refillAmount; i++)
            {
                GameObject obj = Instantiate(nodePrefab, container.transform);
                obj.SetActive(false);
                poolDictionary["Nodes"].Enqueue(obj);
            }
        }

        if (poolDictionary.ContainsKey("Lines") && poolDictionary["Lines"].Count < threshold)
        {
            for (int i = 0; i < refillAmount; i++)
            {
                GameObject obj = Instantiate(linePrefab, container.transform);
                obj.SetActive(false);
                poolDictionary["Lines"].Enqueue(obj);
            }
        }
    }

    public void CreatePool(GameObject prefab, int initialSize, string poolKey)
    {
        if (!poolDictionary.ContainsKey(poolKey))
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);

                if (container != null)
                {
                    obj.transform.SetParent(container.transform);
                }

                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(poolKey, objectPool);
        }
    }

    public GameObject GetObject(string poolKey)
    {
        if (poolDictionary.ContainsKey(poolKey) && poolDictionary[poolKey].Count > 0)
        {
            GameObject obj = poolDictionary[poolKey].Dequeue();
            obj.SetActive(true);
            return obj;
        } else
        {
            switch (poolKey)
            {
                case "Nodes":
                {
                    GameObject obj = Instantiate(nodePrefab);
                    return obj;
                }
                case "Lines":
                {
                    GameObject obj = Instantiate(linePrefab);
                    return obj;
                }
                default:
                    return null;
            }
        }
    }

    public void ReturnObject(GameObject obj, string poolKey)
    {
        obj.SetActive(false);
        if (poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary[poolKey].Enqueue(obj);
        }
    }
}
