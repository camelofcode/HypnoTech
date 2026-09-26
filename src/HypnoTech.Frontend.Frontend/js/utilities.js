(function (root, factory) {
    'use strict';

    if (typeof define === 'function' && define.amd) {
        define([], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory();
    } else {
        root.utilities = factory();
    }
}(this, function () {
    'use strict';

    var checkIfObjectExists = function (obj) {
        return obj !== undefined && obj !== null && (Array.isArray(obj) || typeof obj === 'function' || obj.length === undefined || obj.length > 0);
    };

    return {
        isEmptyOrSpaces: function (str) {
            return str === null || str.match(/^ *$/) !== null;
        },
        exists: function (obj) {
            return checkIfObjectExists(obj);
        },
        formatCurrency: function (value, currencySymbol) {
            currencySymbol = currencySymbol || '£';
            return currencySymbol + value.toFixed(2);
        },
        getDuration: function (start, end) {
            if (checkIfObjectExists(start) && checkIfObjectExists(end)) {
                if (start.indexOf(":") === 2 && end.indexOf(":")) {
                    var startParts = start.split(":");
                    var endParts = end.split(":");
                    var startMinutes = parseInt(startParts[0]) * 60 + parseInt(startParts[1]);
                    var endMinutes = parseInt(endParts[0]) * 60 + parseInt(endParts[1]);

                    return endMinutes - startMinutes;
                }
            }
            return 0;
        },
        formatDateForMvcParser: function (date) {
            if (checkIfObjectExists(date) && date.split) {
                //Assumes date in standard datepicker format: DD/MM/YYYY - Outputs YYYY-MM-DD
                var parts = date.split("/");
                return parts[2] + "-" + parts[1] + "-" + parts[0];
            }
            return null;
        }
    };
}));