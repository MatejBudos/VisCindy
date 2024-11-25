
from neo4j import GraphDatabase
import json

import igraph as ig

class DBClient:
    def __init__( self, uri : str, auth: tuple ) -> None:
        self.driver = self.connect( uri, auth )
      
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
        
    def edges_to_Igraph( self, records : dict, graph : ig.Graph ) -> ig.Graph:
        for record in records:
            graph.add_edge( str(record['source']), str(record['target']) )
        return graph
        
    def vertices_to_Igraph( self, records : dict, graph : ig.Graph  ) -> ig.Graph:
        for record in records:
            graph.add_vertex( name = str(record["id"]) )
        return graph
   
    
if __name__ == "__main__":
    with open('authentification.json', 'r') as file:
        data = json.load(file)
    db = DBClient( data["URI"], (data["Username"],data["NEO4J_PASSWORD"]))
    res2 = db.execute_query( "MATCH (n)\
                            RETURN Id(n) as id;")
    graph = ig.Graph()
    graph = db.vertices_to_Igraph( res2, graph )
    res = db.execute_query( "MATCH (n)-[r]->(m)\
                            RETURN \
                            Id(n) AS source, \
                            Id(m) AS target, \
                            type(r) AS relationship")
    graph = db.edges_to_Igraph( res, graph )
    print(graph)
