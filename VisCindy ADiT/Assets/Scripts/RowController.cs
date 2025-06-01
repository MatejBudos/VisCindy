// In RowController.cs

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // For Button
// using UnityEngine.Events; // Not strictly needed if using lambda for listeners

public class RowController : MonoBehaviour
{
    [Header("Row Elements - Assign in Inspector")]
    public GameObject offsetObject;
    public TMP_Dropdown mainDropdown;
    public TMP_Dropdown dropdown1;
    // public TMP_Dropdown dropdown2; // REMOVED
    public TMP_InputField inputField2; // << NEW: Assign your new TMP_InputField

    [Header("Action Buttons - Assign in Inspector")]
    public Button andButton; // << NEW: Assign your "AND" button
    public Button orButton;  // << NEW: Assign your "OR" button
    // public Button actionButton; // Old "+" button, can be removed if fully replaced

    // Data to be set by the creating controller (VertexController)
    [HideInInspector] public string sourceRowName = "NONE";
    [HideInInspector] public string triggerButtonType = "INITIAL"; // Default for the very first row

    // Static event to request a new row. Parameters: (sourceRowController, buttonTypeClicked)
    public static event System.Action<RowController, string> OnRequestNewRowAdd;

    void Start()
    {
        if (andButton != null)
        {
            andButton.onClick.AddListener(() => RequestNewRow("AND"));
        }
        if (orButton != null)
        {
            orButton.onClick.AddListener(() => RequestNewRow("OR"));
        }
    }

    private void RequestNewRow(string buttonType)
    {
        Debug.Log($"Row '{gameObject.name}' sending request to add new row via '{buttonType}' button.", this);
        OnRequestNewRowAdd?.Invoke(this, buttonType);
    }

    public float GetOffsetX()
    {
        if (offsetObject != null) return offsetObject.transform.localPosition.x;
        Debug.LogWarning($"OffsetObject not assigned in RowController on GameObject: {gameObject.name}", this);
        return 0f;
    }

    private string GetSelectedTextFromDropdown(TMP_Dropdown dropdown, string id) => (dropdown != null && dropdown.options != null && dropdown.value >= 0 && dropdown.value < dropdown.options.Count) ? dropdown.options[dropdown.value].text : (dropdown?.captionText?.text);
    private int GetSelectedIndexFromDropdown(TMP_Dropdown dropdown, string id) => (dropdown != null) ? dropdown.value : -1;

    public string GetMainDropdownSelectedText() => GetSelectedTextFromDropdown(mainDropdown, "Main Dropdown");
    public int GetMainDropdownSelectedIndex() => GetSelectedIndexFromDropdown(mainDropdown, "Main Dropdown");
    public string GetDropdown1SelectedText() => GetSelectedTextFromDropdown(dropdown1, "Dropdown 1");
    public int GetDropdown1SelectedIndex() => GetSelectedIndexFromDropdown(dropdown1, "Dropdown 1");

    public string GetInputFieldValue2() // << NEW
    {
        if (inputField2 != null) return inputField2.text;
        Debug.LogWarning($"InputField2 not assigned in RowController on GameObject: {gameObject.name}", this);
        return string.Empty;
    }

    public RowData GetRowData()
    {
        return new RowData
        {
            // Assign to new field names in RowData
            tag = gameObject.name,              // Was RowObjectName
            OffsetX = GetOffsetX(),
            attribute = GetMainDropdownSelectedText(), // Was MainDropdownValueText
            // MainDropdownValueIndex is no longer in RowData

            operatorValue = GetDropdown1SelectedText(),// Was Dropdown1ValueText
            // Dropdown1ValueIndex is no longer in RowData

            value = GetInputFieldValue2(),      // Was InputFieldValue2

            SourceRowName = this.sourceRowName,
            logic = this.triggerButtonType      // Was TriggerButtonType
        };
    }
   
    
    public void PopulateAttributeDropdown(List<string> attributeOptions)
    {
        if (mainDropdown == null)
        {
            Debug.LogWarning($"Attribute Dropdown (mainDropdown) not assigned on RowController: '{gameObject.name}'. Cannot populate.", this);
            return;
        }

        mainDropdown.ClearOptions(); // Clear existing options

        if (attributeOptions != null && attributeOptions.Count > 0)
        {
            mainDropdown.AddOptions(attributeOptions);
        }
        else
        {
            // Optionally, add a default placeholder if no options are available
            mainDropdown.AddOptions(new List<string> { "--No Attributes--" });
        }
        mainDropdown.value = 0; // Reset to the first item
        mainDropdown.RefreshShownValue(); // Important to update the displayed text of the dropdown
        // Debug.Log($"Populated Attribute dropdown on '{gameObject.name}' with {attributeOptions?.Count ?? 0} options.", this);
    }

    public void ConfigureRow(string name, string srcRowName, string btnType, float xOffset)
    {
        gameObject.name = name;
        this.sourceRowName = srcRowName;
        this.triggerButtonType = btnType;
        if (offsetObject != null)
        {
            RectTransform offsetRect = offsetObject.GetComponent<RectTransform>();
            if (offsetRect != null)
            {
                offsetRect.anchoredPosition = new Vector2(xOffset, offsetRect.anchoredPosition.y);
            }
        }
    }
}