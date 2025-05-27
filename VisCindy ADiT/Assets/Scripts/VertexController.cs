using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy
using TMPro;

public class VertexController : MonoBehaviour
{
    private List<RowController> _managedRows;
    private Transform _vertexRootTransform; // To store the transform of VertexThis(Clone)
    private string _persistentId;
    public string PersistentId => _persistentId;
    [Header("Vertex Configuration")]
    [Tooltip("Assign the TextMeshProUGUI component of the Label GameObject here (e.g., DropdownMenu/Canvas/Label).")]
    public TextMeshProUGUI vertexLabelTextComponent; // Assign this in the Inspector


    void Awake()
    {
        if (string.IsNullOrEmpty(_persistentId))
        {
            _persistentId = System.Guid.NewGuid().ToString();
        }

        if (transform.parent != null)
        {
            _vertexRootTransform = transform.parent;
        }
        else
        {
            Debug.LogError($"VertexController on '{gameObject.name}' does not have a parent. Using own transform as fallback for position.", this);
            _vertexRootTransform = transform;
        }

        // Initial population of rows
        InitializeAndSortRows();
    }

    /// <summary>
    /// Finds all RowController components in children of this GameObject (DropdownMenu)
    /// and sorts them by OffsetX. (true) includes inactive children.
    /// </summary>
    private void FindAndSortRows()
    {
        // GetComponentsInChildren searches this GameObject and all its children.
        _managedRows = new List<RowController>(GetComponentsInChildren<RowController>(true));

        if (_managedRows.Count > 0)
        {
            _managedRows = _managedRows.OrderBy(row => row.GetOffsetX()).ToList();
        }
        // The debug log for count will be in InitializeAndSortRows or RefreshManagedRows
    }

    private void InitializeAndSortRows()
    {
        FindAndSortRows();
        Debug.Log($"[VertexController Awake/Init] '{gameObject.name}' (ID: {PersistentId}) initialized/scanned. Found {_managedRows?.Count ?? 0} rows.", this);
    }

    public void RefreshManagedRows()
    {
        Debug.Log($"[VertexController Refresh] '{gameObject.name}' (ID: {PersistentId}) is refreshing its managed rows...", this);
        FindAndSortRows();
        Debug.Log($"[VertexController Refresh] '{gameObject.name}' (ID: {PersistentId}) refreshed. Now managing {_managedRows?.Count ?? 0} rows.", this);
    }

    public List<RowData> GetAllRowsData()
{
    if (_managedRows == null)
    {
        Debug.LogWarning($"[VertexController GetAllRowsData] '{gameObject.name}' (ID: {PersistentId}): _managedRows is null. Returning empty list.", this);
        return new List<RowData>();
    }

    List<RowData> actualRowsData = new List<RowData>();

    // The first row (_managedRows[0]) is considered a dummy and should be excluded.
    // We iterate from the second row (index 1) onwards.
    int startIndex = 1; // Start processing from the second row

    if (_managedRows.Count == 0)
    {
        // No rows at all (not even a dummy)
        return actualRowsData; // Returns empty list
    }
    
    if (_managedRows.Count > 0 && _managedRows.Count <= startIndex) // Only dummy row(s) or fewer than startIndex implies no actual data rows
    {
        Debug.Log($"[VertexController GetAllRowsData] '{gameObject.name}' (ID: {PersistentId}): Only dummy row(s) found (Total: {_managedRows.Count}, StartIndex for data: {startIndex}). Exporting 0 data rows.", this);
        return actualRowsData; // Returns empty list
    }
    
    // Log which row is being skipped if a dummy row exists.
    // This log will only appear if there's at least one row to be considered dummy.
    if (_managedRows.Count >= startIndex && startIndex > 0) {
         Debug.Log($"[VertexController GetAllRowsData] '{gameObject.name}' (ID: {PersistentId}): Attempting to skip first {startIndex} row(s) as dummy. Processing from index {startIndex}. First potential dummy: '{_managedRows[0].gameObject.name}'.", this);
    }


    for (int i = startIndex; i < _managedRows.Count; i++)
    {
        RowController rowController = _managedRows[i];
        if (rowController != null)
        {
            actualRowsData.Add(rowController.GetRowData());
        }
    }

    Debug.Log($"[VertexController GetAllRowsData] '{gameObject.name}' (ID: {PersistentId}): Processed {_managedRows.Count} managed rows, skipped {startIndex} dummy row(s), returning {actualRowsData.Count} data rows.", this);
    return actualRowsData;
}

