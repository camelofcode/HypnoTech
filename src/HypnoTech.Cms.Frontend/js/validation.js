/*jslint browser: true, vars: true*/
/*global define, module*/
(function (root, factory) {
    'use strict';

    if (typeof define === 'function' && define.amd) {
        define([], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory();
    } else {
        root.validation = factory();
    }
}(this, function () {
    'use strict';

    function getValidation(inputId) {
        var invalid = $("#" + inputId).parents(".form-group").first().find(".text-danger");
        if (invalid) {
            return invalid;
        }
        var valid = $("#" + inputId).parents(".form-group").first().find(".text-success");
        if (valid) {
            return valid;
        }
        return null;
    }

    function addValidation(inputId, cssClass, message) {
        removeValidation(inputId);
        var html = "<div class=\"" + cssClass + " py-2\"><small>" + message + "</small></div>";
        $("#" + inputId).parents(".form-group").first().append(html);
    }

    function removeValidation(inputId) {
        var validation = getValidation(inputId);
        if (validation) {
            validation.remove();
        }
    }

    return {
        valid: function (inputId, message) {
            addValidation(inputId, "text-success", message || "Looks Good!");
        },
        invalid: function (inputId, message) {
            addValidation(inputId, "text-danger", message || "Invalid!");
        },
        remove: function (inputId) {
            removeValidation(inputId);
        }
    };
}));