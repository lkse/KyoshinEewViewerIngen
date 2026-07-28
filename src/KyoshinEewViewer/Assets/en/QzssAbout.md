## What is [DC Report (Disaster and Crisis Management Report)](https://qzss.go.jp/en/overview/services/sv08_dc-report.html)?

A service that broadcasts disaster prevention information using GPS L1S signals via the Quasi-Zenith Satellite System "Michibiki" (QZSS).  
The service area is primarily Asia and Oceania.  
[See here for details on the service area.](https://qzss.go.jp/en/technical/system/dcr.html)

Reception requires a device that supports L1S and can output NMEA to a PC.  
[Check here for compatible devices.](https://qzss.go.jp/en/usage/products/list.html)  
Note that configuration changes or USB-serial conversion may be required depending on the device, module, or board.  
Currently, devices that output QZQSM sentences and u-blox protocol output are supported.

This software parses and displays NMEA output from a GPS receiver.  
Connect a compatible device to your PC and specify the serial port recognized by the system.
