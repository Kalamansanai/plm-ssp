# https: 9696
# http: 9695

import asyncio
import websockets
import ssl
import logging
import cv2
import numpy as np
from time import sleep
from camera import Camera
from threading import Lock, Thread
import socket

DEST_IP = '127.0.0.1'
DEST_PORT = 9697


def get_snapshot():
    camera = Camera.get_instance()
    camera.start()

    # TODO: Capture a couple frames before taking the snapshot to make sure the camera exposure or
    # whatever is stabilized, if needed
    with camera.acquire() as (img, ts):
        success, image_bmp = cv2.imencode(".bmp", img)
        if not success:
            raise Exception('Cannot encode to BMP')

    camera.stop()
    return bytes(image_bmp)


class DummyDetector:
    async def __aenter__(self):
        self.mac = '121212121212'
        self.lock = Lock()
        self.thread = None
        self._conn = websockets.connect("wss://localhost:9696/api/v1/detectors/controller", \
            ssl=ssl._create_unverified_context())
        self.ws = await self._conn.__aenter__()
        return self
    pass

    async def __aexit__(self, *args, **kwargs):
        await self._conn.__aexit__(*args, **kwargs)

    def get_response(self, cmd):
        if cmd == 'Ping':
            return 'Pong'
        else:
            return 'Ok'

    async def run(self):
        await self.ws.send(self.mac)
        self.detector_id = int(await self.ws.recv())

        while True:
            try:
                cmd = await self.ws.recv()
                resp = self.get_response(cmd)
                print(f'Sending response {resp} to cmd {cmd}')
                await self.ws.send(self.get_response(cmd))

                if cmd == "TakeSnapshot":
                    await self.ws.send(get_snapshot())

                if cmd == "StartStreaming":
                    with self.lock:
                        if self.thread is None:
                            self.thread = Thread(target=self.stream_thread)
                            self.working = True
                            self.thread.start()

                if cmd == "StopStreaming":
                    with self.lock:
                        if self.thread is not None:
                            self.working = False
                            self.thread.join()
                            self.thread = None

            except websockets.exceptions.ConnectionClosedError as ex:
                print(ex)
                break

    def stream_thread(self):
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        dest = (DEST_IP, DEST_PORT)
        id_bytes = np.frombuffer((self.detector_id).to_bytes(4, byteorder='little'),
                dtype='uint8')
        camera = Camera.get_instance()
        camera.start()

        # TODO: catch ConnectionRefused and clean up

        while True:
            if not self.working:
                camera.stop()
                break

            with camera.acquire() as (img, ts):
                s, img = cv2.imencode(".jpg", img,
                        [int(cv2.IMWRITE_JPEG_QUALITY), 60])

                start = 0
                stride = 508
                imglen = len(img)
                while start * stride < imglen: 
                    fragment = np.concatenate((id_bytes, img[start*stride:(start+1)*stride]))
                    start += 1
                    sock.sendto(fragment, dest)
                sock.sendto(id_bytes, dest)




async def main():
    async with DummyDetector() as detector:
        await detector.run()

asyncio.run(main())