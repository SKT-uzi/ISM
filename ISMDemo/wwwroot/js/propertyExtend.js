//# sourceURL=utility/propertyExtend.js
function isNullOrEmpty(value) {
    if (value == null)
        return true;

    if (value == undefined)
        return true;

    if (value == "undefined")
        return true;

    if ($.trim(value.toString()) == '')
        return true;

    return false;
}

function isNullObject(value) {
    if (value == null) {
        return true;
    }

    if (value == undefined) {
        return true;
    }

    if (value == "undefined") {
        return true;
    }

    return false;
};

function isNotNumber(value) {
    if (!this.isNullOrEmpty(value)) {
        return !isFinite(value.toString());
    }
    else {
        return true;
    }
};

String.prototype.trim = function () {
    return this.replace(/(^\s*)|(\s*$)/g, "");
};

String.prototype.allTrim = function () {
    return this.replace(/ /g, "");
};

String.prototype.startWith = function (str) {
    var reg = new RegExp("^" + str);
    return reg.test(this);
};

String.prototype.endWith = function (str) {
    var reg = new RegExp(str + "$");
    return reg.test(this);
};

String.prototype.ReplaceAll = function (f, e) {
    var reg = new RegExp(f, "g");
    return this.replace(reg, e);
}

Array.prototype.contain = function (value) {
    for (var i = 0; i < this.length; i++) {
        if (this[i] == value || (!isNullOrEmpty(this[i]) && !isNullOrEmpty(value) && this[i].toString().toLowerCase() == value.toString().toLowerCase())) {
            return true;
        }
    }

    return false;
};

Array.prototype.containProperty = function (value, pName) {
    for (var i = 0; i < this.length; i++) {
        if (this[i][pName] == value || (!isNullOrEmpty(this[i][pName]) && !isNullOrEmpty(value) && this[i][pName].toString().toLowerCase() == value.toString().toLowerCase())) {
            return true;
        }
    }

    return false;
};

Array.prototype.getAt = function (value) {
    for (var i = 0; i < this.length; i++) {
        if (this[i] == value || (!isNullOrEmpty(this[i]) && !isNullOrEmpty(value) && this[i].toString().toLowerCase() == value.toString().toLowerCase())) {
            return i;
        }
    }

    return -1;
};

Array.prototype.getAtByProperty = function (findValue, findPName) {
    for (var i = 0; i < this.length; i++) {
        if (this[i][findPName] == findValue || (!isNullOrEmpty(this[i][findPName]) && !isNullOrEmpty(findValue) && this[i][findPName].toString().toLowerCase() == findValue.toString().toLowerCase())) {
            return i;
        }
    }

    return -1;
};

Array.prototype.removeAt = function (index) {
    for (var i = this.length - 1; i >= 0; i--) {
        if (i == index) {
            this.splice(i, 1);
            return true;
        }
    }

    return false;
};

Array.prototype.removeByAttr = function (value, pName) {
    for (var i = this.length - 1; i >= 0; i--) {
        if (this[i][pName] == value || (!isNullOrEmpty(this[i][pName]) && !isNullOrEmpty(value) && this[i][pName].toString().toLowerCase() == value.toString().toLowerCase())) {
            this.splice(i, 1);
            return true;
        }
    }

    return false;
};

Array.prototype.removeByValue = function (value) {
    for (var i = this.length - 1; i >= 0; i--) {
        if (this[i] == value || (!isNullOrEmpty(this[i]) && !isNullOrEmpty(value) && this[i].toString().toLowerCase() == value.toString().toLowerCase())) {
            this.splice(i, 1);
            return true;
        }
    }

    return false;
};

Array.prototype.getPropertyValue = function (findValue, findPName, getPName) {
    var returnValue;

    for (var i = 0; i < this.length; i++) {
        if (this[i][findPName] == findValue || (!isNullOrEmpty(this[i][findPName]) && !isNullOrEmpty(findValue) && this[i][findPName].toString().toLowerCase() == findValue.toString().toLowerCase())) {
            returnValue = this[i][getPName];
            break;
        }
    }

    return returnValue;
};

Array.prototype.getBy = function (findValue, findPName) {
    var returnItem;

    for (var i = 0; i < this.length; i++) {
        if (this[i][findPName] == findValue || (!isNullOrEmpty(this[i][findPName]) && !isNullOrEmpty(findValue) && this[i][findPName].toString().toLowerCase() == findValue.toString().toLowerCase())) {
            returnItem = this[i];
            break;
        }
    }

    return returnItem;
};

Array.prototype.getListBy = function (findValue, findPName) {
    var returnItemList = new Array();

    for (var i = 0; i < this.length; i++) {
        if (this[i][findPName] == findValue || (!isNullOrEmpty(this[i][findPName]) && !isNullOrEmpty(findValue) && this[i][findPName].toString().toLowerCase() == findValue.toString().toLowerCase())) {
            returnItemList.push(this[i]);
        }
    }

    return returnItemList;
};

Array.prototype.updatePropertyValue = function (findValue, findPName, updateValue, updatePName) {
    for (var i = 0; i < this.length; i++) {
        if (this[i][findPName] == findValue || (!isNullOrEmpty(this[i][findPName]) && !isNullOrEmpty(findValue) && this[i][findPName].toString().toLowerCase() == findValue.toString().toLowerCase())) {
            var tempObj = new Object();
            for (var p in this[i]) {
                tempObj[p] = this[i][p];
            }
            tempObj[updatePName] = updateValue;
            this[i] = tempObj;
            return true;
        }
    }

    return false;
};

Array.prototype.appendPropertyValue = function (findValue, findPName, appendValue, updatePName) {
    var splitChar = '/';
    for (var i = 0; i < this.length; i++) {
        if (this[i][findPName] == findValue || (!isNullOrEmpty(this[i][findPName]) && !isNullOrEmpty(findValue) && this[i][findPName].toString().toLowerCase() == findValue.toString().toLowerCase())) {
            var oldValue = this[i][updatePName];
            var oldValues = oldValue.split(splitChar);
            if (!oldValues.contain(appendValue)) {
                var tempObj = new Object();
                for (var p in this[i]) {
                    tempObj[p] = this[i][p];
                }
                tempObj[updatePName] = oldValue + splitChar + appendValue;

                this[i] = tempObj;
            }

            return true;
        }
    }

    return false;
};

Array.prototype.numberValueCompare = function (compareWith) {
    var result = true;

    if (this.length != compareWith.length) {
        result = false;
    }
    else {
        for (var i = 0; i < this.length; i++) {
            if (this[i] != compareWith[i]) {
                result = false;
                break;
            }
        }
    }

    return result;
};

Array.prototype.getMaxValue = function () {
    return Math.max.apply(Math, this);
};

Array.prototype.getMinValue = function () {
    return Math.min.apply(Math, this);
};