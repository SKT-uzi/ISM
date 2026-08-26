
using ISMSimulator.Entity;
using ISMSimulator.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Threading.Tasks;

namespace ISMSimulator
{
    public partial class Form1 : Form
    {
        private IMqttClient _mqttClient;
        private System.Threading.Timer timerAcceptResult = null;
        private int dueTimeAcceptResult = 1000;
        private System.Threading.Timer timerNetworkAlwaysSend = null;
        private System.Threading.Timer timerCameraAlwaysSend = null;
        private System.Threading.Timer timerScaleAlwaysSend = null;
        private System.Threading.Timer timerEIDAlwaysSend = null;
        private int dueTimeAlwaysSend = 1000;
        private string DeviceID { get; set; }

        private bool timerNetworkContinue { get; set; } = true;
        private bool timerCameraContinue { get; set; } = true;
        private bool timerScaleContinue { get; set; } = true;
        private bool timerEIDContinue { get; set; } = true;

        private JObject recurringJsonObject = null;
        private JObject adhocJsonObject = null;
        private ToolTip toolTip = new ToolTip();

        private string adhocFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "message_adhoc.json");

        private string recurringFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "message_recurring.json");

        private List<string> stringJsons = new List<string>();

        public Form1()
        {
            InitializeComponent();
            this.Text = ConfigurationManager.AppSettings["Title"] ?? String.Empty;
            this.textBox1.Text = "wss://localhost:8101/ism/mqtt";
            this.textBox2.Text = "local-dev-user";
            this.textBox3.Text = "local-dev-password";

            this.SetSendButtonStattus(false);

            string recurringMessage = File.ReadAllText(recurringFilePath);
            recurringJsonObject = JObject.Parse(recurringMessage);

            string adhocMessage = File.ReadAllText(adhocFilePath);
            adhocJsonObject = JObject.Parse(adhocMessage);

            //init textbox value
            var scaleSuccessInfo = JsonConvert.DeserializeObject<SimulatorScale>(recurringJsonObject["ScaleSuccess"].ToString());
            this.txtWeight.Text = scaleSuccessInfo.Value.Weight.ToString();
            this.txtRate.Text = scaleSuccessInfo.Value.Rate_HZ.ToString();

            var UHFSuccessInfo = JObject.Parse(recurringJsonObject["UHFEIDSuccess"].ToString());
            this.txtUHFReader.Text = ((JValue)UHFSuccessInfo["value"]["reader"]).Value?.ToString();

            var LFSuccessInfo = JObject.Parse(recurringJsonObject["LFEIDSuccess"].ToString());
            this.txtLFReader.Text = ((JValue)LFSuccessInfo["value"]["reader"]).Value?.ToString();
            this.txtBaudRate.Text = ((JValue)LFSuccessInfo["value"]["baud"]).Value?.ToString();
            var systemInfo = JObject.Parse(recurringJsonObject["DeviceHealth"].ToString());
            this.txtUptime.Text = ((JValue)systemInfo["value"]["uptime"]).Value?.ToString();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;
                var text = btnConnect.Text;
                if (text == "Connect")
                {
                    await ConnectMqttServerAsync();

                    this.Invoke(new Action(delegate ()
                    {
                        this.SetSendButtonStattus(true);
                        this.SetUIStatusByConnect(true);

                    }));
                }
                else
                {
                    if (timerNetworkAlwaysSend != null)
                    {
                        timerNetworkContinue = false;
                    }
                    if (timerCameraAlwaysSend != null)
                    {
                        timerCameraContinue = false;
                    }
                    if (timerScaleAlwaysSend != null)
                    {
                        timerScaleContinue = false;
                    }
                    if (timerEIDAlwaysSend != null)
                    {
                        timerEIDContinue = false;
                    }

                    SetSendButtonStattus(false);

                    await DisconnectAsync();

                    this.SetUIStatusByConnect(false);
                    this.AddTraceLog($"Disconnected");
                    this.DeviceID = String.Empty;
                    this.btnScaleSendAlways.Enabled = false;
                    this.btnScaleSendOnce.Enabled = false;
                    this.btnScaleStop.Enabled = false;

                }
            }
            catch (Exception ex)
            {
                btnConnect.Text = "Connect";

                if (ex.Message.IndexOf("A task was canceled") > -1)
                {
                    MessageBox.Show($"Connection fail.");
                }
                else
                {
                    MessageBox.Show($"Ex:{ex.Message}");
                }
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private async Task ConnectMqttServerAsync()
        {
            var server = this.textBox1.Text;
            var userName = this.textBox2.Text;
            var password = this.textBox3.Text;

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer(
                    o => o.WithUri(server)
                )
                .WithClientId(Guid.NewGuid().ToString())
                .WithCredentials(userName, password)
                .WithCleanSession()
                .Build();

            _mqttClient.ConnectedAsync += async e =>
            {
                AddTraceLog($"已连接到 MQTT 服务器: {server}");
                await Task.CompletedTask;
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                AddTraceLog($" 已断开与 MQTT 服务器的连接");
                await Task.CompletedTask;
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var message = e.ApplicationMessage.ConvertPayloadToString();
                AddTraceLog($"收到消息: {message}");
                await HandleReceivedMessage(message);
                await Task.CompletedTask;
            };

            try
            {
                await _mqttClient.ConnectAsync(options, CancellationToken.None);

                await SubscribeToTopic("ISMNetworkConfig");
                await SubscribeToTopic("ISMCameraConfig");
                await SubscribeToTopic("ISMEIDConfig");
            }
            catch (Exception ex)
            {
                AddTraceLog($"连接失败: {ex.Message}");
            }
        }

        private async Task DisconnectAsync()
        {
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }

        private async Task SubscribeToTopic(string topic)
        {
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .Build();

                await _mqttClient.SubscribeAsync(new MqttClientSubscribeOptions
                {
                    TopicFilters = new List<MqttTopicFilter> { topicFilter }
                });

                AddTraceLog($"已订阅主题: {topic}");
            }
        }


        public async Task PublishMessage(string topic, string message)
        {
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
                AddTraceLog($"已发布消息到主题 {topic}: {message}");
            }
        }

        public async Task HandleReceivedMessage(string receivedMessage)
        {
            var requestInfo = BaseHelper.DeserializeObject<RequestMessage>(receivedMessage);
            string route = string.Empty;
            string message = string.Empty;

            if(requestInfo.Method == "request")
            {
                if (requestInfo.Type.ToLower() == "lfeid")
                {
                    route = "ISMEIDConfig";
                    if (requestInfo.Args == "scanbluetooth")
                    {
                        if (this.radioScanBlueToothSuccess.Checked)
                        {
                            message = adhocJsonObject["LFEIDScanBluetoothSuccess"].ToString();
                        }
                        else
                        {
                            message = adhocJsonObject["LFEIDScanBluetoothError"].ToString();
                        }
                    }
                    else if (requestInfo.Args == "connectbluetooth")
                    {
                        if (this.radioConnectBlueToothSuccess.Checked)
                        {
                            message = adhocJsonObject["LFEIDConnectBluetoothSuccess"].ToString();
                        }
                        else
                        {
                            message = adhocJsonObject["LFEIDConnectBluetoothError"].ToString();
                        }
                    }
                    else if (requestInfo.Args == "readbluetooth")
                    {
                        BaudRate baudRate = BaseHelper.DeserializeObject<BaudRate>(BaseHelper.SerializeObject(requestInfo.Value, true));
                        this.Invoke((MethodInvoker)delegate
                        {
                            this.txtBaudRate.Text = baudRate.Baud.ToString();
                        });

                        return;
                    }
                    else if (requestInfo.Args == "savebaud")
                    {
                        return;
                    }
                }
                else if (requestInfo.Type.ToLower() == "ethernet")
                {
                    route = "ISMNetworkConfig";
                    if (requestInfo.Args == "auto")
                    {
                        if (this.radio_EthernetAutoSuccess.Checked)
                        {
                            message = adhocJsonObject["EthernetAutoSuccess"].ToString();
                        }
                        else
                        {
                            message = adhocJsonObject["EthernetAutoError"].ToString();
                        }
                    }
                    else if (requestInfo.Args == "manual")
                    {
                        if (this.radio_EthernetAddSuccess.Checked)
                        {
                            message = adhocJsonObject["EthernetManualAddSuccess"].ToString();
                        }
                        else
                        {
                            message = adhocJsonObject["EthernetManualAddError"].ToString();
                        }
                    }
                }
                else if (requestInfo.Type.ToLower() == "wireless")
                {
                    route = "ISMNetworkConfig";
                    if (requestInfo.Args == "connect")
                    {
                        if (this.radio_WifiAddSuccess.Checked)
                        {
                            message = adhocJsonObject["WirelessConnectSuccess"].ToString();
                        }
                        else
                        {
                            message = adhocJsonObject["WirelessConnectError"].ToString();
                        }
                    }
                    else if (requestInfo.Args == "auto")
                    {
                        if (this.radio_WifiAutoSuccess.Checked)
                        {
                            message = adhocJsonObject["WirelessAutoSuccess"].ToString();
                        }
                        else
                        {
                            message = adhocJsonObject["WirelessAutoError"].ToString();
                        }
                    }
                }
                else if (requestInfo.Type.ToLower() == "camera")
                {
                    route = "ISMCameraConfig";
                    if (requestInfo.Args == "startlive")
                    {
                        if (this.radio_CameraStartSuccess.Checked)
                        {
                            message = adhocJsonObject["CameraStartLiveSuccess"].ToString();
                        }
                        else
                        {
                            message = adhocJsonObject["CameraStartLiveError"].ToString();
                        }
                    }
                }
            }

            if (route != string.Empty && message != string.Empty)
            {
                await PublishMessage(route, message);
            }

        }

        private void SetSendButtonStattus(bool status)
        {

            this.btnNetworkSendAlways.Enabled = status;
            this.btnNetworkSendOnce.Enabled = status;
            this.btnNetworkStopSend.Enabled = status;

            this.btnCameraSendAlways.Enabled = status;
            this.btnCameraSendOnce.Enabled = status;
            this.btnCameraStopSend.Enabled = status;

            this.btnScaleSendAlways.Enabled = status;
            this.btnScaleSendOnce.Enabled = status;
            this.btnScaleStop.Enabled = status;

            this.btnEIDSendAlways.Enabled = status;
            this.btnEIDSendOnce.Enabled = status;
            this.btnEIDStopSend.Enabled = status;
        }

        private void SetUIStatusByConnect(bool connectYN)
        {

            btnConnect.Text = connectYN ? "Disconnect" : "Connect";
        }

        private void AddTraceLog(string message, string action = "", bool isInvoke = true)
        {
            var dtNow = DateTime.Now.ToString("hh:mm:sstt");
            this.RunAction(isInvoke, () =>
            {
                if (message.IndexOf("{") < 0)
                {
                    lbxDetailList.Items.Add($"{dtNow}: {message}");
                }
                else
                {
                    lbxDetailList.Items.Add($"{dtNow} {action}: {BaseHelper.RemoveSpecialCharactor(message)}");
                }
                stringJsons.Add(message);
                lbxDetailList.SelectedIndex = lbxDetailList.Items.Count - 1;
            });
        }

        /// <summary>
        /// Runs the action.
        /// </summary>
        /// <param name="isInvoke">if set to <c>true</c> [is invoke].</param>
        /// <param name="doAction">The do action.</param>
        /// <returns></returns>
        private void RunAction(bool isInvoke, Action doAction)
        {
            try
            {
                if (isInvoke)
                    this.Invoke(doAction);
                else
                    doAction();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        //private void RecieveAdhocResponseFromISM()
        //{
        //    //timer AcceptResult
        //    timerAcceptResult = new System.Threading.Timer((arg) =>
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(this.DeviceID))
        //                return;
        //            //monitor ISM request
        //            int successCount = this.SendAdhocResponseToISM(this.DeviceID);
        //        }
        //        catch (Exception ex)
        //        {
        //            if (ex.Message.Contains("The client is not connected."))
        //            {
        //                this.Invoke(new Action(delegate ()
        //                {
        //                    this.DeviceID = String.Empty;
        //                    this.SetUIStatusByConnect(false);

        //                    this.SetSendButtonStattus(false);

        //                    this.AddTraceLog($"Disconnected");
        //                }));
        //            }
        //        }
        //        finally
        //        {
        //            timerAcceptResult?.Change(dueTimeAcceptResult, Timeout.Infinite);
        //        }
        //    }, null, 0, Timeout.Infinite);

        //}

        /// <summary>
        /// Send  Message
        /// </summary>
        private async Task<bool> SendNetworkRecurringMessage()
        {
            string deviceID = this.DeviceID;
            var listMessage = new List<ResponseMessage>();
            var stringsEthernet = new List<string>();
            var stringsWifi = new List<string>();

            var messageInfo = new ResponseMessage();
            // 1.Topic:ISMDeviceStatus
            messageInfo.Topic = "ISMDeviceStatus";
            //Ethernet
            if (this.check_EthernetConnect.Checked)
            {
                stringsEthernet.Add(recurringJsonObject["EthernetConnected"].ToString());
            }
            if (this.check_EthernetNotConnect.Checked)
            {
                stringsEthernet.Add(recurringJsonObject["EthernetDisconnected"].ToString());
            }
            if (this.check_EthernetNoInternet.Checked)
            {
                stringsEthernet.Add(recurringJsonObject["EthernetConnectedNoInternet"].ToString());
            }

            //Wireless
            if (this.check_WirelessContect.Checked)
            {
                stringsWifi.Add(recurringJsonObject["WirelessConnected"].ToString());
            }
            if (this.check_WirelessDisconnect.Checked)
            {
                stringsWifi.Add(recurringJsonObject["WirelessDisconnected"].ToString());
            }
            if (this.check_WirelessNoInternet.Checked)
            {
                stringsWifi.Add(recurringJsonObject["WirelessConnectedNoInternet"].ToString());
            }

            if (stringsEthernet.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsEthernet));
            }
            if (stringsWifi.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsWifi));
            }


            //reset uptime value
            var uptimeInfo = JObject.Parse(recurringJsonObject["DeviceHealth"].ToString());
            decimal upTime = 0;
            decimal.TryParse(this.txtUptime.Text, out upTime);
            ((JValue)uptimeInfo["value"]["uptime"]).Value = upTime;
            messageInfo.Message.Add(uptimeInfo.ToString());
            listMessage.Add(messageInfo);

            foreach (var item in listMessage)
            {
                foreach (var message in item.Message)
                {
                    await PublishMessage(item.Topic, message);
                }
            }
            return true;
        }

        /// <summary>
        /// Send Camera Recurring Message
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SendCameraRecurringMessage()
        {
            string deviceID = this.DeviceID;
            var listMessage = new List<ResponseMessage>();
            var stringsCamera = new List<string>();
            var messageInfo = new ResponseMessage();

            // 1.Topic:ISMDeviceStatus
            messageInfo.Topic = "ISMDeviceStatus";
            //Camera
            if (this.check_CameraOnline.Checked)
            {
                stringsCamera.Add(recurringJsonObject["CameraOnline"].ToString());
            }
            if (this.check_CameraOffline.Checked)
            {
                stringsCamera.Add(recurringJsonObject["CameraOffline"].ToString());
            }

            if (stringsCamera.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsCamera));
            }
            listMessage.Add(messageInfo);

            foreach (var item in listMessage)
            {
                foreach (var message in item.Message)
                {
                    await PublishMessage(item.Topic, message);
                }
            }
            return true;
        }

        /// <summary>
        /// Send Scale Recurring Message
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SendScaleRecurringMessage()
        {
            string deviceID = this.DeviceID;
            var listMessage = new List<ResponseMessage>();
            var stringsScale = new List<string>();
            var stringsScaleResponse = new List<string>();
            var messageInfo = new ResponseMessage();

            // 1.Topic:ISMDeviceStatus
            messageInfo.Topic = "ISMDeviceStatus";

            //Scale
            if (this.check_ScaleOnline.Checked)
            {
                stringsScale.Add(recurringJsonObject["ScaleConnected"].ToString());
            }
            if (this.check_ScaleOffline.Checked)
            {
                stringsScale.Add(recurringJsonObject["ScaleDisconnected"].ToString());
            }
            if (stringsScale.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsScale));
            }

            listMessage.Add(messageInfo);

            // 2.Topic:ISMScaleConfig
            messageInfo = new ResponseMessage();
            messageInfo.Topic = "ISMScaleConfig";

            //Scale
            if (this.check_ScaleSuccess.Checked)
            {
                //reset weight value
                var scaleSuccessInfo = JObject.Parse(recurringJsonObject["ScaleSuccess"].ToString());
                int weight = 0;
                int.TryParse(this.txtWeight.Text, out weight);
                ((JValue)scaleSuccessInfo["value"]["weight"]).Value = weight;

                decimal rate = 0;
                decimal.TryParse(this.txtRate.Text, out rate);
                ((JValue)scaleSuccessInfo["value"]["rate_HZ"]).Value = rate;
                stringsScaleResponse.Add(scaleSuccessInfo.ToString());
            }
            if (this.check_ScaleError.Checked)
            {
                stringsScaleResponse.Add(recurringJsonObject["ScaleError"].ToString());
            }
            if (stringsScaleResponse.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsScaleResponse));
                listMessage.Add(messageInfo);
            }

            foreach (var item in listMessage)
            {
                foreach (var message in item.Message)
                {
                    await PublishMessage(item.Topic, message);
                }
            }
            return true;
        }

        /// <summary>
        /// Send EID Recurring Message
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SendEIDRecurringMessage()
        {
            string deviceID = this.DeviceID;
            var listMessage = new List<ResponseMessage>();
            var stringsUHFEID = new List<string>();
            var stringsLFEIDResponse = new List<string>();
            var stringsUHFEIDResponse = new List<string>();

            var messageInfo = new ResponseMessage();
            // 1.Topic:ISMDeviceStatus
            messageInfo.Topic = "ISMDeviceStatus";

            //UHF EID
            if (this.check_UHFConnect.Checked)
            {
                stringsUHFEID.Add(recurringJsonObject["UHFEIDConnected"].ToString());
            }
            if (this.check_UHFDisconnect.Checked)
            {
                stringsUHFEID.Add(recurringJsonObject["UHFEIDDisconnected"].ToString());
            }

            if (stringsUHFEID.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsUHFEID));
            }

            listMessage.Add(messageInfo);

            // 3.Topic:ISMEIDConfig
            messageInfo = new ResponseMessage();
            messageInfo.Topic = "ISMEIDConfig";

            // LF
            if (this.check_LFSuccess.Checked)
            {
                //reset reader value
                var LFSuccessInfo = JObject.Parse(recurringJsonObject["LFEIDSuccess"].ToString());
                ((JValue)LFSuccessInfo["value"]["reader"]).Value = this.txtLFReader.Text;
                ((JValue)LFSuccessInfo["value"]["baud"]).Value = int.Parse(this.txtBaudRate.Text);
                stringsLFEIDResponse.Add(LFSuccessInfo.ToString());
            }
            if (this.check_LFError.Checked)
            {
                stringsLFEIDResponse.Add(recurringJsonObject["LFEIDError"].ToString());
            }



            //UHF
            if (this.check_UHFSuccess.Checked)
            {
                //reset reader value
                var UHFSuccessInfo = JObject.Parse(recurringJsonObject["UHFEIDSuccess"].ToString());
                ((JValue)UHFSuccessInfo["value"]["reader"]).Value = this.txtUHFReader.Text;
                stringsUHFEIDResponse.Add(UHFSuccessInfo.ToString());
            }
            if (this.check_UHFError.Checked)
            {
                stringsUHFEIDResponse.Add(recurringJsonObject["UHFEIDError"].ToString());
            }
            if (stringsLFEIDResponse.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsLFEIDResponse));
            }
            if (stringsUHFEIDResponse.Count > 0)
            {
                messageInfo.Message.Add(this.GetRandomMessage(stringsUHFEIDResponse));
            }
            listMessage.Add(messageInfo);

            foreach (var item in listMessage)
            {
                foreach (var message in item.Message)
                {
                    await PublishMessage(item.Topic, message);
                }
            }
            return true;
        }


        /// <summary>
        /// Get Random Message
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public string GetRandomMessage(List<string> arrMessage)
        {
            Random ran = new Random();
            int n = ran.Next(arrMessage.Count);
            return arrMessage[n];
        }

        /// <summary>
        /// Refreshes the end sort pen.
        /// </summary>
        //private int SendAdhocResponseToISM(string deviceID)
        //{
        //    int successCount = 0;
        //    try
        //    {
        //        string route = string.Empty;
        //        string message = string.Empty;

        //        var allResponseText = IotCentralConnect.SendMQTTMessage(deviceID, "GET_ISM_ALL_REQUEST");

        //        if (!string.IsNullOrEmpty(allResponseText))
        //        {
        //            allResponseText = $"[{allResponseText}]";
        //            var requestList = BaseHelper.DeserializeObject<List<RequestMessage>>(allResponseText);

        //            foreach (var requestInfo in requestList)
        //            {
        //                string data = BaseHelper.SerializeObject(requestInfo, true);
        //                this.AddTraceLog("Recieved:" + data);

        //                if (requestInfo.Type.ToLower() == "lfeid")
        //                {
        //                    route = "ISMEIDConfig";
        //                    if (requestInfo.Args == "scanbluetooth")
        //                    {
        //                        if (this.radioScanBlueToothSuccess.Checked)
        //                        {
        //                            message = adhocJsonObject["LFEIDScanBluetoothSuccess"].ToString();
        //                        }
        //                        else
        //                        {
        //                            message = adhocJsonObject["LFEIDScanBluetoothError"].ToString();
        //                        }
        //                    }
        //                    else if (requestInfo.Args == "connectbluetooth")
        //                    {
        //                        if (this.radioConnectBlueToothSuccess.Checked)
        //                        {
        //                            message = adhocJsonObject["LFEIDConnectBluetoothSuccess"].ToString();
        //                        }
        //                        else
        //                        {
        //                            message = adhocJsonObject["LFEIDConnectBluetoothError"].ToString();
        //                        }
        //                    }
        //                    else if (requestInfo.Args == "readbluetooth")
        //                    {
        //                        BaudRate baudRate = BaseHelper.DeserializeObject<BaudRate>(BaseHelper.SerializeObject(requestInfo.Value, true));
        //                        this.Invoke((MethodInvoker)delegate
        //                        {
        //                            this.txtBaudRate.Text = baudRate.Baud.ToString();
        //                        });
        //                        IotCentralConnect.SendIoTCentalCmd(deviceID, "CLEAN_ISMEIDConfig");
        //                        continue;
        //                    }
        //                    else if (requestInfo.Args == "savebaud")
        //                    {
        //                        IotCentralConnect.SendIoTCentalCmd(deviceID, "CLEAN_ISMEIDConfig");
        //                        continue;
        //                    }
        //                }
        //                else if (requestInfo.Type.ToLower() == "ethernet")
        //                {
        //                    route = "ISMNetworkConfig";
        //                    if (requestInfo.Args == "auto")
        //                    {
        //                        if (this.radio_EthernetAutoSuccess.Checked)
        //                        {
        //                            message = adhocJsonObject["EthernetAutoSuccess"].ToString();
        //                        }
        //                        else
        //                        {
        //                            message = adhocJsonObject["EthernetAutoError"].ToString();
        //                        }
        //                    }
        //                    else if (requestInfo.Args == "manual")
        //                    {
        //                        if (this.radio_EthernetAddSuccess.Checked)
        //                        {
        //                            message = adhocJsonObject["EthernetManualAddSuccess"].ToString();
        //                        }
        //                        else
        //                        {
        //                            message = adhocJsonObject["EthernetManualAddError"].ToString();
        //                        }
        //                    }
        //                }
        //                else if (requestInfo.Type.ToLower() == "wireless")
        //                {
        //                    route = "ISMNetworkConfig";
        //                    if (requestInfo.Args == "connect")
        //                    {
        //                        if (this.radio_WifiAddSuccess.Checked)
        //                        {
        //                            message = adhocJsonObject["WirelessConnectSuccess"].ToString();
        //                        }
        //                        else
        //                        {
        //                            message = adhocJsonObject["WirelessConnectError"].ToString();
        //                        }
        //                    }
        //                    else if (requestInfo.Args == "auto")
        //                    {
        //                        if (this.radio_WifiAutoSuccess.Checked)
        //                        {
        //                            message = adhocJsonObject["WirelessAutoSuccess"].ToString();
        //                        }
        //                        else
        //                        {
        //                            message = adhocJsonObject["WirelessAutoError"].ToString();
        //                        }
        //                    }
        //                }
        //                else if (requestInfo.Type.ToLower() == "camera")
        //                {
        //                    route = "ISMCameraConfig";
        //                    if (requestInfo.Args == "startlive")
        //                    {
        //                        if (this.radio_CameraStartSuccess.Checked)
        //                        {
        //                            message = adhocJsonObject["CameraStartLiveSuccess"].ToString();
        //                        }
        //                        else
        //                        {
        //                            message = adhocJsonObject["CameraStartLiveError"].ToString();
        //                        }
        //                    }
        //                }
        //                IotCentralConnect.SendMQTTMessage2(deviceID, "SEND_ISM_MQTT", route, message);
        //                successCount++;
        //                this.AddTraceLog($"Send:{message}");
        //            }
        //        }
        //        return successCount;
        //    }
        //    catch (Exception ex)
        //    {
        //        this.AddTraceLog($"Exception:{ex.Message}");
        //        MessageBox.Show(ex.Message);
        //        return successCount;
        //    }
        //}

        /// <summary>
        /// lbxDetail Selected Index Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbxDetail_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lbxDetailList.SelectedIndices.Count > 0)
            {
                this.toolTip.Active = true;
                this.toolTip.AutoPopDelay = 30000;
                this.toolTip.SetToolTip(this.lbxDetailList, BaseHelper.ConvertJsonString(stringJsons[this.lbxDetailList.SelectedIndex]));
            }
            else
            {
                this.toolTip.Active = false;
            }
        }

        /// <summary>
        /// Clear Message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ClearMessage_Click(object sender, EventArgs e)
        {
            lbxDetailList.Items.Clear();
            stringJsons.Clear();
        }

        #region Network Tab
        /// <summary>
        /// NetWork Send Once
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnNetWorkSendOnce_Click(object sender, EventArgs e)
        {
            await this.SendNetworkRecurringMessage();
        }

        /// <summary>
        /// NetWork Always Send 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNetworkSendAlways_Click(object sender, EventArgs e)
        {
            timerNetworkContinue = true;
            this.btnNetworkSendAlways.Enabled = false;
            timerNetworkAlwaysSend = new System.Threading.Timer((arg) =>
            {
                try
                {
                    var result = this.SendNetworkRecurringMessage().Result;
                    if (result == true && timerNetworkContinue == false)
                    {
                        this.AddTraceLog($"Network message stop send.");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The client is not connected."))
                    {
                        this.Invoke(new Action(delegate ()
                        {
                            this.DeviceID = String.Empty;
                            this.SetUIStatusByConnect(false);
                            this.btnNetworkSendAlways.Enabled = false;
                            this.AddTraceLog($"Disconnected");
                        }));
                    }
                }
                finally
                {
                    if (timerNetworkContinue)
                    {
                        timerNetworkAlwaysSend?.Change(dueTimeAlwaysSend, Timeout.Infinite);
                    }
                }
            }, null, 0, Timeout.Infinite);
        }

        /// <summary>
        /// NetWork Stop Send 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNetworkStopSend_Click(object sender, EventArgs e)
        {
            if (timerNetworkAlwaysSend != null)
            {
                timerNetworkContinue = false;
                this.btnNetworkSendAlways.Enabled = true;
            }
        }
        #endregion

        #region Scale Tab
        /// <summary>
        /// Scale Send Once 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_ScaleSendOnce_Click(object sender, EventArgs e)
        {
            await this.SendScaleRecurringMessage();
        }

        /// <summary>
        /// Scale Always Send
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_ScaleSendAlways_Click(object sender, EventArgs e)
        {
            timerScaleContinue = true;
            this.btnScaleSendAlways.Enabled = false;
            timerScaleAlwaysSend = new System.Threading.Timer((arg) =>
            {
                try
                {
                    var result = this.SendScaleRecurringMessage().Result;
                    if (result == true && timerScaleContinue == false)
                    {
                        this.AddTraceLog($"Scale message stop send.");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The client is not connected."))
                    {
                        this.Invoke(new Action(delegate ()
                        {
                            this.DeviceID = String.Empty;
                            this.SetUIStatusByConnect(false);
                            this.btnScaleSendAlways.Enabled = false;
                            this.AddTraceLog($"Disconnected");
                        }));
                    }
                }
                finally
                {
                    if (timerScaleContinue)
                    {
                        timerScaleAlwaysSend?.Change(dueTimeAlwaysSend, Timeout.Infinite);
                    }
                }
            }, null, 0, Timeout.Infinite);
        }

        /// <summary>
        /// Scale Stop Send Message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScaleStopSendMessage_Click(object sender, EventArgs e)
        {
            if (timerScaleAlwaysSend != null)
            {
                timerScaleContinue = false;
                this.btnScaleSendAlways.Enabled = true;
            }
        }

        #endregion

        #region Camera

        private async void btnCameraSendOnce_Click(object sender, EventArgs e)
        {
            await this.SendCameraRecurringMessage();
        }

        private async void btnCameraSendAlways_Click(object sender, EventArgs e)
        {
            timerCameraContinue = true;
            this.btnCameraSendAlways.Enabled = false;
            timerCameraAlwaysSend = new System.Threading.Timer((arg) =>
            {
                try
                {
                    var result = this.SendCameraRecurringMessage().Result;
                    if (result == true && timerCameraContinue == false)
                    {
                        this.AddTraceLog($"Camera message stop send");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The client is not connected."))
                    {
                        this.Invoke(new Action(delegate ()
                        {
                            this.DeviceID = String.Empty;
                            this.SetUIStatusByConnect(false);
                            this.btnCameraSendAlways.Enabled = false;
                            this.AddTraceLog($"Disconnected");
                        }));
                    }
                }
                finally
                {
                    if (timerCameraContinue)
                    {
                        timerCameraAlwaysSend?.Change(dueTimeAlwaysSend, Timeout.Infinite);
                    }
                }
            }, null, 0, Timeout.Infinite);
        }

        private void btnCameraStopSend_Click(object sender, EventArgs e)
        {
            if (timerCameraAlwaysSend != null)
            {
                timerCameraContinue = false;
                this.btnCameraSendAlways.Enabled = true;
            }
        }

        #endregion


        #region EID
        private async void btnEIDSendOnce_Click(object sender, EventArgs e)
        {
            await this.SendEIDRecurringMessage();
        }

        private async void btnEIDSendAlways_Click(object sender, EventArgs e)
        {
            timerEIDContinue = true;
            this.btnEIDSendAlways.Enabled = false;
            timerEIDAlwaysSend = new System.Threading.Timer((arg) =>
            {
                try
                {
                    var result = this.SendEIDRecurringMessage().Result;
                    if (result == true && timerEIDContinue == false)
                    {
                        this.AddTraceLog($"EID message stop send.");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The client is not connected."))
                    {
                        this.Invoke(new Action(delegate ()
                        {
                            this.DeviceID = String.Empty;
                            this.SetUIStatusByConnect(false);
                            this.btnEIDSendAlways.Enabled = false;
                            this.AddTraceLog($"Disconnected");
                        }));
                    }
                }
                finally
                {
                    if (timerEIDContinue)
                    {
                        timerEIDAlwaysSend?.Change(dueTimeAlwaysSend, Timeout.Infinite);
                    }
                }
            }, null, 0, Timeout.Infinite);
        }

        private void btnEIDStopSend_Click(object sender, EventArgs e)
        {
            if (timerEIDAlwaysSend != null)
            {
                timerEIDContinue = false;
                this.btnEIDSendAlways.Enabled = true;
            }
        }
        #endregion

        private void check_ScaleError_CheckedChanged(object sender, EventArgs e)
        {
            this.txtWeight.Enabled = !this.check_ScaleError.Checked;
            this.txtRate.Enabled = !this.check_ScaleError.Checked;
        }

        private void check_LFError_CheckedChanged(object sender, EventArgs e)
        {
            this.txtLFReader.Enabled = !this.check_LFError.Checked;
        }

        private void check_UHFError_CheckedChanged(object sender, EventArgs e)
        {
            this.txtUHFReader.Enabled = !this.check_UHFError.Checked;
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}
