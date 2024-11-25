
from neo4j import GraphDatabase
import json
from layouter import Layouter
import igraph as ig
class DBClient:
    def __init__( self, uri : str, auth: tuple ) -> None:
        self.driver = self.connect( uri, auth )
        self.g = ig.Graph()

    def connect( self, uri, auth ) -> bool:
        driver = GraphDatabase.driver(uri, auth=auth)
        #raises exceptions
        driver.verify_connectivity()
        return driver
    
    def execute_query( self, query : str, database : str = "neo4j"):
        with self.driver.session(database=database) as session:
            result = session.run(query)
            records = [record.data() for record in result]
            return records
        
    
        
    
if __name__ == "__main__":
    with open('authentification.json', 'r') as file:
        data = json.load(file)
    db = DBClient( data["URI"], (data["Username"],data["NEO4J_PASSWORD"]))
    res = db.execute_query( "MATCH (n)-[r]->(m)\
                            RETURN \
                            n { id: n.id} AS source, \
                            m { id: m.id} AS target, \
                            type(r) AS relationship")
    print(res)
