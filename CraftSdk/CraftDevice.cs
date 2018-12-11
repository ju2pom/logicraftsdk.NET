
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace CraftSdk
{
    public class CraftDevice
    {
        private WebSocket client;

        private string sessionId = "";
        private string host1 = "wss://echo.websocket.org";
        private string host = "ws://localhost:10134";
        private bool receivedAck;
        private Task backgroundTask;
        private CancellationTokenSource cancellationTokenSource;
        private AutoResetEvent autoResetEvent;

        public event Action<CrownRootObject> CrownTurned;
        public event Action<CrownRootObject> CrownTouched;

        public bool TryRegister(Process process, Guid guid)
        {
            // build the connection request packet 
            CrownRegisterRootObject registerRootObject = new CrownRegisterRootObject();
            registerRootObject.message_type = "register";
            registerRootObject.plugin_guid = guid.ToString();
            registerRootObject.execName = process.MainModule.ModuleName;
            registerRootObject.PID = Convert.ToInt32(process.Id);
            string serializedObject = JsonConvert.SerializeObject(registerRootObject);

            // only connect to active session process
            registerRootObject.PID = Convert.ToInt32(process.Id);
            int activeConsoleSessionId = Win32.WTSGetActiveConsoleSessionId();

            // if we are running in active session?
            if (process.SessionId == activeConsoleSessionId)
            {  
                this.client.Send(serializedObject);

                return true;
            }

            return false;
        }

        public void Connect()
        {
            if (this.backgroundTask != null && this.client.IsAlive)
            {
                return;
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            this.backgroundTask = Task.Run(() => this.ConnectAndListen(), this.cancellationTokenSource.Token);
        }

        public void Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.autoResetEvent.Set();
        }

        public bool IsConnected => this.client?.IsAlive == true && this.receivedAck;

        public string LastErrorMessage { get; private set; }

        public void ChangeTool(string toolName)
        {
            try
            {
                ToolChangeObject toolChangeObject = new ToolChangeObject();
                toolChangeObject.message_type = "tool_change";
                toolChangeObject.session_id = this.sessionId;
                toolChangeObject.tool_id = toolName;

                string serializedObject = JsonConvert.SerializeObject(toolChangeObject);
                client.Send(serializedObject);
            }
            catch (Exception ex)
            {
                this.LastErrorMessage = ex.Message;
            }
        }

        public void GiveToolFeedback(string toolName, string toolOption, string value)
        {
            ToolUpdateRootObject toolUpdateRootObject = new ToolUpdateRootObject
            {
                tool_id = toolName,
                message_type = "tool_update",
                session_id = sessionId,
                show_overlay = "true",
                tool_options = new List<ToolOption> { new ToolOption { name = toolOption, value = value } }
            };

            string serialized = JsonConvert.SerializeObject(toolUpdateRootObject);
            client.Send(serialized);
        }

        private void ConnectAndListen()
        {
            try
            {
                this.client = new WebSocket(this.host);

                client.OnOpen += this.OnOpen;
                client.OnError += (ss, ee) => this.LastErrorMessage = ee.Message;
                client.OnMessage += this.OnNewMessage;
                client.OnClose += this.OnClose;
                client.Connect();

                autoResetEvent = new AutoResetEvent(false);
                while (!this.cancellationTokenSource.Token.IsCancellationRequested)
                {
                    autoResetEvent.WaitOne();

                }
            }
            catch (Exception ex)
            {
                this.LastErrorMessage = ex.Message;
            }
        }

        private void OnNewMessage(object sender, MessageEventArgs e)
        {
            CrownRootObject crownRootObject = JsonConvert.DeserializeObject<CrownRootObject>(e.Data);

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
                this.receivedAck = true;
            }
            this.autoResetEvent.Set();
        }

        private void OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("Connection opened");
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine("Connection closed");
        }
    }
}
