var lastClickTime = null;
var expiredTimer = null;
var expiredTimerInterval = null;
var expiredDuration = null;

$(function () {
    expiredTimerInterval = parseInt(serverConfigConsts.ExpiredCheckingInterval) * 1000;
    expiredDuration = parseInt(serverConfigConsts.ExpiredDuration);
    lastClickTime = moment();

    if (expiredTimer != null) {
        window.clearInterval(expiredTimer);
        expiredTimer = null;
    }

    // Monitors the time when the page was last operated
    $(document).on('click dblclick keydown mousemove touchmove focus blur change paste', function (event) {
        lastClickTime = moment();
    });

    expiredTimer = window.setInterval(function () {
        var span = moment().diff(lastClickTime, "minutes");        

        if (span > expiredDuration) {
            var isIframeEmbedded = window.self != window.top ? true : false;
            var isInit = !isIframeEmbedded; // If the page is not embedded, the ism will be initialized.

            if (isInit) { // Init
                maskHelper.blockUI();
                window.location.href = "/" + $("#hidISMVPath").val() + "/Home/SignOutForInit";
            }
            else { // In the Embedded iframe of chute side web app
                callRoute_Normal("/Home/SignOut", "POST", null, function (result) {
                }, function (errorMsg) {
                });
                parent.postMessage("HideSystemSettings", "*");
            }
        }
    }, expiredTimerInterval);
});