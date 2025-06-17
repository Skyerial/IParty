import asyncio
import websockets
import json
import random
import math
import time
from typing import List, Dict




class MobileClientSimulator:
    def __init__(self, client_id: str, server_url: str):
        self.client_id = client_id
        self.server_url = server_url
        self.websocket = None
        self.running = False
        self.player_registered = False

        # Simulate different player configs
        colors = ["Blue", "Yellow", "Green", "Red"]
        self.player_config = {
            "name": f"Player_{client_id}",
            "color": colors[int(client_id) % len(colors)]
        }

        # Simulate button state
        self.button_state = {"A": False, "B": False, "C": False, "D": False}

    async def connect(self):
        """Connect to the proxy server (which forwards to Unity)"""
        try:
            print(f"[Client {self.client_id}] Connecting to proxy at {self.server_url}")
            self.websocket = await websockets.connect(self.server_url)
            self.running = True
            print(f"[Client {self.client_id}] ✅ Connected successfully!")
            return True
        except Exception as e:
            print(f"[Client {self.client_id}] ❌ Connection failed: {e}")
            return False

    async def register_player(self):
        """Send initial player configuration"""
        if not self.websocket or self.player_registered:
            return

        try:
            message = json.dumps(self.player_config)
            await self.websocket.send(message)
            self.player_registered = True
            print(f"[Client {self.client_id}] 👤 Player registered: {self.player_config}")

        except Exception as e:
            print(f"[Client {self.client_id}] ❌ Failed to register player: {e}")

    async def send_analog_input(self, x: float, y: float, buttons: Dict[str, bool] = None):
        """Send analog stick input"""
        if not self.websocket:
            return

        if buttons:
            self.button_state.update(buttons)

        command = {
            "type": "analog",
            "x": x,
            "y": y,
            "A": self.button_state["A"],
            "B": self.button_state["B"],
            "C": self.button_state["C"],
            "D": self.button_state["D"]
        }

        try:
            message = json.dumps(command)
            await self.websocket.send(message)
            # Only print occasionally to avoid spam
            if random.random() < 0.05:  # 5% chance (reduced from 10%)
                print(f"[Client {self.client_id}] 🎮 Input: x={x:.2f}, y={y:.2f}, buttons={any(self.button_state.values())}")

        except Exception as e:
            print(f"[Client {self.client_id}] ❌ Failed to send input: {e}")
            self.running = False

    async def listen_for_messages(self):
        """Listen for messages from the server"""
        try:
            while self.running:
                try:
                    response = await asyncio.wait_for(self.websocket.recv(), timeout=1.0)
                    if "controller" not in response:  # Don't spam controller updates
                        print(f"[Client {self.client_id}] 📨 Received: {response}")
                except asyncio.TimeoutError:
                    continue  # Keep listening
        except websockets.exceptions.ConnectionClosed:
            print(f"[Client {self.client_id}] 🔌 Connection closed by server")
            self.running = False
        except Exception as e:
            print(f"[Client {self.client_id}] 👂 Listen error: {e}")
            self.running = False

    async def simulate_movement_pattern(self):
        """Simulate realistic movement patterns"""
        patterns = [
            ("circle", self._circle_movement),
            ("figure_eight", self._figure_eight_movement),
            ("random", self._random_movement),
            ("button_mash", self._button_mashing)
        ]

        pattern_name, pattern_func = random.choice(patterns)
        print(f"[Client {self.client_id}] 🎯 Starting {pattern_name} movement pattern")
        await pattern_func()

    async def _circle_movement(self):
        """Simulate circular movement"""
        angle = random.uniform(0, 2 * math.pi)  # Start at random angle
        for _ in range(200):
            if not self.running:
                break
            x = 0.8 * math.cos(angle)
            y = 0.8 * math.sin(angle)
            await self.send_analog_input(x, y)
            angle += 0.1
            await asyncio.sleep(0.05)

    async def _figure_eight_movement(self):
        """Simulate figure-8 movement"""
        t = random.uniform(0, 2 * math.pi)  # Start at random phase
        for _ in range(200):
            if not self.running:
                break
            x = 0.8 * math.sin(t)
            y = 0.8 * math.sin(2 * t)
            await self.send_analog_input(x, y)
            t += 0.1
            await asyncio.sleep(0.05)

    async def _random_movement(self):
        """Simulate random movement with occasional button presses"""
        for _ in range(200):
            if not self.running:
                break
            x = random.uniform(-1.0, 1.0)
            y = random.uniform(-1.0, 1.0)

            # Random button presses
            buttons = {
                "A": random.random() < 0.15,
                "B": random.random() < 0.10,
                "C": random.random() < 0.08,
                "D": random.random() < 0.05
            }

            await self.send_analog_input(x, y, buttons)
            await asyncio.sleep(random.uniform(0.03, 0.1))

    async def _button_mashing(self):
        """Simulate button mashing with minimal stick movement"""
        for _ in range(100):
            if not self.running:
                break

            buttons = {
                "A": random.random() < 0.8,
                "B": random.random() < 0.6,
                "C": random.random() < 0.4,
                "D": random.random() < 0.3
            }

            x = random.uniform(-0.3, 0.3)
            y = random.uniform(-0.3, 0.3)

            await self.send_analog_input(x, y, buttons)
            await asyncio.sleep(0.08)

    async def disconnect(self):
        """Disconnect from the server"""
        self.running = False
        if self.websocket:
            await self.websocket.close()
            print(f"[Client {self.client_id}] 🚪 Disconnected")

