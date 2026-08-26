var Done = function () {    
    $("#step-done:visible").addClass("is-active");

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
        // Checked Done
        $(document).on(clickEvent, "[data-toggle='checked-done']", function () {
            initCompleteEvent(function () {
                console.log("success");
                window.location.href = "/" + $("#hidISMVPath").val() + "/Setting/InitDone";
            }, function (isCustomError, errorMsg) {
                // Error code
                console.log("error:" + errorMsg);
            });
        });
    };

    // Init complete
    var initCompleteEvent = function (callBackSuc, callBackFailed) {
        callRoute_Normal("/Setting/InitComplete", "POST", null, function (result) {
            maskHelper.unblockUI();
            if (result.toString() == "OK") {
                callBackSuc();
            }
            else {
                callBackFailed(true, result);
            }
        }, function (errorMsg) {
            maskHelper.unblockUI();
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
            registerEvents();
            initResource();
            initStepStatus();
        }
    }
}();