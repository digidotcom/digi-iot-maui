# Digi IoT Library for .NET MAUI [ ![NuGet](https://img.shields.io/nuget/v/DigiIoT.Maui)](https://www.nuget.org/packages/DigiIoT.Maui/)

This project contains the source code of the Digi IoT Library for .NET MAUI, an
easy-to-use API developed in C# that simplifies the development of
cross-platform mobile applications in .NET MAUI that communicate with Digi IoT
devices over Bluetooth Low Energy. Among the compatible Digi IoT devices you
can find the [ConnectCore](http://www.digi.com/connectcore/) radio frequency
(RF) modules and the [XBee](http://www.digi.com/xbee/) ones.

The XBee Digi IoT Library for .NET MAUI has one main module: **DigiIoT.Maui**,
which contains the necessary APIs to develop cross-platform mobile applications
in .NET MAUI.

The project includes the C# source code and some examples that show how to
use the available APIs. The examples are also available in source code format.

The main features of the library include:

* Supported hardware:
  * Digi ConnectCore devices.
  * Digi XBee 3 devices:
    * XBee 3 Zigbee
    * XBee 3 802.15.4
    * XBee 3 DigiMesh
    * XBee 3 Cellular
* Support for communicating with the supported hardware over Bluetooth Low
Energy:
  * Connect
  * Disconnect
  * Send data
  * Receive data (directly or through events)  
* XBee specific functionality:
  * Configure common parameters with specific setters and getters.
  * Configure any other parameter with generic methods.
  * Execute AT commands.
  * Apply configuration changes.
  * Write configuration changes.
  * Reset the device.
  * Support for User Data Relay frames, allowing the communication between
different interfaces (Serial and MicroPython).
  * IO lines management:
    * Configure IO lines.
    * Set IO line value.
    * Read IO line value.


## Start Here

The best place to get started is the 
[Digi IoT Library for .NET MAUI User Guide](https://www.digi.com/resources/documentation/digidocs/90002569).


## How to Contribute

The contributing guidelines are in the 
[CONTRIBUTING.md](https://github.com/digidotcom/digi-iot-maui/blob/master/CONTRIBUTING.md) 
document.


## License

Copyright 2023, Digi International Inc.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, you can obtain one at http://mozilla.org/MPL/2.0/.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
