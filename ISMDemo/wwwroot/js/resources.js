var resources = {
    requestCultureName: null,
    localizedStrings: null,

    getValue: function (name) {
        var returnValue = "";
        for (var i = 0; i < this.localizedStrings.length; i++) {
            if (this.localizedStrings[i].Name == name) {
                returnValue = this.localizedStrings[i].Value;
                break;
            }
        }

        return returnValue;
    },

    getEnumNameByKey: function (enumName, key) {
        var returnValue = "";

        var enumStr = this.getValue(enumName);
        if (!isNullOrEmpty(enumStr)) {
            var enumObj = JSON.parse(enumStr);
            $.each(enumObj, function (k, v) {
                if (key.toString() == k.toString()) {
                    returnValue = v;
                    return false;
                }                
            });
        }

        return returnValue;
    }
};