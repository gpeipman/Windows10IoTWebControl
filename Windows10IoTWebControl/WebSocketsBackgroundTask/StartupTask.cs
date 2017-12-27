using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace WebSocketsBackgroundTask
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const string WebInterfaceUrl = "ws://localhost:5000/";

        private bool _isClosing = false;
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;
            _deferral = taskInstance.GetDeferral();

            var tasks = new Task[2];
            tasks[0] = Task.Run(async () => { await Report(); });
            tasks[1] = Task.Run(async () => { await Read(); });

            Task.WaitAll(tasks);

            _deferral.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _isClosing = true;
        }

        private async Task Report()
        {
            var rnd = new Random();

            using (var client = new ClientWebSocket())
            {
                var ct = new CancellationToken();

                await client.ConnectAsync(new Uri(WebInterfaceUrl), ct);

                while (true)
                {
                    if (_isClosing)
                    {
                        break;
                    }

                    var message = "Report " + (rnd.NextDouble() * 100);
                    var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await client.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                    await Task.Delay(10000);
                }

                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            }
        }

        private async Task Read()
        {
            using (var client = new ClientWebSocket())
            {
                var ct = new CancellationToken();

                await client.ConnectAsync(new Uri(WebInterfaceUrl), ct);

                while (true)
                {
                    if (_isClosing)
                    {
                        break;
                    }

                    var fromSocket = await ReceiveStringAsync(client, ct);
                    Debug.WriteLine(fromSocket);
                }

                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            }
        }

        private static async Task<string> ReceiveStringAsync(ClientWebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
