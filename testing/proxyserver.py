import asyncio
import websockets
import json
import uuid
from typing import Dict, Set
import logging

# webserver = "http://196.168.2.8:8080"
websocket = "ws://192.168.2.8:8181"

# Set up logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class WebSocketProxy:
    def __init__(self, target_server: str, proxy_host: str = "localhost", proxy_port: int = 8182):
        self.target_server = target_server
        self.proxy_host = proxy_host
        self.proxy_port = proxy_port

        # Track connections: fake_id -> (client_websocket, server_websocket)
        self.connections: Dict[str, tuple] = {}

        # Track which fake IDs are in use
        self.active_fake_ids: Set[str] = set()


    async def handle_client(self, websocket):
        """Handle incoming client connections"""
        # Generate a unique fake client ID
        fake_client_id = f"client_{uuid.uuid4().hex[:8]}"
        self.active_fake_ids.add(fake_client_id)

        logger.info(f"🔗 New client connected, assigned fake ID: {fake_client_id}")

        try:
            # Connect to the actual Unity server
            server_ws = await websockets.connect(self.target_server)
            logger.info(f"📡 Connected to Unity server for {fake_client_id}")

            # Store the connection pair
            self.connections[fake_client_id] = (websocket, server_ws)

            # Create tasks to relay messages in both directions
            client_to_server_task = asyncio.create_task(
                self.relay_messages(websocket, server_ws, f"{fake_client_id} -> Unity", fake_client_id)
            )
            server_to_client_task = asyncio.create_task(
                self.relay_messages(server_ws, websocket, f"Unity -> {fake_client_id}", fake_client_id)
            )

            # Wait for either connection to close
            done, pending = await asyncio.wait(
                [client_to_server_task, server_to_client_task],
                return_when=asyncio.FIRST_COMPLETED
            )

            # Cancel remaining tasks
            for task in pending:
                task.cancel()

        except Exception as e:
            logger.error(f"❌ Error handling client {fake_client_id}: {e}")
        finally:
            # Cleanup
            await self.cleanup_connection(fake_client_id)

    async def relay_messages(self, from_ws, to_ws, direction: str, fake_client_id: str):
        """Relay messages between websockets"""
        try:
            async for message in from_ws:
                await to_ws.send(message)
                # Only log occasionally to avoid spam
                if "analog" not in message:  # Don't log frequent analog inputs
                    logger.debug(f"📨 {direction}: {message[:100]}...")
        except websockets.exceptions.ConnectionClosed:
            logger.info(f"🔌 Connection closed: {direction}")
        except Exception as e:
            logger.error(f"❌ Relay error {direction}: {e}")

    async def cleanup_connection(self, fake_client_id: str):
        """Clean up a connection"""
        if fake_client_id in self.connections:
            client_ws, server_ws = self.connections[fake_client_id]

            # Close connections if still open
            if not client_ws.closed:
                await client_ws.close()
            if not server_ws.closed:
                await server_ws.close()

            # Remove from tracking
            del self.connections[fake_client_id]
            self.active_fake_ids.discard(fake_client_id)

            logger.info(f"🧹 Cleaned up connection: {fake_client_id}")

    async def start_proxy(self):
        """Start the proxy server"""
        logger.info(f"🚀 Starting WebSocket proxy on {self.proxy_host}:{self.proxy_port}")
        logger.info(f"📡 Proxying to Unity server: {self.target_server}")
        logger.info("=" * 60)

        server = await websockets.serve(
            self.handle_client,
            self.proxy_host,
            self.proxy_port
        )

        logger.info(f"✅ Proxy server started! Connect your test clients to:")
        logger.info(f"   ws://{self.proxy_host}:{self.proxy_port}")
        logger.info("=" * 60)

        return server

async def main():
    # Configuration
    UNITY_SERVER = websocket  # Your Unity server
    PROXY_HOST = "localhost"
    PROXY_PORT = 8182

    print("WebSocket Proxy for Multi-Client Testing")
    print("=" * 50)

    # Create and start proxy
    proxy = WebSocketProxy(UNITY_SERVER, PROXY_HOST, PROXY_PORT)
    server = await proxy.start_proxy()

    try:
        # Keep the proxy running
        await server.wait_closed()
    except KeyboardInterrupt:
        logger.info("\n⌨Proxy interrupted by user")
    finally:
        logger.info("Shutting down proxy server...")
        server.close()
        await server.wait_closed()

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n⌨Interrupted by user")
    except Exception as e:
        print(f"Error: {e}")
