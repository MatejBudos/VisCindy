import igraph as ig
import matplotlib.pyplot as plt
import numpy as np
import json
class Layouter:
    def __init__( self, vertices, edges ) -> None:
        
        
        self.g = ig.Graph()
        self.g.add_vertices( vertices )
        self.g.add_edges( edges )
        self.nodesNum = self.g.vcount()
        self.layout_functions = {
        "grid": self.g.layout_grid_3d,
        "sphere": self.g.layout_sphere,
        "kk": self.g.layout_kamada_kawai_3d,
        "fr": self.g.layout_fruchterman_reingold_3d,
        "random": self.g.layout_random_3d
        }
    def layout( self, layout_type : str = "random") -> None:
        layout_function = self.layout_functions.get(layout_type, self.g.layout_random_3d)
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
        data["edges"] = self.route_edges()
        with open("graph_data.json", "w") as f:
            json.dump(data, f, indent=3)
    
        return data

    def draw( self, draw_edges = True ):
        fig = plt.figure()
        ax = fig.add_subplot(111, projection="3d")
        x_coords, y_coords, z_coords = zip( *[ node[ "coords" ] for node in self.g.vs ] )
        ax.scatter( x_coords, y_coords, z_coords, c="blue", marker="o" )
        for idx, (x, y, z) in enumerate(zip(x_coords, y_coords, z_coords)):
            ax.text(x, y, z, str(idx), color='red', fontsize=10)
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








if __name__ == "__main__":
    l = Layouter([i for i in range( 15 )],
                        [(0,1),(1,2),(2,3),(3,4),(4,5),(5,6),
                       (6,7),(7,8),(8,9),(9,10),(10,11),(11,12),
                       (12,13),(13,14),(14,0)])
    
    l = Layouter([i for i in range( 15 )], 
                        [(0, 1), (0, 2), (0, 3), (1, 2), 
                       (1, 4), (2, 5),(3, 6), (4, 7), 
                       (5, 8), (6, 9), (7, 10), (8, 11),
                       (9, 12), (10, 13), (11, 14), (12, 13), 
                       (13, 14),(3, 7), (5, 10), (8, 12) ])
    l.layout("sphere")
    l.draw()
    l.export()
  

  
   
