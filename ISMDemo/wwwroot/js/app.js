var clickEvent = "click";
var timerInterval = 1000;
var localOffsetHours = null;
var mqtt_client = null;
var defaultDateFormat = "YYYY-MM-DD";
var serverConfigConsts = null;
var hasNetworkCheckedDone = false;
var hasSignOut = false;
var lastEthernetMsg = null;
var lastWifiMsg = null;

//Gobal switch button events
$(document).delegate(".switch", clickEvent, function () {
    var obj = $(this).find('[type="checkbox"]:not(":disabled")');
    if (obj.is(":checked")) {
        obj.prop("checked", false);
    }
    else {
        obj.prop("checked", true);
    }
});

//Global modal events
$(document).delegate("[data-toggle='open-modal']", clickEvent, function () {
    var dataID = $(this).attr("data-target");
    var $this = $('[data-id="' + dataID + '"]');
    newModalAction.openModal($this);
});
$(document).on("click", '[data-toggle="close-modal"]', function () {
    var $this = $(this);
    newModalAction.closeModal($this);
});

// Common Dropdown Function
function dropDown() {
    $(document).on("click", '[data-toggle="open-dropdown-menu"]', function () {
        $(this).siblings(".dropdown-menu").fadeIn(100, function () {
            $(this).parent().append('<div class="dropdown-mask"></div>');
            if ($(window).height() - $(this)[0].getBoundingClientRect().top < $(this)[0].scrollHeight) {
                $(this).addClass("dropdown-above");
            }
        });
    });
    $(document).on("click", '[data-toggle="close-dropdown-menu"]', function () {
        $(this).fadeOut(100);
        $(this).siblings(".dropdown-mask").detach();
        $(this).parent().find(".dropdown-above").removeClass("dropdown-above");
    });
    $(document).on("click", ".dropdown-mask", function () {
        $(this).siblings(".dropdown-menu").fadeOut(100);
        $(this).parent().find(".dropdown-above").removeClass("dropdown-above");
        $(this).detach();
    });
};

$(document).delegate("[data-link]", clickEvent, function () {
    var url = $(this).data("link");
    if (url.length !== 0) {
        window.location.href = url;
    }
});

// Check Label
function checkLabel() {
    $(".form-group .form-line").each(function () {
        if ($(this).val().length != 0) {
            $(this).parent().addClass("mini-label");
        }
        else {
            $(this).parent().removeClass("mini-label");
        }
    });

    $(".select-toggle .form-line").each(function () {
        if ($(this).val().length != 0 || $(this).text().length != 0) {
            $(this).parents(".form-group").addClass("mini-label");
        }
        else {
            $(this).parents(".form-group").removeClass("mini-label");
        }
    });
};

//Language events
$(document).delegate("[data-toggle='select-language']", clickEvent, function () {
    $(this).parent().addClass("open");
});
$(document).delegate("#selectLanguage li", clickEvent, function () {
    var selectedLang = $(this).data("target");
    $("#hidSelectedCulture").val(selectedLang);
    $("#selectLanguage").submit();
});
$(document).delegate(".header-action .popover", clickEvent, function () {
    $(this).parent().removeClass("open");
});

// Exit system settings, back to Chute Side Web App
$(document).delegate("[data-toggle='exit-settings']", clickEvent, function () {
    var isIframeEmbedded = window.self != window.top ? true : false;

    if (isIframeEmbedded) {
        callRoute_Normal("/Home/SignOut", "POST", null, function (result) {
        }, function (errorMsg) {
        });
        parent.postMessage("HideSystemSettings", "*");
    } else {
        window.location.href = "/" + $("#hidISMVPath").val() + "/Home/SignOutForInit";
    }
});

