using UnityEngine;

public class Edge
{
    public LineRenderer lineRenderer;
    public GameObject startNode;
    public GameObject endNode;
    public bool isSelfLoop;
    public GameObject uiElementInstance;

    // Modified Constructor
    public Edge(LineRenderer lr, GameObject start, GameObject end, GameObject uiPrefab, Transform uiParentTransform)
    {
        lineRenderer = lr;
        startNode = start;
        endNode = end;
        isSelfLoop = (start == end);

        if (uiPrefab != null && !isSelfLoop) // Don't add UI for self-loops by default
        {
            // Instantiate the UI prefab as a child of the designated UI holder
            uiElementInstance = Object.Instantiate(uiPrefab, uiParentTransform);
            uiElementInstance.name = $"UI_For_Edge_{start.name}_to_{end.name}"; // Optional: give it a descriptive name
            uiElementInstance.SetActive(true); // Activate the instantiated UI
        }
        else if (isSelfLoop)
        {
            // Explicitly ensure no UI for self-loops if a prefab was passed
            if (uiElementInstance != null) Object.Destroy(uiElementInstance);
            uiElementInstance = null;
        }
    }

    // Modified UpdatePosition
    // lineBaseZValue: The Z depth at which the line itself is drawn
    // uiZOffset: A small offset to apply to the UI element's Z relative to the line's Z plane (e.g., -0.01f to be slightly in front)
    public void UpdatePosition(float lineBaseZValue, float uiZOffset)
    {
        if (lineRenderer == null) return;

        Vector3 worldStartPos, worldEndPos;

        if (isSelfLoop)
        {
            if (startNode == null) return;
            SieraHandler.Instance.DrawSelfLoopGraphic(lineRenderer, startNode, lineBaseZValue);
            if (uiElementInstance != null && uiElementInstance.activeSelf)
            {
                uiElementInstance.SetActive(false); // Keep UI hidden for self-loops
            }
            return;
        }
        else // For non-self-loops
        {
            if (startNode == null || endNode == null)
            {
                // If a node is gone, potentially hide UI and mark for cleanup later
                if (uiElementInstance != null) uiElementInstance.SetActive(false);
                return;
            }
            worldStartPos = startNode.transform.position;
            worldEndPos = endNode.transform.position;

            lineRenderer.SetPosition(0, new Vector3(worldStartPos.x, worldStartPos.y, lineBaseZValue));
            lineRenderer.SetPosition(1, new Vector3(worldEndPos.x, worldEndPos.y, lineBaseZValue));
        }

        // Update UI Element Position for non-self-loops
        if (uiElementInstance != null)
        {
            if (!uiElementInstance.activeSelf && !isSelfLoop && startNode != null && endNode != null)
            {
                uiElementInstance.SetActive(true); // Re-activate if nodes are valid and not a self-loop
            }

            // Calculate the world midpoint of the edge
            Vector3 edgeWorldMidpoint = (worldStartPos + worldEndPos) / 2.0f;

            // Position the UI element at the midpoint in XY.
            // The Z will be based on the line's Z plane plus the specified offset.
            // This assumes your UI element is designed to be positioned in world space (e.g., on a World Space Canvas,
            // or a Screen Space - Camera canvas where world positions translate correctly).
            uiElementInstance.transform.position = new Vector3(edgeWorldMidpoint.x, edgeWorldMidpoint.y, lineBaseZValue + uiZOffset);

            // Optional: If your UI element is a 3D object and needs to face the camera (billboarding)
            // if (Camera.main != null)
            // {
            //     uiElementInstance.transform.LookAt(uiElementInstance.transform.position + Camera.main.transform.rotation * Vector3.forward,
            //                                Camera.main.transform.rotation * Vector3.up);
            // }
            // For standard RectTransform-based UI on a Canvas, the Canvas usually handles orientation.
        }
    }

    public void DestroyUIElement()
    {
        if (uiElementInstance != null)
        {
            Object.Destroy(uiElementInstance);
            uiElementInstance = null;
        }
    }
}