# Adder RedPSU API
This example C# code allows you to control and interact with the Adder RedPSU product.

https://www.adder.com/en/kvm-solutions/adder-red-psu

IMPORTANT: The code is provide as is, with no support or warranty.

It provides the following functions:

- Observable collections for the Power Outputs, Users and PSU's.
- Asynchronous updates to the collections.
- Turn individual or All Power Outputs ON and OFF
- Configurable Output, Network and System Settings
- User Managenent
- Event driven changes


Using the API Class
- For the asynchronous updates to work, the API class must not be initiated globally. SynchronizationContext is used to switch between the working and main threads when working with the events.
- You must provide an IP Address, Username and Password.
- The API is restful, to get or update the current configurations, you can either use GetData or GetDataAsync.  GetDataAsync will run on another thread.
- If AutoUpdate is enabled, calling GetDataAsync will automatically fetch the configuration every 2 seconds.
- Any updates to the Observerable collections trigger corresponding events.
