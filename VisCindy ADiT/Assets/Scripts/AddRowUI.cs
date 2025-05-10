using UnityEngine;
using UnityEngine.UI; // Required for UI elements like LayoutGroup

public class AddRowUI : MonoBehaviour
{
    // Public variable to assign your prefab in the Inspector
    // This is the "Row" prefab you want to duplicate.
    public GameObject prefabToCopy;

    // Public variable to assign your Layout Group GameObject in the Inspector.
    // This is the parent RectTransform where the new row will be added.
    public RectTransform layoutGroupParent;

    // Public variable to define how much the new row's offset should be incremented horizontally.
    public float horizontalOffsetIncrement = 30f;

    // Public variable for the horizontal offset of the first row, or rows added without a source button.
    public float initialHorizontalOffset = 0f;

    public void AddNewRow(GameObject clickedButton = null)
    {
        if (prefabToCopy == null)
        {
            Debug.LogError("AddNewRow: Prefab to copy is not assigned in the AddRowUI script! Please assign it in the Inspector.");
            return;
        }

        if (layoutGroupParent == null)
        {
            Debug.LogError("AddNewRow: Layout Group Parent is not assigned in the AddRowUI script! Please assign it in the Inspector.");
            return;
        }

        GameObject newRowInstance = Instantiate(prefabToCopy);
        if (newRowInstance == null)
            return;

        // Set the parent of the newly instantiated row
        newRowInstance.transform.SetParent(layoutGroupParent, false);
        Debug.Log("AddNewRow: Parent set to: " + layoutGroupParent.name);

        // --- Determine Source Offset Transform from clickedButton ---
        RectTransform sourceOffsetTransform = null;
        if (clickedButton != null)
        {
            Transform buttonParent = clickedButton.transform.parent;
            if (buttonParent != null && buttonParent.name == "Offset")
            {
                sourceOffsetTransform = buttonParent.GetComponent<RectTransform>();
                if (sourceOffsetTransform == null)
                {
                    Debug.LogError("AddNewRow: The 'Offset' parent of the clicked button does not have a RectTransform component. Button: " + clickedButton.name, clickedButton);
                }
            }
            else if (buttonParent != null)
            {
                Debug.LogWarning("AddNewRow: Clicked button's parent is not named 'Offset'. It's named '" + buttonParent.name + "'. Cannot use it for source offset calculation. Button: " + clickedButton.name, clickedButton);
            }
            else
            {
                 Debug.LogWarning("AddNewRow: Clicked button has no parent. Cannot determine source offset. Button: " + clickedButton.name, clickedButton);
            }
        }
        else
        {
            Debug.Log("AddNewRow: No clickedButton provided (e.g., called from CopyMe or for an initial row).");
        }

        // --- Handle Offset for the new row's "Offset" child ---
        RectTransform newRowOffsetChildRect = newRowInstance.transform.Find("Offset")?.GetComponent<RectTransform>();

        if (newRowOffsetChildRect != null)
        {
            Vector2 newCalculatedAnchoredPos;

            if (sourceOffsetTransform != null)
            {
                // Calculate new offset based on source's "Offset" child's anchoredPosition
                Vector2 currentSourceOffsetAnchoredPos = sourceOffsetTransform.anchoredPosition;
                newCalculatedAnchoredPos = new Vector2(currentSourceOffsetAnchoredPos.x + horizontalOffsetIncrement, currentSourceOffsetAnchoredPos.y);
                Debug.Log($"AddNewRow: Source 'Offset' RectTransform from clicked button's parent used. Source AnchoredPos: {currentSourceOffsetAnchoredPos}. New 'Offset' AnchoredPos: {newCalculatedAnchoredPos}", newRowOffsetChildRect);
            }
            else
            {
                // No valid sourceOffsetTransform (either no button, button parent not 'Offset', or called without button)
                // Use initialHorizontalOffset. Assume Y position should be the prefab's default Y for its "Offset" child.
                newCalculatedAnchoredPos = new Vector2(initialHorizontalOffset, newRowOffsetChildRect.anchoredPosition.y);
                Debug.Log($"AddNewRow: No valid source 'Offset' from button or no button provided. Using initialHorizontalOffset ({initialHorizontalOffset}). New 'Offset' AnchoredPos set to: {newCalculatedAnchoredPos}", newRowOffsetChildRect);
            }
            newRowOffsetChildRect.anchoredPosition = newCalculatedAnchoredPos;
        }
        else
        {
            Debug.LogWarning("AddNewRow: Could not find child GameObject named 'Offset' with a RectTransform in the new row instance: " + newRowInstance.name + ". Offset will not be adjusted.", newRowInstance);
        }

        // --- Post-Instantiation Debugging & Activation ---
        RectTransform newRowRectTransform = newRowInstance.GetComponent<RectTransform>();
        if (newRowRectTransform != null)
        {
            Debug.Log("AddNewRow: New row instance RectTransform anchoredPosition: " + newRowRectTransform.anchoredPosition +
                      ", localScale: " + newRowRectTransform.localScale +
                      ", localPosition: " + newRowRectTransform.localPosition, newRowInstance);
        }
        else
        {
            Debug.LogWarning("AddNewRow: The instantiated newRowInstance does not have a RectTransform component. Is it a UI element?", newRowInstance);
        }

        Debug.Log("AddNewRow: newRowInstance.activeSelf: " + newRowInstance.activeSelf + ", newRowInstance.activeInHierarchy: " + newRowInstance.activeInHierarchy, newRowInstance);

        if (!prefabToCopy.activeSelf)
        {
            Debug.LogWarning("AddNewRow: The source 'prefabToCopy' (" + prefabToCopy.name + ") itself is inactive in the project. Instantiated objects will also start inactive.", prefabToCopy);
        }
        
        if (!newRowInstance.activeSelf)
        {
            Debug.LogWarning("AddNewRow: newRowInstance was inactive after instantiation. Forcing activeSelf = true.", newRowInstance);
            newRowInstance.SetActive(true);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupParent);
        Debug.Log("AddNewRow: LayoutRebuilder.ForceRebuildLayoutImmediate called on " + layoutGroupParent.name, layoutGroupParent);
        
        Debug.Log("New row '" + newRowInstance.name + "' added to the layout group: " + layoutGroupParent.name + ". Check Hierarchy and Scene view.", newRowInstance);
    }
}
