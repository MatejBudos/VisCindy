[System.Serializable]
public class RowData
{
    // Old: public string RowObjectName;
    public string tag; // << RENAMED from RowObjectName

    public float OffsetX; // Stays as is

    // Old: public string MainDropdownValueText;
    public string attribute; // << RENAMED from MainDropdownValueText
    // MainDropdownValueIndex is to be deleted

    // Old: public string Dropdown1ValueText;
    public string operatorValue; // << RENAMED from Dropdown1ValueText
    // Dropdown1ValueIndex is to be deleted

    // Old: public string InputFieldValue2;
    public string value; // << RENAMED from InputFieldValue2

    public string parent; // Stays as is

    // Old: public string TriggerButtonType;
    public string logic; // << RENAMED from TriggerButtonType

    // Constructor or other methods might be useful if you have them,
    // but are not strictly necessary for JsonUtility serialization of public fields.

    public override string ToString()
    {
        // Update ToString to reflect new field names if you use it for debugging
        return $"Row Tag: '{tag}', OffsetX: {OffsetX:F2}, Attribute: '{attribute}', Operator: '{operatorValue}', Value: '{value}', Parent: '{parent}', Logic: '{logic}'";
    }
}