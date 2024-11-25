from flask import Flask
from flask_restful import Api, Resource, reqparse
from graphDB_client import DBClient
from layouter import Layouter
import json

app = Flask( __name__ )
api = Api( app )
query_args = reqparse.RequestParser()
query_args.add_argument("query", type = str, help = "No query provided")

class APIAuraDB(Resource):
    def __init__(self) -> None:
        with open('authentification.json', 'r') as file:
            data = json.load(file)
            self.client = DBClient( data["URI"], (data["Username"],data["NEO4J_PASSWORD"]))

        self.layouter = Layouter()
        self.graph = None


    def get( self, layout_type : str ):
        self.layouter.layout( layout_type )

    def put( self, query : str ):
        args = query_args.parse_args()


api.add_resource( APIAuraDB, "/api/query", "/api/<string:layout_type>" )




if __name__ == "__main__":
    app.run( debug = True )