using TMPro; // For TMP_Dropdown
using UnityEngine;
using UnityEngine.UI; // Required for UnityEngine.UI.Button if you use actionButton

public class RowController : MonoBehaviour
{
    [Header("Row Elements - Assign in Inspector")]
    public GameObject offsetObject;     // The "Offset" GameObject whose localPosition.x determines hierarchy/order
    public TMP_Dropdown mainDropdown;   // The first Dropdown (e.g., named "Dropdown" in your image)
    public TMP_Dropdown dropdown1;      // The second Dropdown (e.g., "Dropdown (1)")
    public TMP_Dropdown dropdown2;      // The third Dropdown (e.g., "Dropdown (2)")

    [Header("Optional Row Elements - Assign in Inspector")]
    public Button actionButton;         // Standard Unity UI Button. Assign if this row has an actionable button.

    /// <summary>
    /// Gets the local X position of the Offset GameObject.
    /// This value can be used to determine hierarchical order or visual layout.
    /// </summary>
    public float GetOffsetX()
    {
        if (offsetObject != null)
        {
            return offsetObject.transform.localPosition.x;
        }
        Debug.LogWarning($"OffsetObject not assigned in RowController on GameObject: {gameObject.name}", this);
        return 0f;
    }

    // Helper method to get selected text from a TMP_Dropdown
    private string GetSelectedTextFromDropdown(TMP_Dropdown dropdown, string dropdownIdentifier)
    {
        if (dropdown == null)
        {
            Debug.LogWarning($"{dropdownIdentifier} not assigned in RowController on GameObject: {gameObject.name}", this);
            return null;
        }

        if (dropdown.options != null && dropdown.value >= 0 && dropdown.value < dropdown.options.Count)
        {
            return dropdown.options[dropdown.value].text;
        }
        else
        {
            // This can happen if options list is empty or value is somehow out of sync
            Debug.LogWarning($"{dropdownIdentifier} on {gameObject.name} has an invalid selection. Value: {dropdown.value}, Options Count: {(dropdown.options?.Count ?? 0)}. Returning caption text or null.", this);
            return dropdown.captionText?.text; // Fallback to visible caption text, could be null
        }
    }

    // Helper method to get selected index from a TMP_Dropdown
    private int GetSelectedIndexFromDropdown(TMP_Dropdown dropdown, string dropdownIdentifier)
    {
        if (dropdown == null)
        {
            Debug.LogWarning($"{dropdownIdentifier} not assigned in RowController on GameObject: {gameObject.name}", this);
            return -1;
        }
        return dropdown.value; // 'value' is the index for TMP_Dropdown
    }

    // --- Public Getters for Dropdown Data ---
    public string GetMainDropdownSelectedText() => GetSelectedTextFromDropdown(mainDropdown, "Main Dropdown");
    public int GetMainDropdownSelectedIndex() => GetSelectedIndexFromDropdown(mainDropdown, "Main Dropdown");

    public string GetDropdown1SelectedText() => GetSelectedTextFromDropdown(dropdown1, "Dropdown 1");
    public int GetDropdown1SelectedIndex() => GetSelectedIndexFromDropdown(dropdown1, "Dropdown 1");

    public string GetDropdown2SelectedText() => GetSelectedTextFromDropdown(dropdown2, "Dropdown 2");
    public int GetDropdown2SelectedIndex() => GetSelectedIndexFromDropdown(dropdown2, "Dropdown 2");

    /// <summary>
    /// Collects all relevant data from this row's UI elements into a RowData object.
    /// </summary>
    public RowData GetRowData()
    {
        return new RowData
        {
            RowObjectName = gameObject.name, // Useful for debugging: name of the GameObject this RowController is on
            OffsetX = GetOffsetX(),
            MainDropdownValueText = GetMainDropdownSelectedText(),
            MainDropdownValueIndex = GetMainDropdownSelectedIndex(),
            Dropdown1ValueText = GetDropdown1SelectedText(),
            Dropdown1ValueIndex = GetDropdown1SelectedIndex(),
            Dropdown2ValueText = GetDropdown2SelectedText(),
            Dropdown2ValueIndex = GetDropdown2SelectedIndex()
            // If 'actionButton' is used, you might want to store information about its state
            // or associate an action ID if the button's purpose is dynamic.
        };
    }
}