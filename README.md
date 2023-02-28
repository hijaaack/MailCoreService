## Disclaimer
This is a personal guide not a peer reviewed journal or a sponsored publication. We make
no representations as to accuracy, completeness, correctness, suitability, or validity of any
information and will not be liable for any errors, omissions, or delays in this information or any
losses injuries, or damages arising from its display or use. All information is provided on an as
is basis. It is the reader’s responsibility to verify their own facts.

The views and opinions expressed in this guide are those of the authors and do not
necessarily reflect the official policy or position of any other agency, organization, employer or
company. Assumptions made in the analysis are not reflective of the position of any entity
other than the author(s) and, since we are critically thinking human beings, these views are
always subject to change, revision, and rethinking at any time. Please do not hold us to them
in perpetuity.

## MailCoreService

Simple .NET Core TwinCAT HMI Service for sending emails. 

Based on the [FluentEmail](https://github.com/lukencode/FluentEmail)

For testing the functions locally I recommend using the [Papercut-SMTP client](https://github.com/ChangemakerStudios/Papercut-SMTP)

If you want to see a sample HMI-project with this package go [here](https://github.com/hijaaack/TcHmiMailCoreServiceSample)

## Functions

The package consists of a service configuration window;

![enter image description here](https://user-images.githubusercontent.com/75740551/221829968-97beddf2-1246-43a0-a1d3-d16443b2a7fb.png)

..and a method;

![enter image description here](https://user-images.githubusercontent.com/75740551/221830144-d080bee9-3097-4eb6-ae45-d85b96881f71.png)

## Prerequisites

License: [TF2200](https://www.beckhoff.com/sv-se/products/automation/twincat/tfxxxx-twincat-3-functions/tf2xxx-tc3-hmi/tf2200.html) is needed in the HMI-Server to be able to use this C# Server Extension.

Visual Studio 2019 or newer is needed to open this .NET Core project. 

## Installation

Easiest way to install this package is inside your TwinCAT HMI Project. 
**Right click** References and click "Manage NuGet Packages.." then browse for the file and install it! 

![enter image description here](https://user-images.githubusercontent.com/75740551/101645035-32cef100-3a36-11eb-88f4-eeaccd3366d6.png)
