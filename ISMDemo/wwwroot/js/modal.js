"use strict";

// Common Get Data-Bind Value
function getDataBind(obj, property) {
	var x, y, z;
	if (obj.attr("data-bind") !== undefined) {
		y = obj.attr("data-bind").split(",");
	}
	else {
		return;
	}
	for (x in y) {
		z = y[x].split(":");
		if ($.trim(z[0]) === property) {
			return z[1];
		}
	}
}

window.newLoadingAction = {
	addLoading: function (obj) {
		obj.addClass("loading");
	},
	removeLoading: function (obj) {
		obj.removeClass("loading");
	}
};

window.newModalAction = {
	openModal: function (obj) {
		var $this = obj;
		var animationInCls = getDataBind($this, "animationIn");

		if (!$this.find(".modal-content").hasClass("open")) {
			// Not Compatible with IE<10, can replace with a setTimeout function
			$this.find(".modal-content").addClass(animationInCls).one("animationend", function () {
				$(this).removeClass(animationInCls);
			});
			$this.addClass("open");
		}
	},
	closeModal: function (obj) {
		var $this = obj;
		var $modal = $this.parents(".modal");
		if ((isNullOrEmpty($modal) || $modal.length == 0) && $this.hasClass("modal")) {
			$modal = $this;
        }

		if ($modal.hasClass("open")) {
			var animationOutCls = getDataBind($modal, "animationOut");
			// Not Compatible with IE<10, can replace with a setTimeout function
			$modal.addClass(animationOutCls).one("animationend", function () {
				$(this).removeClass("open");
				$(this).removeClass(animationOutCls);
			});
		}
	},
	initModal: function (options) {
		var $this = options.obj;
		var modalTitle = options.title;
		var modalContent = options.content;
		var modalPrimaryAction = options.primaryAction;
		var modalSecondaryAction = options.secondaryAction;
		if (modalTitle !== undefined) {
			$this.find(".modal-title").text(modalTitle);
		}
		if (modalContent !== undefined) {
			$this.find(".modal-body p").text(modalContent);
		}
		if (modalPrimaryAction !== undefined) {
			$this.find(".btn-primary").text(modalPrimaryAction);
		}
		if (modalSecondaryAction !== undefined) {
			$this.find(".btn-secondary").removeClass("hide").text(modalSecondaryAction);
		}
	},
	closeAllModals: function () {
		var modals = $(".modal.open");
		if (modals != null && modals.length > 0) {
			for (var i = 0; i < modals.length; i++) {
				var tempModal = $(modals[i]);

				var animationOutCls = getDataBind(tempModal, "animationOut");

				// Not Compatible with IE<10, can replace with a setTimeout function
				tempModal.addClass(animationOutCls).one("animationend", function () {
					$(this).removeClass("open");
					$(this).removeClass(animationOutCls);
				});
			}
        }
    }
};

function openErrorMsgModal(content) {
	newModalAction.closeAllModals();
	$(".modal-message-warn .modal-pic icon").removeClass("icon-warn-l").removeClass("icon-success-l").addClass("icon-warn-l");
	$(".modal-message-warn .modal-message-tit").html(resources.getValue("Common_MsgBoxTitle_Error"));
	$(".modal-message-warn p").html(content);
	newModalAction.openModal($(".modal-message-warn"));
}

function openReminderMsgModal(content) {
	newModalAction.closeAllModals();
	$(".modal-message-warn .modal-pic icon").removeClass("icon-warn-l").removeClass("icon-success-l").addClass("icon-warn-l");
	$(".modal-message-warn .modal-message-tit").html(resources.getValue("Common_MsgBoxTitle_Reminder"));
	$(".modal-message-warn p").html(content);
	newModalAction.openModal($(".modal-message-warn"));
}

function openSuccessMsgModal(content) {
	newModalAction.closeAllModals();
	$(".modal-message-warn .modal-pic icon").removeClass("icon-warn-l").removeClass("icon-success-l").addClass("icon-success-l");
	$(".modal-message-warn .modal-message-tit").html(resources.getValue("Common_MsgBoxTitle_Suc"));
	$(".modal-message-warn p").html(content);
	newModalAction.openModal($(".modal-message-warn"));
}

function openCustomMsgModal(title, content, iconCls) {
	newModalAction.closeAllModals();
	$(".modal-message-warn .modal-pic icon").removeClass("icon-warn-l").removeClass("icon-success-l").addClass(iconCls);
	$(".modal-message-warn .modal-message-tit").html(title);
	$(".modal-message-warn p").html(content);
	newModalAction.openModal($(".modal-message-warn"));
}