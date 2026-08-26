var Camera = function () {
    let mqtt_client = null;

    let visionConfigCameraSection = JSON.parse($("#hidVisionConfigCameraSection").val());
    let isConfigInit = !$("#headerInit").hasClass("hide");
    if (isConfigInit) {
        $("#step-camera:visible").addClass("is-active");
    }
    else {
        $("#camera:visible").addClass("is-active");
    }

    let CONNECTED_TEXT = null;
    let DISCONNECTED_TEXT = null;
    let CONNECTED_DESCRIPTION = null;
    let DISCONNECTED_DESCRIPTION = null;
    let NOT_COMPLETED_TEXT = null;
    let COMPLETION_DATE_TEXT = null;

    const dom_cameraStatus = $(".camera-status");
    const dom_loadingWrap = $("main");
    const dom_loadingBody = $("body");
    const dom_headerCurrentStep = $("#step-camera");

    var cameraHasConnected = false;

    // init Resource
    var initResource = function () {
        CONNECTED_TEXT = resources.getValue("DeviceStatus_Connected");
        DISCONNECTED_TEXT = resources.getValue("DeviceStatus_NotConnected");
        CONNECTED_DESCRIPTION = resources.getValue("Camera_Tips_Connected");
        DISCONNECTED_DESCRIPTION = resources.getValue("Camera_Tips_NotConnected");
        NOT_COMPLETED_TEXT = resources.getValue("Common_Label_NotCompleted");
        COMPLETION_DATE_TEXT = resources.getValue("Common_Label_CompletionDate");
        DONE_TEXT = resources.getValue("Common_Button_Done");
    };

    let registerEvents = function () {
        // Set camera
        $(document).delegate("[data-toggle='settings-camera']", clickEvent, function () {
            dom_loadingBody.addClass("loading");
            sendCameraRequest("startlive");
        });
    };

    var initDefaultValues = function () {
        if (isNullOrEmpty(visionConfigCameraSection)) {
            visionConfigCameraSection = {
                IsCompleted: false,
                CompletedDate: null
            };
            checkCameraStatus();
        }
    };

    // MQTT: init MQTT Client
    var initMQTTClient = function () {
        mqtt_client = MqttClient.init();

        mqtt_client.on("message", (topic, message, packet) => {
            switch (topic) {
                case "ISMDeviceStatus":
                    handleCameraStatusMessage(message);
                    break;
                case "ISMCameraConfig":
                    handleCameraResponseMessage(message);
                    break;
            }
        });
    };

    // MQTT: Send Camera Request
    var sendCameraRequest = function () {
        let result = {
            method: "request",
            type: "camera",
            args: "startlive"
        };
        let jsonData = JSON.stringify(result);
        mqtt_client.publish('ISMCameraConfig', jsonData, { qos: 2, retain: false });
    }

    // MQTT: handle Camera Status Message
    var handleCameraStatusMessage = function (message) {
        let data = JSON.parse(message);
        if (!isNullOrEmpty(data.statusType) && data.statusType == "camera") {
            cameraHasConnected = data.isConnected;
            checkCameraStatus();
        }
    }

    // MQTT: handle Camera Response Message
    var handleCameraResponseMessage = function (message) {
        let data = JSON.parse(message);

        // Response
        writeActionLog("handleMQTTMessage-start. Message:" + message);
        if (!isNullOrEmpty(data) && !isNullOrEmpty(data.method) && data.method == "response" && !isNullOrEmpty(data.type) && data.type == "camera" && !isNullOrEmpty(data.args)) {
            switch (data.args) {
                case "startlive":
                    if (!isNullOrEmpty(data.value) && !isNullOrEmpty(data.value.status) && data.value.status == "success") {
                        visionConfigCameraSection.IsCompleted = true;                        
                        visionConfigCameraSection.CompletedDate = moment().format("YYYY-MM-DD hh:mm:ss");                        
                    } else if (!isNullOrEmpty(data.value) && !isNullOrEmpty(data.value.status) && data.value.status == "error") {
                        $(".modal-message-content").text(data.value.errorMessage);
                        $(".modal-message-fail").addClass("open");

                        visionConfigCameraSection.IsCompleted = false;
                        visionConfigCameraSection.CompletedDate = null;
                    }
                    checkCameraStatus();

                    dom_loadingBody.removeClass("loading");
                    initCameraEvent(visionConfigCameraSection, function () {
                        console.log("update success");
                    }, function (isCustomError, errorMsg) {
                        // Error code
                        console.log("error:" + errorMsg);
                    });
                break;
            }
        }
    };

    // Check camera status
    var checkCameraStatus = function () {
        let cameraHasChecked = visionConfigCameraSection.IsCompleted;        

        let statusIcon = dom_cameraStatus.find(".status-icon");
        let statusTitle = dom_cameraStatus.find(".status-title");
        let statusDescription = dom_cameraStatus.find(".status-description");
        let objChecked = dom_cameraStatus.find(".box-list");
        let objCheckedStatusIcon = dom_cameraStatus.find(".checked-status");
        let objStatus = dom_cameraStatus.find(".box-status");

        if (cameraHasConnected) {
            dom_cameraStatus.removeClass("mt-l");
            statusIcon.addClass("connected");
            objChecked.removeClass("hide");
            if (cameraHasChecked) {
                dom_headerCurrentStep.addClass("is-done");
            } else {
                dom_headerCurrentStep.removeClass("is-done");
            }
        } else {
            dom_cameraStatus.addClass("mt-l");
            statusIcon.removeClass("connected");
            objChecked.addClass("hide");
            dom_headerCurrentStep.removeClass("is-done");
        }

        if (cameraHasChecked) {
            objCheckedStatusIcon.addClass("checked");
        } else {
            objCheckedStatusIcon.removeClass("checked");
        }

        statusTitle.text(cameraHasConnected ? CONNECTED_TEXT : DISCONNECTED_TEXT);
        var statusDescText = "";
        if (cameraHasConnected && !cameraHasChecked) { // Camera is connected but not complete config
            statusDescText = CONNECTED_DESCRIPTION;
        } else if (!cameraHasConnected) { // Camera is not connected
            statusDescText = DISCONNECTED_DESCRIPTION;
        }
        statusDescription.text(statusDescText);
        objStatus.empty();
        if (cameraHasChecked) {            
            var spanElem = document.createElement("span");
            spanElem.innerText = DONE_TEXT;
            var smallElem = document.createElement("small");
            smallElem.innerText = COMPLETION_DATE_TEXT + " " + moment(visionConfigCameraSection.CompletedDate).format("MM/DD/YYYY");
            objStatus.append(spanElem);
            objStatus.append(smallElem);
        } else {
            objStatus.text(NOT_COMPLETED_TEXT);
        }
        dom_loadingWrap.removeClass("loading2");
    }

    // init Camera config file
    var initCameraEvent = function (data, callBackSuc, callBackFailed) {
        callRoute_Normal("/Setting/InitCamera", "POST", data, function (result) {
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

    return {
        init: function () {
            initMQTTClient();
            registerEvents();
            initResource();
            initDefaultValues();
            if (isConfigInit) {
                initStepStatus();
            }
            checkCameraStatus();
        }
    }
}();