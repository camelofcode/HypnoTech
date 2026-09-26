/*jslint browser: true, vars: true*/
/*global define, module*/
(function (root, factory) {
    'use strict';

    if (typeof define === 'function' && define.amd) {
        define([], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory();
    } else {
        root.messages = factory();
    }
}(this, function () {
    'use strict';

    function addMessage(message, alertClass) {
        var alertDiv = document.createElement("div");
        alertDiv.setAttribute("class", "alert " + alertClass);
        alertDiv.innerText = message;

        var main = document.querySelector("main");
        main.parentNode.insertBefore(alertDiv, main);

        setTimeout(function () {
            $(alertDiv).fadeTo(500, 500).slideUp(500, function() {
                alertDiv.parentNode.removeChild(alertDiv);
            });
        }, 3000);
    }

    return {
        confirmation: function (message) {
            addMessage(message, "alert-success");
        },
        danger: function (message) {
            addMessage(message, "alert-danger");
        }
    };
}));