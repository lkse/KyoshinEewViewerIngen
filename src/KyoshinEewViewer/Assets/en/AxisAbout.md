## What is [AXIS](https://axis.prioris.jp/)?

A free service that delivers real-time earthquake and weather information via WebSocket.  
By default, KyoshinEewViewer for ingen uses the [JMA Disaster Prevention XML (PULL)](http://xml.kishou.go.jp/xmlpull.html) feed to receive earthquake information, which introduces a latency of **approximately 1-3 minutes** from announcement to reception.  
Using AXIS reduces this to **just a few seconds**.

### Currently Supported Channels

- `eew`
    - Available in the Kyoshin Monitor tab.
    - Warning status may not be stable.
- Earthquake information, tsunami information, and more are planned for future updates.

### Notes

- AXIS is in beta testing, and its stability within this application has not been fully verified. Unexpected errors may occur.
- Past data cannot be received through AXIS, so historical information not yet reflected in JMA feeds is unavailable.
- Service use is limited to non-commercial purposes.

> [!WARNING]
> Redistribution, republishing, copying, modification, and commercial use of information received via AXIS are prohibited.
> Please exercise caution if recording or streaming video.
