using System.Collections.Generic; // For List<>
// Make sure RowData is accessible here, either defined in the same scope or via its namespace
// Assuming RowData class is already defined as per previous discussions

[System.Serializable]
public class GraphExportData
{
    public List<VertexExportData> vertices;
    public List<EdgeExportData> edges;
    public string limit;
    public Apoc apoc;

    public GraphExportData()
    {
        vertices = new List<VertexExportData>();
        edges = new List<EdgeExportData>();
        limit = "None";
    }
}

[System.Serializable]
public class VertexExportData
{
    public string id;             // Unique identifier for this vertex
    public string vertexLabel; 
    public List<RowData> rowsData; // Data from all (non-dummy) rows within this vertex

    public VertexExportData()
    {
        rowsData = new List<RowData>();
    }
}

[System.Serializable]
public class EdgeExportData
{
    public string fromVertexId; // ID of the vertex this edge starts from
    public string toVertexId;   // ID of the vertex this edge ends at
    public string edgeName;     // Optional: Name of the edge GameObject itself for debugging/identification
    
    public string relationshipType;
    public string minValue;
    public string maxValue;

    public EdgeExportData()
    {
        relationshipType = "CONNECTED"; 
        minValue = "None";
        maxValue = "None";
    }
}