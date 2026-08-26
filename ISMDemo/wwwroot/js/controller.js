"use strict";

function switchView_Normal(route, method, data) {
    $.ajax({
        type: method,
        url: route,
        contentType: "application/x-www-form-urlencoded",
        data: data,
        async: true,
        crossDomain: false,
        cache: false,
        success: function (result) {
            console.log(result);
        },
        error: function (error) {
            switch (error.status) {
                case 0:
                    console.log(error);
                    break;
                case 401:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Unauthorized";
                    break;
                case 403:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/AccessDenied";
                    break;
                case 500:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Error";
                    break;
                default:
                    console.log(error);
            }
        }
    });
}

function renderPartialView_JSON(route, method, data, callBackSuc, callBackFail) {
    $.ajax({
        type: method,
        url: route,
        contentType: "application/json",
        data: data == null ? null : JSON.stringify(data),
        async: true,
        crossDomain: false,
        cache: false,
        success: function (result) {
            callBackSuc(result);
        },
        error: function (error) {
            switch (error.status) {
                case 0:
                    callBackFail(resources.getValue("Common_ErrorMsg_Offline"));
                    break;
                case 401:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Unauthorized";
                    break;
                case 403:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/AccessDenied";
                    break;
                case 500:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Error";
                    break;
                default:
                    var errorMsg = "";
                    if (isNullOrEmpty(error) || isNullOrEmpty(error.responseText)) {
                        errorMsg = resources.getValue("Common_ErrorMsg_Unknown");
                    }
                    else {
                        errorMsg = error.responseText;
                    }
                    callBackFail(errorMsg);
            }
        }
    });
}

function renderPartialView_Normal(route, method, data, callBackSuc, callBackFail) {
    $.ajax({
        type: method,
        url: route,
        contentType: "application/x-www-form-urlencoded",
        data: data,
        async: true,
        crossDomain: false,
        cache: false,
        success: function (result) {
            callBackSuc(result);
        },
        error: function (error) {
            switch (error.status) {
                case 0:
                    callBackFail(resources.getValue("Common_ErrorMsg_Offline"));
                    break;
                case 401:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Unauthorized";
                    break;
                case 403:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/AccessDenied";
                    break;
                case 500:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Error";
                    break;
                default:
                    var errorMsg = "";
                    if (isNullOrEmpty(error) || isNullOrEmpty(error.responseText)) {
                        errorMsg = resources.getValue("Common_ErrorMsg_Unknown");
                    }
                    else {
                        errorMsg = error.responseText;
                    }
                    callBackFail(errorMsg);
            }
        }
    });
}

function callRoute_JSON(route, method, data, callBackSuc, callBackFail) {
    $.ajax({
        type: method,
        url: route,
        contentType: "application/json",
        data: data == null ? null : JSON.stringify(data),
        async: true,
        crossDomain: false,
        cache: false,
        success: function (result) {
            if (callBackSuc != null) {
                callBackSuc(result);
            }
        },
        error: function (error, status, errorThrown) {
            switch (error.status) {
                case 0:
                    if (callBackFail != null) {
                        callBackFail(resources.getValue("Common_ErrorMsg_Offline"));
                    }
                    break;
                case 401:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Unauthorized";
                    break;
                case 403:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/AccessDenied";
                    break;
                case 500:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Error";
                    break;
                default:
                    if (callBackFail != null) {
                        var errorMsg = "";
                        if (isNullOrEmpty(error) || isNullOrEmpty(error.responseText)) {
                            errorMsg = resources.getValue("Common_ErrorMsg_Unknown");
                        }
                        else {
                            errorMsg = error.responseText;
                        }
                        callBackFail(errorMsg);
                    }
            }
        }
    });
}

function callRoute_Normal(route, method, data, callBackSuc, callBackFail, flag = true) {
    $.ajax({
        type: method,
        url: flag ? ("/" + $("#hidISMVPath").val() + route) : route,
        contentType: "application/x-www-form-urlencoded",
        data: data,
        async: true,
        crossDomain: false,
        cache: false,
        success: function (result) {
            if (callBackSuc != null) {
                callBackSuc(result);
            }
        },
        error: function (error) {
            switch (error.status) {
                case 0:
                    if (callBackFail != null) {
                        callBackFail(resources.getValue("Common_ErrorMsg_Offline"));
                    }
                    break;
                case 401:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Unauthorized";
                    break;
                case 403:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/AccessDenied";
                    break;
                case 500:
                    window.location.href = "/" + $("#hidISMVPath").val() + "/Home/Error";
                    break;
                default:
                    if (callBackFail != null) {
                        var errorMsg = "";
                        if (isNullOrEmpty(error) || isNullOrEmpty(error.responseText)) {
                            errorMsg = resources.getValue("Common_ErrorMsg_Unknown");
                        }
                        else {
                            errorMsg = error.responseText;
                        }
                        callBackFail(errorMsg);
                    }
            }
        }
    });
}