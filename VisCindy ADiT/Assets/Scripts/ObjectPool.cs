using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool SharedInstance;
    public GameObject nodePrefab;
    public GameObject linePrefab;
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        SharedInstance = this;
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
            //if empty Pull create new Line or Node
            if (poolKey.Equals("Nodes"))
            {
                GameObject obj = Instantiate(nodePrefab);
                return obj;
            } else if (poolKey.Equals("Lines"))
            {
                GameObject obj = Instantiate(linePrefab);
                return obj;
            } else
            {
                return null;
            }
        }

        Debug.LogWarning($"Pool with key {poolKey} is empty or doesn't exist.");
        return null;
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
