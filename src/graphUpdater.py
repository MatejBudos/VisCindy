from flask_restful import Resource, reqparse
from flask import request, jsonify
from graphDB_client import DBClient

class GraphUpdater(Resource):
    def __init__(self):
        # Initialize the database client
        self.db_client = DBClient()

    def post(self):
        try:
            # Parse JSON data from Unity
            data = request.get_json()
            if not data or "changes" not in data:
                return {"error": "Invalid payload, 'changes' key is missing"}, 400
            
            changes = data["changes"]
            
            # Loop through changes in reverse
            for change in reversed(changes):
                action_type = change.get("actionType")
                if not action_type:
                    continue  # Skip if actionType is missing
                
                # Process action based on type
                if action_type == "addNode":
                    self.add_node(change)
                elif action_type == "deleteNode":
                    self.delete_node(change)
                elif action_type == "updateProperty":
                    self.update_property(change)
                elif action_type == "addRelationship":
                    self.add_relationship(change)
                elif action_type == "deleteRelationship":
                    self.delete_relationship(change)
                else:
                    print(f"Unknown actionType: {action_type}")

            return {"message": "Graph updated successfully"}, 200

        except Exception as e:
            return {"error": str(e)}, 500

    def add_node(self, change):
        # Example: Add a node to the Neo4j database
        label = change.get("label", "Node")
        properties = change.get("properties", {})
        properties_query = ", ".join(f"{key}: '{value}'" for key, value in properties.items())
        query = f"CREATE (n:{label} {{{properties_query}}})"
        self.db_client.execute_query(query)

    def delete_node(self, change):
        # Example: Delete a node by ID or property
        node_id = change.get("nodeId")
        if node_id:
            query = f"MATCH (n) WHERE id(n) = {node_id} DETACH DELETE n"
            self.db_client.execute_query(query)

    def update_property(self, change):
        # Example: Update a node's property
        node_id = change.get("nodeId")
        properties = change.get("properties", {})
        if node_id and properties:
            properties_query = ", ".join(f"n.{key} = '{value}'" for key, value in properties.items())
            query = f"MATCH (n) WHERE id(n) = {node_id} SET {properties_query}"
            self.db_client.execute_query(query)

    def add_relationship(self, change):
        # Example: Add a relationship between nodes
        from_id = change.get("fromNodeId")
        to_id = change.get("toNodeId")
        rel_type = change.get("relationshipType", "RELATED_TO")
        if from_id and to_id:
            query = f"""
            MATCH (a), (b)
            WHERE id(a) = {from_id} AND id(b) = {to_id}
            CREATE (a)-[:{rel_type}]->(b)
            """
            self.db_client.execute_query(query)

    def delete_relationship(self, change):
        # Example: Delete a relationship
        from_id = change.get("fromNodeId")
        to_id = change.get("toNodeId")
        rel_type = change.get("relationshipType", "RELATED_TO")
        if from_id and to_id:
            query = f"""
            MATCH (a)-[r:{rel_type}]->(b)
            WHERE id(a) = {from_id} AND id(b) = {to_id}
            DELETE r
            """
            self.db_client.execute_query(query)
