using System;
using System.Threading;
using System.Threading.Tasks;
using BemfaCloud;
using BemfaCloud.Connectors;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Models;

namespace BemfaCloudTests.Concole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connector = new BemfaConnectorBuilder()
                .WithTcp()
                .WithSecret("在此处填写你的私钥")
                .WithTopics("pcSoftOpenEmbyWebOverTcp006")
                .WithTls()
                .WithErrorHandler((e) =>
                {
                    Console.WriteLine($"[LOG][{DateTime.Now}] {e.Message}");
                })
                .WithMessageHandler((e) =>
                {
                    Console.WriteLine($"收到消息：" + e);
                })
                .Build();

            await connector.ConnectAsync();

            while (true)
            {
                string readStr = Console.ReadLine();
                if (readStr.StartsWith("set:"))
                {
                    string val = readStr.Substring(4);
                    await connector.PublishAsync("pcSoftOpenEmbyWebOverTcp006", val);
                }
                else if (readStr.StartsWith("up:"))
                {
                    string val = readStr.Substring(3);
                    await connector.UpdateAsync("pcSoftOpenEmbyWebOverTcp006", val);
                }
                else if (readStr.StartsWith("t") || readStr.StartsWith("time"))
                {
                    if (connector is TcpConnector)
                    {
                        await (connector as TcpConnector).GetServerDateTime(DateTimeType.DateTime);
                    }
                }
                else if (readStr.Equals("q", StringComparison.OrdinalIgnoreCase)
                    || readStr.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            connector.Dispose();
            Console.WriteLine($"OK");
        }
    }
}