var EID = function () {
    let deviceList = [];
    let currentEIDType = false
    let UHFEIDConnected = false;
    let visionConfigEIDSection = JSON.parse($("#hidVisionConfigEIDSection").val());
    let currentLFEIDBaudRate = visionConfigEIDSection.LF.BaudRate;
    let visionConfigBaudRateList = JSON.parse($("#hidVisionConfigBaudRateList").val());
    let isConfigInit = !$("#headerInit").hasClass("hide");
    if (isConfigInit) {
        $("#step-EID:visible").addClass("is-active");
    }
    else {
        $("#EID:visible").addClass("is-active");
    }

    let CONNECTED_TEXT = null;
    let NOT_CONNECTED_TEXT = null;
    let WAITING_TEXT = null;
    let NOT_COMPLETED_TEXT = null;
    let DONE_TEXT = null;
    let DESC_TEXT = null;

    var lastGetLFEIDReaderDate = null;
    var lastGetUHFEIDReaderDate = null;
    var lfEIDReaderExpiredTimer = null;
    var uhfEIDReaderExpiredTimer = null;
    let eidReaderExpiredDuration = null;
    let dom_LFEIDDebug = null;
    let autoHideTag = null;

    const dom_EIDStatus = $(".EID-status"),
        dom_loadingWrap = $("main"),
        dom_headerCurrentStep = $("#step-EID"),
        dom_indexStatusTitle = $("main").find(".status-title"),
        dom_LFEIDSettings = $(".lf-eid-settings"),
        dom_LFEIDBaudRate = $(".lf-eid-baud-rate"),        
        dom_LFEIDDeviceSettings = $(".test-lf-eid"),
        dom_UHFEIDSettings = $(".uhf-eid-settings"),
        dom_avaiableDevices = $(".available-devices"),
        dom_bluetoothTest = $(".test-bluetooth-eid"),
        dom_UHFEIDTest = $(".test-uhf-reader");

    // init Resource
    var initResource = function (type) {
        CONNECTED_TEXT = resources.getValue("DeviceStatus_Connected");
        NOT_CONNECTED_TEXT = resources.getValue("DeviceStatus_NotConnected");

        EID_UHF_CONNECTED_TEXT = resources.getValue("EID_Title_UHF_Connected");
        EID_UHF_NOT_CONNECTED_TEXT = resources.getValue("EID_Title_UHF_NotConnected");

        CONNECTED_NOT_DONE_TEXT = resources.getValue("EID_Tips_NotDone");
        CONNECTED_DONE_TEXT = resources.getValue("EID_Tips_Done");
        NO_LF_EID_TEXT = resources.getValue("EID_Tips_NoLFEID");
        NO_UHF_EID_TEXT = resources.getValue("EID_Tips_NoUHFEID");
        CHECKED_TEXT = resources.getValue("EID_Label_CheckAgain");
        TEST_TEXT = resources.getValue("EID_Label_TestAgain");
        WAITING_TEXT = resources.getValue("EID_Reader_ScanNewTag");
        COMPLETION_DATE_TEXT = resources.getValue("Common_Label_CompletionDate");
        NOT_COMPLETED_TEXT = resources.getValue("Common_Label_NotCompleted");
        DONE_TEXT = resources.getValue("Common_Button_Done");
        DESC_TEXT = resources.getValue("DeviceStatus_Error_EIDDisconnected_Desc_3");
    };

    var registerEvents = function () {
        // Set LF EID
        $(document).on(clickEvent, "[data-toggle='settings-LF-EID']", function () {
            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            currentEIDType = "LF_EID";
            showSubPage(currentLevel, targetID);
            var hasPaired = visionConfigEIDSection.LF.IsPaired;
            if (hasPaired == false) {
                checkAvailableDevices();
                refresh_LF_EID_Status();
                $("#LF-EID-IsPaired-No").prop("checked", "checked");
                $("#LF-EID-IsPaired-Yes").removeProp("checked");
            } else if (hasPaired == true) {
                refresh_LF_EID_Status();
                $("#LF-EID-IsPaired-Yes").prop("checked", "checked");
                $("#LF-EID-IsPaired-No").removeProp("checked");
            }
            check_LF_EID_Paired_Page();
        });

        // Select LF EID Connection Method
        $(document).on("change", "[name='LF-EID-IsPaired']", function () {
            if ($(this).val() == "yes") {
                visionConfigEIDSection.LF.IsPaired = true;
                visionConfigEIDSection.LF.BluetoothDeviceName = "";
                visionConfigEIDSection.LF.BluetoothMacAddress = "";
                visionConfigEIDSection.LF.IsCompleted = null;
                visionConfigEIDSection.LF.CompletedDate = null;
            }
            else if ($(this).val() == "no") {
                visionConfigEIDSection.LF.IsPaired = false;
                visionConfigEIDSection.LF.IsCompleted = null;
                visionConfigEIDSection.LF.CompletedDate = null;
                checkAvailableDevices();
            }
            writeUserLog("EID - Connection Method IsPaired:" + $(this).val());
            initEIDEvent(visionConfigEIDSection, function () {
                refresh_LF_EID_Status();
                check_LF_EID_Paired_Page();
                console.log("success");
            }, function (isCustomError, errorMsg) {
                console.log("error:" + errorMsg);
            });
        });

        // Select LF EID Bluetooth Device
        $(document).on(clickEvent, ".list-devices li", function () {
            let obj = $(this);
            let objMyDeviceStatus = dom_LFEIDSettings.find(".my-device .box-status");
            if (objMyDeviceStatus.is(":visible")) {
                objMyDeviceStatus.addClass("loading1");
            }
            selectedDevice = obj.text();
            selectedMacAddress = obj.attr("id");
            dom_loadingWrap.addClass("loading");

            // Reset Data
            visionConfigEIDSection.LF.IsPaired = false;
            visionConfigEIDSection.LF.BluetoothDeviceName = selectedDevice;
            visionConfigEIDSection.LF.BluetoothMacAddress = selectedMacAddress;
            visionConfigEIDSection.LF.IsCompleted = null;
            visionConfigEIDSection.LF.CompletedDate = null;
            writeUserLog("EID - ConnectBluetooth, BluetoothDeviceName: " + selectedDevice + ", BluetoothMacAddress: " + selectedMacAddress);
            initEIDEvent(visionConfigEIDSection, function () {
                writeUserLog("EID - Save Successful.");
                console.log("success");
            }, function (isCustomError, errorMsg) {
                // Error code
                console.log("error:" + errorMsg); z
            });
            sendEIDRequest("LFEID", "connectbluetooth", selectedDevice);
        });

        // Set LF EID Bluetooth Device 
        $(document).on(clickEvent, "[data-toggle='settings-LF-EID-test']", function () {
            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            showSubPage(currentLevel, targetID);
            check_LF_TestEID_Page();
        });

        // LF:Test EID (not paired) Click
        $(document).on(clickEvent, "[data-toggle='test-bluetooth-eid']", function () {
            dom_LFEIDDebug = $(".section-debug").eq(1);
            currentLFEIDBaudRate = visionConfigEIDSection.LF.BaudRate;
            dom_LFEIDBaudRate.text(currentLFEIDBaudRate);
            sendEIDRequest("LFEID", "readbluetooth", {baud: currentLFEIDBaudRate});
            console.log("Send baud rate ", currentLFEIDBaudRate);

            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            showSubPage(currentLevel, targetID);
            $(".btn-done:visible").addClass("btn-disabled");
            $(".list-results:visible li").eq(0).find("span").addClass("default").text(WAITING_TEXT);
            checkBluetoothEID();
        });

        // LF:Test EID (paired)
        $(document).on(clickEvent, "[data-toggle='test-Paired-EID']", function () {
            dom_LFEIDDebug = $(".section-debug").eq(0);
            currentLFEIDBaudRate = visionConfigEIDSection.LF.BaudRate;
            dom_LFEIDBaudRate.text(currentLFEIDBaudRate);
            sendEIDRequest("LFEID", "readbluetooth", {baud: currentLFEIDBaudRate});
            console.log("Send baud rate ", currentLFEIDBaudRate);

            let targetID = $(this).data("target");                                             
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            showSubPage(currentLevel, targetID);
            $(".btn-done:visible").addClass("btn-disabled");
            $(".list-results:visible li").eq(0).find("span").addClass("default").text(WAITING_TEXT);
            clearMyDevice();
        });

        // Change Baud Rate
        $(document).on(clickEvent, "[data-toggle='try-new-baudrate']", function () {
            const btnChangeBaudrate = dom_LFEIDDebug.find(".btn-change-baudrate");
            const btnUpdating = dom_LFEIDDebug.find(".btn-updating");
            const objTag = dom_LFEIDDebug.find(".tag");

            btnChangeBaudrate.addClass("hide");
            btnUpdating.removeClass("hide");

            currentLFEIDBaudRate = getBaudRate(currentLFEIDBaudRate);
            dom_LFEIDBaudRate.text(currentLFEIDBaudRate);
            sendEIDRequest("LFEID", "readbluetooth", {baud: currentLFEIDBaudRate});
            console.log("Try new baud rate ", currentLFEIDBaudRate);

            btnUpdating.addClass("hide");
            objTag.removeClass("hide");
            // Auto hide tag
            autoHideTag = setTimeout(hideTag, 10000);

        });

        // Close Baud Rate Tag
        $(document).on(clickEvent, "[data-toggle='close-tag']", function () {
            const btnChangeBaudrate = dom_LFEIDDebug.find(".btn-change-baudrate");
            const objTag = $(this).parent();

            objTag.addClass("hide");
            btnChangeBaudrate.removeClass("hide");
            clearTimeout(autoHideTag);
        });

        // Set UHF EID 
        $(document).on(clickEvent, "[data-toggle='settings-UHF-EID']", function () {
            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            currentEIDType = "UHF_EID";
            showSubPage(currentLevel, targetID);
            check_UHF_EID_Status();
        });


        // Test UHF EID Click 
        $(document).on(clickEvent, "[data-toggle='test-UHF-EID']", function () {
            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            showSubPage(currentLevel, targetID);
            $(".btn-done:visible").addClass("btn-disabled");
            $(".list-results:visible li").eq(0).find("span").addClass("default").text(WAITING_TEXT);
        });

        // EID Checked Not Done
        $(document).on(clickEvent, "[data-toggle='checked-not-done']", function () {
            let target = $(this).parent().data("id");
            switch (target) {
                case "test-bluetooth-eid":
                case "settings-LF-EID-reader":
                    visionConfigEIDSection.LF.IsCompleted = false;
                    visionConfigEIDSection.LF.CompletedDate = null;
                    break;
                case "settings-UHF-EID-reader":
                    visionConfigEIDSection.UHF.IsCompleted = false;
                    visionConfigEIDSection.UHF.CompletedDate = null;
                    break;
                default:
            }
            initEIDEvent(visionConfigEIDSection, function () {
                console.log("success");
            }, function (isCustomError, errorMsg) {
                console.log("error:" + errorMsg);
            });
            $(".btn-back:visible").trigger("click");
            if(autoHideTag){
                clearTimeout(autoHideTag);
                hideTag();
            }
        });

        // EID Checked Done
        $(document).on(clickEvent, "[data-toggle='checked-done']", function () {
            let target = $(this).parent().data("id");
            switch (target) {
                case "test-bluetooth-eid":
                case "settings-LF-EID-reader":
                    visionConfigEIDSection.LF.BaudRate = currentLFEIDBaudRate;
                    sendEIDRequest("LFEID", "savebaud", {baud: visionConfigEIDSection.LF.BaudRate})
                    visionConfigEIDSection.LF.IsCompleted = true;
                    visionConfigEIDSection.LF.CompletedDate = moment().format('YYYY-MM-DD hh:mm:ss');
                    break;
                case "settings-UHF-EID-reader":
                    visionConfigEIDSection.UHF.IsCompleted = true;
                    visionConfigEIDSection.UHF.CompletedDate = moment().format('YYYY-MM-DD hh:mm:ss');
                    break;
                default:
            }
            initEIDEvent(visionConfigEIDSection, function () {
                console.log("success");
            }, function (isCustomError, errorMsg) {
                console.log("error:" + errorMsg);
            });
            $(".btn-back:visible").trigger("click");
            if(autoHideTag){
                clearTimeout(autoHideTag);
                hideTag();
            }
        });

        // Back
        $(document).on(clickEvent, ".btn-back", function () {
            let targetID = $(this).data("target");
            let obj = $(this).closest("[data-page-level]");
            let currentLevel = obj.data("page-level");
            showParentPage(currentLevel, targetID);
            var currentPage = $(".main-settings").not(".hide");
            if (currentPage.hasClass("test-lf-eid")) {
                check_LF_TestEID_Page();
            }
            else if (currentPage.hasClass("lf-eid-settings")) {
                check_LF_EID_Paired_Page();
            }
            else if (currentPage.hasClass("uhf-eid-settings")) {
                check_UHF_EID_Status();
            }
            if (currentLevel == 2) { //reback to eid index
                checkEIDStatus();
            }
            if(autoHideTag){
                clearTimeout(autoHideTag);
                hideTag();
            }
        });


        // Listen for exit page events
        window.addEventListener('beforeunload', function () {
            // Clear timer
            clearInterval(lfEIDReaderExpiredTimer);
            clearInterval(uhfEIDReaderExpiredTimer);

            if(visionConfigEIDSection.LF.BaudRate != currentLFEIDBaudRate){
                sendEIDRequest("LFEID", "readbluetooth", {baud:visionConfigEIDSection.LF.BaudRate});
                console.log("Restore baud rate ", visionConfigEIDSection.LF.BaudRate);
            }
        });
    };

    // init MQTT Client
    var initMQTTClient = function () {
        mqtt_client = MqttClient.init();
        mqtt_client.on("message", (topic, message, packet) => {
            switch (topic) {
                case "ISMDeviceStatus":
                    handleDeviceStatus(message);
                    break;
                case "ISMEIDConfig":
                    handleEIDResponseMessage(message);
                    break;
            }
        });
    };

    // MQTT: send EID Request 
    var sendEIDRequest = function (type, args, value) {
        let result = null;
        if (type == "LFEID") {
            result = {
                method: "request",
                type: type,
                args: args,
                value: value
            };
        } else if (type == "UHFEID") {
            result = {
                method: "request",
                type: type
            };
        }
        let jsonData = JSON.stringify(result);
        mqtt_client.publish('ISMEIDConfig', jsonData, { qos: 2, retain: false });
    }

    // MQTT: handle Device Status Message
    var handleDeviceStatus = function (message) {
        let data = JSON.parse(message);
        console.log(data);
        let needUpdate = false;
        if (data.statusType == "UHFEID") {
            UHFEIDConnected = data.isConnected;
            var currentPage = $(".main-settings").not(".hide");
            checkEIDStatus();
            if (currentPage.hasClass("test-uhf-reader") && UHFEIDConnected == false) {
                check_UHF_EID();
                check_UHF_EID_Status();
            }
            if (currentPage.hasClass("uhf-eid-settings")) {
                check_UHF_EID_Status();
            }
        }
        if (needUpdate) {
            initEIDEvent(visionConfigEIDSection, function () {
                console.log("success");
            }, function (isCustomError, errorMsg) {
                console.log("error:" + errorMsg);
            });
        }
    };

    // MQTT: handle EID Response Message 
    var handleEIDResponseMessage = function (message) {
        let data = JSON.parse(message);
        console.log(data);
        // Response
        if (!isNullOrEmpty(data) && data.method == "response") {
            if (!isNullOrEmpty(data.type) && data.type == "LFEID") {
                if (!isNullOrEmpty(data.args) && data.args == "scanbluetooth") {
                    let objTitle = dom_avaiableDevices.find(".section-box-title");
                    deviceList = data.value.list;
                    //  if (!isNullOrEmpty(data.value) && !isNullOrEmpty(data.value.list)) {
                    if (!isNullOrEmpty(data.value.status) && data.value.status == "success") {
                        initDeviceList();
                        objTitle.removeClass("loading1");
                        dom_avaiableDevices.removeClass("hide");
                        $(".main-tips").addClass("hide");
                    } else if (!isNullOrEmpty(data.value.status) && data.value.status == "error") {
                        $("#deviceList").find("li").not(".template").remove();

                        $(".modal-lf-message-content").text(data.value.errorMessage);
                        $(".modal-lf-message-fail").addClass("open");
                        dom_avaiableDevices.addClass("hide");
                        $(".my-device").addClass("hide");
                        objTitle.removeClass("loading1");
                    }
                    // check current bluetooth is available
                    if (isNullOrEmpty(visionConfigEIDSection.LF.BluetoothMacAddress) == false) {
                        var hasConnetedDevcie = deviceList.filter((item) => { return item.macAddress == visionConfigEIDSection.LF.BluetoothMacAddress }).length > 0;
                        if (hasConnetedDevcie == false) {
                            visionConfigEIDSection.LF.BluetoothDeviceName = "";
                            visionConfigEIDSection.LF.BluetoothMacAddress = "";
                            visionConfigEIDSection.LF.IsCompleted = null;
                            visionConfigEIDSection.LF.CompletedDate = null;
                            initEIDEvent(visionConfigEIDSection, function () {
                                console.log("success");
                            }, function (isCustomError, errorMsg) {
                                console.log("error:" + errorMsg);
                            });
                            $(".my-device").addClass("hide");
                        }
                    }
                    dom_loadingWrap.removeClass("loading");
                }
                else if (!isNullOrEmpty(data.args) && data.args == "connectbluetooth") {
                    if (!isNullOrEmpty(data.value)) {
                        if (!isNullOrEmpty(data.value.status) && data.value.status == "success") {
                            console.log("Connect to bluetooth success");
                            initDeviceList();
                            if (isNullOrEmpty(visionConfigEIDSection.LF.BluetoothDeviceName) == false) {
                                $("[name='bluetoothDevice']").text(visionConfigEIDSection.LF.BluetoothDeviceName);
                                $("[name='bluetoothDevice']").attr("id", visionConfigEIDSection.LF.BluetoothMacAddress);
                            }
                            check_LF_EID_Paired_Page();
                        }
                        else if (!isNullOrEmpty(data.value.status) && data.value.status == "error") {
                            console.log("Connect to bluetooth error");
                            dom_LFEIDSettings.find(".my-device .box-status").removeClass("loading1");
                            visionConfigEIDSection.LF.BluetoothDeviceName = $(".my-connected-device").text();
                            visionConfigEIDSection.LF.BluetoothMacAddress = $(".my-connected-device").attr("id");
                            initEIDEvent(visionConfigEIDSection, function () {
                                //check_LF_EID_Paired_Page();
                                console.log("success");
                            }, function (isCustomError, errorMsg) {
                                console.log("error:" + errorMsg);
                            });


                            $(".modal-lf-message-content").text(data.value.errorMessage);
                            $(".modal-lf-message-fail").addClass("open");
                        }

                        writeUserLog("EID - ConnectBluetooth:" + data.value.status)
                    }
                    dom_loadingWrap.removeClass("loading");
                }
            }
        }
        else if (!isNullOrEmpty(data)) {
            var handleLFReader = $(".main-settings").not(".hide").hasClass("test-lf-reader");
            var handleUHFReader = $(".main-settings").not(".hide").hasClass("test-uhf-reader");
            if (!isNullOrEmpty(data.value) && !isNullOrEmpty(data.value.status) && data.value.status == "success") {
                console.log("check success");
                if (data.type == "LFEID" && currentEIDType == "LF_EID" && handleLFReader) {
                    lastGetLFEIDReaderDate = new Date();
                    $(".lf-eid-reader").text(data.value.reader);
                    $(".btn-done").removeClass("btn-disabled");
                    $(".modal-lf-message-content").empty();
                    $(".modal-lf-message-fail").removeClass("open");
                    if (!isNullOrEmpty(data.value.baud)) {
                        currentLFEIDBaudRate = data.value.baud;
                        dom_LFEIDBaudRate.text(currentLFEIDBaudRate);
                        console.log("Receive Baud Rate ", currentLFEIDBaudRate);
                    }
                } else if (data.type == "UHFEID" && currentEIDType == "UHF_EID" && handleUHFReader) {
                    lastGetUHFEIDReaderDate = new Date();
                    $(".uhf-eid-reader").text(data.value.reader);
                    $(".btn-done").removeClass("btn-disabled");
                    $(".modal-uhf-message-content").empty();
                    $(".modal-uhf-message-fail").removeClass("open");
                }
            }
            else if (!isNullOrEmpty(data.value) && !isNullOrEmpty(data.value.status) && data.value.status == "error") {
                console.log("check error");
                if (data.type == "LFEID" && currentEIDType == "LF_EID" && handleLFReader) {
                    lastGetLFEIDReaderDate = new Date();
                    $(".lf-eid-reader").text("");
                    $(".btn-done").addClass("btn-disabled");
                    $(".modal-lf-message-content").text(data.value.errorMessage);
                    $(".modal-lf-message-fail").addClass("open");
             
                } else if (data.type == "UHFEID" && currentEIDType == "UHF_EID" && handleUHFReader) {
                    lastGetUHFEIDReaderDate = new Date();
                    $(".uhf-eid-reader").text("");
                    $(".btn-done").addClass("btn-disabled");
                    $(".modal-uhf-message-content").text(data.value.errorMessage);
                    $(".modal-uhf-message-fail").addClass("open");
                }
            }
        }
    };

    // check EID Status David
    var checkEIDStatus = function () {
        let statusIcon = dom_EIDStatus.find(".status-icon");
        let LF_EID_StatusText = dom_EIDStatus.find(".LF_EID-status");
        let UHF_EID_StatusText = dom_EIDStatus.find(".UHF_EID-status");
        let hasLFCompleted = visionConfigEIDSection.LF.IsCompleted;
        let hasUHFCompleted = visionConfigEIDSection.UHF.IsCompleted;

        LF_EID_StatusText.empty();
        if (hasLFCompleted) {
            var spanElem = document.createElement("span");
            spanElem.innerText = DONE_TEXT;
            var smallElem = document.createElement("small");
            smallElem.innerText = COMPLETION_DATE_TEXT + " " + moment(visionConfigEIDSection.CompletedDate).format("MM/DD/YYYY");
            LF_EID_StatusText.append(spanElem);
            LF_EID_StatusText.append(smallElem);

        } else {
            LF_EID_StatusText.text(NOT_COMPLETED_TEXT);
        }

        UHF_EID_StatusText.empty();
        if (hasUHFCompleted) {
            var spanElem = document.createElement("span");
            spanElem.innerText = DONE_TEXT;
            var smallElem = document.createElement("small");
            smallElem.innerText = COMPLETION_DATE_TEXT + " " + moment(visionConfigEIDSection.CompletedDate).format("MM/DD/YYYY");
            UHF_EID_StatusText.append(spanElem);
            UHF_EID_StatusText.append(smallElem);
        } else {
            UHF_EID_StatusText.text(NOT_COMPLETED_TEXT);
        }

        if (UHFEIDConnected == true) {
            dom_indexStatusTitle.text(EID_UHF_CONNECTED_TEXT);
            statusIcon.addClass("connected");
        } else if (UHFEIDConnected == false) {
            dom_indexStatusTitle.text(EID_UHF_NOT_CONNECTED_TEXT);
            statusIcon.removeClass("connected");
        }

        if (hasLFCompleted || hasUHFCompleted) {
            dom_headerCurrentStep.addClass("is-done");
            $(".status-description").html("&nbsp;");
        } else {
            $(".status-description").html(DESC_TEXT);
            dom_headerCurrentStep.removeClass("is-done");
        }

        //system setting page hide status description
        if ($("#headerInit").hasClass("hide")) {
            $(".status-description").html("&nbsp;");
        }

        dom_loadingWrap.removeClass("loading2");
    }

    // Refresh LF EID
    var refresh_LF_EID_Status = function () {
        let objWrap = dom_LFEIDSettings.find(".main-box-body .main-box-content");
        let objStatus = dom_LFEIDSettings.find(".main-box-header .box-status");
        let objTips = dom_LFEIDSettings.find(".main-tips");
        if (objTips.is(":visible")) {
            objTips.addClass("loading2");
        } else {
            objWrap.addClass("loading2");
        }
        objStatus.addClass("loading1");
    }

    // Check LF EID David
    var check_LF_EID_Paired_Page = function () {
        let hasCompleted = visionConfigEIDSection.LF.IsCompleted;
        let hasBluetoothDevice = isNullOrEmpty(visionConfigEIDSection.LF.BluetoothMacAddress) ? false : true;
        let hasPaired = visionConfigEIDSection.LF.IsPaired;

        let objWrap = dom_LFEIDSettings.find(".main-box-body .main-box-content");
        let objCheckItem = dom_LFEIDSettings.find(".check-item");
        let objMyDevice = dom_LFEIDSettings.find(".my-device");
        let objAvailableDevices = dom_LFEIDSettings.find(".available-devices");
        let objTips = dom_LFEIDSettings.find(".main-tips");
        let objStatus = dom_LFEIDSettings.find(".main-box-header .box-status");
        let objMyDeviceStatus = dom_LFEIDSettings.find(".my-device .box-status");
        let objTest = objCheckItem.find(".test-paired");
        let objTestIcon = objTest.find(".checked-status");
        let objTestStatus = objTest.find(".box-status");
        objTips.addClass("hide");
        if (visionConfigEIDSection.LF.IsPaired == true || hasPaired == true) {
            objCheckItem.removeClass("hide");
            objMyDevice.addClass("hide");
            objAvailableDevices.addClass("hide");

        } else if (visionConfigEIDSection.LF.IsPaired == false || hasPaired == false) {
            objCheckItem.addClass("hide");
            objAvailableDevices.removeClass("hide");
            if (hasBluetoothDevice) {
                objMyDevice.removeClass("hide");
                if (isNullOrEmpty(visionConfigEIDSection.LF.BluetoothDeviceName) == false) {
                    $("[name='bluetoothDevice']").text(visionConfigEIDSection.LF.BluetoothDeviceName);
                    $("[name='bluetoothDevice']").attr("id", visionConfigEIDSection.LF.BluetoothMacAddress);
                }
            } else {
                objMyDevice.addClass("hide");
            }
            initDeviceList();
        }

        objTestStatus.empty();
        if (hasCompleted) {
            var spanElem = document.createElement("span");
            spanElem.innerText = DONE_TEXT;
            var smallElem = document.createElement("small");
            smallElem.innerText = COMPLETION_DATE_TEXT + " " + moment(visionConfigEIDSection.CompletedDate).format("MM/DD/YYYY");
            objTestStatus.append(spanElem);
            objTestStatus.append(smallElem);
            objTestIcon.addClass("checked");
        }
        else {
            objTestStatus.text(NOT_COMPLETED_TEXT);
            objTestIcon.removeClass("checked");
        }

        objStatus.removeClass("loading1");
        objWrap.removeClass("loading2");
        objTips.removeClass("loading2");
        dom_LFEIDSettings.removeClass("loading2");
        if (objMyDeviceStatus.is(":visible")) {
            objMyDeviceStatus.removeClass("loading1");
            objMyDeviceStatus.text(hasBluetoothDevice ? CONNECTED_TEXT : NOT_CONNECTED_TEXT);
        }
    }

    // Check LF EID Available Bluetooth Devices
    var checkAvailableDevices = function () {
        deviceList = [];
        let objTitle = dom_avaiableDevices.find(".section-box-title");
        objTitle.addClass("loading1");
        //dom_loadingWrap.addClass("loading");
        sendEIDRequest("LFEID", "scanbluetooth");
    }

    // check LF Test EID Page
    var check_LF_TestEID_Page = function () {
        let hasBluetoothDevice = isNullOrEmpty(visionConfigEIDSection.LF.BluetoothMacAddress) ? false : true;
        let hasCompleted = visionConfigEIDSection.LF.IsCompleted;

        let objWrap = dom_LFEIDDeviceSettings.find(".main-box-body");
        let objCheckItem = dom_LFEIDDeviceSettings.find(".check-item");
        let objTips = dom_LFEIDDeviceSettings.find(".main-tips");
        let objStatus = dom_LFEIDDeviceSettings.find(".main-box-header .box-status");
        let objCheckedStatusIcon = dom_LFEIDDeviceSettings.find(".checked-status");
        let objCheckedStatus = dom_LFEIDDeviceSettings.find(".test-bluetooth-eid .box-status")
        if (hasBluetoothDevice) {
            objTips.addClass("hide");
            objCheckItem.removeClass("hide");
        } else {
            objTips.removeClass("hide");
            objCheckItem.addClass("hide");
        }
        objCheckedStatus.empty();
        if (hasCompleted) {
            var spanElem = document.createElement("span");
            spanElem.innerText = DONE_TEXT;
            var smallElem = document.createElement("small");
            smallElem.innerText = COMPLETION_DATE_TEXT + " " + moment(visionConfigEIDSection.CompletedDate).format("MM/DD/YYYY");
            objCheckedStatus.append(spanElem);
            objCheckedStatus.append(smallElem);
            objCheckedStatusIcon.addClass("checked");
        } else {
            objCheckedStatus.text(NOT_COMPLETED_TEXT);
            objCheckedStatusIcon.removeClass("checked");
        }
        objWrap.removeClass("loading2");
        objStatus.removeClass("loading1").text(hasBluetoothDevice ? CONNECTED_TEXT : NOT_CONNECTED_TEXT);
    }

    // Check Bluetooth EID
    var checkBluetoothEID = function () {
        let hasBluetoothDevice = isNullOrEmpty(visionConfigEIDSection.LF.BluetoothMacAddress) ? false : true;
        let objWrap = dom_bluetoothTest.find(".main-box-body");
        let objStatus = dom_bluetoothTest.find(".main-box-header .box-status");
        if (!hasBluetoothDevice) {
            $(".btn-back:visible").trigger("click");
        }
        objWrap.removeClass("loading2");
        objStatus.removeClass("loading1").text(hasBluetoothDevice ? CONNECTED_TEXT : NOT_CONNECTED_TEXT);
    }

    // check UHF EID Status
    var check_UHF_EID_Status = function () {
        let hasConnected = UHFEIDConnected;
        let hasCompleted = visionConfigEIDSection.UHF.IsCompleted;
        let objWrap = dom_UHFEIDSettings.find(".main-box-body");
        let objCheckItem = dom_UHFEIDSettings.find(".check-item");
        let objTips = dom_UHFEIDSettings.find(".main-tips");
        let objStatus = dom_UHFEIDSettings.find(".main-box-header .box-status");

        let objEIDCheck = objCheckItem.find(".check-uhf");

        let objTest = objCheckItem.find(".test-UHF");
        let objTestIcon = objTest.find(".checked-status");
        let objTestStatus = objTest.find(".box-status");
        if (hasConnected) {
            objTips.addClass("hide");
            objCheckItem.removeClass("hide");
        } else {
            showParentPage(3, "settings-UHF-EID");

            objTips.removeClass("hide");
            objCheckItem.addClass("hide");
        }

        objStatus.text(hasConnected ? CONNECTED_TEXT : NOT_CONNECTED_TEXT);
        objTestStatus.empty();
        // objEIDCheck.find(".box-status").empty();
        console.log(objEIDCheck)
        if (hasCompleted) {
            var spanElem = document.createElement("span");
            spanElem.innerText = DONE_TEXT;
            var smallElem = document.createElement("small");
            smallElem.innerText = COMPLETION_DATE_TEXT + " " + moment(visionConfigEIDSection.CompletedDate).format("MM/DD/YYYY");
            objTestStatus.append(spanElem);
            objTestStatus.append(smallElem);
            objTestIcon.addClass("checked");
        }
        else {
            objTestStatus.text(NOT_COMPLETED_TEXT);
            objTestIcon.removeClass("checked");
        }
        objStatus.removeClass("loading1");
        objWrap.removeClass("loading2");
        checkEIDStatus();
    }

    // Check UHF EID
    var check_UHF_EID = function () {
        let hasConnected = visionConfigEIDSection.UHF.IsCompleted;
        let objWrap = dom_UHFEIDTest.find(".main-box-body");
        let objStatus = dom_UHFEIDTest.find(".main-box-header .box-status");
        if (!hasConnected) {
            $(".btn-back:visible").trigger("click");
        }
        objWrap.removeClass("loading2");
        objStatus.removeClass("loading1").text(hasConnected ? CONNECTED_TEXT : NOT_CONNECTED_TEXT);
    }

    // init Bluetooth List
    var initDeviceList = function () {
        $("#deviceList").find("li").not(".template").remove();
        if (deviceList.length > 0) {
            for (let i = 0; i < deviceList.length; i++) {
                var liTemp = $("#deviceList").find(".template").clone();
                liTemp.removeClass("hide").removeClass("template");
                if (deviceList[i].macAddress != visionConfigEIDSection.LF.BluetoothMacAddress) {
                    liTemp.attr("id", deviceList[i].macAddress);
                    liTemp.find("span").text(deviceList[i].deviceName);
                    $('#deviceList').append(liTemp);
                }
            };
        }
    }

    // Clear Connected Device
    var clearMyDevice = function () {
        if (currentEIDType == "LF_EID") {
            let devicelist = $("[name='bluetoothDevice']");
            if (devicelist.length > 0) {
                for (let i = 0; i < devicelist.length; i++) {
                    $(devicelist[i]).text("");
                }
            }
        }
        else if (currentEIDType == "UHF_EID") {
            let devicelist = $("[name='UHFDevice']");
            if (devicelist.length > 0) {
                for (let i = 0; i < devicelist.length; i++) {
                    $(devicelist[i]).text("");
                }
            }
        }
    }

    // Init EID config file
    var initEIDEvent = function (data, callBackSuc, callBackFailed) {
        callRoute_Normal("/Setting/InitEID", "POST", data, function (result) {
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

    // Get Baud Rate From List
    var getBaudRate = function(baudRate){
        if(!isNullOrEmpty(baudRate)){
            if(visionConfigBaudRateList.length != 0){
                let index = visionConfigBaudRateList.indexOf(baudRate);
                if(index != -1 && index < visionConfigBaudRateList.length -1){
                    return visionConfigBaudRateList[index + 1];
                }else{
                    return visionConfigBaudRateList[0];
                }
            }
            return baudRate;
        }else{
            return visionConfigEIDSection.LF.BaudRate;
        } 
    }

    function hideTag() {
        const btnChangeBaudrate = dom_LFEIDDebug.find(".btn-change-baudrate");
        const btnUpdating = dom_LFEIDDebug.find(".btn-updating");
        const objTag = dom_LFEIDDebug.find(".tag");

        btnChangeBaudrate.removeClass("hide");
        btnUpdating.addClass("hide");
        objTag.addClass("hide");
    }

    // Clear the Live EID reader
    var clearLiveEIDReader = function () {
        lfEIDReaderExpiredTimer = window.setInterval(function () {
            if (lastGetLFEIDReaderDate != null && getDateSecondsDiff(lastGetLFEIDReaderDate) > eidReaderExpiredDuration) {
                $(".lf-eid-reader").text("");
                $(".list-results:visible li").eq(0).find("span").addClass("default").text(WAITING_TEXT);
                lastGetLFEIDReaderDate = null;
            }
        }, 1000);

        uhfEIDReaderExpiredTimer = window.setInterval(function () {
            if (lastGetUHFEIDReaderDate != null && getDateSecondsDiff(lastGetUHFEIDReaderDate) > eidReaderExpiredDuration) {
                $(".uhf-eid-reader").text("");
                $(".list-results:visible li").eq(0).find("span").addClass("default").text(WAITING_TEXT);
                lastGetUHFEIDReaderDate = null;
            }
        }, 1000);
    };

    return {
        init: function () {
            initMQTTClient();
            registerEvents();
            initResource();
            if (isConfigInit) {
                initStepStatus();
            }
            checkEIDStatus();
            eidReaderExpiredDuration = parseInt(serverConfigConsts.EIDReaderExpiredDuration);
            clearLiveEIDReader();
        }
    }
}();