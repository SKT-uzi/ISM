var Login = function () {
    const dom_keyboardSpeButton = $(".keyboard li.spe");
    const dom_codeHiddenInput = $(".form-code");
    const dom_title = $(".code-title");
    const dom_codeViewSection = $(".code-view");

    let TEXT_DESC_ENTERPWD = null;
    let TEXT_DESC_TRYAGAIN = null;
    let TEXT_BUTTON_CANCEL = null;
    let TEXT_BUTTON_DELETE = null;

    var initResources = function () {
        TEXT_DESC_ENTERPWD = resources.getValue("Password_Desc_EnterPassword");
        TEXT_DESC_TRYAGAIN = resources.getValue("Login_Desc_TryAgain");
        TEXT_BUTTON_CANCEL = resources.getValue("Common_Button_Cancel");
        TEXT_BUTTON_DELETE = resources.getValue("Password_Button_Delete");
    };

    var registerEvents = function () {
        // Change keyboard background
        $(document).on("touchstart", ".keyboard li:not(.invisible)", function () {
            $(this).addClass("deepen");
        });
        $(document).on("touchmove", ".keyboard li:not(.invisible)", function (e) {
            $(this).removeClass("deepen");
            e.preventDefault();
        });
        $(document).on("touchend", ".keyboard li:not(.invisible)", function () {
            $(this).removeClass("deepen");
        });

        // Enter password
        $(document).delegate(".enter-password .keyboard li:not(.invisible)", clickEvent, function () {
            if ($(this).hasClass("cancel")) { // If cancel, exit system settings and back to Chute Side Web App
                dom_title.text(TEXT_DESC_ENTERPWD);

                var isIframeEmbedded = window.self != window.top ? true : false;
                var isInit = !isIframeEmbedded; // If the page is not embedded, the ism will be initialized.

                if (isInit) { // Init
                    maskHelper.blockUI();
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Welcome";                    
                }
                else { // In the Embedded iframe of chute side web app
                    callRoute_Normal("/Home/SignOut", "POST", null, function (result) {
                    }, function (errorMsg) {
                    });
                    parent.postMessage("HideSystemSettings", "*");
                }
            }
            else if ($(this).hasClass("spe")) {
                // Delete code
                let oldVal = dom_codeHiddenInput.val();
                let n = oldVal.length;
                let newVal = oldVal.substr(0, n - 1);
                dom_codeHiddenInput.val(newVal);
                n = newVal.length;
                dom_codeViewSection.find("i:eq(" + n + ")").removeClass("fill");
                if (n === 0) {
                    changeToCancel();
                }
            }
            else {
                changeToClear();
                let newVal = dom_codeHiddenInput.val() + $(this).text();
                let n = newVal.length;
                dom_codeHiddenInput.val(newVal);
                dom_codeViewSection.find("i:lt(" + n + ")").addClass("fill");

                if (n >= 6) {
                    loginEvent(newVal, function (result) {
                        if (result == "INIT") {
                            window.location.href = "/" + $("#hidISMVPath").val() + "/Setting/Network";
                        } else {
                            window.location.href = "/" + $("#hidISMVPath").val() + "/Setting/Overview";
                        }
                    }, function (isCustomError, errorMsg) {
                        // Error code
                        dom_title.text(isCustomError ? resources.getValue(errorMsg) : TEXT_DESC_TRYAGAIN);
                        dom_codeViewSection.addClass("shake").one("animationend", function () {
                            $(this).removeClass("shake");
                            dom_codeViewSection.find("i").removeClass("fill");
                        });
                        dom_codeHiddenInput.val("");
                        // Change button to cancel
                        changeToCancel();
                    });
                }
            }
        });
    };  

    var loginEvent = function (password, callBackSuc, callBackFailed) {
        maskHelper.blockUI();

        var data = {
            password: password,
            localOffsetHours: localOffsetHours
        };

        callRoute_Normal("/Home/UserLogin", "POST", data, function (result) {
            maskHelper.unblockUI();
            if (result.toString() == "INIT" || result.toString() == "COMPLETED") {                
                callBackSuc(result.toString());
            }
            else {
                callBackFailed(true, result);
            }
        }, function (errorMsg) {
            maskHelper.unblockUI();
            callBackFailed(false, errorMsg);
        });
    };

    var changeToCancel = function () {
        dom_keyboardSpeButton.addClass("cancel").text(TEXT_BUTTON_CANCEL);
    };

    var changeToClear = function () {
        dom_keyboardSpeButton.removeClass("cancel").text(TEXT_BUTTON_DELETE);
    };

    return {
        init: function () {
            registerEvents();
            initResources();
        }
    }
}();