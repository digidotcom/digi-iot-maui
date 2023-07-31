XBee BLE MicroPython Sample Application
=======================================

This application demonstrates the usage of the Digi IoT Library for .NET MAUI
in order to send and receive data over Bluetooth Low Energy to the MicroPython
application running on the XBee device.

The example scans for Bluetooth devices and allows you to connect to your
XBee device. Once connected, you can ask the MicroPython application that is
running on the XBee to read and send the temperature and humidity values.

Demo requirements
-----------------

To run this example you need:

* One XBee3 module in API mode and its corresponding carrier board (XBIB-C).
* An Android or iOS device.
* The XCTU application (available at www.digi.com/xctu).
* The PyCharm IDE with the [Digi XBee MicroPython plugin](https://plugins.jetbrains.com/plugin/12445-xbee-micropython)
  installed.

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

First, build the application. Then, you need to run the
'relay_frames_temperature' sample in the module. Follow these steps to do so:

1. Launch PyCharm.
2. In the Welcome screen, click the **Import XBee MicroPython Sample Project**
   option and import the **Relay Frames Temperature** sample (XBEE > 
   COMMUNICATION).
3. Run the sample project.
   NOTE: when asked to enable the MicroPython REPL mode, click No.

Finally, launch this sample application. In the first page of the application 
you have to select your XBee device from the list of Bluetooth devices. Tap on
it and enter the Bluetooth password you initially configured when you enabled
BLE on your XBee device.

If the connection is successful, the Temperature and Humidity page appears. It
shows the values received from the module (initially empty), a control to select
the refresh rate and a button to start or stop the readings.

To test the communication, select the desired refresh rate and tap on **Start**.
Verify that the values of the temperature and humidity are received and change
at the specified rate.

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