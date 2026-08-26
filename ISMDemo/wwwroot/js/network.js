var Network = function () {
    let mqtt_client = null;

    let currentPage = null;
    let currentProtocol = null;
    let wifiType = null;
    let hasIPValue = false;
    let needHandle = true;
    let wifiWPAMinLength = 8;
    let wifiOtherMinLength = 5;
    let defaultWirelessQuality = 1;

    let visionConfigNetworkSection = JSON.parse($("#hidVisionConfigNetworkSection").val());
    let isConfigInit = !$("#headerInit").hasClass("hide");
    if (isConfigInit) {
        $("#step-network:visible").addClass("is-active");
    }
    else {
        $("#network:visible").addClass("is-active");
    }

    let CONNECTED_TEXT = null;
    let DISCONNECTED_TEXT = null;
    let NO_INTERNET_TEXT = null;
    let CONNECTED_NOT_DONE_TEXT = null;
    let ESTABLISH_CONNECTIVITY_TEXT = null;
    let NO_ETHERNET_TEXT = null;
    let NO_WIFI_TEXT = null;
    let SETUP_WIFI_WARNING_MSG_TEXT = null;

    const dom_networkStatus = $(".network-status");
    const dom_ethernetSetings = $(".ethernet-settings");
    const dom_wifiSettings = $(".wifi-settings");
    const dom_wifiNetworkSettings = $(".wifi-network-settings");
    const dom_loadingWrap = $("main");
    const dom_headerCurrentStep = $("#step-network");

    const dom_ipAddressInput = $("input[name='ipAddress']");
    const dom_subnetMaskInput = $("input[name='subnetMask']");
    const dom_gatewayInput = $("input[name='gateway']");

    const dom_ipAddressWifiInput = $("input[name='ipAddress_wifi']");
    const dom_subnetMaskWifiInput = $("input[name='subnetMask_wifi']");
    const dom_gatewayWifiInput = $("input[name='gateway_wifi']");

    const dom_wifiSSIDInput = $("#wifi_ssid");
    const dom_wifiKeyInput = $("#wifi_key");

    var connectStatus = {
        NotConnected: 0,
        NoInternet: 1,
        Connected: 2
    };

    var ethernetStatus = 0;
    var lastEthernetStatus = 0;
    var wifiStatus = 0;
    var lastWifiStatus = 0;

    var registerEvents = function () {
        $("#frmNetwork").validate({
            ignore: ".ignore",
            onkeyup: false,
            debug: true,
            rules: {
                ipAddress: { required: true, IP4Checker: true },
                subnetMask: { required: true, subnetMaskChecker: true },
                gateway: { required: true, IP4Checker: true }
            },
            focusCleanup: false,
            success: function (element) {
                $(element).closest(".form-group").removeClass("has-error");
                $(element).closest(".form-group").removeAttr("data-error");
            },
            errorPlacement: function (error, element) {
                $(element).closest(".form-group").addClass("has-error");
                $(element).closest(".form-group").attr("data-error", error.html());
            },
            highlight: function (element) {
            },
            unhighlight: function (element) {
                $(element).closest(".form-group").removeClass("has-error");
                $(element).closest(".form-group").removeAttr("data-error");
            }
        });

        $("#frmAddNetwork").validate({
            ignore: ".ignore",
            onkeyup: false,
            debug: true,
            rules: {
                wifi_key: { minlength: wifiOtherMinLength }
            },
            updater: function () {
                var selectedWifiTypeId = $("input[name='security-type']:checked").attr("id");
                var wifiTypeLastChar = selectedWifiTypeId.charAt(selectedWifiTypeId.length - 1);                
                if (wifiTypeLastChar == 2 || wifiTypeLastChar == 3) { // WPA2 Person/WPA2 Enterprise
                    $('#frmAddNetwork').validate().settings.rules.wifi_key.minlength = wifiWPAMinLength;
                } else {
                    $('#frmAddNetwork').validate().settings.rules.wifi_key.minlength = wifiOtherMinLength;
                }
            },
            focusCleanup: false,
            success: function (element) {
                $(element).closest(".form-group").removeClass("has-error");
                $(element).closest(".form-group").removeAttr("data-error");
            },
            errorPlacement: function (error, element) {
                $(element).closest(".form-group").addClass("has-error");
                $(element).closest(".form-group").attr("data-error", error.html());
            },
            highlight: function (element) {
            },
            unhighlight: function (element) {
                $(element).closest(".form-group").removeClass("has-error");
                $(element).closest(".form-group").removeAttr("data-error");
            }
        });

        // Set Ethernet
        $(document).delegate("[data-toggle='settings-ethernet']", clickEvent, function () {
            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            currentPage = "ethernetNetwork";
            showSubPage(currentLevel, targetID);

            // get protocol form config file
            currentProtocol = isNullOrEmpty(visionConfigNetworkSection.Ethernet.IPAssignmentMode) ? "auto" : visionConfigNetworkSection.Ethernet.IPAssignmentMode;

            if (currentProtocol == "auto") {
                initInputs();
                $("#ethernet-connection-type-1").prop("checked", "checked");
                $("#ethernet-connection-type-2").removeProp("checked");
            } else {
                $("#ethernet-connection-type-2").prop("checked", "checked");
                $("#ethernet-connection-type-1").removeProp("checked");
            }
            hasIPValue = false;
            checkEthernetStatus();
        });

        // Change Etherenet IP Assignment
        $(document).on("change", "[name='ethernet-connection-type']", function () {
            let obj = $(this).parent().find("#ethernet-connection-type-2");
            let objParent = $(this).parents(".main-box-body");
            let objInput = objParent.find(".form-control");
            let objAction = objParent.find(".form-action");
            let objConnection = objParent.find(".connection-details");
            let objStatus = dom_ethernetSetings.find(".box-status");
            hasIPValue = false;
            if (obj.is(":checked")) {
                // IP-Manual
                currentProtocol = "manual";
                objInput.removeClass("form-disabled");
                objAction.removeClass("hide");
                objConnection.removeClass("loading");
                objStatus.removeClass("loading1");
            } else {
                // IP-DHCP
                dom_ipAddressInput.val("");
                dom_subnetMaskInput.val("");
                dom_gatewayInput.val("");
                currentProtocol = "auto";
                objInput.addClass("form-disabled");
                objAction.addClass("hide");
                objConnection.addClass("loading");
                dom_ipAddressInput.closest(".form-group").removeClass("has-error");
                dom_ipAddressInput.closest(".form-group").removeAttr("data-error");
                dom_subnetMaskInput.closest(".form-group").removeClass("has-error");
                dom_subnetMaskInput.closest(".form-group").removeAttr("data-error");
                dom_gatewayInput.closest(".form-group").removeClass("has-error");
                dom_gatewayInput.closest(".form-group").removeAttr("data-error");
            }
            checkEthernetStatus();
        });

        // Set Wi-Fi
        $(document).delegate("[data-toggle='settings-wifi']", clickEvent, function () {
            if (ethernetStatus == connectStatus.NotConnected) {
                let targetID = $(this).data("target");
                let obj = $(this).closest("[data-page-level]");
                let currentLevel = obj.data("page-level");
                currentPage = "wifi";
                showSubPage(currentLevel, targetID);
                initConnectedWifi();
                checkWifiStatus();
            } else { // Need to unplug ethernet cable first to setup WiFi network              
                $(".modal-message-warning .modal-message-content").text(SETUP_WIFI_WARNING_MSG_TEXT);
                $(".modal-message-warning").addClass("open");
            }
        });

        // Add New Wi-Fi Network
        $(document).delegate(".list-networks .add-network", clickEvent, function () {
            // Init wifi security key
            if ($(".form-security-key .icon-eye-off").length > 0) {
                $(".form-security-key .icon").removeClass("icon-eye-off").addClass("icon-eye-on");
                $("#wifi_key").attr("type", "password");
            }
            dom_wifiSSIDInput.val("");
            $("input[name='security-type']").prop("checked", false);
            dom_wifiKeyInput.val("");
            $(".form-security-key").addClass("hide");
            $("#frmAddNetwork").validate().resetForm();
            newModalAction.openModal($(".modal-add-network"));
        });

        // Toggle Wi-Fi Network Security Type
        $(document).on("change", "[name='security-type']", function () {
            let obj = $(this).parent().find("#security-type-1");
            let objParent = $(this).parents(".list-parameters");
            let objkeyInput = objParent.find(".form-security-key");
            let ssid = dom_wifiSSIDInput.val().trim();
            let key = dom_wifiKeyInput.val().trim();
            let objBtnSave = $("[data-toggle='save-network']");
            wifiType = $(this).attr("data-security-type");

            if (obj.is(":checked")) {
                // Open Type, No Security Key
                objkeyInput.addClass("hide");
                dom_wifiKeyInput.addClass("ignore");
                if (ssid) {
                    objBtnSave.removeClass("btn-disabled");
                } else {
                    objBtnSave.addClass("btn-disabled");
                }
            } else {
                // Need Security Key
                objkeyInput.removeClass("hide");
                dom_wifiKeyInput.removeClass("ignore");
                if (ssid && key) {
                    objBtnSave.removeClass("btn-disabled");
                } else {
                    objBtnSave.addClass("btn-disabled");
                }
            }

            $("#frmAddNetwork").validate().resetForm();

            // The updater function is triggered to update the validation rule
            $('#frmAddNetwork').validate().settings.updater();
        });

        // Enable Save Button -- Add Wifi
        $(document).on("input", ".modal-add-network .form-control", function () {
            let ssid = dom_wifiSSIDInput.val().trim();
            let key = dom_wifiKeyInput.val().trim();
            let objBtnSave = $("[data-toggle='save-network']");
            if (ssid && (key || $("#security-type-1").is(":checked"))) {
                objBtnSave.removeClass("btn-disabled");
            } else {
                objBtnSave.addClass("btn-disabled");
            }
        });

        // Add and Connect Wifi
        $(document).delegate("[data-toggle='save-network']", clickEvent, function () {
            let objBtnSave = $(".modal-add-network .modal-header .btn-primary");
            if (!objBtnSave.hasClass("btn-disabled") && $("#frmAddNetwork").valid()) {
                let objWifiWrap = dom_wifiSettings.find(".main-box-body"); 
                let ssid = dom_wifiSSIDInput.val().trim();
                let key = wifiType != resources.getValue("Network_Label_SecurityType_1_Data") ? dom_wifiKeyInput.val().trim() : "";
                var isSecured = key != "" ? true : false;

                visionConfigNetworkSection.Wireless = {
                    SSID: ssid,
                    SecurityType: wifiType,
                    SecurityKey: key,
                    Secured: isSecured
                };
                initNetworkEvent(visionConfigNetworkSection, function () {
                    console.log("update success");
                }, function (isCustomError, errorMsg) {
                    // Error code
                    console.log("error:" + errorMsg);
                });
                refreshWifiStatus();
                initConnectedWifi();
                sendWifiConnectRequest();
                newModalAction.closeModal($(this));
                
                objWifiWrap.addClass("loading");
            }
        });

        // Prevent Input Lose Cursor
        $(document).on("mousedown", ".form-password .btn-toggle-password", function (event) {
            event.preventDefault();
        });

        // Toggle Password Type
        $(document).on(clickEvent, ".form-password .btn-toggle-password", function () {
            const objInput = $(this).parent().find(".form-control");
            const objIcon = $(this).find(".icon");

            objInput.attr("type", objInput.attr("type") === "password" ? "text" : "password");
            objIcon.toggleClass("icon-eye-on  icon-eye-off");

            const caretPosition = objInput[0].selectionStart;
            setTimeout(() => {
                objInput[0].setSelectionRange(caretPosition, caretPosition);
            }, 10);
        });

        // Set Wi-Fi Network
        $(document).delegate("[data-toggle='settings-wifi-network']", clickEvent, function () {
            if (wifiStatus != connectStatus.NotConnected) {
                let targetID = $(this).data("target");
                let obj = $(this).closest("[data-page-level]");
                let currentLevel = obj.data("page-level");
                currentPage = "wifiNetwork";
                showSubPage(currentLevel, targetID);
                initInputs();
                hasIPValue = false;
                checkWifiNetworkStatus();
            } else {
                // If the connect status is not connected, it will allow the client to edit the network
                var wirelessConfig = visionConfigNetworkSection.Wireless;
                dom_wifiSSIDInput.val(wirelessConfig.SSID);
                $("#frmAddNetwork label:contains('" + wirelessConfig.SecurityType + "')").each(function () {
                    var elemId = $(this).attr("for");
                    $("#" + elemId).prop("checked", true);
                    $("#" + elemId).trigger("change");
                });
                if (!isNullOrEmpty(wirelessConfig.SecurityKey)) {
                    dom_wifiKeyInput.val(wirelessConfig.SecurityKey);
                }
                // Init wifi security key
                if ($(".form-security-key .icon-eye-off").length > 0) {
                    $(".form-security-key .icon").removeClass("icon-eye-off").addClass("icon-eye-on");
                    $("#wifi_key").attr("type", "password");
                }

                $("#frmAddNetwork").validate().resetForm();
                newModalAction.openModal($(".modal-add-network"));
            }
        });

        // Edit
        $(document).on("input", ".connection-details .form-control", function () {
            let objAction = $(this).parents(".connection-details").find(".form-action");
            objAction.removeClass("hide");
        });

        // Cancel
        $(document).delegate("[data-toggle='cancel-value']", clickEvent, function () {
            let objParent = $(this).parents(".connection-details");
            let objAction = objParent.find(".form-action");
            objParent.addClass("loading");
            objAction.addClass("hide");
            initEthernetManualInputElems(); 

            window.setTimeout(function () {
                objParent.removeClass("loading");
                objAction.removeClass("hide");
            }, 100);
        });

        // Save
        $(document).delegate("[data-toggle='save-value']", clickEvent, function () {
            let isPassed = true;
            let objParent = $(this).parents(".connection-details");
            let objAction = objParent.find(".form-action");

            objParent.addClass("loading");

            if ($(this).closest(".ethernet-settings").length) {
                let ip = dom_ipAddressInput.val();
                let subnetmask = dom_subnetMaskInput.val();
                let gateway = dom_gatewayInput.val();
                let allInputIsEmpty = isNullOrEmpty(ip) && isNullOrEmpty(subnetmask) && isNullOrEmpty(gateway);

                // If all input is empty, allow save.
                if (!allInputIsEmpty) {
                    isPassed = validateInputs(0);
                    if (!isPassed) {
                        objParent.removeClass("loading");
                        return;
                    }

                    refreshEthernetStatus();
                }
                objAction.addClass("hide");

                visionConfigNetworkSection.Ethernet = {
                    IPAssignmentMode: "manual",
                    IPAddress: ip,
                    SubnetMask: subnetmask,
                    Gateway: gateway
                };
                initNetworkEvent(visionConfigNetworkSection, function () {
                    console.log("update success");
                }, function (isCustomError, errorMsg) {
                    // Error code
                    console.log("error:" + errorMsg);
                });

                if (!allInputIsEmpty) {
                    changeToManual();
                } else {
                    window.setTimeout(function () {
                        objParent.removeClass("loading");
                        objAction.removeClass("hide");
                    }, 100);
                }
            }
        });

        // Back
        $(document).delegate(".btn-back", clickEvent, function () {
            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            if (currentLevel == 2) {
                currentPage = "";
            }
            if (currentLevel == 3) {
                currentPage = "wifi";
                initConnectedWifi();
            }
            showParentPage(currentLevel, targetID);
        });       
    };

    // Init MQTT Client
    var initMQTTClient = function () {
        mqtt_client = MqttClient.init();

        mqtt_client.on("message", (topic, message, packet) => {
            switch (topic) {
                case "ISMDeviceStatus":
                    handleNetworkStatusMessage(message);
                    break;
                case "ISMNetworkConfig":
                    handleNetworkResponseMessage(message);
                    break;
            }
        });
    };

    // MQTT: handle Nework status message
    var handleNetworkStatusMessage = function (message) {
        if (needHandle) {
            let data = JSON.parse(message);
            if (!isNullOrEmpty(data.statusType) && data.statusType == "ethernet") {
                // 0: NotConnected; 1: NoInternet; 2: Connected
                if (data.isConnected) {
                    if (data.internetAccessible) {
                        ethernetStatus = connectStatus.Connected;
                    } else {
                        ethernetStatus = connectStatus.NoInternet;
                    }
                } else {
                    ethernetStatus = connectStatus.NotConnected;
                }
                console.log("ethernetStatus:" + ethernetStatus);
                checkNetworkStatus();
                if (currentPage == "ethernetNetwork" && lastEthernetStatus != ethernetStatus) {
                    checkEthernetStatus();
                }
                if (lastEthernetStatus != ethernetStatus) {
                    // Add MQTT ISMDeviceStatus message log
                    var logMessage = "Network " + (isConfigInit ? "Init" : "Setting") + " MQTT - Received message successfully. Topic: ISMDeviceStatus, \nMessage: " + message.toString();
                    writeEthernetStatusLog(logMessage);
                }

                lastEthernetStatus = ethernetStatus;
            }

            if (!isNullOrEmpty(data.statusType) && data.statusType == "wireless") {
                if (data.isConnected) {
                    if (data.internetAccessible) {
                        wifiStatus = connectStatus.Connected;
                    } else {
                        wifiStatus = connectStatus.NoInternet;
                    }
                } else {
                    wifiStatus = connectStatus.NotConnected;
                }
                console.log("wifiStatus:" + wifiStatus);
                checkNetworkStatus();
                if (currentPage == "wifi") {
                    initConnectedWifi();
                }
                if (currentPage == "wifiNetwork") {
                    checkWifiNetworkStatus();
                }

                if (lastWifiStatus != wifiStatus) {
                    // Add MQTT ISMDeviceStatus message log
                    var logMessage = "Network " + (isConfigInit ? "Init" : "Setting") + " MQTT - Received message successfully. Topic: ISMDeviceStatus, \nMessage: " + message.toString();
                    writeWifiStatusLog(logMessage);
                }

                lastWifiStatus = wifiStatus;
            }
        }
    };

    // MQTT: handle Nework Response message
    var handleNetworkResponseMessage = function (message) {
        let data = JSON.parse(message);
       
        // Response
        if (!isNullOrEmpty(data) && data.method == "response") {
            if (!isNullOrEmpty(data.args) && data.args == "auto") {
                if (!isNullOrEmpty(data.type) && data.type == "ethernet") {
                    if (!$("#ethernet-connection-type-1").is(":checked")) {
                        return;
                    }
                    let hasInternet = ethernetStatus == connectStatus.Connected ? true : false;
                    let objEthernetWrap = dom_ethernetSetings.find(".main-box-body");
                    let objConnection = objEthernetWrap.find(".connection-details");
                    objConnection.removeClass("loading");

                    let objInput = objEthernetWrap.find(".form-control");
                    let objAction = objEthernetWrap.find(".form-action");

                    objInput.addClass("form-disabled");
                    objAction.addClass("hide");

                    let objChildren = objEthernetWrap.children();
                    let objTips = dom_ethernetSetings.find(".main-tips");
                    let objStatus = dom_ethernetSetings.find(".box-status");
                    objChildren.removeClass("hide");
                    objTips.addClass("hide");
                    objStatus.text(hasInternet ? CONNECTED_TEXT : NO_INTERNET_TEXT);

                    if (data.value.status == "success") {
                        hideErrorMsgModal();
                        visionConfigNetworkSection.Ethernet.IPAssignmentMode = "auto";
                        dom_ipAddressInput.val(data.value.ip);
                        dom_subnetMaskInput.val(data.value.subnetmask);
                        dom_gatewayInput.val(data.value.gateway);
                        hasIPValue = true;
                        initNetworkEvent(visionConfigNetworkSection, function () {
                            console.log("update success");
                        }, function (isCustomError, errorMsg) {
                            // Error code
                            console.log("error:" + errorMsg);
                        });
                    } else if (data.value.status == "error") {
                        dom_ipAddressInput.val("");
                        dom_subnetMaskInput.val("");
                        dom_gatewayInput.val("");

                        showErrorMsgModal("ethernetNetwork", data.value.errorMessage);                        
                    }

                    objStatus.removeClass("loading1");
                }
                else if (!isNullOrEmpty(data.type) && data.type == "wireless") {
                    let hasInternet = wifiStatus == connectStatus.Connected ? true : false;
                    let objWifiWrap = dom_wifiNetworkSettings.find(".main-box-body");                     
                    objWifiWrap.removeClass("loading");

                    let objChildren = objWifiWrap.children();
                    let objStatus = dom_wifiNetworkSettings.find(".box-status");

                    objChildren.removeClass("hide");                    
                    objStatus.text(hasInternet ? CONNECTED_TEXT : NO_INTERNET_TEXT);

                    if (data.value.status == "success") {
                        hideErrorMsgModal();
                        dom_ipAddressWifiInput.val(data.value.ip);
                        dom_subnetMaskWifiInput.val(data.value.subnetmask);
                        dom_gatewayWifiInput.val(data.value.gateway);
                        hasIPValue = true;
                    } else if (data.value.status == "error") {
                        initInputs();
                        showErrorMsgModal("wifiNetwork", data.value.errorMessage); 
                    }
                    objStatus.removeClass("loading1");
                }
            }
            else if (!isNullOrEmpty(data.args) && data.args == "manual") {
                if (!isNullOrEmpty(data.type) && data.type == "ethernet") {
                    let objStatus = dom_ethernetSetings.find(".box-status");
                    objStatus.removeClass("loading1");
                    let objEthernetWrap = dom_ethernetSetings.find(".main-box-body");
                    let objConnection = objEthernetWrap.find(".connection-details");
                    objConnection.removeClass("loading");

                    if (data.value.status == "success") {
                        hideErrorMsgModal();
                        hasIPValue = false;   
                        checkEthernetStatus();
                    } else if (data.value.status == "error") {
                        showErrorMsgModal("ethernetNetwork", data.value.errorMessage); 
                        objConnection.find(".form-action").removeClass("hide");
                    }
                }
            }
            else if (!isNullOrEmpty(data.args) && data.args == "connect") {
                if (!isNullOrEmpty(data.type) && data.type == "wireless") {
                    let data = JSON.parse(message);
                    let objWifiWrap = dom_wifiSettings.find(".main-box-body");
                    let objStatus = dom_wifiSettings.find(".box-status");
                    objWifiWrap.removeClass("loading");
                    objStatus.removeClass("loading1");
                    hasIPValue = false;
 
                    if (data.value.status == "error") {
                        showErrorMsgModal("wifi", data.value.errorMessage);
                    } else {
                        hideErrorMsgModal();
                    }
                }
            }
            needHandle = true;
        }
    }

    // MQTT: change IPAssignmentMode to Manual
    var changeToManual = function () {
        needHandle = false;
        let result = {
            method: "request",
            type: "ethernet",
            args: "manual"
        };
        let jsonData = JSON.stringify(result);
        mqtt_client.publish("ISMNetworkConfig", jsonData, { qos: 2, retain: false });
    }

    // MQTT: send connect wifi Request
    var sendWifiConnectRequest = function () {
        let result = {
            method: "request",
            type: "wireless",
            args: "connect"
        };
        let jsonData = JSON.stringify(result);       
        mqtt_client.publish('ISMNetworkConfig', jsonData, { qos: 2, retain: false });
    }

    //Check Network
    var checkNetworkStatus = function () {
        var wifiSSID = visionConfigNetworkSection.Wireless.SSID;
        let statusDescriptionText;
        let ethernetHasInternet = ethernetStatus == connectStatus.Connected ? true : false;
        let wifiHasInternet = wifiStatus == connectStatus.Connected ? true : false;
        let statusIcon = dom_networkStatus.find(".status-icon");
        let statusTitle = dom_networkStatus.find(".status-title");
        let statusDescription = dom_networkStatus.find(".status-description");
        let ethernetStatusText = dom_networkStatus.find(".ethernet-status");
        let wifiStatusText = dom_networkStatus.find(".wifi-status");

        if (ethernetHasInternet || wifiHasInternet) {
            statusIcon.removeClass("no-internet").addClass("connected");
            dom_headerCurrentStep.addClass("is-done");
        } else if (ethernetStatus == connectStatus.NoInternet
            || wifiStatus == connectStatus.NoInternet) {            
            statusIcon.removeClass("connected").addClass("no-internet");
            dom_headerCurrentStep.removeClass("is-done");
        } else {
            statusIcon.removeClass("connected").removeClass("no-internet");
            dom_headerCurrentStep.removeClass("is-done");
        }
        var statusTitleText = DISCONNECTED_TEXT;
        if (ethernetHasInternet || wifiHasInternet) {
            statusTitleText = CONNECTED_TEXT;
        } else if (ethernetStatus == connectStatus.NoInternet || wifiStatus == connectStatus.NoInternet) {
            statusTitleText = NO_INTERNET_TEXT;
        }
        statusTitle.text(statusTitleText);

        if ((ethernetHasInternet && wifiHasInternet)
            || (ethernetHasInternet && wifiStatus == connectStatus.NoInternet)
            || (wifiHasInternet && ethernetStatus == connectStatus.NoInternet)
        ) {
            statusDescriptionText = "";
        } else if (ethernetHasInternet && wifiStatus == connectStatus.NotConnected) {
            // If the client hasn't set up the wifi already, display the following tips
            if (isNullOrEmpty(wifiSSID)) {
                statusDescriptionText = NO_WIFI_TEXT;
            } else {
                statusDescriptionText = "";
            }
        } else if (wifiHasInternet && ethernetStatus == connectStatus.NotConnected) {
            // Wireless is connected and ethernet is not connected
            statusDescriptionText = NO_ETHERNET_TEXT;
        } else if (ethernetStatus == connectStatus.NotConnected && wifiStatus == connectStatus.NotConnected) {
            // Ethernet and wireless are all not connected
            statusDescriptionText = ESTABLISH_CONNECTIVITY_TEXT;
        } else {
            // At least one of Ethernet and wireless is no internet, and the other is not connected or they are no internet.
            statusDescriptionText = CONNECTED_NOT_DONE_TEXT;
        }
        statusDescription.text(statusDescriptionText);
        ethernetStatusText.text(getNetworkStatusText(ethernetStatus));
        wifiStatusText.text(getNetworkStatusText(wifiStatus));
        dom_loadingWrap.removeClass("loading2");
    }

    // Refresh Ethernet
    var refreshEthernetStatus = function () {
        let objStatus = dom_ethernetSetings.find(".box-status");
        let objConnection = dom_ethernetSetings.find(".connection-details");

        objStatus.addClass("loading1");
        objConnection.addClass("loading");
    }

    //Check Ethernet Status
    var checkEthernetStatus = function () {
        let hasConnected = ethernetStatus > 0 ? true : false;
        let hasInternet = ethernetStatus == connectStatus.Connected ? true : false;
        let objEthernetWrap = dom_ethernetSetings.find(".main-box-body");
        let objChildren = objEthernetWrap.children();
        let objTips = dom_ethernetSetings.find(".main-tips");
        let objStatus = dom_ethernetSetings.find(".box-status");
        let objInput = objEthernetWrap.find(".form-control");
        let objAction = objEthernetWrap.find(".form-action");

        if (hasConnected) {
            if (hasIPValue) {
                objStatus.text(hasInternet ? CONNECTED_TEXT : NO_INTERNET_TEXT);
            } else {
                if (currentProtocol == "auto") {
                    refreshEthernetStatus();
                    objChildren.removeClass("hide");
                    objTips.addClass("hide");
                    objStatus.text(hasInternet ? CONNECTED_TEXT : NO_INTERNET_TEXT);
                    let result = {
                        method: "request",
                        type: "ethernet",
                        args: currentProtocol
                    };
                    let jsonData = JSON.stringify(result);
                    needHandle = false;
                    mqtt_client.publish("ISMNetworkConfig", jsonData, { qos: 2, retain: false });
                } else if (currentProtocol == "manual") {
                    objInput.removeClass("form-disabled");
                    objChildren.removeClass("hide");
                    objTips.addClass("hide");
                    objAction.removeClass("hide");
                    objStatus.text(hasInternet ? CONNECTED_TEXT : NO_INTERNET_TEXT);

                    initEthernetManualInputElems();
                    hasIPValue = true;
                }
            }
        } else {
            objChildren.addClass("hide");
            objTips.removeClass("hide");
            objStatus.text(DISCONNECTED_TEXT);
            objStatus.removeClass("loading1");
            objEthernetWrap.removeClass("loading2");
            hasIPValue = false;
        }
    }

    // Init Inputs Empty
    var initInputs = function () {
        if (currentPage == "ethernetNetwork") {
            let objParent = $("[name='ethernet-connection-type']").parents(".main-box-body");
            let objAction = objParent.find(".form-action");
            objAction.addClass("hide");
            dom_ipAddressInput.val("");
            dom_subnetMaskInput.val("");
            dom_gatewayInput.val("");
            dom_ipAddressInput.closest(".form-group").removeClass("has-error").removeAttr("data-error");
            dom_subnetMaskInput.closest(".form-group").removeClass("has-error").removeAttr("data-error");
            dom_gatewayInput.closest(".form-group").removeClass("has-error").removeAttr("data-error");
        }
        if (currentPage == "wifiNetwork") {
            dom_ipAddressWifiInput.val("");
            dom_subnetMaskWifiInput.val("");
            dom_gatewayWifiInput.val("");
            dom_ipAddressWifiInput.closest(".form-group").removeClass("has-error").removeAttr("data-error");
            dom_subnetMaskWifiInput.closest(".form-group").removeClass("has-error").removeAttr("data-error");
            dom_gatewayWifiInput.closest(".form-group").removeClass("has-error").removeAttr("data-error");
        }
    }

    // Init connected wifi
    var initConnectedWifi = function () {
        var wifiSSID = visionConfigNetworkSection.Wireless.SSID;
        if (!isNullOrEmpty(wifiSSID)) {
            let objMyNetworks = dom_wifiSettings.find(".my-networks");
            let objStatus = objMyNetworks.find(".box-status");

            $("#myWifi").empty();
            let qualityClass = getQualityClass(defaultWirelessQuality);
            let securedClass = visionConfigNetworkSection.Wireless.Secured ? "private" : "";
            let str = '<icon class="icon icon-sm icon-wifi-signal '
            str += qualityClass + ' ' + securedClass + '"></icon>';
            str += '<div class="box-item">' + wifiSSID + '</div>'
            $('#myWifi').append(str);
            $('#wifiSSID').html(wifiSSID);
            $('#myWifiDetail').empty();
            $('#myWifiDetail').append(str);
            objMyNetworks.removeClass("hide");
            objMyNetworks.attr("data-enable", true);
            var statusText = getNetworkStatusText(wifiStatus);
            objStatus.text(statusText);

            // If has finished the configuration of wifi, clear the status description
            let statusDescription = dom_networkStatus.find(".status-description");
            if (statusDescription.text() == NO_WIFI_TEXT) {
                statusDescription.text("");
            }
        } else {
            $("#myWifi").empty();
            $('#myWifiDetail').empty();
            let objMyNetworks = dom_wifiSettings.find(".my-networks");
            let objStatus = objMyNetworks.find(".box-status");
            objMyNetworks.addClass("hide");
            objMyNetworks.attr("data-enable", false);
            objStatus.text(DISCONNECTED_TEXT);
        }
    }

    // Refresh Wi-Fi
    var refreshWifiStatus = function () {
        let objMyNetworks = dom_wifiSettings.find(".my-networks");
        let objStatus = objMyNetworks.find(".box-status");

        if (objMyNetworks.is(":visible")) {
            objStatus.addClass("loading1");
        }
    }

    // Check Wi-Fi
    var checkWifiStatus = function () {
        let objWifiWrap = dom_wifiSettings.find(".main-box-body");
        let objMyNetworks = dom_wifiSettings.find(".my-networks");
        let objStatus = objMyNetworks.find(".box-status");

        objStatus.removeClass("loading1");
        objWifiWrap.removeClass("loading2");
    }

    // Refresh Wi-Fi Network
    var refreshWifiNetworkStatus = function () {
        let objWifiWrap = dom_wifiNetworkSettings.find(".main-box-body");
        let objStatus = dom_wifiNetworkSettings.find(".box-status");        

        objStatus.addClass("loading1");
        objWifiWrap.addClass("loading");
    }
    // Check Wi-Fi Network
    var checkWifiNetworkStatus = function () {
        let hasConnected = wifiStatus != connectStatus.NotConnected ? true : false;
        let hasInternet = wifiStatus == connectStatus.Connected ? true : false;        
        let objStatus = dom_wifiNetworkSettings.find(".box-status");

        if (hasConnected) {
            if (hasIPValue) {
                objStatus.text(hasInternet ? CONNECTED_TEXT : NO_INTERNET_TEXT);
            } else {
                currentProtocol = "auto";
                refreshWifiNetworkStatus();
                objStatus.text(hasInternet ? CONNECTED_TEXT : NO_INTERNET_TEXT);
                let result = {
                    method: "request",
                    type: "wireless",
                    args: currentProtocol
                };
                let jsonData = JSON.stringify(result);
                needHandle = false;
                mqtt_client.publish("ISMNetworkConfig", jsonData, { qos: 2, retain: false });                
            }
        } else {
            objStatus.text(DISCONNECTED_TEXT);
            objStatus.removeClass("loading1");
            hasIPValue = false;
        }
    }

    // validate Inputs
    var validateInputs = function (type) {
        let isPassed = true;
        let inputSelector = "input[type='text']";
        let validator = null;
        let controlsToValidate = null;
        if (type == 0) {
            validator = $("#frmNetwork").validate();
            controlsToValidate = $("#frmNetwork").children().find(inputSelector);
        }

        if (controlsToValidate.length > 0) {
            $.each(controlsToValidate, function (i, e) {
                let selector = $(e);
                if (!validator.element(selector)) {
                    isPassed = false;
                }
            });
        }

        return isPassed;
    };

    // init Default Values
    var initDefaultValues = function () {
        if (isNullOrEmpty(visionConfigNetworkSection)) {
            visionConfigNetworkSection = {
                Ethernet: {
                    IPAssignmentMode: "auto",
                    IPAddress: "",
                    SubnetMask: "",
                    Gateway: ""
                },
                Wireless: {
                    SSID: "",
                    SecurityType: "",
                    SecurityKey: "",
                    Secured: false,
                }
            };
            checkNetworkStatus();
        }
    };

    // init Resource
    var initResource = function () {
        CONNECTED_TEXT = resources.getValue("DeviceStatus_Connected");
        DISCONNECTED_TEXT = resources.getValue("DeviceStatus_NotConnected");
        NO_INTERNET_TEXT = resources.getValue("DeviceStatus_NoInternet");
	    CONNECTED_NOT_DONE_TEXT = resources.getValue("Network_Tips_NotConnected");	
        ESTABLISH_CONNECTIVITY_TEXT = resources.getValue("Network_Tips_Establish_Connectivity");
        CONNECTED_DONE_TEXT = resources.getValue("Network_Tips_Connected");
        NO_ETHERNET_TEXT = isConfigInit ? resources.getValue("Network_Tips_NoEthernet_Init")
            : resources.getValue("Network_Tips_NoEthernet_Update");
        NO_WIFI_TEXT = isConfigInit ? resources.getValue("Network_Tips_NoWifi_Init")
            : resources.getValue("Network_Tips_NoWifi_Update");
        ADD_NEW_NETWORK_TEXT = resources.getValue("Network_Title_AddNetwork");
        PLACEHOLDER_ENTER_KEY_TEXT = resources.getValue("Network_Placeholder_EnterKey");
        PLACEHOLDER_NO_KEY_TEXT = resources.getValue("Network_Placeholder_NoKey");
        SETUP_WIFI_WARNING_MSG_TEXT = resources.getValue("Network_Message_Warning_SetUp_Wifi");
    };

    // get css-style by wifi quality
    var getQualityClass = function (quality) {
        let qualityClass = "very-low";
        if (isNotNumber(quality)) {
            qualityClass = "very-strong";
        } else {
            if (quality >= 0 && quality <= 0.2) {
                qualityClass = "very-low";
            } else if (quality > 0.2 && quality <= 0.5) {
                qualityClass = "low";
            } else if (quality > 0.5 && quality <= 0.8) {
                qualityClass = "strong";
            } else if (quality > 0.8) {
                qualityClass = "very-strong";
            }
        }
        return qualityClass;
    }

    // Init Network config file
    var initNetworkEvent = function (data, callBackSuc, callBackFailed) {
        callRoute_Normal("/Setting/InitNetwork", "POST", data, function (result) {
            if (result.toString() == "OK") {
                callBackSuc();
            }
            else {
                callBackFailed(true, result);
            }
        }, function (errorMsg) {
            callBackFailed(false, errorMsg);
        });
    };

    var initStepStatus = function () {
        let stepStatus = JSON.parse($("#hidStepStatus").val());
        if (stepStatus.CameraCompleted) {
            $("#step-camera:visible").addClass("is-done");
        }
        if (stepStatus.ScaleCompleted) {
            $("#step-scale:visible").addClass("is-done");
        }
        if (stepStatus.EIDCompleted) {
            $("#step-EID:visible").addClass("is-done");
        }
    };

    var getNetworkStatusText = function (statusCode) {
        var statusText = DISCONNECTED_TEXT;
        if (statusCode == connectStatus.NoInternet) {
            statusText = NO_INTERNET_TEXT;
        }
        else if (statusCode == connectStatus.Connected) {
            statusText = CONNECTED_TEXT;
        }
        return statusText;
    };

    var initEthernetManualInputElems = function () {
        var ipAddress = visionConfigNetworkSection.Ethernet.IPAddress;
        var subnetMask = visionConfigNetworkSection.Ethernet.SubnetMask;
        var gateway = visionConfigNetworkSection.Ethernet.Gateway;
        dom_ipAddressInput.val(!isNullOrEmpty(ipAddress) ? ipAddress : "");
        dom_subnetMaskInput.val(!isNullOrEmpty(subnetMask) ? subnetMask : "");
        dom_gatewayInput.val(!isNullOrEmpty(gateway) ? gateway : "");
    };

    var showErrorMsgModal = function (pageName, errMsg) {
        if (currentPage == pageName) {
            $(".modal-message-fail .modal-message-content").text(errMsg);
            $(".modal-message-fail").addClass("open");
        }
    };

    var hideErrorMsgModal = function () {
        $(".modal-message-fail .modal-message-content").empty();
        $(".modal-message-fail").removeClass("open");
    };

    return {
        init: function () {
            initMQTTClient();
            registerEvents();
            initResource();
            initDefaultValues();
            if (isConfigInit) {
                initStepStatus();
            }
            checkNetworkStatus();
        }
    }
}();