using UnityEngine;
using TMPro; // For TMP_Dropdown and TMP_InputField

public class ApocUIController : MonoBehaviour
{
    [Header("APOC UI Elements - Assign in Inspector")]
    public TMP_Dropdown traversalTypeDropdown;
    public TMP_InputField minInputField;
    public TMP_InputField maxInputField;
    public TMP_InputField startNodeInputField;
    public TMP_InputField endNodeInputField;
    public TMP_Dropdown uniqueDropdown; // Assuming this is a dropdown for uniqueness options
    public TMP_InputField relFilterInputField;
    public TMP_InputField limitInputField; // APOC specific limit

    /// <summary>
    /// Retrieves the current settings from the APOC UI elements.
    /// Returns null for fields if their corresponding input is empty to match wasApocUsed logic.
    /// </summary>
    public Apoc GetApocSettings()
    {
        Apoc settings = new Apoc();

        // TraversalType Dropdown
        if (traversalTypeDropdown != null && traversalTypeDropdown.options.Count > traversalTypeDropdown.value && traversalTypeDropdown.value >= 0)
        {
            settings.TraversalType = traversalTypeDropdown.options[traversalTypeDropdown.value].text;
        }
        else
        {
            settings.TraversalType = traversalTypeDropdown?.captionText?.text; // Fallback to caption or null
        }

        // InputFields - return null if empty, otherwise the text.
        settings.min = (minInputField != null && !string.IsNullOrEmpty(minInputField.text)) ? minInputField.text : null;
        settings.max = (maxInputField != null && !string.IsNullOrEmpty(maxInputField.text)) ? maxInputField.text : null;
        settings.start = (startNodeInputField != null && !string.IsNullOrEmpty(startNodeInputField.text)) ? startNodeInputField.text : null;
        settings.end = (endNodeInputField != null && !string.IsNullOrEmpty(endNodeInputField.text)) ? endNodeInputField.text : null;
        settings.relFilter = (relFilterInputField != null && !string.IsNullOrEmpty(relFilterInputField.text)) ? relFilterInputField.text : null;
        settings.limit = (limitInputField != null && !string.IsNullOrEmpty(limitInputField.text)) ? limitInputField.text : null;

        // Unique Dropdown
        if (uniqueDropdown != null && uniqueDropdown.options.Count > uniqueDropdown.value && uniqueDropdown.value >= 0)
        {
            settings.unique = uniqueDropdown.options[uniqueDropdown.value].text;
        }
        else
        {
            settings.unique = uniqueDropdown?.captionText?.text; // Fallback to caption or null
        }

        // Log for debugging what's being fetched
        Debug.Log($"[ApocUIController] Fetched Settings: Start='{settings.start}', End='{settings.end}', TravType='{settings.TraversalType}', Min='{settings.min}', Max='{settings.max}', Unique='{settings.unique}', RelFilter='{settings.relFilter}', Limit='{settings.limit}'");

        return settings;
    }
}