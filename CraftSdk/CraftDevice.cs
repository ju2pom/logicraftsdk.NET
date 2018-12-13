
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CraftSdk
{
    public class CraftDevice
    {
        private const int receiveChunkSize = 256;

        private ClientWebSocket client;
        private string sessionId = "";
        private string host1 = "wss://echo.websocket.org";
        private string host = "ws://localhost:10134";
        private bool receivedAck;
        private CancellationTokenSource cancellationTokenSource;

        public event Action<CrownRootObject> CrownTurned;
        public event Action<CrownRootObject> CrownTouched;

        public async Task<bool> TryRegister(Process process, Guid guid)
        {
            // build the connection request packet 
            CrownRegisterRootObject registerRootObject = new CrownRegisterRootObject();
            registerRootObject.message_type = "register";
            registerRootObject.plugin_guid = guid.ToString();
            registerRootObject.execName = process.MainModule.ModuleName;
            registerRootObject.PID = Convert.ToInt32(process.Id);

            // only connect to active session process
            registerRootObject.PID = Convert.ToInt32(process.Id);
            int activeConsoleSessionId = Win32.WTSGetActiveConsoleSessionId();

            // if we are running in active session?
            if (process.SessionId == activeConsoleSessionId)
            {
                await this.Send(registerRootObject);

                return true;
            }

            return false;
        }

        public void Connect()
        {
            if (this.client?.State == WebSocketState.Open)
            {
                return;
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => this.ConnectAndListen(), this.cancellationTokenSource.Token);
        }

        public async void Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            await this.client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        public bool IsConnected => this.client?.State == WebSocketState.Open && this.receivedAck;

        public string LastErrorMessage { get; private set; }

        public async Task ChangeTool(string toolName)
        {
            try
            {
                ToolChangeObject toolChangeObject = new ToolChangeObject();
                toolChangeObject.message_type = "tool_change";
                toolChangeObject.session_id = this.sessionId;
                toolChangeObject.tool_id = toolName;

                await this.Send(toolChangeObject);
            }
            catch (Exception ex)
            {
                this.LastErrorMessage = ex.Message;
            }
        }

        public async void GiveToolFeedback(string toolName, string toolOption, string value)
        {
            ToolUpdateRootObject toolUpdateRootObject = new ToolUpdateRootObject
            {
                tool_id = toolName,
                message_type = "tool_update",
                session_id = sessionId,
                show_overlay = "true",
                tool_options = new List<ToolOption> { new ToolOption { name = toolOption, value = value } }
            };

            await this.Send(toolUpdateRootObject);
        }

        private void ConnectAndListen()
        {
            try
            {
                this.client = new ClientWebSocket();
                this.client.ConnectAsync(new Uri(this.host), CancellationToken.None).Wait();

                Task.Run(this.Receive);
            }
            catch (Exception ex)
            {
                this.LastErrorMessage = ex.Message;
            }
        }

        private async Task Receive()
        {
            byte[] buffer = new byte[receiveChunkSize];
            while (this.client.State == WebSocketState.Open)
            {
                var result = await this.client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await this.client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    CrownRootObject crownRootObject = JsonConvert.DeserializeObject<CrownRootObject>(message);
                    this.OnNewMessage(crownRootObject);
                }
            }
        }

        private void OnNewMessage(CrownRootObject crownRootObject)
        {
            if (crownRootObject.message_type == "crown_turn_event")
            {
                this.CrownTurned?.Invoke(crownRootObject);
            }
            else if (crownRootObject.message_type == "crown_touch_event")
            {
                this.CrownTouched?.Invoke(crownRootObject);
            }
            else if (crownRootObject.message_type == "register_ack")
            {
                // save the session id as this is used for any communication with Logi Options 
                sessionId = crownRootObject.session_id;
                Console.WriteLine($"Register response: {crownRootObject.state}");
                this.receivedAck = true;
            }

            Console.WriteLine($"received: {crownRootObject.message_type}");
        }

        private async Task Send(object registerRootObject)
        {
            string serializedObject = JsonConvert.SerializeObject(registerRootObject);
            var message = Encoding.ASCII.GetBytes(serializedObject);
            var buffer = new ArraySegment<Byte>(message, 0, message.Length);

            await this.client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
