import asyncio
import json
import socket
import sys

from dataclasses import dataclass
from enum import IntEnum

sock = socket.socket(socket.AF_INET,
					 socket.SOCK_DGRAM)

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
        dict_repr = self.__dict__
        return json.dumps(dict_repr)

    def __str__(self):
        return f'{self.action} | {self.json_data}'


class QueueServerProtocol:
    def connection_made(self, transport):
        self.transport = transport

    def datagram_received(self, data, addr):
        message = data.decode()
        request = QueueRequest.from_message(message)

        if request.action == RequestAction.HELLO:
            if request.data.get('type') == 'ping':
                print(f'sending pong to {addr}')
                response = QueueRequest(RequestAction.HELLO, "pong")
                self.transport.sendto(
                    response.to_json().encode('utf-8'),
                    addr
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