class FourClientTester:
    def __init__(self, proxy_url: str = "ws://localhost:8182"):
        self.proxy_url = proxy_url
        self.clients: List[MobileClientSimulator] = []

    async def create_clients(self, num_clients: int = 4):
        """Create and connect multiple client simulators"""
        print(f"🚀 Creating {num_clients} client simulators...")
        print(f"📡 Connecting to proxy: {self.proxy_url}")
        print("   (Proxy will forward to Unity server)")
        print("=" * 50)

        # Create clients
        for i in range(num_clients):
            client = MobileClientSimulator(
                client_id=str(i + 1),
                server_url=self.proxy_url
            )
            self.clients.append(client)

        # Connect all clients with small delays to avoid overwhelming
        successful_connections = 0
        for i, client in enumerate(self.clients):
            await asyncio.sleep(0.5)  # Increased delay
            if await client.connect():
                successful_connections += 1
            else:
                print(f"Client {client.client_id} failed to connect")

        # Keep only successful connections
        self.clients = [client for client in self.clients if client.running]

        print(f"Successfully connected {successful_connections}/{num_clients} clients")
        return successful_connections > 0

    async def register_all_players(self):
        """Register all connected clients as players"""
        if not self.clients:
            return

        print("\nRegistering all players...")

        for client in self.clients:
            await client.register_player()
            await asyncio.sleep(0.5)  # Stagger registrations

        print("All players registered!")

    async def start_simulation(self, duration: int = 60):
        """Start the movement simulation for all clients"""
        if not self.clients:
            return

        print(f"\n🎮 Starting {duration}s simulation with {len(self.clients)} clients...")
        print("(Press Ctrl+C to stop early)")
        print("-" * 50)

        # Start all client tasks
        all_tasks = []

        for client in self.clients:
            # Start listening task
            listen_task = asyncio.create_task(client.listen_for_messages())
            all_tasks.append(listen_task)

            # Start movement simulation task
            movement_task = asyncio.create_task(client.simulate_movement_pattern())
            all_tasks.append(movement_task)

        # Run simulation for specified duration
        try:
            await asyncio.sleep(duration)
            print(f"\n⏰ {duration} seconds completed!")
        except KeyboardInterrupt:
            print("\n⌨️ Simulation interrupted by user")

        # Cancel all tasks
        print("🛑 Stopping all client activities...")
        for task in all_tasks:
            task.cancel()

        # Wait a bit for tasks to clean up
        await asyncio.sleep(0.5)

    async def disconnect_all(self):
        """Disconnect all clients"""
        if not self.clients:
            return

        print("\n🚪 Disconnecting all clients...")

        for client in self.clients:
            await client.disconnect()
            await asyncio.sleep(0.2)  # Stagger disconnections

        print("✅ All clients disconnected")

    async def run_full_test(self, duration: int = 60):
        """Run the complete test scenario"""
        try:
            # Step 1: Connect clients
            if not await self.create_clients(4):
                print("❌ No clients connected successfully. Exiting.")
                return

            # Step 2: Register players
            await self.register_all_players()

            # Step 3: Run simulation
            await self.start_simulation(duration)

        except Exception as e:
            print(f"❌ Test error: {e}")
        finally:
            # Step 4: Clean up
            await self.disconnect_all()

async def main():
    """Main test function"""
    print("🎯 Unity Server 4-Client Mobile Simulator (Proxy Version)")
    print("=" * 55)

    # Configuration - now connects to proxy instead of Unity directly
    proxy_url = "ws://localhost:8182"

    # Get test duration from user
    try:
        duration_input = input("Enter test duration in seconds [60]: ").strip()
        duration = int(duration_input) if duration_input else 60
    except ValueError:
        duration = 60

    print(f"⚙️ Configuration:")
    print(f"   Proxy: {proxy_url}")
    print(f"   Duration: {duration} seconds")
    print(f"   Clients: 4")
    print(f"   Note: Make sure the proxy is running first!")

    # Create and run tester
    tester = FourClientTester(proxy_url)
    await tester.run_full_test(duration)

    print("\n🎉 Test completed!")

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n⌨️ Test interrupted by user")
    except Exception as e:
        print(f"❌ Error: {e}")