    private string GetVertexLabel()
    {
        if (vertexLabelTextComponent != null)
        {
            return vertexLabelTextComponent.text;
        }
        else
        {
            // Fallback: Try to find it dynamically if not assigned.
            // This assumes a specific hierarchy: this GameObject (DropdownMenu) -> Canvas -> Label
            Transform canvasTransform = transform.Find("Canvas");
            if (canvasTransform != null)
            {
                Transform labelTransform = canvasTransform.Find("Label");
                if (labelTransform != null)
                {
                    TextMeshProUGUI tmpText = labelTransform.GetComponent<TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        Debug.LogWarning($"VertexLabelTextComponent was not assigned on '{gameObject.name}'. Found 'Canvas/Label/Text (TMP)' dynamically. Please assign in Inspector for future reliability.", this);
                        vertexLabelTextComponent = tmpText; // Cache for next time
                        return tmpText.text;
                    }
                    // Optional: Check for standard UI.Text if TMP not found
                    UnityEngine.UI.Text uiText = labelTransform.GetComponent<UnityEngine.UI.Text>();
                    if (uiText != null) {
                        Debug.LogWarning($"VertexLabelTextComponent was not assigned and TMP_Text not found on 'Canvas/Label'. Found UI.Text dynamically. Please assign in Inspector and prefer TMP_Text.", this);
                        return uiText.text;
                    }
                }
            }
            Debug.LogWarning($"VertexLabelTextComponent not assigned and 'Canvas/Label' with a Text component not found on '{gameObject.name}'. Vertex label will be empty.", this);
            return string.Empty; // Or a default name, or null
        }
    }

    public VertexExportData GetExportData()
    {
        if (_managedRows == null)
        {
            Debug.LogWarning($"[VertexController GetExportData] '{gameObject.name}' (ID: {this.PersistentId}): _managedRows was null. Attempting to find and sort rows now...", this);
            FindAndSortRows();
        }

        VertexExportData data = new VertexExportData
        {
            id = this.PersistentId,
            vertexLabel = GetVertexLabel(), // << MODIFIED: Get the label
            rowsData = GetAllRowsData()     // GetAllRowsData will now exclude the dummy row
        };

        // ... (Your existing debug logs for rowsData count can remain) ...
        Debug.Log($"[VertexController GetExportData for ID {this.PersistentId}] " +
                  $"Label: '{data.vertexLabel}', " +
                  $"Exporting with rowsData count: {(data.rowsData?.Count ?? -1)}. " +
                  $"Internal _managedRows count (incl. dummy if present): {(_managedRows?.Count ?? -1)}.", this);

        return data;
    }

    [ContextMenu("Print All Row Data to Console")]
    public void PrintAllRowDataToConsole()
    {
        // Ensure rows are loaded if we try to print them before activation
        if (_managedRows == null)
        {
            Debug.LogWarning($"[VertexController PrintAllRowData] '{gameObject.name}' (ID: {PersistentId}): _managedRows was null. Attempting to find/sort rows before printing.", this);
            FindAndSortRows();
        }

        Debug.Log($"--- Data for Vertex: {gameObject.name} (ID: {PersistentId}) ---", this);
        Debug.Log($"Attempting to print from {(_managedRows?.Count ?? 0)} managed rows.", this);
        List<RowData> allData = GetAllRowsData();

        if (allData.Count == 0)
        {
            Debug.Log("No row data found or no rows currently managed by this vertex.", this);
            return;
        }

        for (int i = 0; i < allData.Count; i++)
        {
            Debug.Log($"Row {i + 1} Data (from '{allData[i].RowObjectName}'): {allData[i].ToString()}", this);
        }
        Debug.Log($"--- End Data for Vertex: {gameObject.name} ---", this);
    }
}