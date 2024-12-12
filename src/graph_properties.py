from neo4j import GraphDatabase
from graphDB_client import DBClient
import json
from flask import request, session
import igraph as ig
from flask_restful import Resource

class GraphProperties(Resource):
    
    def get(self, graphId : int ):
        q1 = """
        MATCH (n {graphId: $graphId})-[r]-(n1 {graphId: $graphId})
        WITH KEYS(r) AS keys
        UNWIND keys AS key
        RETURN COLLECT(DISTINCT key) AS edge_properties
        """
        
        q2 = """
        MATCH (n {graphId: $graphId})-[r]-(n1 {graphId: $graphId})
        RETURN COLLECT(DISTINCT type(r)) AS relation_types
        """
        q3 = """
        MATCH (n {graphId: $graphId})-[r]->()
        UNWIND keys(n) AS node_keys
        UNWIND labels(n) AS node_labels
        RETURN collect(DISTINCT node_keys) AS node_properties, 
        collect(DISTINCT node_labels) AS node_labels
        """
        params = {"graphId": graphId}
        db = DBClient()
        edge_properties = db.execute_query( q1, params )[0]
        relation_types = db.execute_query( q2, params )[0]
        node_properties_labela = db.execute_query( q3, params )[0]
        return {**edge_properties, **relation_types, **node_properties_labela}

        


if "__main__" == __name__:
    p = GraphProperties()
    print(p.get( 1 ))