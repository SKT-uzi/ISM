var Dashboard = function () {
    let mqtt_client = null;

    let ethernetHasConnected = null;
    let ethernetHasInternet = null;
    let wifiHasConnected = null;
    let wifiHasInternet = null;
    let cameraHasConnected = null;
    let scaleHasConnected = null;
    let uhfEIDHasConnected = null;
    var lastGetLFEIDReaderDate = null;
    var lastGetUHFEIDReaderDate = null;
    var lfEIDReaderExpiredTimer = null;
    var uhfEIDReaderExpiredTimer = null;

    $("#overview:visible").addClass("is-active");

    let CONNECTED_TEXT = null;
    let DISCONNECTED_TEXT = null;
    let NO_INTERNET_TEXT = null;
    let NO_IP_TEXT = null;
    let hasRecordViewDashboard = false;
    let visionConfigNetworkSection = JSON.parse($("#hidVisionConfigNetworkSection").val());
    let currentEthernetIPMode = visionConfigNetworkSection.Ethernet.IPAssignmentMode;
    let ethernetManualIPAddr = visionConfigNetworkSection.Ethernet.IPAddress;

    // Init Resources
    var initResources = function () {
        CONNECTED_TEXT = resources.getValue("DeviceStatus_Connected");
        DISCONNECTED_TEXT = resources.getValue("DeviceStatus_NotConnected");
        NO_INTERNET_TEXT = resources.getValue("DeviceStatus_NoInternet");
        NO_IP_TEXT = resources.getValue("Common_Label_None");
    };

    // update language beforo go to ISMOverview
    let registerEvents = function () {
        $(document).delegate("#btnISMOverView", clickEvent, function () {
            window.location.href = "/" + $("#hidISMVPath").val() + "/Setting/Overview";
        });

        // Receive the message from ChuteSideWeb
        window.addEventListener("message", function (event) {
            try {
                if (event.data == "OpenISMDashboard" && !hasRecordViewDashboard) {
                    callRoute_Normal("/Home/WriteViewDashboardLog", "POST", null, function () {
                        hasRecordViewDashboard = true;
                    }, function (errorMsg) {                        
                    });
                }
            } catch { }
        });

        // Listen for exit page events
        window.addEventListener('beforeunload', function () {
            // Clear timer
            clearInterval(lfEIDReaderExpiredTimer);
            clearInterval(uhfEIDReaderExpiredTimer);
        });
    };

    // MQTT: init MQTT Client
    var initMQTTClient = function () {
        mqtt_client = MqttClient.init();

        mqtt_client.on("message", (topic, message, packet) => {
            switch (topic) {
                case "ISMDeviceStatus":
                    handleDeviceErrorMessage(message);
                    break;
                case "ISMScaleConfig":
                    handleScaleConfigMessage(message);
                    break;
                case "ISMEIDConfig":
                    handleEIDConfigMessage(message);
                    break;
                case "ISMNetworkConfig":
                    handleNetworkResponseMessage(message);
                    break;
            }
        });
    };

    // Handle Device Error Message
    var handleDeviceErrorMessage = function (message) {
        let data = JSON.parse(message);
        if (!isNullOrEmpty(data.statusType) && data.statusType == "system") {
            if (!isNullOrEmpty(message)) {
                $("#device-uptime").text(secondsToHMS(data.value.uptime));
            }
        } else {
            switch (data.statusType) {
                case "ethernet":
                    if (ethernetHasConnected != data.isConnected
                        || ethernetHasInternet != data.internetAccessible) {
                        ethernetHasConnected = data.isConnected;
                        ethernetHasInternet = data.internetAccessible;
                        checkNetworkStatus();
                        if (!data.isConnected) {
                            showDeviceIP(data.statusType, NO_IP_TEXT);
                        } else {
                            if (currentEthernetIPMode == "auto") {
                                // If the ethernet status has changed, need to update the ethernet ip address
                                sendNetworkAutoMsg(data.statusType);
                            } else {
                                showDeviceIP(data.statusType, ethernetManualIPAddr);
                            }
                        }
                    }
                    break;
                case "wireless":
                    if (wifiHasConnected != data.isConnected
                        || wifiHasInternet != data.internetAccessible) {
                        wifiHasConnected = data.isConnected;
                        wifiHasInternet = data.internetAccessible;
                        checkNetworkStatus();
                        if (!data.isConnected) {
                            showDeviceIP(data.statusType, NO_IP_TEXT);
                        } else {
                            // If the wifi status has changed, need to update the wifi ip address
                            sendNetworkAutoMsg(data.statusType);
                        }
                    }
                    break;
                case "camera":
                    cameraHasConnected = data.isConnected;
                    checkCameraStatus();
                    break;
                case "scale":
                    scaleHasConnected = data.isConnected;
                    checkScaleStatus();
                    break;
                case "UHFEID":
                    uhfEIDHasConnected = data.isConnected;
                    checkEIDStatus();
                    break;
            }
        }
    };

    // Handle Network Response Message
    var handleNetworkResponseMessage = function (message) {
        let data = JSON.parse(message);

        // Response
        if (!isNullOrEmpty(data) && data.method == "response") {
            if (!isNullOrEmpty(data.args) && data.args == "auto") {
                if (!isNullOrEmpty(data.type)
                    && (data.type == "ethernet" || data.type == "wireless")) {
                    let ipAddress = data.value.status == "success" ? data.value.ip : NO_IP_TEXT;
                    if (data.type == "ethernet") {
                        if (ethernetHasConnected) {
                            if (currentEthernetIPMode == "auto") {
                                showDeviceIP(data.type, ipAddress);
                            } else {
                                showDeviceIP(data.type, ethernetManualIPAddr);
                            }
                        } else {
                            showDeviceIP(data.type, NO_IP_TEXT);
                        }
                    }
                    if (data.type == "wireless") {
                        if (wifiHasConnected) {
                            showDeviceIP(data.type, ipAddress);
                        } else {
                            showDeviceIP(data.type, NO_IP_TEXT);
                        }
                    }
                }
            }
        }
    }

    // Handle Scale Config Message
    var handleScaleConfigMessage = function (message) {
        let data = JSON.parse(message);
        if (data.type == "scale") {
            if (data.value.status == "success") {
                showLiveData(data.type, data.value.weight);
            }
            else {
                showLiveData(data.type, "--");
            }
        }
    }

    // Handle EID Config Message
    var handleEIDConfigMessage = function (message) {
        let data = JSON.parse(message);
        if (data.type == "LFEID") {
            lastGetLFEIDReaderDate = new Date();

            if (data.value.status == "success") {
                var lfEIDReader = data.value.reader.toString();
                if (!isNullOrEmpty(lfEIDReader)) {
                    showLiveData(data.type, lfEIDReader.substr(-4));
                } else {
                    showLiveData(data.type, "--");
                }

            }
            else {
                showLiveData(data.type, "--");
            }
        } else if (data.type == "UHFEID") {
            lastGetUHFEIDReaderDate = new Date();

            if (data.value.status == "success") {
                var uhfEIDReader = data.value.reader.toString();
                if (!isNullOrEmpty(uhfEIDReader)) {
                    showLiveData(data.type, uhfEIDReader.substr(-4));
                } else {
                    showLiveData(data.type, "--");
                }
            }
            else {
                showLiveData(data.type, "--");
            }
        }
    }

    // Check Network Status
    var checkNetworkStatus = function () {
        if (!ethernetHasConnected) {
            $("#ethernet-status").removeClass("success").removeClass("warning").addClass("fail");
            $("#ethernet-status").text(DISCONNECTED_TEXT);
        } else {
            if (ethernetHasInternet) {
                $("#ethernet-status").removeClass("fail").removeClass("warning").addClass("success");
                $("#ethernet-status").text(CONNECTED_TEXT);
            } else {
                $("#ethernet-status").removeClass("success").removeClass("fail").addClass("warning");
                $("#ethernet-status").text(NO_INTERNET_TEXT);
            }
        }
        if (!wifiHasConnected) {
            $("#wifi-status").removeClass("success").removeClass("warning").addClass("fail");
            $("#wifi-status").text(DISCONNECTED_TEXT);
            $("#ssid").text("");
        } else {
            $("#ssid").text(visionConfigNetworkSection.Wireless.SSID);
            if (wifiHasInternet) {
                $("#wifi-status").removeClass("fail").removeClass("warning").addClass("success");
                $("#wifi-status").text(CONNECTED_TEXT);
            } else {
                $("#wifi-status").removeClass("success").removeClass("fail").addClass("warning");
                $("#wifi-status").text(NO_INTERNET_TEXT);
            }
        }

        if (ethernetHasInternet || wifiHasInternet) {
            $("#network-icon").removeClass("status-icon-1").removeClass("status-icon-2 no-internet").addClass("status-icon-2 connected");
        } else if (ethernetHasConnected || wifiHasConnected) {
            $("#network-icon").removeClass("status-icon-1").removeClass("status-icon-2 connected").addClass("status-icon-2 no-internet");
        } else {
            $("#network-icon").removeClass("status-icon-2 connected").removeClass("status-icon-2 no-internet").addClass("status-icon-1");
        }
    };

    // Check Camera Status
    var checkCameraStatus = function () {
        if (!cameraHasConnected) {
            $("#camera-status").removeClass("success").addClass("fail");
            $("#camera-status").text(DISCONNECTED_TEXT);
            $("#camera-icon").removeClass("status-icon-2 connected").addClass("status-icon-1");
        } else {
            $("#camera-status").removeClass("fail").addClass("success");
            $("#camera-status").text(CONNECTED_TEXT);
            $("#camera-icon").removeClass("status-icon-1").addClass("status-icon-2 connected");
        }
    };

    // Check Scale Status
    var checkScaleStatus = function () {
        if (!scaleHasConnected) {
            $("#scale-status").removeClass("success").addClass("fail");
            $("#scale-status").text(DISCONNECTED_TEXT);
            $("#scale-icon").removeClass("status-icon-2 connected").addClass("status-icon-1");
        } else {
            $("#scale-status").removeClass("fail").addClass("success");
            $("#scale-status").text(CONNECTED_TEXT);
            $("#scale-icon").removeClass("status-icon-1").addClass("status-icon-2 connected");
        }
    };

    // Check EID Status
    var checkEIDStatus = function () {
        if (!uhfEIDHasConnected) {
            $("#UHFEID-status").removeClass("success").addClass("fail");
            $("#UHFEID-status").text(DISCONNECTED_TEXT);
        } else {
            $("#UHFEID-status").removeClass("fail").addClass("success");
            $("#UHFEID-status").text(CONNECTED_TEXT);
        }

        if (uhfEIDHasConnected) {
            $("#EID-icon").removeClass("status-icon-1").addClass("status-icon-2 connected");
        } else {
            $("#EID-icon").removeClass("status-icon-2 connected").addClass("status-icon-1");
        }
    };

    // Convert seconds to hours minutes seconds
    var secondsToHMS = function (seconds) {
        let hours = Math.floor(seconds / 3600);
        let minutes = Math.floor((seconds % 3600) / 60);
        let remainingSeconds = (seconds % 60).toFixed(2);

        return hours + 'h ' + minutes + 'm ' + remainingSeconds + 's';
    };

    // Change value after flashing
    var changeData = function (ele, val) {
        const obj = $(ele);
        const lastVal = obj.text();

        // Weight needs to be updated in real time
        if (ele == ".live-weight" && isNullOrEmpty(val)) {
            val = "--";
        }

        // Only if the current value is not empty and is not equal to the last value, update the value
        if (!isNullOrEmpty(val) && val != lastVal) {
            obj.addClass("flash").one("animationend", () => {
                obj.text(val);
                obj.removeClass("flash");
            });
        }
    };

    // Show live data
    var showLiveData = function (dataType, val) {
        switch (dataType) {
            case "scale": {
                changeData(".live-weight", val);
                break;
            }
            case "LFEID": {
                changeData(".live-lf-eid", val);
                break;
            }
            case "UHFEID": {
                changeData(".live-uhf-eid", val);
                break;
            }
            default: {
                console.error("Unknown data type: " + dataType);
                break;
            }
        }
    };

    var sendNetworkAutoMsg = function (networkType) {
        let result = {
            method: "request",
            type: networkType,
            args: "auto"
        };
        let jsonData = JSON.stringify(result);
        mqtt_client.publish("ISMNetworkConfig", jsonData, { qos: 2, retain: false });
    };

    var showDeviceIP = function (networkType, ipAddr) {
        var ipAddressElem = $("#" + networkType + "-ip");
        var currentIPAddress = ipAddressElem.text();
        if (ipAddr != currentIPAddress) {
            ipAddressElem.text(ipAddr);
        }
    };

    // Clear the Live EID reader
    var clearLiveEIDReader = function () {
        lfEIDReaderExpiredTimer = window.setInterval(function () {
            if (lastGetLFEIDReaderDate != null && getDateSecondsDiff(lastGetLFEIDReaderDate) > eidReaderExpiredDuration) {
                showLiveData("LFEID", "--");
                lastGetLFEIDReaderDate = null;
            }
        }, 1000);

        uhfEIDReaderExpiredTimer = window.setInterval(function () {
            if (lastGetUHFEIDReaderDate != null && getDateSecondsDiff(lastGetUHFEIDReaderDate) > eidReaderExpiredDuration) {
                showLiveData("UHFEID", "--");
                lastGetUHFEIDReaderDate = null;
            }
        }, 1000);
    };

    return {
        init: function () {
            initMQTTClient();
            initResources();            
            registerEvents();
            clearLiveEIDReader();
        }
    }
}();