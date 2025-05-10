from neo4j import GraphDatabase
import random
import json


def connectToDB():
    with open('../src/authentification.json', 'r') as file:
        data = json.load(file)

        uri = data["URI"]
        username = data["Username"]
        password = data["NEO4J_PASSWORD"]

    return GraphDatabase.driver(uri, auth=(username, password))



    
def execute_query( query : str ):
    with driver.session() as session:
        result = session.run( query )
        records = [record.data() for record in result]
        return records

path_query = """
     MATCH (a {label: "Node_14", graphId: 2}),(b {label: "Node_8", graphId: 2})
CALL apoc.path.expandConfig(a, {
    terminatorNodes: [b],
    maxLevel: 10,
    uniqueness: "NODE_GLOBAL"
})
YIELD path
WITH path, [n IN nodes(path) |  elementId(n)] AS NeoIds
RETURN path, NeoIds

"""
driver = connectToDB()
print( execute_query( path_query ) ,'\n')

spanning_query = """MATCH (start {label: 'Node_2', graphId : 2 })
CALL apoc.path.spanningTree(start, {
  relationshipFilter: "CONNECTED>",
  labelFilter: "+Node",
  maxLevel: 10
})
YIELD path
WITH path, [n IN nodes(path) |  elementId(n)] AS NeoIds
RETURN path, NeoIds
"""

print(execute_query( spanning_query ))





query_all = """
MATCH (n)
WHERE n.graphId = 2
WITH collect(n) AS nodes
MATCH (start {label: 'Node_2' })
WHERE start in nodes
CALL apoc.path.spanningTree(start, {
  relationshipFilter: "CONNECTED>",
  labelFilter: "+Node",
  maxLevel: 10
})
YIELD path
WITH [n in nodes(path) ] as nodes
MATCH (a {label: "Node_2"}), (b {label: "Node_14"})
WHERE a IN nodes AND b IN nodes
CALL apoc.path.expandConfig(a, {
  terminatorNodes: [b],
  maxLevel: 10,
  uniqueness: "NODE_GLOBAL"
})
YIELD path
RETURN path, [n IN nodes(path) | elementId(n)] AS NeoIds
"""



