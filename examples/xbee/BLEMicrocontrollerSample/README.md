XBee BLE Microcontroller Sample Application
===========================================

This application demonstrates the usage of the Digi IoT Library for .NET MAUI
in order to send and receive data over Bluetooth Low Energy to a
microcontroller connected to the XBee serial interface.

The example scans for Bluetooth devices and allows you to connect to your
XBee device. Once connected, you can send a file to a microcontroller (in this
case, your PC running another XBee library sample) and see the progress of the
operation.

Demo requirements
-----------------

To run this example you need:

* One XBee3 module in API mode and its corresponding carrier board (XBIB or
  equivalent).
* An Android or iOS device.
* The XCTU application (available at www.digi.com/xctu).
* The 'Receive Bluetooth File' sample of the [XBee Java Library](https://github.com/digidotcom/xbee-java/tree/master/examples/communication/bluetooth/ReceiveBluetoothFileSample)
  or the [XBee Python Library](https://github.com/digidotcom/xbee-python/tree/master/examples/communication/bluetooth/ReceiveBluetoothFileSample).

Demo setup
----------

Make sure the hardware is set up correctly:

1. The mobile device is powered on.
2. The XBee module is plugged into the XBee adapter and connected to your
   computer's USB or serial port.
3. The XBee module in in API mode and has the Bluetooth interface enabled and
   configured. You can use XCTU to do it.

Demo run
--------

First, build the application. Then, you need to run the 'Receive Bluetooth File'
sample of the XBee Java Library or the XBee Python Library. Follow the
instructions explained in that sample's README file to do so.

After that, launch this sample application. In the first page of the application
you have to select your XBee device from the list of Bluetooth devices. Tap on
it and enter the Bluetooth password you initially configured when you enabled
BLE on your XBee device.

If the connection is successful, the Send File page appears. Tap on the
**Load and send file** button and select the file to send. Verify that the
progress bar is updated as long as the file is sent.

Compatible with
---------------

* Android devices
* iOS devices

License
-------

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