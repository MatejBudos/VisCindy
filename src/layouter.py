import igraph as ig
import matplotlib.pyplot as plt
import numpy as np
import json
from flask import request
from flask_restful import Resource
class Layouter(Resource):
    
    def __init__( self ) -> None:
        self.g = ig.Graph()
        self.layout_functions = { 
        "grid": self.g.layout_grid_3d,
        "sphere": self.g.layout_sphere,
        "kk": self.g.layout_kamada_kawai_3d,
        "fr": self.g.layout_fruchterman_reingold_3d,
        "random": self.g.layout_random_3d
        }

    def set_graph( self, vertices, edges ) -> None:
        self.g.clear()
        self.g.add_vertices( vertices )
        self.g.add_edges( edges ) 

    def layout( self, layout_type : str = "random" ) -> None:
        try:
            layout_function = self.layout_functions[layout_type]
        except KeyError:
            raise KeyError(f"Layout {layout_type} does not exist")
        layout = layout_function()
        for index, coord in enumerate( layout ):
            self.g.vs[ index ][ "coords" ] = coord
        
        return layout
       

    def route_edges( self ):
        #(v1,v2) : {"start" : (x,y,z), "end" : (x,y,z)} 
        edges = {}
        for edge in self.g.es:  
            source = edge.source_vertex
            target = edge.target_vertex
            edges[ edge.index ] = { "start": source[ "coords" ], 
                                    "end": target[ "coords" ] }
        return edges

    def export( self ):
        data = { "nodes":{}, "edges":{} }
        for node in self.g.vs:
            data[ "nodes" ][ node.index ] = { "coords": node["coords"] }
       
        data[ "edges" ] = self.route_edges()
        #with open("graph_data.json", "w") as f:
        #   json.dump(data, f, indent=3)
    
        return data

    def get( self, layout_type : str ):
        if self.g.vcount() == 0:
            self.set_graph([i for i in range( 15 )], 
                        [(0, 1), (0, 2), (0, 3), (1, 2), 
                       (1, 4), (2, 5),(3, 6), (4, 7), 
                       (5, 8), (6, 9), (7, 10), (8, 11),
                       (9, 12), (10, 13), (11, 14), (12, 13), 
                       (13, 14),(3, 7), (5, 10), (8, 12) ])

        self.layout( layout_type )
        return {"message": "Success", "data": self.export()}, 200
       


    def draw( self, draw_edges = True ):
        fig = plt.figure()
        ax = fig.add_subplot(111, projection="3d")

        for vertex in self.g.vs:
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
    





