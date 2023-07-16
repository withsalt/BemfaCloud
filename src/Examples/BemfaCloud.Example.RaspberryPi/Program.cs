using System;
using System.Device.Gpio;
using System.Threading.Tasks;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Devices;

namespace BemfaCloud.Example.RaspberryPi
{
    internal class Program
    {
        /// <summary>
        /// 控制GPIO（物理引脚7，GPIO4）
        /// </summary>
        private const int PIN = 7;

        static async Task Main(string[] args)
        {
            using IBemfaConnector connector = new BemfaConnectorBuilder()
                .WithMqtt()
                .WithSecret("在此处填写你的私钥")
                .WithTopics("pcSoftOpenEmbyWeb006")
                .WithErrorHandler((e) =>
                {
                    Console.WriteLine($"[LOG][{DateTime.Now}] {e.Message}");
                })
                .WithMessageHandler((e) =>
                {
                    if (e.CommandType == Models.CommandType.GetTimestamp)
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

            Switch(connector);

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

        static void Switch(IBemfaConnector connector)
        {
            //注意！！！只能运行在树莓派上面，或者gpio库支持的板子上
            using GpioController controller = new GpioController(PinNumberingScheme.Board);
            // 设置引脚为输出模式
            controller.OpenPin(PIN, PinMode.Output);
            if (!controller.IsPinOpen(PIN))
            {
                Console.WriteLine($"Set pin {PIN} output mode failed.");
            }

            BemfaSwitch @switch = new BemfaSwitch("pcSoftOpenEmbyWeb006", connector);
            @switch.On += (e) =>
            {
                //执行开关打开动作，设置为高电平
                controller.Write(PIN, PinValue.High);
                Console.WriteLine("打开开关了");

                return true;
            };
            @switch.Off += (e) =>
            {
                //执行开关关闭动作，设置为低电平
                controller.Write(PIN, PinValue.Low);
                Console.WriteLine("开关关闭");
                return true;
            };
        }
    }
}