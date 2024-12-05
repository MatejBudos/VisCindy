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

layouter_args = reqparse.RequestParser()
layouter_args.add_argument("graph", type = str, help = "No graph provided")



api.add_resource( Layouter, "/api/layouter" )
api.add_resource( DBClient, "/api/query" )




if __name__ == "__main__":
    app.run( debug = True )