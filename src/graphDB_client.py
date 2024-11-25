
from neo4j import GraphDatabase
import json
from layouter import Layouter
import igraph as ig

class DBClient:
    def __init__( self, uri : str, auth: tuple ) -> None:
        self.driver = self.connect( uri, auth )
        self.g = ig.Graph()
        self.layouter = Layouter()

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
        
    def edges_to_Igraph( self, records : dict ) -> None:
        for record in records:
            self.g.add_edge( str(record['source']), str(record['target']) )
        
    def vertices_to_Igraph( self, records : dict ) -> None:
        for record in records:
            self.g.add_vertex( name = str(record["id"]) )

    def layout( self, layout_type : str ):
        self.layouter.layout( layout_type )
        self.layouter.draw()
        return self.layouter.export()


        
    
if __name__ == "__main__":
    with open('authentification.json', 'r') as file:
        data = json.load(file)
    db = DBClient( data["URI"], (data["Username"],data["NEO4J_PASSWORD"]))
    res2 = db.execute_query( "MATCH (n)\
                            RETURN elementId(n) as id;")
    db.vertices_to_Igraph( res2 )
    res = db.execute_query( "MATCH (n)-[r]->(m)\
                            RETURN \
                            elementId(n) AS source, \
                            elementId(m) AS target, \
                            type(r) AS relationship")
    db.edges_to_Igraph( res )
    db.layouter.set_graph( db.g )
    db.layout("grid")
