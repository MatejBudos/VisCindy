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
driver = connectToDB()
with driver.session() as session:
    result = session.run("""
     MATCH (a {label: "Node_14", graphId: 2}),(b {label: "Node_8", graphId: 2})
CALL apoc.path.expandConfig(a, {
    terminatorNodes: [b],
    maxLevel: 10,
    uniqueness: "NODE_GLOBAL"
})
YIELD path
WITH path, [n IN nodes(path) |  elementId(n)] AS NeoIds
RETURN path, NeoIds

""")
    records = [record.data() for record in result]
    print( records )