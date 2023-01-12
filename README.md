# cross-connection-csharp

cross-connection is used to provide a cross-protocol pub/sub-mode connection library among multiple devices.

### Features

- Pub/Sub connection similar to mqtt protocol.
- Network register/detection for auto connection when device reboot.
- Auto connection when network disconnect/reconnected.
- Cross protocol (TCP/IP, Bluetooth(support on [android library](https://github.com/TW-Smart-CoE/cross-connection-android))) connection. 

### Platform support

[android](https://github.com/TW-Smart-CoE/cross-connection-android)

[python](https://github.com/TW-Smart-CoE/cross-connection-py)

[csharp](https://github.com/TW-Smart-CoE/cross-connection-csharp)

[unity](https://github.com/TW-Smart-CoE/cross-connection-unity)


## Development

Clone this project as git submodule of any CSharp project.

```
git submodule add -b main git@github.com:TW-Smart-CoE/cross-connection-csharp.git CConn
```

### Unity

If you use cross-connection-csharp in Unity project, clone the submodule to _YourProject/Assets/Plugins/CConn_

Check [unity](https://github.com/TW-Smart-CoE/cross-connection-unity) as a reference.
