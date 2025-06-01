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
    [Header("Row Management Settings")]
    [Tooltip("The Row Prefab to instantiate.")]
    public GameObject rowPrefab; // Assign your modified Row Prefab (e.g., "VertexRowSubButons")
    [Tooltip("The Transform under which new rows will be parented (e.g., a LayoutGroup GameObject).")]
    public Transform rowsParentContainer; // Assign the parent for rows (e.g., "Row" object in image_278939.png which has a LayoutGroup)
    [Tooltip("Horizontal increment for the 'Offset' child of new rows.")]
    public float rowOffsetXIncrement = 30f;
    [Tooltip("The X offset for the very first data row ('Row 0').")]
    public float initialDataRowOffsetX = 0f;

    private int _nextAvailableRowNumber = 0;


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
        RowController.OnRequestNewRowAdd += HandleRowAddRequest;
        SieraHandler.OnGraphNodePropertiesAvailable += HandleGraphNodePropertiesAvailable; // << SUBSCRIBE to event
        
        InitializeAndSortRows();
        EnsureInitialDataRow();
        
    }
    void OnDestroy()
    {
        RowController.OnRequestNewRowAdd -= HandleRowAddRequest;
        SieraHandler.OnGraphNodePropertiesAvailable -= HandleGraphNodePropertiesAvailable; // << UNSUBSCRIBE from event
    }
    private void HandleGraphNodePropertiesAvailable()
    {
        Debug.Log($"[VertexController] '{gameObject.name}' (ID: {PersistentId}) received OnGraphNodePropertiesAvailable event. Updating attribute dropdowns for its rows.", this);
        if (_managedRows != null && SieraHandler.Instance != null)
        {
            List<string> nodeProps = SieraHandler.Instance.LastFetchedNodeProperties;
            foreach (RowController rowCtrl in _managedRows)
            {
                if (rowCtrl != null)
                {
                    rowCtrl.PopulateAttributeDropdown(nodeProps);
                }
            }
        }
    }
    
    
    

    /// <summary>
    /// Finds all RowController components in children of this GameObject (DropdownMenu)
    /// and sorts them by OffsetX. (true) includes inactive children.
    /// </summary>
    private void FindAndSortRows()
    {
        if (rowsParentContainer == null) {
            Debug.LogError($"VertexController on '{gameObject.name}': Rows Parent Container is not assigned. Cannot find or manage rows.", this);
            _managedRows = new List<RowController>(); // Ensure it's not null
            return;
        }
        // Get RowControllers only from direct children of rowsParentContainer to avoid nested issues.
        _managedRows = new List<RowController>();
        for(int i = 0; i < rowsParentContainer.childCount; i++)
        {
            RowController rc = rowsParentContainer.GetChild(i).GetComponent<RowController>();
            if(rc != null)
            {
                _managedRows.Add(rc);
            }
        }

        if (_managedRows.Count > 0)
        {
            _managedRows = _managedRows.OrderBy(row => row.GetOffsetX()).ToList();
        }
    }

    private void InitializeAndSortRows()
    {
        FindAndSortRows(); // Populates _managedRows
        // Determine the next available row number based on existing rows
        _nextAvailableRowNumber = 0;
        if (_managedRows != null) {
            foreach (var rowCtrl in _managedRows)
            {
                if (rowCtrl.gameObject.name.StartsWith("Row "))
                {
                    if (int.TryParse(rowCtrl.gameObject.name.Substring(4), out int existingIndex))
                    {
                        if (existingIndex >= _nextAvailableRowNumber)
                        {
                            _nextAvailableRowNumber = existingIndex + 1;
                        }
                    }
                }
            }
        }
        Debug.Log($"[VertexController Awake/Init] '{gameObject.name}' (ID: {PersistentId}) initialized. Found {_managedRows?.Count ?? 0} rows. Next row number: {_nextAvailableRowNumber}.", this);
    }
    private void EnsureInitialDataRow()
    {
        if (GetAllRowsData().Count == 0) 
        {
            Debug.Log($"[VertexController EnsureInitialDataRow] No data rows found for '{gameObject.name}'. Creating 'Row 0'.", this);
            // AddNewRowInternal already attempts to populate the dropdown.
            AddNewRowInternal(null, "INITIAL", initialDataRowOffsetX);
        }
    }
    
    private void HandleRowAddRequest(RowController sourceRowController, string buttonType)
    {
        // Ensure the request is coming from a row managed by this VertexController instance
        if (sourceRowController != null && _managedRows != null && _managedRows.Contains(sourceRowController))
        {
            float sourceOffsetX = sourceRowController.GetOffsetX();
            AddNewRowInternal(sourceRowController, buttonType, sourceOffsetX + rowOffsetXIncrement);
        }
        else if (sourceRowController != null)
        {
            Debug.LogWarning($"VertexController for '{PersistentId}' received row add request from an unmanaged or unknown RowController: '{sourceRowController.gameObject.name}'. Request ignored.", this);
        }
    }
    
    private string GetDisplayLabelFromDropdownMenu() // Renamed to be specific
    {
        if (vertexLabelTextComponent != null)
        {
            // Debug.Log($"[{gameObject.name}] GetDisplayLabelFromDropdownMenu returning: '{vertexLabelTextComponent.text}'");
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
                        vertexLabelTextComponent = tmpText; // Cache it for next time
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
            Debug.LogWarning($"VertexLabelTextComponent not assigned and 'Canvas/Label' with a Text component not found on '{gameObject.name}'. Display label from dropdown menu will be empty.", this);
            return string.Empty; // Or a default name, or null
        }
    }
    
    /// <summary>
    /// Derives the "vX" style ID from the root vertex GameObject's name.
    /// Falls back to PersistentId (GUID) if parsing fails.
    /// </summary>
    public string GetDerivedExportIdAndLabel()
    {
        if (_vertexRootTransform != null)
        {
            string rootName = _vertexRootTransform.gameObject.name; // e.g., "Vertex_v0" or potentially just "v0"
            if (rootName.StartsWith("Vertex_v") && rootName.Length > "Vertex_v".Length)
            {
                return rootName.Substring("Vertex_".Length); // Extracts "v0", "v1", etc.
            }
            else if (rootName.StartsWith("v") && rootName.Length > 1 && char.IsDigit(rootName[1]))
            {
                // Handles cases where the GameObject name might just be "v0", "v1", etc.
                return rootName;
            }
            else
            {
                Debug.LogWarning($"Vertex root name '{rootName}' on '{gameObject.name}' (root: '{_vertexRootTransform.name}') does not follow 'Vertex_vX' or 'vX' pattern. Falling back to PersistentId for export ID.", _vertexRootTransform.gameObject);
            }
        }
        else
        {
            Debug.LogError($"VertexController on '{gameObject.name}' has no _vertexRootTransform. Falling back to PersistentId for export ID.", this);
        }
        return this.PersistentId; // Fallback to the GUID
    }
    
    private RowController AddNewRowInternal(RowController sourceRow, string triggerType, float targetOffsetX)
    {
        if (rowPrefab == null) { Debug.LogError("Row Prefab not assigned in VertexController!", this); return null; }
        if (rowsParentContainer == null) { Debug.LogError("Rows Parent Container not assigned in VertexController!", this); return null; }

        GameObject newRowGO = Instantiate(rowPrefab, rowsParentContainer);
        RowController newRowCtrl = newRowGO.GetComponent<RowController>();

        if (newRowCtrl != null)
        {
            string newRowName = $"Row {_nextAvailableRowNumber}";
            string sourceRowName = sourceRow != null ? sourceRow.gameObject.name : "NONE";
            newRowCtrl.ConfigureRow(newRowName, sourceRowName, triggerType, targetOffsetX);
            if (SieraHandler.Instance != null && SieraHandler.Instance.LastFetchedNodeProperties != null)
            {
                newRowCtrl.PopulateAttributeDropdown(SieraHandler.Instance.LastFetchedNodeProperties);
            }
            else
            {
                // Properties might not be fetched yet. The event HandleGraphNodePropertiesAvailable will cover it later.
                // Or, provide a default empty state.
                newRowCtrl.PopulateAttributeDropdown(new List<string>()); // Initialize with empty or placeholder
            }
            
            _nextAvailableRowNumber++; // Increment for the next one
        }
        else
        {
            Debug.LogError("Instantiated row prefab is missing RowController component!", newRowGO);
            Destroy(newRowGO);
            return null;
        }

        newRowGO.SetActive(true);
        
        // Re-scan and re-sort all rows. This also updates _managedRows.
        RefreshManagedRows();
        Debug.Log($"Added new row '{newRowCtrl.gameObject.name}' (Source: {newRowCtrl.sourceRowName}, Trigger: {newRowCtrl.triggerButtonType}, OffsetX: {newRowCtrl.GetOffsetX()}).", newRowCtrl);
        return newRowCtrl;
    }

    
    
    public void RefreshManagedRows() 
    {
        Debug.Log($"[VertexController Refresh] '{gameObject.name}' (ID: {PersistentId}) is refreshing its managed rows...", this);
        FindAndSortRows(); // Re-scans children of rowsParentContainer and sorts them
        // Update _nextAvailableRowNumber based on the potentially new set of rows
        // This is important if rows can also be deleted, or if Refresh is called for other reasons.
        int maxIndexFound = -1;
        if (_managedRows != null) {
            foreach (var rowCtrl in _managedRows)
            {
                if (rowCtrl.gameObject.name.StartsWith("Row "))
                {
                    if (int.TryParse(rowCtrl.gameObject.name.Substring(4), out int existingIndex))
                    {
                        if (existingIndex > maxIndexFound)
                        {
                            maxIndexFound = existingIndex;
                        }
                    }
                }
            }
        }
        _nextAvailableRowNumber = maxIndexFound + 1;
        Debug.Log($"[VertexController Refresh] '{gameObject.name}' (ID: {PersistentId}) refreshed. Now managing {_managedRows?.Count ?? 0} rows. Next row number is {_nextAvailableRowNumber}.", this);
    }

    public List<RowData> GetAllRowsData()
    {
        if (_managedRows == null)
        {
            Debug.LogWarning($"[VertexController GetAllRowsData] '{gameObject.name}' (ID: {PersistentId}): _managedRows is null. Attempting to initialize.", this);
            FindAndSortRows(); // Try to initialize if null
            if (_managedRows == null) return new List<RowData>(); // Still null, return empty
        }

        List<RowData> actualRowsData = new List<RowData>();
        int startIndex = 0; // << Process ALL rows found in _managedRows

        for (int i = startIndex; i < _managedRows.Count; i++)
        {
            RowController rowController = _managedRows[i];
            if (rowController != null)
            {
                actualRowsData.Add(rowController.GetRowData());
            }
        }
        Debug.Log($"[VertexController GetAllRowsData] '{gameObject.name}' (ID: {PersistentId}): Returning {actualRowsData.Count} data rows from {_managedRows.Count} managed rows.", this);
        return actualRowsData;
    }

    
    
    public string GetVertexLabel()
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
            return string.Empty;
        }
    }

    public VertexExportData GetExportData()
    {
        if (_managedRows == null)
        {
            Debug.LogWarning($"[VertexController GetExportData] '{gameObject.name}' (PersistentID: {this.PersistentId}): _managedRows was null. Attempting to find and sort rows now...", this);
            FindAndSortRows();
        }

        string derivedIdAndLabel = GetDerivedExportIdAndLabel();

        VertexExportData data = new VertexExportData
        {
            id = derivedIdAndLabel,
            vertexLabel = derivedIdAndLabel,
            rowsData = GetAllRowsData()
        };

        Debug.Log($"[VertexController GetExportData for Export ID '{data.id}'] " +
                  $"Label: '{data.vertexLabel}', " +
                  $"Exporting with rowsData count: {(data.rowsData?.Count ?? -1)}. " +
                  $"Internal _managedRows count: {(_managedRows?.Count ?? -1)}.", this);

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
            // OLD LINE THAT CAUSED THE ERROR:
            // Debug.Log($"Row {i + 1} Data (from '{allData[i].RowObjectName}'): {allData[i].ToString()}", this);

            // CORRECTED LINE:
            Debug.Log($"Row {i + 1} Data (from '{allData[i].tag}'): {allData[i].ToString()}", this);
        }
        Debug.Log($"--- End Data for Vertex: {gameObject.name} ---", this);
    }
}