// Hide System Settings, back to Chute Side Web App
var isIframeEmbedded = window.self != window.top ? true : false;
if (isIframeEmbedded) {
    let StartY;
    $(document).on("touchstart", "body", function (e) {
        StartY = e.touches[0].pageY;
    });
    $(document).on("touchmove", "body", function (e) {
        const EndY = e.touches[0].pageY;
        const moveY = EndY - StartY;
        if (moveY < -30 && !hasSignOut) {
            hasSignOut = true;
            setTimeout(function () {
                // If the current page is Dashboard, doesn't need to sign out
                if (window.location.pathname.toLowerCase().indexOf("/dashboard/") < 0) {
                    callRoute_Normal("/Home/SignOut", "POST", null, function (result) {
                    }, function (errorMsg) {
                        hasSignOut = false;
                    });
                }
                parent.postMessage("HideSystemSettings", "*");
            }, 200);
        }
    });
}
// Show panel help
$(document).on("click", ".btn-help", function () {
    $(".panel-help").addClass("open");
});

// Close panel
$(document).on("click", "[data-toggle='close-panel']", function () {
    $(this).parents(".panel").removeClass("open");
});
$(document).on("click", "body", function (e) {
    if (!$(e.target).closest(".panel, .btn-help").length) {
        $(".panel").removeClass("open");
    }
});

// Toggle question
$(document).on("click", ".top-question-list li", function () {
    $(this).toggleClass("unfold").siblings().removeClass("unfold");
});

function chk_userlanguage() {
    //if (navigator.userLanguage) {
    //    baseLang = navigator.userLanguage.toLowerCase();
    //} else {
    //    baseLang = navigator.language.toLowerCase();
    //}
    var baseLang = moment.locale();

    //if (baseLang == "zh-cn") {
    //    defaultDateFormat = "YYYY/MM/DD";
    //}
    //else if (baseLang == "en-us" || baseLang == "en") {
    //    defaultDateFormat = "MM/DD/YYYY";
    //}
    //else {
    //    defaultDateFormat = "DD/MM/YYYY";
    //}
    defaultDateFormat = "MM/DD/YYYY";
}

// Check access device and its width
function checkDevice() {

    var eleStyle = document.createElement("style");
    document.querySelector("head").appendChild(eleStyle);
    eleStyle.innerHTML = [
        '.alert-tips {',
        'position: fixed;',
        'z-index: 10070;',
        'width: 100%;',
        'background-color: rgba(0,0,0,.6);',
        'top: -134px;',
        'left: 0;',
        'text-align: center;',
        'transition: all .3s ease;',
        '}',
        '.alert-tips-content {',
        'display: inline-block;',
        'padding: 30px 40px;',
        'color: #fff;',
        'font-size: 16px;',
        '}',
        '.autoShow {',
        'transform: translateY(134px);',
        '}',
        'autoHide {',
        'transform: translateY(-134px);',
        '}',
        '#btnYes, #btnNo {',
        'display: inline-block;',
        'margin: 5px;',
        'padding: 5px 20px;',
        'margin-bottom: 0;',
        'font-weight: 400;',
        'font-size: 14px;',
        'line-height: 1.42857143;',
        'text-align: center;',
        'white-space: nowrap;',
        'vertical-align: middle;',
        'cursor: pointer;',
        'border: 0;',
        'border-radius: 2px;',
        'background-image:  none;',
        'background-color: #e6e6e6;',
        '}',
        '#btnNo {',
        'background-color: #dd5f5f;',
        'color: #fff;',
        '}'
    ].join(" ");

    $(document).on("click", "#btnYes", function () {
        $(".alert-tips").addClass("autoHide");
        $(".alert-tips").fadeOut(800, function () {
            $(this).detach();
        });
        $("body").removeAttr("data-paint");
    });
    $(document).on("click", "#btnNo", function () {
        document.body.innerHTML = "";
    });

    // Show alert tips
    function showAlertTips(alertHtml, autoHide) {
        $("body").append(alertHtml);
        setTimeout(function () {
            $(".alert-tips").addClass("autoShow");
        }, 200);
        if (!autoHide) {
            clearTimeout(autoHideAlertTips);
        }
    }

    // Hide alert tips
    function hideAlertTips() {
        $(".alert-tips").addClass("autoHide");
        $(".alert-tips").fadeOut(800, function () {
            $(this).detach();
        });
    }

    var autoHideAlertTips;
    var SMALL_SIZE = 1280;
    function checkAlertTips() {
        // Auto hide alert tips after 10s
        autoHideAlertTips = setTimeout(function () {
            hideAlertTips();
        }, 10000);

        var isMobile = !!navigator.userAgent.match(/mobile/i);
        var isSmallScreen = window.screen.width < SMALL_SIZE;
        var isSmallViewport = document.body.clientWidth !== 0 && document.body.clientWidth < (SMALL_SIZE - 1);
        var isShowing = !!$(".alert-tips").length;

        var smallScreenHTML = [
            '<div class="alert-tips">',
            '<div class="alert-tips-content">',
            '<p>' + resources.getValue("InvalidResolution_Desc_1").replace("{0}", SMALL_SIZE.toString()) + '</p>',
            '<button id="btnYes">' + resources.getValue("InvalidResolution_Button_OK") + '</button>',
            /*'<button id="btnNo">' + resources.getValue("InvalidResolution_Button_Close") + '</button>',*/
            '</div>',
            '</div>'
        ].join("");
        var smallViewportHTML = [
            '<div class="alert-tips">',
            '<div class="alert-tips-content">' + resources.getValue("InvalidResolution_Desc_2").replace("{0}", SMALL_SIZE.toString()) + '</div>',
            '</div>'
        ].join("");

        if (isMobile || isSmallScreen || isSmallViewport) {
            var alertHtml;
            var autoHide = false;

            if (isSmallScreen) {
                alertHtml = smallScreenHTML;
            } else {
                if (isSmallViewport) {
                    alertHtml = smallViewportHTML;
                    autoHide = true;
                }
            }

            if (!isShowing) {
                showAlertTips(alertHtml, autoHide);
            }
        } else {
            hideAlertTips();
        }

        var lazyCheckDevice;
        $(window).one("resize", function () {
            clearTimeout(autoHideAlertTips);
            clearTimeout(lazyCheckDevice);
            lazyCheckDevice = setTimeout(function () {
                checkAlertTips();
            }, 100);
        });
    }
    checkAlertTips();
};

