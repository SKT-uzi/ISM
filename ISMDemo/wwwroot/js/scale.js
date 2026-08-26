var Scale = function () {
    const dom_loadingWrap = $("main");
    const dom_headerCurrentStep = $("#step-scale");
    const dom_backBtn = $(".btn-back");
    const dom_doneBtn = $(".btn-done");
    const dom_indexPage = $("[data-page-level='1']");
    const dom_indexStatusIcon = dom_indexPage.find(".status-icon");
    const dom_indexStatusTitle = dom_indexPage.find(".status-title");
    const dom_indexStatusDesc = dom_indexPage.find(".status-description");
    const dom_indexConfigLink = dom_indexPage.find(".box-list");
    const dom_indexChkStatusIcon = dom_indexPage.find(".checked-status");
    const dom_indexBoxStatus = dom_indexPage.find(".box-status");
    const dom_configPage = $("[data-page-level='2']");
    const dom_configBoxStatus = dom_configPage.find(".box-status");

    let DONE_TEXT = null;
    let TEXT_DEVICESTATUS_CONNECTED = null;
    let TEXT_DESC_CONNECTED = null;
    let TEXT_DEVICESTATUS_NOTCONNECTED = null;
    let TEXT_DESC_DISCONNECTED = null;
    let COMPLETION_DATE_TEXT = null;
    let NOT_COMPLETED_TEXT = null;
    let visionConfigScaleSection = JSON.parse($("#hidVisionConfigScaleSection").val());
    var scaleConnected = false;
    console.log(visionConfigScaleSection);
    let isConfigInit = !$("#headerInit").hasClass("hide");
    if (isConfigInit) {
        $("#step-scale:visible").addClass("is-active");
    }
    else {
        $("#scale:visible").addClass("is-active");
    }

    var initResources = function () {
        TEXT_PLACEHOLDER_WAITFORRESULT = resources.getValue("Common_PlaceHolder_WaitForResult");
        TEXT_DEVICESTATUS_CONNECTED = resources.getValue("DeviceStatus_Connected");
        TEXT_DESC_CONNECTED = resources.getValue("Scale_Desc_Connected");
        TEXT_DEVICESTATUS_NOTCONNECTED = resources.getValue("DeviceStatus_NotConnected");
        TEXT_DESC_DISCONNECTED = resources.getValue("DeviceStatus_Error_ScaleDisconnected_Desc_3");
        DONE_TEXT = resources.getValue("Common_Button_Done");
        COMPLETION_DATE_TEXT = resources.getValue("Common_Label_CompletionDate");
        NOT_COMPLETED_TEXT = resources.getValue("Common_Label_NotCompleted");
    };

    var registerEvents = function () {
        // Config scale button 
        $(document).delegate("[data-toggle='settings-scale']", clickEvent, function () {
            showScaleConfigPage();
        });

        // Back 
        $(document).delegate(".btn-back", clickEvent, function () {
            showScaleDefaultPage();
        });
        $(document).delegate(".technical-information", clickEvent, function () {
            $(".scale-main").addClass("hide");
            $(".scale-information").removeClass("hide");
            $(".main-right").find('button[data-page-level="2"]').addClass("hide");
        });


        // Scale check not done
        $(document).delegate("[data-toggle='checked-not-done']", clickEvent, function () {
            visionConfigScaleSection.IsCompleted = false;
            visionConfigScaleSection.CompletedDate = null;
            initScaleEvent(visionConfigScaleSection, function () {
                dom_backBtn.trigger("click");
                refreshStatus();
                console.log("success");
            }, function (isCustomError, errorMsg) {
                console.log("error:" + errorMsg);
            });
        });

        // Scale check done
        $(document).delegate("[data-toggle='checked-done']", clickEvent, function () {
            visionConfigScaleSection.IsCompleted = true;
            visionConfigScaleSection.CompletedDate = moment().format('YYYY-MM-DD hh:mm:ss');
            initScaleEvent(visionConfigScaleSection, function () {
                dom_backBtn.trigger("click");
                refreshStatus();
                console.log("success");
            }, function (isCustomError, errorMsg) {
                console.log("error:" + errorMsg);
            });
        });
    };

    var initMQTTClient = function () {
        mqtt_client = MqttClient.init();

        mqtt_client.on("message", (topic, message, packet) => {
            switch (topic) {
                case "ISMDeviceStatus":
                    handleScaleErrorMessage(message);
                    break;
                case "ISMScaleConfig":
                    if (scaleConnected) {
                        handleScaleResponseMessage(message);
                    }
                    else {
                        if ($(".scale-status").hasClass("hide")) {
                            showScaleDefaultPage();
                        }
                    }
                    break;
            }
        });
    };

    var showScaleConfigPage = function () {

        dom_configPage.removeClass("hide");
        dom_indexPage.addClass("hide");
    };

    var showScaleDefaultPage = function () {
        if ($(".scale-information").hasClass("hide") == false) {
            $(".scale-main").removeClass("hide");
            $(".scale-information").addClass("hide");
            $(".main-right").find('button[data-page-level="2"]').removeClass("hide");
        }
        else {
            dom_indexPage.removeClass("hide");
            dom_configPage.addClass("hide");
        }
    };

    var handleScaleErrorMessage = function (message) {
        let data = JSON.parse(message);

        if (isNullOrEmpty(data) || isNullOrEmpty(data.statusType) || data.statusType != "scale") {
            return;
        }
        console.log(data);
        scaleConnected = data.isConnected;
        refreshStatus();
    };

    var resetPageData = function () {
        $(".rate").removeClass("tag-success");
        $(".rate").removeClass("tag-warning");
        $(".rate").removeClass("tag-danger");
        $(".icon-tech-status").removeClass("bad very-bad").addClass("hide");;
        $(".scale-settings .list-results span").text("");
        dom_doneBtn.addClass("btn-disabled");
    }

    var handleScaleResponseMessage = function (message) {
        let data = JSON.parse(message);
        if (isNullOrEmpty(data) || isNullOrEmpty(data.type) || data.type != "scale") {
            return;
        }
        if (isNullOrEmpty(data.value) || isNullOrEmpty(data.value.status)) {
            writeActionLog("handleMQTTMessage-quit. Reason: Not a completed scale response message.");
            return;
        }
        console.log(data);
        var detail = data.value;

        resetPageData();

        if (data.value.status == "success") {
            $(".current-item-weight").text(detail.weight);
            var rate = detail.rate_HZ;
            $(".rate").text(detail.rate_HZ + " Hz");
            $(".icon-tech-status").removeClass("hide");
            if (rate >= 2) {
                $(".rate").addClass("tag-success");
                $(".icon-tech-status").addClass("hide");
            } else if (rate >= 1 && rate <= 2) {
                $(".rate").addClass("tag-warning");
                $(".icon-tech-status").addClass("bad");
            } else if (rate < 1) {
                $(".rate").addClass("tag-danger");
                $(".icon-tech-status").addClass("very-bad");
            }
            $(".raw-serial").text(detail.rawserial);
            $(".baud").text(detail.baud);
            $(".parity").text(detail.parity);
            $(".bits").text(detail.bits);
            dom_doneBtn.removeClass("btn-disabled");

            $(".modal-message-content").empty();
            $(".modal-message-fail").removeClass("open");
        }
        else {
            if ($(".scale-settings").hasClass("hide") == false) {
                $(".modal-message-content").text(data.value.errorMessage);
                $(".modal-message-fail").addClass("open");
            }
        }
    };

    var refreshStatus = function () {
        dom_indexBoxStatus.empty();
        if (visionConfigScaleSection.IsCompleted) {
            var spanElem = document.createElement("span");
            spanElem.innerText = DONE_TEXT;
            var smallElem = document.createElement("small");
            smallElem.innerText = COMPLETION_DATE_TEXT + " " + moment(visionConfigScaleSection.CompletedDate).format("MM/DD/YYYY");
            dom_indexBoxStatus.append(spanElem);
            dom_indexBoxStatus.append(smallElem);
            dom_indexChkStatusIcon.addClass("checked");
            dom_headerCurrentStep.addClass("is-done");
        }
        else {
            dom_indexBoxStatus.text(NOT_COMPLETED_TEXT);
            dom_indexChkStatusIcon.removeClass("checked");
            dom_headerCurrentStep.removeClass("is-done");
        }

        if (scaleConnected) {
            dom_indexConfigLink.removeClass("hide");
            dom_indexStatusTitle.text(TEXT_DEVICESTATUS_CONNECTED);
            dom_indexStatusDesc.text(TEXT_DESC_CONNECTED);
            dom_configBoxStatus.text(TEXT_DEVICESTATUS_CONNECTED);
            dom_indexStatusIcon.addClass("connected");

            if (visionConfigScaleSection.IsCompleted) {
                dom_indexStatusDesc.html("&nbsp;");
            }
        }
        else {
            dom_indexStatusTitle.text(TEXT_DEVICESTATUS_NOTCONNECTED);
            dom_indexStatusDesc.text(TEXT_DESC_DISCONNECTED);
            dom_configBoxStatus.text(TEXT_DEVICESTATUS_NOTCONNECTED);
            dom_indexStatusIcon.removeClass("connected");
            dom_indexConfigLink.addClass("hide");
            dom_headerCurrentStep.removeClass("is-done");
        }
        dom_loadingWrap.removeClass("loading2");
    };

    var initScaleEvent = function (data, callBackSuc, callBackFailed) {
        callRoute_Normal("/Setting/InitScale", "POST", data, function (result) {
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
            initResources();
            refreshStatus();
            if (isConfigInit) {
                initStepStatus();
            }
        }
    }
}();