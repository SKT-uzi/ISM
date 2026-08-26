var maskHelper = {
    blockUI: function (message) {
        if ($("body > .blockMsg") != null) {
            $.unblockUI();
        }
        $.blockUI({
            css: {
                border: "none",
                padding: "15px",
                backgroundColor: "transparent",
                "-webkit-border-radius": "10px",
                "-moz-border-radius": "10px",
                opacity: .9,
                color: "#FFF",
                height: "100px",
                "line-height": "100px"
            },
            message: "<div class=\"maskTitle\"></div>"
        });
    },

    blockUINoGif: function () {
        if ($("body > .blockMsg") != null) {
            $.unblockUI();
        }
        $.blockUI({
            css: {
                border: "none",
                padding: "15px",
                backgroundColor: "transparent",
                "-webkit-border-radius": "10px",
                "-moz-border-radius": "10px",
                opacity: .9,
                color: "#FFF",
                height: "100px",
                "line-height": "100px"
            },
            message: "<div class=\"\"></div>"
        });
    },

    blockUIForDark: function () {
        if ($("body > .blockMsg") != null) {
            $.unblockUI();
        }
        $.blockUI({
            css: {
                border: "none",
                padding: "15px",
                backgroundColor: "transparent",
                "-webkit-border-radius": "10px",
                "-moz-border-radius": "10px",
                opacity: .9,
                color: "#FFF",
                height: "100px",
                "line-height": "100px"
            },
            message: "<div class=\"maskTitle-dark\"></div>"
        });
    },

    blockUIWithRadial: function () {
        if ($("body > .blockMsg") != null) {
            $.unblockUI();
        }
        $.blockUI({
            css: {
                border: "none",
                padding: "15px",
                backgroundColor: "transparent",
                "-webkit-border-radius": "10px",
                "-moz-border-radius": "10px",
                opacity: .9,
                color: "#FFF",
                height: "100px",
                "line-height": "100px"
            },
            message: "<div class=\"block-ui-radial\"></div>"
        });

        maskHelper.setRadialControl($(".block-ui-radial"), 0);
    },

    unblockUI: function () {
        window.setTimeout(function () {
            $.unblockUI();
        }, 500);
    },

    setNewMessage: function (message) {
        if ($("body > .blockMsg") != null) {
            $("body > .blockMsg .maskTitle").html(message);
        }
        else {
            this.blockUI(message);
        }
    },

    refreshProgressValue: function (value) {
        var html = "";
        html += "<div class=\"loadMask\">";
        html += "<div class=\"loadPercent\">" + formatHelper.toPercent(value, 1) + "&nbsp;%&nbsp;" + msgConfig.blockUI.completed + "</div>";
        html += "<div style=\"width: " + formatHelper.toPercent(value, 2) + "%;\" class=\"loadProgress\"></div>";
        html += "</div>";

        if (!commonHelper.isNullOrEmpty($("body > .blockMsg"))) {
            if (!commonHelper.isNullOrEmpty($("body > .blockMsg .loadMask"))) {
                $("body > .blockMsg .loadMask").remove();
            }

            $("body > .blockMsg").append(html);
        }
        else {
            this.blockUI(html);
        }
    },

    setRadialControl: function (sender, progressValue) {
        var radialObj = sender.data("radialIndicator");
        if (radialObj == null) {
            sender.radialIndicator({
                //barColor: "#" + appConfig.progrssColorList[Math.ceil(Math.random() * 7)],
                barColor: "#" + (sender.hasClass("get-data") ? "E30327" : "FF7F27"),
                radius: 60,
                barWidth: 15,
                initValue: progressValue * 100,
                roundCorner: true,
                percentage: true
            });
        }
        else {
            radialObj.value(progressValue * 100);
        }
    },

    initDotProgressBar: function (parentObj, dotCount) {
        //parentObj.attr("bgcolor", "#" + appConfig.progrssColorList[Math.ceil(Math.random() * 7)]);
        for (var i = 0; i < dotCount; i++) {
            parentObj.append($("<div class=\"dot\"></div>"));
        }
    },

    refreshDotProgressValue: function (parentObj, doneCount) {
        var dotObjList = parentObj.find(".dot");
        dotObjList.each(function (index, dom) {
            if (index < doneCount) {
                $(dom).addClass("completed");
                //$(dom).css("background-color", $(parentObj).attr("bgcolor"));
                //$(dom).css("box-shadow", "0px 0px 10px 1px rgba(240,216,217,0.7)");
            }
        });

        if (dotObjList.length == doneCount) {
            parentObj.closest(".section").addClass("completed");
            //parentObj.closest(".section").css("border", "3px solid " + $(parentObj).attr("bgcolor"));
            //parentObj.closest(".section").css("box-shadow", "0px 0px 10px 1px rgba(240,216,217,0.7)");
        }
    }
};