using UnityEngine;
using UnityEngine.EventSystems;

public class VertexClickHandler : MonoBehaviour
{
    public SieraHandler sieraHandler; // Assign this in the Inspector by dragging SieraMain

    void Start()
    {
        // Attempt to find SieraHandler if not assigned in the Inspector
        if (sieraHandler == null)
        {
            sieraHandler = FindObjectOfType<SieraHandler>();
            if (sieraHandler == null)
            {
                Debug.LogError("SieraHandler not found for VertexClickHandler on " + gameObject.name + ". Please assign it in the Inspector.");
            }
        }
    }

    public void OnPointerClick()
    {
        if (sieraHandler != null)
        {
            sieraHandler.SelectVertex(this.gameObject);
        }
        else
        {
            Debug.LogError("SieraHandler not assigned or found for Vertex: " + gameObject.name);
        }
    }
}