jQuery.validator.addMethod("dangerChars1", function (value, element) {
    $.validator.messages.dangerChars1 = resources.getValue("Validation_DangerChars");
    return this.optional(element) || !(/[`<>"]/im.test(value));
});

jQuery.validator.addMethod("fixlength", function (value, element, param) {
    $.validator.messages.fixlength = resources.getValue("Validation_FixLength");
    var length = Array.isArray(value) ? value.length : this.getLength(value, element);
    return this.optional(element) || length == param;
});

jQuery.validator.addMethod("letterandnumbers", function (value, element) {
    $.validator.messages.letterandnumbers = resources.getValue("Validation_LetterAndNumbers");
    return this.optional(element) || !(/[^\w]/g.test(value));
});

jQuery.validator.addMethod('IP4Checker', function (value) {
    $.validator.messages.IP4Checker = resources.getValue("InvalidResolution_Desc_IP");
    return value.match(/^(?:(?:25[0-5]|2[0-4]\d|1?\d{1,2})(?:\.(?!$)|$)){4}$/);
});

jQuery.validator.addMethod('subnetMaskChecker', function (value) {
    $.validator.messages.subnetMaskChecker = resources.getValue("InvalidResolution_Desc_SubnetMask");
    return value.match(/^(255)\.(0|128|192|224|240|248|252|254|255)\.(0|128|192|224|240|248|252|254|255)\.(0|128|192|224|240|248|252|254|255)/);
});