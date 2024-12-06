import requests
import time
URL = 'http://127.0.0.1:5000/api/'
session = requests.Session()

q = "MATCH (n)\
            OPTIONAL MATCH (n)-[r]->(m)\
            WITH\
            Id(n) AS id,\
            collect(CASE\
                WHEN m IS NOT NULL THEN {source: Id(n), target: Id(m), relationship: type(r)}\
                ELSE NULL\
            END) AS edges\
            RETURN\
            id,\
            [edge IN edges WHERE edge IS NOT NULL] AS edges;\
            "
query = {"query":q}
query_response = session.post(URL + "query", json=query)

if query_response.status_code == 200:
    print("Query response:", query_response.json())
else:
    print("Query failed with status code:", query_response.status_code)

response = query_response.json()
print(response)


layout_type = "grid"   # Options: grid, sphere, kk, fr, random
graph = {"data":query_response.json()["data"], "layout_type": layout_type}

layout_response = session.post(URL +"layouter",json=graph)
if layout_response.status_code == 200:
    print("Layout response:", layout_response.json())
else:
    print("Layout request failed with status code:", layout_response.status_code)

# Close the session
session.close()

