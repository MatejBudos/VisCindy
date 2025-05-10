using UnityEngine;

public class SieraHandler : MonoBehaviour
{
    public GameObject vertexPrefab;
    public GameObject vertexHolder;
    
    public void AddNewNodeObject()
    {
        if(vertexPrefab == null)
            return;
        if (vertexHolder == null)
            return;

        GameObject vertex =  Instantiate(vertexPrefab, vertexHolder.transform, false);
        vertex.transform.SetParent(vertexPrefab.transform);
        if (!vertex.activeSelf)
        {
            vertex.SetActive(true);
        }
        
    }
}