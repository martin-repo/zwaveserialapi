# Z-Wave Serial API

This API is for developers who want to create their own home automation tool. If you're looking for a fully featured solution you may want to use [Home Assistant](https://developers.home-assistant.io/docs/api/rest/), and connect to it via REST API and/or WebSocket API.

Since this API doesn't support Z-Wave protocol security it should not be used for home security, door locks, etc.

## Not planned (ie. will most likely not happen)
- Security (ie. S0 and S2 classed communication with nodes)
- Multi Channel
- Associations (device signalling other device, bypassing controller)
- Scenes