// Show top page
function showTopPage() {
    $("[data-page-level]").each(function () {
        if ($(this).data("page-level") === 1) {
            $(this).removeClass("hide");
        }
        else {
            $(this).addClass("hide");
        }
    });
}

// Show parent page
function showParentPage(currentLevel, targetID) {
    const parentLevel = currentLevel - 1;
    $("[data-page-level]").each(function () {
        if ($(this).data("page-level") === parentLevel) {
            $(this).removeClass("hide");
            if ((!!$(this).data("id") && targetID) && $(this).data("id") !== targetID) {
                $(this).addClass("hide");
            }
        }
        else {
            $(this).addClass("hide");
        }
    });
};

// Show sub page
function showSubPage(currentLevel, targetID) {
    const nextLevel = currentLevel + 1;
    $("[data-page-level]").each(function () {
        if ($(this).data("page-level") === nextLevel) {
            $(this).removeClass("hide");
            if ((!!$(this).data("id") && targetID) && $(this).data("id") !== targetID) {
                $(this).addClass("hide");
            }
        }
        else {
            $(this).addClass("hide");
        }
    });
};

// Scroll to the specified position
function scrollToTarget(ele) {
    var n = $(".main-body").offset().top;
    var m = ele.offset().top;
    var l = $(".main-body").scrollTop();
    var s = l - (n - m);
    s = s < l ? s : l;
    $(".main-body").animate({ scrollTop: s }, 200);
}

function writeActionLog(logMsg) {
    var data = {
        logMessage: logMsg
    };
    var currentUrlPath = window.location.pathname.toLowerCase();
    if (logMsg.indexOf("MQTTADHOCLOG:") >= 0
        && (currentUrlPath.indexOf("/dashboard") < 0 && currentUrlPath.indexOf("/overview") < 0)) {
        logMsg = logMsg.replace("MQTTADHOCLOG:", "");
        data.logMessage = logMsg;
        callRoute_Normal("/Home/WriteISMActionLog", "POST", data, function (result) { }, function (errorMsg) { });
    }
    console.log(new Date().toLocaleTimeString() + "; " + logMsg);
}

