var MqttClient = function () {
    const clientId = "mqttjs_ism_" + Math.random().toString(16).substr(2, 8);
    const scheme = location.protocol == "https:" ? "wss" : "ws";
    var mqttServerDirName = $("#hidISMVPath").val();
    const host = scheme + "://" + location.host + "/" + mqttServerDirName + "/mqtt";//ism
    const options = {
        keepalive: 30,
        clientId: clientId,
        protocolId: "MQTT",
        protocolVersion: 4,
        clean: false,
        reconnectPeriod: 1000,
        connectTimeout: 30 * 1000,
        will: {
            topic: "WillMsg",
            payload: "Connection Closed abnormally..!",
            qos: 0,
            retain: false
        },
        rejectUnauthorized: false
    };

    const client = mqtt.connect(host, options);

    client.on("connect", () => {
        printMessage("MQTT - Client connected. ClientID: [" + clientId + "]");
        // Subscribe
        client.subscribe("ISMDeviceStatus", { qos: 2 });
        client.subscribe("ISMNetworkConfig", { qos: 2 });
        client.subscribe("ISMCameraConfig", { qos: 2 });
        client.subscribe("ISMScaleConfig", { qos: 2 });
        client.subscribe("ISMEIDConfig", { qos: 2 });
    });
    client.on("error", (err) => {
        printMessage("MQTT - Connection error. Detail: [" + err + "]");
        client.end();
    });
    client.on("reconnect", () => {
        printMessage("MQTT - Client reconnecting...");
    });
    client.on("close", () => {
        printMessage("MQTT - Client disconnected. ClientID: [" + clientId + "]");
    });

    //Received
    client.on("message", (topic, message, packet) => {
        var logMessage = "";

        switch (topic) {
            case "ISMDeviceStatus":
                logMessage = "MQTT - Received message successfully. Topic: " + topic + ", \nMessage: " + message.toString();
                break;
            case "ISMNetworkConfig":
            case "ISMCameraConfig":
            case "ISMScaleConfig":
            case "ISMEIDConfig":
                var data = JSON.parse(message);
                if (!isNullOrEmpty(data)) {
                    if (!isNullOrEmpty(data.method)) {
                        if (data.method == "request") {
                            logMessage = "MQTTADHOCLOG:MQTT - Send message successfully. Topic: " + topic + ", \nMessage: " + message.toString();
                        }
                        else if(data.method == "response") {
                            logMessage = "MQTTADHOCLOG:MQTT - Received message successfully. Topic: " + topic + ", \nMessage: " + message.toString();
                        }
                    } else {
                        logMessage = "MQTT - Received message successfully. Topic: " + topic + ", \nMessage: " + message.toString();
                    }
                }
                break;
            default:
                logMessage = "MQTT - Received message successfully. Topic: " + topic + ", \nMessage: " + message.toString();
                break;
        }

        printMessage(logMessage);
    });   

    var printMessage = function (msg) {
        writeActionLog(msg);
    };

    return {
        init: function () {
            return client;
        }
    }
}();