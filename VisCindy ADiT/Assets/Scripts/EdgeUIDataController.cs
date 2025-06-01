using UnityEngine;
using TMPro; // For TMP_Dropdown and TMP_InputField

public class EdgeUIDataController : MonoBehaviour
{
    [Header("Edge UI Elements - Assign in Inspector")]
    [Tooltip("The Dropdown for Relationship Type (e.g., 'RelType').")]
    public TMP_Dropdown relationshipTypeDropdown;

    [Tooltip("The InputField for the Minimum value (e.g., 'Min').")]
    public TMP_InputField minValueInputField;

    [Tooltip("The InputField for the Maximum value (e.g., 'Max').")]
    public TMP_InputField maxValueInputField;

    // A simple struct to bundle the data for easy return
    public struct EdgeUIValues
    {
        public string RelationshipType;
        public string MinValue;
        public string MaxValue;
    }

    /// <summary>
    /// Retrieves the current values from the assigned UI elements.
    /// If an input field is empty, its corresponding value will be "None".
    /// </summary>
    public EdgeUIValues GetCurrentValues()
    {
        EdgeUIValues values = new EdgeUIValues();

        // Get Relationship Type from Dropdown
        if (relationshipTypeDropdown != null)
        {
            if (relationshipTypeDropdown.options != null && 
                relationshipTypeDropdown.value >= 0 && 
                relationshipTypeDropdown.value < relationshipTypeDropdown.options.Count)
            {
                values.RelationshipType = relationshipTypeDropdown.options[relationshipTypeDropdown.value].text;
            }
            else if (relationshipTypeDropdown.captionText != null) // Fallback to current caption if no valid selection
            {
                values.RelationshipType = string.IsNullOrEmpty(relationshipTypeDropdown.captionText.text) ? "None" : relationshipTypeDropdown.captionText.text;
            }
            else
            {
                values.RelationshipType = "None"; // Default if dropdown is problematic
            }
        }
        else
        {
            Debug.LogWarning($"EdgeUIDataController on '{gameObject.name}': RelationshipType Dropdown is not assigned.", this);
            values.RelationshipType = "None";
        }

        // Get Min Value from InputField
        if (minValueInputField != null)
        {
            values.MinValue = string.IsNullOrEmpty(minValueInputField.text) ? "None" : minValueInputField.text;
        }
        else
        {
            Debug.LogWarning($"EdgeUIDataController on '{gameObject.name}': MinValue InputField is not assigned.", this);
            values.MinValue = "None";
        }

        // Get Max Value from InputField
        if (maxValueInputField != null)
        {
            values.MaxValue = string.IsNullOrEmpty(maxValueInputField.text) ? "None" : maxValueInputField.text;
        }
        else
        {
            Debug.LogWarning($"EdgeUIDataController on '{gameObject.name}': MaxValue InputField is not assigned.", this);
            values.MaxValue = "None";
        }

        return values;
    }
}