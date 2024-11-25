import igraph as ig
import matplotlib.pyplot as plt
import numpy as np
import json

class Layouter:
    
    def __init__( self ) -> None:
        pass
        
    def layout( self, graph : ig.Graph, layout_type : str = "random" ) -> None:
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
            self.g.vs[ index ][ "coords" ] = coord
        
        return layout

    def route_edges( self, graph: ig.Graph ):
        #(v1,v2) : {"start" : (x,y,z), "end" : (x,y,z)} 
        edges = {}
        for edge in graph:  
            source = edge.source_vertex
            target = edge.target_vertex
            edges[ edge.index ] = { "start": source[ "coords" ], 
                                    "end": target[ "coords" ] }
        return edges

    def export( self, graph : ig.Graph ):
        data = { "nodes":{}, "edges":{} }
        for node in graph:
            data[ "nodes" ][ node.name ] = { "coords": node["coords"] }
       
        data[ "edges" ] = self.route_edges( graph )
        #with open("graph_data.json", "w") as f:
        #   json.dump(data, f, indent=3)
    
        return data

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





