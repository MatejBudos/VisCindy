from neo4j import GraphDatabase
import random
import json

with open('../src/authentification.json', 'r') as file:
    data = json.load(file)

uri = data["URI"]
username = data["Username"]
password = data["NEO4J_PASSWORD"]

driver = GraphDatabase.driver(uri, auth=(username, password))
def clear_database():
    with driver.session() as session:
        session.run("MATCH (n) DETACH DELETE n")


def create_graph_with_3d_positions(node_count, graph_id):
    with driver.session() as session:
        
        # Create nodes with 3D positions
        for i in range(node_count):
            label = f"Node_{i}"
            description = f"This is node {i} in the graph."
            x, y, z = random.uniform(0, 100), random.uniform(0, 100), random.uniform(0, 100)
            
            session.run(
                """
                CREATE (n:Node {label: $label, description: $description, x: $x, y: $y, z: $z, graphId: $graph_id})
                """,
                label=label, description=description, x=x, y=y, z=z, graph_id = graph_id
            )
        
        print(f"Created {node_count} nodes.")

def generate_random_edges(max_edges, graph_id):
    
    NodeTypes = ["NodeTypeA", "NodeTypeB", "NodeTypeC" ]
    with driver.session() as session:
        query = """
        MATCH (n:Node) 
        WHERE n.graphId = $graph_id 
        RETURN n.label AS label
        """
        params = {"graph_id": graph_id}
        result = session.run(query, params)
        nodes = [record["label"] for record in result]
        
        relationShips = ["RelTypeA", "RelTypeB", "RelTypeC" ]
        # Create random edges
        for _ in range(max_edges):
            source, target = random.sample(nodes, 2)
            label = f"Edge_{source}_{target}"
            description = f"Random edge between {source} and {target}."
            nodeType = NodeTypes[ random.randrange( 3 ) ]

            relationShip = relationShips[ random.randrange( 3 ) ]
            session.run(
                """
                MATCH (n1:Node {label: $source, graphId : $graph_id}), (n2:Node {label: $target, graphId : $graph_id})
                CREATE (n1)-[:CONNECTED {label: $label, description: $description}]->(n2)
                """,
                source=source, target=target, relationShip = relationShip, label=label, description=description, graph_id=graph_id
            )
        
        print(f"Created up to {max_edges} random edges.")

if __name__ == "__main__":
    graph_count = int(input("Enter the number of graphs to create: "))
    node_count = int(input("Enter the number of nodes to create: "))
    max_edges = int(input("Enter the maximum number of edges to create: "))
    clear_database()
    for graph_id in range( 1, graph_count + 1):
        create_graph_with_3d_positions(node_count, graph_id)
        generate_random_edges(max_edges, graph_id)
    print("Graph creation complete!")
