from flask import Flask, request
from flask_restful import Api, reqparse
from graphDB_client import DBClient
from layouter import Layouter
from graphDB_client import DBClient
from layouter import Layouter

app = Flask( __name__ )
api = Api( app )
query_args = reqparse.RequestParser()
query_args.add_argument("query", type = str, help = "No query provided")


#layouter_args = reqparse.RequestParser()
#layouter_args.add_argument("layout_type", type = str, help = "No layout type provided")
#layouter_args.add_argument("nodes", type = str, help = "No nodes provided")


api.add_resource( DBClient, "/api/query" )
api.add_resource( Layouter, "/api/<string:layout_type>" )



if __name__ == "__main__":
    app.run( debug = True )