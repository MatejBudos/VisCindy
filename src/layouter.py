import igraph as ig
import matplotlib.pyplot as plt
import numpy as np
class Layouter:
    def __init__( self, nodesNum, edges ) -> None:
        self.nodesNum = nodesNum
        self.edges = edges
        self.g = ig.Graph( nodesNum, edges )



    def grid_layout( self, scale : int = 1) -> None:
        gridSize: int = round( self.nodesNum ** ( 1 / 3 ) )
        x,y,z = 0,0,0
        for node in self.g.vs:
            node["x"] = scale * x
            node["y"] = scale * y
            node["z"] = scale * z
            x += 1
            if x % gridSize == 0:
                y += 1
                x = 0
            
            if x % gridSize == 0 and y % gridSize == 0:
                z += 1
                y = 0
            

    def spherical_layout( self, scale : int = 1 ):
        phi = (1 + np.sqrt(5)) / 2 
        for num, node in enumerate(self.g.vs):
            theta = 2 * np.pi * (num / phi)
            z = 1 - (2 * num / (self.nodesNum - 1))  
            x = np.sqrt(1 - z * z) * np.cos(theta)
            y = np.sqrt(1 - z * z) * np.sin(theta)
            node["x"] = x * scale
            node["y"] = y * scale
            node["z"] = z * scale 


    def route_edges( self ):
        #(v1,v2) : {"start" : (x,y,z), "end" : (x,y,z)} 
        edges = {}
        for edge in self.g.es:  
            source = edge.source_vertex 
            target = edge.target_vertex 
            x_start, y_start = source[ "x" ], source[ "y" ]
            x_end, y_end = target[ "x" ], target[ "y" ]
            z_start, z_end = source[ "z" ], target[ "z" ]
            edges[ edge ] = {"start": ( x_start, y_start, z_start ), 
                             "end": ( x_end, y_end, z_end ) }
        return edges

    def draw( self ):
        self.spherical_layout( 10 )
        x_coords = [ node[ "x" ] for node in self.g.vs ] 
        y_coords = [ node[ "y" ] for node in self.g.vs ]
        z_coords = [ node[ "z" ] for node in self.g.vs ]
        fig = plt.figure()
        ax = fig.add_subplot(111, projection="3d")
        ax.scatter( x_coords, y_coords, z_coords, c="blue", marker="o" )
        edges = self.route_edges()
        for edge in edges.values():
            start = edge[ "start" ]
            end = edge[ "end" ]
            plt.plot((start[0], end[0]), (start[1], end[1]),(start[2], end[2]), label="Line")
        ax.set_xlabel("X")
        ax.set_ylabel("Y")
        ax.set_zlabel("Z")
        ax.set_title("3D Node Layout")
        plt.show()








if __name__ == "__main__":
    l = Layouter( 50, [(1, 2), (1, 25), (1, 27), (1, 43), (1, 44), (2, 3), (2, 5), (2, 9), 
                       (2, 15), (2, 30),  (2, 33), (2, 39), (3, 7), (3, 24), (3, 26), 
                       (3, 27), (3, 29), (3, 45), (3, 48), (4, 11),  (4, 18), (4, 30), 
                       (4, 31), (4, 42), (4, 46), (5, 15), (5, 34), (5, 41), (5, 50), 
                       (6, 13),  (6, 19), (6, 21), (6, 23), (6, 28), (6, 38), (6, 49), 
                       (7, 19), (7, 25), (7, 30), (7, 41),  (8, 19), (8, 24), (8, 25), 
                       (8, 28), (8, 30), (8, 31), (9, 19), (9, 26), (9, 37), (9, 46),  
                       (9, 48), (10, 28), (10, 45), (11, 16), (11, 36), (11, 38), 
                       (11, 39), (12, 13), (12, 15),  (12, 24), (12, 37), (12, 50), 
                       (13, 14), (13, 16), (13, 34), (13, 36), (13, 38), (14, 29),  
                       (14, 30), (15, 24), (15, 25), (15, 26), (15, 32), (15, 33), 
                       (16, 36), (16, 37), (16, 41),  (16, 44), (17, 23), (17, 28), 
                       (17, 37), (17, 41), (18, 37), (18, 47), (19, 28), (19, 29),  
                       (19, 30), (19, 36), (19, 39), (19, 47), (20, 26), (21, 25), 
                       (21, 34), (21, 36), (21, 47),  (21, 50), (22, 23), (22, 24), 
                       (22, 27), (22, 34), (22, 47), (24, 27), (24, 43), (24, 46),  
                       (25, 31), (26, 45), (27, 36), (27, 45), (30, 34), (30, 39), 
                       (30, 48), (31, 41), (32, 37),  (32, 40), (32, 50), (33, 37), 
                       (34, 40), (35, 40), (36, 44), (36, 45), (36, 49), (37, 40),  
                       (38, 40), (39, 49), (39, 50), (40, 42), (40, 49), (43, 47), 
                       (44, 48)]
)
    l.draw()

  
   
