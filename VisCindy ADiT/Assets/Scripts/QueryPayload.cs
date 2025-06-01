using System;

[System.Serializable]
public class QueryPayload
{
    public string query;
    public bool apoc;

    public QueryPayload( string query , bool apoc = false ){
        this.query = query;
        this.apoc = apoc;
    }
}