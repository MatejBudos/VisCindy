import igraph as ig
import matplotlib.pyplot as plt
import numpy as np
import json
from flask import request, session
from flask_restful import Resource
class Layouter(Resource):
    
    def layout( self, graph, layout_type : str = "random" ) -> None:
        layout_functions = { 
        "grid": graph.layout_grid_3d,
        "sphere": graph.layout_sphere,
        "kk": graph.layout_kamada_kawai_3d,
        "fr": graph.layout_fruchterman_reingold_3d,
        "random": graph.layout_random_3d
        }
        try:
            layout_function = layout_functions[layout_type]
        except KeyError:
            raise KeyError(f"Layout {layout_type} does not exist")
        layout = layout_function()
        for index, coord in enumerate( layout ):
            graph.vs[ index ][ "coords" ] = coord
        
        return graph
       

    def route_edges( self, graph ):
        #(v1,v2) : {"start" : (x,y,z), "end" : (x,y,z)} 
        edges = {}
        for edge in graph.es:  
            source = edge.source_vertex
            target = edge.target_vertex
            edges[ edge.index ] = { "start": source[ "coords" ], 
                                    "end": target[ "coords" ],
                                     "NeoId": edge["NeoId"] }
        return edges

    def export( self, graph ):
        data = { "nodes":{}, "edges":{} }
        for node in graph.vs:
            data[ "nodes" ][ node.index ] = {   "coords": node["coords"], 
                                                "NeoId": node["NeoId"]}

       
        data[ "edges" ] = self.route_edges( graph )
        #with open("graph_data.json", "w") as f:
        #   json.dump(data, f, indent=3)
    
        return data
    

    #neaktualne
    def get( self, layout_type : str ):
        if session.get("records", None ) is None:
            return {"Message":"Did not query graph"}, 500
        
        graph = self.records_to_Igraph( session["records"])
        graph = self.layout( graph, layout_type )
        return self.export( graph ), 200
        
    def post( self, layout_type : str ):
        data = request.get_json()["nodes"]
        graph = self.payload_to_Igraph( data )
        graph = self.layout( graph, layout_type)
        return self.export( graph ), 200
    
    def payload_to_Igraph( self, payload : dict ) -> ig.Graph:
        graph = ig.Graph()
        edges = []
        id_dict = {}
        for vertex, element in enumerate( payload, start = 1 ):
            node = element["node_id"]
            neighbours = element["edges"]
            vertex = str(vertex)
            graph.add_vertex( vertex )
            id_dict[ node ] = vertex
            graph.vs.find( name = vertex )["NeoId"] = node
            for target in neighbours:
                edges.append( ( node, target ) )
                
        for edge in edges:
            source, target = edge
            if id_dict.get(target, None):
                graph.add_edge( id_dict[source], id_dict[target] )

        #dummy hodnoty pre teraz
        for index in range(len(graph.es)):
            graph.es[ index ]["NeoId"] = "null"
        return graph   

    def records_to_Igraph( self, records : list ) -> ig.Graph:
        graph = ig.Graph()
        edges = []
        NeoEdgeIds = []
        print(records)
        for record in records:
            vertex = str(record['id'])
            graph.add_vertex( vertex )
            
            #add attribute to node "NeoId" : record["NeoId"]
            graph.vs.find(name=vertex)["NeoId"] = record["NeoId"]
            for edge in record['edges']:
                edges.append( ( vertex, str(edge['target']) ) )
                NeoEdgeIds.append( edge['NeoId'] )
        
        graph.add_edges( edges )
        for index, EdgeId in enumerate( NeoEdgeIds ):
            graph.es[ index ]["NeoId"] = EdgeId
        return graph   


    def draw( self, graph, draw_edges = True ):
        fig = plt.figure()
        ax = fig.add_subplot(111, projection="3d")

        for vertex in graph.vs:
            x, y, z = vertex["coords"]
            ax.scatter( x, y, z, c="blue", marker="o" )
            ax.text(x, y, z, vertex["name"], color='red', fontsize=10)
        if draw_edges:
            edges = self.route_edges()
            for edge in edges.values():
                Sx, Sy, Sz = edge[ "start" ]
                Ex, Ey, Ez = edge[ "end" ]
                plt.plot((Sx, Ex), (Sy, Ey),(Sz, Ez), label="Line")
        ax.set_xlabel("X")
        ax.set_ylabel("Y")
        ax.set_zlabel("Z")
        ax.set_title("3D Node Layout")
        plt.show()
    





