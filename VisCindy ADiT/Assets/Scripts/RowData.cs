using UnityEngine;
public class RowData
{
    public string RowObjectName; // Name of the Row's GameObject for identification/debugging

    [Header("Layout Information")]
    public float OffsetX;       // Hierarchical offset value

    [Header("Main Dropdown Data")]
    public string MainDropdownValueText;
    public int MainDropdownValueIndex;

    [Header("Dropdown 1 Data")]
    public string Dropdown1ValueText;
    public int Dropdown1ValueIndex;

    [Header("Dropdown 2 Data")]
    public string Dropdown2ValueText;
    public int Dropdown2ValueIndex;

    // Consider adding fields for any other data you might need from the row,
    // e.g., input field values, toggle states, button identifiers.

    public override string ToString()
    {
        return $"Row: '{RowObjectName}', OffsetX: {OffsetX:F2}, MainDrop: '{MainDropdownValueText}' (Idx:{MainDropdownValueIndex}), Drop1: '{Dropdown1ValueText}' (Idx:{Dropdown1ValueIndex}), Drop2: '{Dropdown2ValueText}' (Idx:{Dropdown2ValueIndex})";
    }
}