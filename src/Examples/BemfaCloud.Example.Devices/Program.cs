using System;
using System.Drawing;
using System.Threading.Tasks;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Devices;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Example.Devices
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using IBemfaConnector connector = new BemfaConnectorBuilder()
                .WithMqtt()
                .WithSecret("569e65d2343544ebb9e3377426b3bf95")
                .WithTopics("testSocket001", "testLight002", "testFan003", "testSensor004", "testAircon005", "testSwitch006", "testCurtain009")
                .WithErrorHandler((e) =>
                {
                    Console.WriteLine($"[LOG][{DateTime.Now}] {e.Message}");
                })
                .WithMessageHandler((e) =>
                {
                    if (e.Type == Models.CommandType.GetTimestamp)
                    {
                        Console.WriteLine($"收到消息：" + e);
                    }
                })
                .Build();

            bool isConnect = await connector.ConnectAsync();
            if (!isConnect)
            {
                throw new Exception("Connect with server faild.");
            }

            //开关
            Switch(connector);
            //插座
            Socket(connector);
            //灯
            Light(connector);
            //风扇
            Fan(connector);
            //传感器
            Sensor(connector);
            //空调
            Aircon(connector);
            //窗帘
            Curtain(connector);

            while (true)
            {
                string readStr = Console.ReadLine();
                if (readStr.Equals("q", StringComparison.OrdinalIgnoreCase)
                    || readStr.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                //输入【updsen】更新传感器数据
                else if (readStr.Equals("updsen", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateSensorValue();
                }
            }

            await connector.DisconnectAsync();
            Console.WriteLine($"OK");
        }

        static void Switch(IBemfaConnector connector)
        {
            BemfaSwitch @switch = new BemfaSwitch("testSwitch006", connector);
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
        }

        static void Socket(IBemfaConnector connector)
        {
            BemfaSocket socket = new BemfaSocket("testSocket001", connector);
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

        static void Light(IBemfaConnector connector)
        {
            BemfaLight light = new BemfaLight("testLight002", connector);
            light.On += (MessageEventArgs e, int brightness, Color color) =>
            {
                //执行开灯动作
                Console.WriteLine($"哦呦~需要打开灯，亮度：{brightness}，颜色：{color}");
                return true;
            };
            light.Off += (e) =>
            {
                //执行关灯动作
                Console.WriteLine("哦呦~需要关闭灯");
                return true;
            };
            light.OnException += (e) =>
            {
                Console.WriteLine($"发生了异常：{e.Message}");
            };
            light.OnMessage += (e) =>
            {
                Console.WriteLine($"收到无法解析的消息：{e.ToString()}");
            };
        }

        static void Fan(IBemfaConnector connector)
        {
            BemfaFan fan = new BemfaFan("testFan003", connector);
            fan.On += (MessageEventArgs e, int level, bool headStatus) =>
            {
                //执行打开风扇动作
                Console.WriteLine($"哦呦~需要打风扇，档位：{level}，是否摇头：{headStatus}");
                return true;
            };
            fan.Off += (e) =>
            {
                //执行关闭风扇动作
                Console.WriteLine("哦呦~需要关闭风扇");
                return true;
            };
            fan.OnException += (e) =>
            {
                Console.WriteLine($"发生了异常：{e.Message}");
            };
            fan.OnMessage += (e) =>
            {
                Console.WriteLine($"收到无法解析的消息：{e.ToString()}");
            };
        }

        private static BemfaSensor _sensor = null;

        static void Sensor(IBemfaConnector connector)
        {
            if (_sensor == null)
            {
                _sensor = new BemfaSensor("testSensor004", connector);
                _sensor.OnException += (e) =>
                {
                    Console.WriteLine($"发生了异常：{e.Message}");
                };
            }
        }

        static void UpdateSensorValue()
        {
            if (_sensor == null)
            {
                throw new Exception("还未实例化传感器！");
            }
            _sensor.WithTemperature(new Random().Next(10, 30))
                .WithHumidity(new Random().Next(30, 80))
                .WithDeviceStatus(DeviceStatus.On)
                .WithPM25(new Random().Next(30, 150))
                .Update();
        }

        static void Aircon(IBemfaConnector connector)
        {
            BemfaAircon aircon = new BemfaAircon("testAircon005", connector);
            aircon.On += (MessageEventArgs e, AirconMode mode, double temp, int level, bool? isLeftAndRightSweeping, bool? isUpAndDownSweeping) =>
            {
                //执行打开空调动作
                Console.WriteLine($"哦呦~需要打空调，模式：{mode}，温度：{temp}， 风速：{(level == 0 ? "自动" : level)}，是否左右扫风：{isLeftAndRightSweeping}，是否上下扫风：{isUpAndDownSweeping}");
                return true;
            };
            aircon.Off += (e) =>
            {
                //执行关闭空调动作
                Console.WriteLine("哦呦~需要关闭空调");
                return true;
            };
            aircon.OnException += (e) =>
            {
                Console.WriteLine($"发生了异常：{e.Message}");
            };
            aircon.OnMessage += (e) =>
            {
                Console.WriteLine($"收到无法解析的消息：{e.ToString()}");
            };
        }

        static void Curtain(IBemfaConnector connector)
        {
            BemfaCurtain curtain = new BemfaCurtain("testCurtain009", connector);
            curtain.On += (e, percentage) =>
            {
                //执行打开窗帘动作
                Console.WriteLine($"哦呦~需要打窗帘，打开至：{percentage}");
                return true;
            };
            curtain.Off += (e) =>
            {
                //执行关闭窗帘动作
                Console.WriteLine($"哦呦~需要关闭窗帘");
                return true;
            };
            curtain.Pause += (e) =>
            {
                //执行暂停窗帘动作
                Console.WriteLine("哦呦~需要暂停窗帘");
                return true;
            };
            curtain.OnException += (e) =>
            {
                Console.WriteLine($"发生了异常：{e.Message}");
            };
            curtain.OnMessage += (e) =>
            {
                Console.WriteLine($"收到无法解析的消息：{e.ToString()}");
            };
        }
    }
}