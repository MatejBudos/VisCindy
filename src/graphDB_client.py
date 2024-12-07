
from neo4j import GraphDatabase
import json
from flask import request
import igraph as ig
from flask_restful import Resource
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
    
    def execute_query( self, query : str, database : str = "neo4j"):
        with self.driver.session(database=database) as session:
            result = session.run(query)
            records = [record.data() for record in result]
            return records
        
    
        
    def post(self):
        data = request.get_json()
        query = data.get("query")
        records = self.execute_query(query)
        return {"message": "Query executed successfully", "data": records}, 200
    

    
if __name__ == "__main__":
    with open('authentification.json', 'r') as file:
        data = json.load(file)
    db = DBClient()
    query = "MATCH (n)\
            OPTIONAL MATCH (n)-[r]->(m)\
            WITH\
            Id(n) AS id,\
            collect(CASE\
                WHEN m IS NOT NULL THEN {source: Id(n), target: Id(m), relationship: type(r)}\
                ELSE null\
            END) AS edges\
            RETURN\
            id,\
            [edge IN edges WHERE edge IS NOT NULL] AS edges;\
            "
    res = db.execute_query( query )
        
  
    for r in res:
        print(r)
    
