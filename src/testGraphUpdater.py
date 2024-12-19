import unittest
from flask import Flask
from flask_restful import Api
from graphUpdater import GraphUpdater

class TestGraphUpdater(unittest.TestCase):
    def setUp(self):
        # Set up Flask app and API
        self.app = Flask(__name__)
        self.api = Api(self.app)
        self.api.add_resource(GraphUpdater, '/update_graph')

        # Create a test client
        self.client = self.app.test_client()

    def test_post_changes(self):
        # Define the test JSON payload
        payload = {
            "0": {
                "actionType": "addNode",
                "properties": {"x": 80.5, "y": 55.5, "randomAttr": "abcde", "graphId": 1}
            }
        }

        # Transform payload to match expected structure (convert dict to 'changes' array)
        transformed_payload = {
            "changes": [value for key, value in payload.items()]
        }

        print(transformed_payload)

        # Send a POST request to the endpoint
        response = self.client.post(
            '/update_graph',
            json=transformed_payload,
            content_type='application/json'
        )

        # Check the response
        self.assertEqual(response.status_code, 200)
        self.assertIn("Graph updated successfully", response.get_json().get("message"))

if __name__ == '__main__':
    unittest.main()

