from neo4j import GraphDatabase
import json
from flask import request, session
import igraph as ig
from flask_restful import Resource
from layouter import Layouter
from graphDB_client import DBClient

class GraphGetter( Resource ):
    def __init__(self):
        self.client = DBClient()

    def get(self, graphId):
        query = """
        MATCH (n {graphId: $graphId})
        OPTIONAL MATCH (n)-[r]-(m)
        WITH
            Id(n) AS id,
            elementId(n) as NeoId,
            collect(CASE
                WHEN m IS NOT NULL THEN {source: Id(n), target: Id(m), relationship: type(r), NeoId: elementId(r)}
                ELSE null
            END) AS edges
        RETURN
            id, NeoId,
            [edge IN edges WHERE edge IS NOT NULL] AS edges;
        """
        params = {"graphId": graphId}
       
        records = self.client.execute_query(query, params)
        if not records:
            return {}, 500
        self.saveToSession( records, graphId )
        return self.layoutRecords( records )
    
    def post( self ):
        data = request.get_json()
        query = data["query"]
        apoc = data["apoc"]
        print(apoc)
        

        records = self.client.execute_query( query )
        #dat na true
        print(records)
        if not apoc:
            return self.layoutApoc( records )
        return self.layoutRecords( records )
    
    def layoutApoc( self, records ):
        path = records[0]["path"]
        neoIds = records[0]["NeoIds"]
        result = []
        for i, node in enumerate(neoIds):
            result.append({ "id": i, "NeoId":neoIds[i],"edges": []})
            if i < len(neoIds) - 1:
                result[i]["edges"].append({"source":i, "target":i+1,"NeoId": i})
        print(result)
        return self.layoutRecords( result )


    def saveToSession( self, records, graphId ):
        session["records"] = records
        session["graphId"] = graphId

    def layoutRecords( self, records ):
        l = Layouter()
        graph = l.records_to_Igraph( records )
        graph = l.layout( graph,"kk" )
        json_graph = l.export( graph )
        return json_graph
