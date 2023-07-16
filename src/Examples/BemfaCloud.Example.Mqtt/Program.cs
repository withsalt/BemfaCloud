using System;
using System.Threading.Tasks;
using BemfaCloud.Connectors.Builder;

namespace BemfaCloud.Example.Mqtt
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connector = new BemfaConnectorBuilder()
                .WithMqtt()
                .WithSecret("在此处填写你的私钥")
                .WithTopics("pcSoftOpenEmbyWeb006")
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
                    await connector.PublishAsync("pcSoftOpenEmbyWeb006", val);
                }
                else if (readStr.StartsWith("up:"))
                {
                    string val = readStr.Substring(3);
                    await connector.UpdateAsync("pcSoftOpenEmbyWeb006", val);
                }
                else if (readStr.Equals("q", StringComparison.OrdinalIgnoreCase) 
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