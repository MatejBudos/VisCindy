
from neo4j import GraphDatabase
import json
from flask import request, session
import igraph as ig
from flask_restful import Resource
from layouter import Layouter
class DBClient(Resource):
    def __init__( self ) -> None:
        with open('authentification.json', 'r') as file:
            data = json.load(file)
            self.driver = self.connect( data["URI"], (data["Username"],data["NEO4J_PASSWORD"]) )
      
    def connect( self, uri, auth ) -> bool:
        driver = GraphDatabase.driver(uri, auth=auth)
        #ak vyhodi exception je mozne ze auradb instance sa stopla
        driver.verify_connectivity()
        return driver
    
    def execute_query( self, query : str, params : dict = {}, database : str = "neo4j"):
        with self.driver.session(database=database) as session:
            result = session.run(query, params)
            records = [record.data() for record in result]
            return records
        
    

    def get(self, graphId):
        query = """
        MATCH (n {graphId: $graphId})
        OPTIONAL MATCH (n)-[r]->(m)
        WITH
            Id(n) AS id,
            collect(CASE
                WHEN m IS NOT NULL THEN {source: Id(n), target: Id(m), relationship: type(r)}
                ELSE null
            END) AS edges
        RETURN
            id,
            [edge IN edges WHERE edge IS NOT NULL] AS edges;
        """
        params = {"graphId": graphId}
        records = self.execute_query(query, params)
        if not records:
            return {}, 500
        session["records"] = records
        session["graphId"] = graphId 
        l = Layouter()
        graph = l.records_to_Igraph( records )
        graph = l.layout( graph )
        json_graph = l.export( graph )
        return json_graph
    

    
if __name__ == "__main__":
    with open('authentification.json', 'r') as file:
        data = json.load(file)
    db = DBClient()
    res = db.get( 2 )
    
    print(res)

    
