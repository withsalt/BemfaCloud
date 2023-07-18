# BemfaCloud
巴法云 C#/.NET SDK。支持TCP和MQTT协议接入，并提供超级简单的设备操作API。跨平台，支持Linux、Windows、OSX，支持树莓派。  

## 特别感谢
巴法云：https://cloud.bemfa.com/  
MQTTnet： https://github.com/dotnet/MQTTnet  
GodSharp.Socket: https://github.com/godsharp/GodSharp.Socket  

## 构建状态
|  包名  |  版本 | 说明  |
| ------------ | ------------ | ------------ |
| BemfaCloud  | [![NuGet Version](https://img.shields.io/nuget/v/BemfaCloud.svg?style=flat)](https://www.nuget.org/packages/BemfaCloud/)  | 基础包，如果只是使用TCP或MQTT连接，只需使用这个库。   |
| BemfaCloud.Devices  | [![NuGet Version](https://img.shields.io/nuget/v/BemfaCloud.Devices.svg?style=flat)](https://www.nuget.org/packages/BemfaCloud.Devices/)  | 高级设备操作API，提供简单易用的设备操作API。  |


## 快速开始
### 新建主题
登录到巴法云之后，点击新建主题。  
![](https://github.com/withsalt/BemfaCloud/blob/main/docs/images/001.png?raw=true)
Tips：  
巴发云通过**主题名称后三位**判断主题类型  
- 001: 插座设备;  
- 002: 灯泡设备;  
- 003: 风扇设备;  
- 004: 传感器设备;  
- 005: 空调设备;  
- 006: 开关设备;  
- 009: 窗帘设备;  

如`testSwitch006`,创建之后即为开关设备。  

### 安装Nuget包
通过Nuget包管理器安装`BemfaCloud` [![NuGet Version](https://img.shields.io/nuget/v/BemfaCloud.svg?style=flat)](https://www.nuget.org/packages/BemfaCloud/) 和`BemfaCloud.Devices` [![NuGet Version](https://img.shields.io/nuget/v/BemfaCloud.Devices.svg?style=flat)](https://www.nuget.org/packages/BemfaCloud.Devices/)  
或  
通过Terminal安装：  
```shell
dotnet add package BemfaCloud
dotnet add package BemfaCloud.Devices
```

### 使用一个开关设备
控制一个开关关设备`testSwitchMqtt001`的`TCP`接入完整示例。  
```csharp
using System;
using System.Threading.Tasks;
using BemfaCloud;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Devices;
using BemfaCloud.Models;

namespace ConsoleApp7
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //构建一个Connector对象
            using IBemfaConnector connector = new BemfaConnectorBuilder()
                .WithTcp()
                .WithSecret("在此处填写你的私钥")
                .WithTopics("testSwitch001")  //可订阅多个
                .WithErrorHandler((e) =>
                {
                    Console.WriteLine($"[LOG][{DateTime.Now}] {e.Message}");
                })
                .WithMessageHandler((MessageEventArgs e) =>
                {
                    if (e.Type == CommandType.GetTimestamp)
                    {
                        Console.WriteLine($"收到消息：" + e);
                    }
                })
                .Build();

            //连接到服务器
            bool isConnect = await connector.ConnectAsync();
            if (!isConnect)
            {
                throw new Exception("Connect with server faild.");
            }

            //使用开关设备
            BemfaSwitch @switch = new BemfaSwitch("testSwitch001", connector);
            @switch.On += (e) =>
            {
                //执行开关打开动作
                Console.WriteLine("哦呦~需要打开开关");
                return true;
            };
            @switch.Off += (e) =>
            {
                //执行开关关闭动作
                Console.WriteLine("哦呦~需要关闭开关");
                return true;
            };
            @switch.OnException += (e) =>
            {
                Console.WriteLine($"发生了异常：{e.Message}");
            };
            @switch.OnMessage += (e) =>
            {
                Console.WriteLine($"收到无法解析的消息：{e.ToString()}");
            };

            while (true)
            {
                string readStr = Console.ReadLine();
                if (readStr.Equals("q", StringComparison.OrdinalIgnoreCase)
                    || readStr.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            await connector.DisconnectAsync();
            Console.WriteLine($"OK");
        }
    }
}
```

## 示例
#### 使用TCP连接
```csharp
using IBemfaConnector connector = new BemfaConnectorBuilder()
    .WithTcp()
    .WithSecret("在此处填写你的私钥")
    .WithTopics("testSwitchMqtt001")  //可订阅多个
    .WithErrorHandler((e) =>
    {
        Console.WriteLine($"[LOG][{DateTime.Now}] {e.Message}");
    })
    .WithMessageHandler((MessageEventArgs e) =>
    {
        Console.WriteLine($"收到消息：" + e);
    })
    .Build();

//连接到服务器
bool isConnect = await connector.ConnectAsync();
if (!isConnect)
{
    throw new Exception("Connect with server faild.");
}
```

#### 使用MQTT
```csharp
using IBemfaConnector connector = new BemfaConnectorBuilder()
    .WithMqtt()
    .WithSecret("在此处填写你的私钥")
    .WithTopics("testSwitchMqtt001")  //可订阅多个
    .WithErrorHandler((e) =>
    {
        Console.WriteLine($"[LOG][{DateTime.Now}] {e.Message}");
    })
    .WithMessageHandler((MessageEventArgs e) =>
    {
        Console.WriteLine($"收到消息：" + e);
    })
    .Build();

//连接到服务器
bool isConnect = await connector.ConnectAsync();
if (!isConnect)
{
    throw new Exception("Connect with server faild.");
}
```

### 支持的设备类型
支持巴法云常见的设备。  

| 设备  | 枚举  |  类  |
| ------------ | ------------ | ------------ |
| 插座  | 001  | BemfaSocket  |
| 灯  | 002  | BemfaLight  |
| 风扇  | 003  | BemfaFan  |
| 传感器  | 004  | BemfaSensor  |
| 空调  | 005  | BemfaAircon  |
| 开关  | 006  | BemfaSwitch  |
| 窗帘  | 009  | BemfaCurtain  |


#### 插座示例
```csharp
static void Socket(IBemfaConnector connector)
{
    BemfaSocket socket = new BemfaSocket("填写设备主题", connector);
    socket.On += (e) =>
    {
        //执行打开插座动作
        Console.WriteLine("哦呦~需要打开插座");
        return true;
    };
    socket.Off += (e) =>
    {
        //执行关闭插座动作
        Console.WriteLine("哦呦~需要关闭插座");
        return true;
    };
    socket.OnException += (e) =>
    {
        Console.WriteLine($"发生了异常：{e.Message}");
    };
    socket.OnMessage += (e) =>
    {
        Console.WriteLine($"收到无法解析的消息：{e.ToString()}");
    };
}
```

对于`On`或`Off`一类的操作，在操作完成之后，需要返回操作是否成功，用于向服务器指示当前设备状态。比如成功之后返回true，会像服务器指示已经完成操作且成功了。  

### 更多完整示例
https://github.com/withsalt/BemfaCloud/tree/main/src/Examples

## 树莓派
得益于.NET的完全跨平台特性，且微软提供的用于直接访问GPIO的库（System.Device.Gpio）。我们可以C#基于树莓派搭建一个强大的智能家居控制中心。  
树莓派示例：https://github.com/withsalt/BemfaCloud/blob/main/src/Examples/BemfaCloud.Example.RaspberryPi/Program.cs  