function writeUserLog(logMsg) {
    var data = {
        logMessage: logMsg
    };
    callRoute_Normal("/Home/WriteISMActionLog", "POST", data, function (result) { }, function (errorMsg) { });
    console.log(new Date().toLocaleTimeString() + "; " + logMsg);
}

function writeEthernetStatusLog(logMsg) {
    if (lastEthernetMsg != logMsg) {
        lastEthernetMsg = logMsg;
        writeUserLog(logMsg);
    }
}

function writeWifiStatusLog(logMsg) {
    if (lastWifiMsg != logMsg) {
        lastWifiMsg = logMsg;
        writeUserLog(logMsg);
    }
}

function getDateSecondsDiff(lastDate) {
    return Math.floor((new Date() - lastDate) / 1000);    
}

$(function () {
    checkDevice();

    if (!isNullObject($("#hidServerConfigConsts")) && !isNullOrEmpty($("#hidServerConfigConsts").val())) {
        serverConfigConsts = JSON.parse($("#hidServerConfigConsts").val());
    }

    localOffsetHours = -1 * (moment().zone() / 60);
    chk_userlanguage();

    // Listen the network status of init
    if (!$("#headerInit").hasClass("hide")) {
        if ($("#step-network").hasClass("is-active")
            || $("#step-camera").hasClass("is-active")
            || $("#step-scale").hasClass("is-active")
            || $("#step-EID").hasClass("is-active")
            || $("#step-done").hasClass("is-active")) {
            var mqtt_clientForNetwork = MqttClient.init();

            var isEthernetChecked = false;
            var isWifiChecked = false;
            var isEthernetConnected = false;
            var isWifiConnected = false;
            var isDonePage = $("#step-done").hasClass("is-active") ? true : false;

            if (isDonePage) {
                var maxTimeoutSeconds = 2;

                // If it can't receive the network status after the max seconds, will treat the network as not connected
                window.setTimeout(function () {
                    if (!$("#step-network:visible").hasClass("is-done")) {
                        $("main").removeClass("loading2");
                        // There are some not completed steps, show not comopleted
                        $(".network-not-completed").removeClass("hide");
                        $(".settings-completed").addClass("hide");
                        $(".settings-not-completed").removeClass("hide");
                    }
                }, maxTimeoutSeconds * 1000);
            }

            mqtt_clientForNetwork.on("message", (topic, message, packet) => {
                if (topic == "ISMDeviceStatus") {
                    let data = JSON.parse(message);
                    if (!isNullOrEmpty(data.statusType) && data.statusType == "ethernet") {
                        isEthernetChecked = true;
                        isEthernetConnected = data.isConnected && data.internetAccessible;
                    }

                    if (!isNullOrEmpty(data.statusType) && data.statusType == "wireless") {
                        isWifiChecked = true;
                        isWifiConnected = data.isConnected && data.internetAccessible;
                    }
                }

                if (isEthernetConnected || isWifiConnected) {
                    $("#step-network:visible").addClass("is-done");
                } else {
                    $("#step-network:visible").removeClass("is-done");
                }

                // If current page is Done page, check the network status
                if (isDonePage && (isEthernetConnected || isWifiConnected || (isEthernetChecked && isWifiChecked))) {
                    $("main").removeClass("loading2");
                    if ($("#step-network:visible").hasClass("is-done")) {
                        $(".network-not-completed").addClass("hide");
                    }
                    if ($("#step-network:visible").hasClass("is-done")
                        && $("#step-camera:visible").hasClass("is-done")
                        && $("#step-scale:visible").hasClass("is-done")
                        && $("#step-EID:visible").hasClass("is-done")) {
                        // If all setting steps are completed, show done completed
                        $("#step-done:visible").addClass("is-done")
                        $(".settings-not-completed").addClass("hide");
                        $(".settings-completed").removeClass("hide");
                    } else {
                        // There are some not completed steps, show not comopleted
                        if (!$("#step-network:visible").hasClass("is-done")) {
                            $(".network-not-completed").removeClass("hide");
                        }
                        $(".settings-completed").addClass("hide");
                        $(".settings-not-completed").removeClass("hide");
                    }
                }
            });
        }
    }
});