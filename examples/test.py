import requests
import time
import json
URL = 'http://127.0.0.1:5000/api/'
session = requests.Session()

query_response = session.get(URL + "graph/1")

if query_response.status_code == 200:
    response = query_response.json()
    print("Query response:", response)
else:
    print("Query failed with status code:", query_response.status_code)


query = """
        MATCH (n {graphId: 1})
        OPTIONAL MATCH (n)-[r]->(m)
        WITH
            Id(n) AS id,
            elementId(n) as NeoId,
            collect(CASE
                WHEN m IS NOT NULL THEN {source: Id(n), target: Id(m), relationship: type(r), NeoId: elementId(r)}
                ELSE null
            END) AS edges
        RETURN
            id, NeoId,
            [edge IN edges WHERE edge IS NOT NULL] AS edges;
        """
payload = {"query":query}
query_response = session.post(URL + "graph/query", json=payload)
if query_response.status_code == 200:
    response = query_response.json()
    print("Query response:", response)
else:
    print("Query failed with status code:", query_response.status_code)



#with open("graph_data.json", "w") as json_file:
    #json.dump(response, json_file, indent=4)