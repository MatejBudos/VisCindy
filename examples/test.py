import requests
URL = 'http://127.0.0.1:5000/api/'
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
records = requests.post( URL + "query", json=query)
print(records.json())


layout_type = "grid" #grid, sphere, kk, fr, random
response = requests.get( URL + layout_type) 
print(response.json())
