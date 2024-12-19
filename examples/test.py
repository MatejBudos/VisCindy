import requests
import time
import json
URL = 'http://127.0.0.1:5000/api/'
session = requests.Session()

query_response = session.get(URL + "graph/1")

if query_response.status_code == 200:
    print("Query response:", query_response.json())
else:
    print("Query failed with status code:", query_response.status_code)

response = query_response.json()
print(response)


with open("graph_data.json", "w") as json_file:
    json.dump(response, json_file, indent=4)