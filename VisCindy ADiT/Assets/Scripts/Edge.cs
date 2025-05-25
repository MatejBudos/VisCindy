// Can be defined inside SieraHandler.cs or as a separate simple class file

using UnityEngine;

public class Edge
{
    public LineRenderer lineRenderer;
    public GameObject startNode;
    public GameObject endNode;
    public bool isSelfLoop;

    public Edge(LineRenderer lr, GameObject start, GameObject end)
    {
        lineRenderer = lr;
        startNode = start;
        endNode = end;
        isSelfLoop = (start == end);
    }

    public void UpdatePosition(float lineZDepth)
    {
        if (lineRenderer == null) return;

        if (isSelfLoop)
        {
            if (startNode == null) return;
            // Re-apply the self-loop drawing logic based on the startNode's current position
            // We'll call a method from SieraHandler for this
            SieraHandler.Instance.DrawSelfLoopGraphic(lineRenderer, startNode, lineZDepth);
        }
        else
        {
            if (startNode == null || endNode == null) return;
            lineRenderer.SetPosition(0, new Vector3(startNode.transform.position.x, startNode.transform.position.y, lineZDepth));
            lineRenderer.SetPosition(1, new Vector3(endNode.transform.position.x, endNode.transform.position.y, lineZDepth));
        }
    }
}