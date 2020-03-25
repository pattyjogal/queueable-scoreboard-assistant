import asyncio
import json
import socket
import sys

from dataclasses import dataclass
from enum import IntEnum

sock = socket.socket(socket.AF_INET,
					 socket.SOCK_DGRAM)

clients = set()
queue_state = []

class RequestAction(IntEnum):
    HELLO = 0
    QUEUE_PROPAGATE = 1

@dataclass
class QueueRequest:
    action: RequestAction
    data: object

    @classmethod
    def from_message(cls, json_str):
        json_parsed = json.loads(json_str)

        action = RequestAction(json_parsed['Action'])
        data = json.loads(json_parsed['JsonData'])

        return cls(action, data)

    def to_json(self) -> str:
        return json.dumps({
            'Action': self.action,
            'JsonData': json.dumps(self.data)
            })

    def __str__(self):
        return f'{self.action} | {self.json_data}'


class QueueServerProtocol:
    def connection_made(self, transport):
        self.transport = transport

    def datagram_received(self, data, addr):
        global queue_state
        global clients

        message = data.decode()
        print(f'Got message {message}')
        request = QueueRequest.from_message(message)

        if request.action == RequestAction.HELLO:
            if request.data.get('type') == 'ping':
                clients.add(addr)

                print(f'sending pong to {addr}')
                response = QueueRequest(RequestAction.HELLO, "pong")
                self.transport.sendto(
                    response.to_json().encode('utf-8'),
                    addr
                    )
        elif request.action == RequestAction.QUEUE_PROPAGATE:
            #TODO: Lock the queue
            queue_state = request.data

            for client in clients - {addr}:
                print(f'propagating queue to {addr}')
                response = QueueRequest(RequestAction.QUEUE_PROPAGATE, queue_state)
                print(f'Json rep: {response.to_json()}')
                self.transport.sendto(
                    response.to_json().encode('utf-8'),
                    client
                    )

async def main():
    if len(sys.argv) != 3:
        raise ValueError()

    loop = asyncio.get_event_loop()

    transport, protocol = await loop.create_datagram_endpoint(
        lambda: QueueServerProtocol(),
        local_addr=tuple(sys.argv[1:]))

    try:
        await asyncio.sleep(3600)  # Serve for 1 hour.
    finally:
        transport.close()

if __name__ == "__main__":
    asyncio.run(main());