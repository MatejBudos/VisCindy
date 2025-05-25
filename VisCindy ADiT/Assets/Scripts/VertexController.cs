using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy

public class VertexController : MonoBehaviour
{
    private List<RowController> _managedRows;

    void Awake()
    {
        // Initial population of rows
        InitializeAndSortRows();
    }

    /// <summary>
    /// Finds all RowController components in children and sorts them by OffsetX.
    /// This is the core logic for populating/refreshing the _managedRows list.
    /// </summary>
    private void FindAndSortRows()
    {
        // GetComponentsInChildren<RowController>(true) will include inactive GameObjects.
        // If you only want active rows, use GetComponentsInChildren<RowController>(false) or omit the boolean.
        _managedRows = new List<RowController>(GetComponentsInChildren<RowController>(true));

        if (_managedRows.Count == 0)
        {
            Debug.LogWarning($"VertexController on '{gameObject.name}' found no RowController components in its children during scan.", this);
        }
        else
        {
            _managedRows = _managedRows.OrderBy(row => row.GetOffsetX()).ToList();
        }
    }

    /// <summary>
    /// Initializes the rows list. Typically called once in Awake.
    /// </summary>
    private void InitializeAndSortRows()
    {
        FindAndSortRows();
        Debug.Log($"VertexController on '{gameObject.name}' initialized with {_managedRows?.Count ?? 0} rows.", this);
    }

    /// <summary>
    /// Public method to explicitly re-scan and update the list of managed rows.
    /// Call this AFTER dynamically adding or removing row GameObjects as children of this Vertex.
    /// </summary>
    public void RefreshManagedRows()
    {
        Debug.Log($"VertexController on '{gameObject.name}' is refreshing its managed rows...", this);
        FindAndSortRows();
        Debug.Log($"VertexController on '{gameObject.name}' refreshed. Now managing {_managedRows?.Count ?? 0} rows.", this);
    }

    public List<RowData> GetAllRowsData()
    {
        if (_managedRows == null)
        {
            Debug.LogWarning($"VertexController on '{gameObject.name}': _managedRows is null. Ensure Awake has run or RefreshManagedRows() has been called after dynamic changes.", this);
            return new List<RowData>(); // Return empty list to prevent errors
        }

        List<RowData> allData = new List<RowData>();
        foreach (RowController rowController in _managedRows)
        {
            if (rowController != null) // Good practice
            {
                allData.Add(rowController.GetRowData());
            }
        }
        return allData;
    }

    [ContextMenu("Print All Row Data to Console")]
    public void PrintAllRowDataToConsole()
    {
        Debug.Log($"--- Data for Vertex: {gameObject.name} (Attempting to print {_managedRows?.Count ?? 0} managed rows) ---", this);
        List<RowData> allData = GetAllRowsData(); // This will use the current _managedRows list

        if (allData.Count == 0 && (_managedRows == null || _managedRows.Count == 0) ) // Check if genuinely no rows or list is empty
        {
            Debug.Log("No row data found or no rows currently managed by this vertex.", this);
            if (_managedRows == null) Debug.Log("(_managedRows list itself is null)", this);
            return;
        }
         else if (allData.Count == 0 && _managedRows != null && _managedRows.Count > 0)
        {
            Debug.LogWarning("Managed rows list is not empty, but GetAllRowsData returned no data. Check RowController.GetRowData().", this);
        }


        for(int i = 0; i < allData.Count; i++)
        {
            Debug.Log($"Row {i+1} Data (from '{allData[i].RowObjectName}'): {allData[i].ToString()}", this);
        }
        Debug.Log($"--- End Data for Vertex: {gameObject.name} ---", this);
    }
}