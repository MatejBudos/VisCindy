import requests
URL = 'http://127.0.0.1:5000/api/'
layout_type = "grid"
response = requests.get( URL + layout_type) 
print(response.json())