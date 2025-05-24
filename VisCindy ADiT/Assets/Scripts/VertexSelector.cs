using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
public class VertexSelector : MonoBehaviour, IMixedRealityPointerHandler
{
    public DrawGraph drawGraph; // Referencia na DrawGraph skript

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (drawGraph != null && (drawGraph.isAddEdgeMode || drawGraph.isRemoveEdgeMode || drawGraph.isRemoveNodeMode))
        {
            // Zavola funkciu na vyber vrcholu
            drawGraph.OnVertexSelected(gameObject.name); // Predpokladame, ze meno GameObjectu je kluc v _nodesDictionary
        }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
}
