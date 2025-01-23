from neo4j import GraphDatabase
import json
from flask import request, session
import igraph as ig
from flask_restful import Resource
from layouter import Layouter

class DBClient():
    def __init__(self) -> None:
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

    def close(self):
        """Close the Neo4j driver to release resources."""
        if self.driver:
            self.driver.close()

if __name__ == "__main__":
    with open('authentification.json', 'r') as file:
        data = json.load(file)
    db = DBClient()
    res = db.get( 2 )
    print(res)
    db.close()
