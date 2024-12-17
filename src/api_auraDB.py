from flask import Flask, request, session
from flask_restful import Api, reqparse
from graphDB_client import DBClient
from layouter import Layouter
from graphDB_client import DBClient
from layouter import Layouter
from graph_properties import GraphProperties

app = Flask( __name__ )
app.secret_key = "test123"
api = Api( app )
query_args = reqparse.RequestParser()
query_args.add_argument("query", type = str, help = "No query provided")

layouter_args = reqparse.RequestParser()
layouter_args.add_argument("graph", type = str, help = "No graph provided")



api.add_resource( Layouter, "/api/layouter/<string:layout_type>" )
api.add_resource( DBClient, "/api/graph/<int:graphId>" )
api.add_resource( GraphProperties, "/api/properties/<int:graphId>" )




if __name__ == "__main__":
    app.run( debug = True )