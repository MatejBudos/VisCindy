from flask import Flask, request
from flask_restful import Api, reqparse
from graphDB_client import DBClient
from layouter import Layouter
from graphDB_client import DBClient
from layouter import Layouter
from flask_restful import Resource

app = Flask( __name__ )
api = Api( app )
query_args = reqparse.RequestParser()
query_args.add_argument("query", type = str, help = "No query provided")


class GraphManager:
    def __init__(self):
        self.graph = None

    def set_graph(self, graph):
        self.graph = graph

    def get_graph(self):
        return self.graph
    



class GraphServiceApi(Resource):
    def __init__(self) -> None:
        self.client = DBClient()
        self.layouter = Layouter()
    
    def get( self, layout_type : str ):
        graph = self.layouter.layout( graph_manager.get_graph(), layout_type )
        return {"message": "Success", "data": self.layouter.export( graph )}, 200

    def post(self):
        data = request.get_json()
        query = data.get("query")
        records = self.client.execute_query(query)
        graph = self.client.records_to_Igraph( records )
        graph_manager.set_graph(graph)
        return {"message": "Query executed successfully", "data": records}, 200





api.add_resource( GraphServiceApi, "/api/query","/api/<string:layout_type>" )




if __name__ == "__main__":
    graph_manager = GraphManager()
    app.run( debug = True )