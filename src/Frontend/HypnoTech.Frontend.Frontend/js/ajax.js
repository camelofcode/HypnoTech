/*jslint browser: true, vars: true*/
/*global define, module*/
(function (root, factory) {
    'use strict';

    if (typeof define === 'function' && define.amd) {
        define([], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory();
    } else {
        root.ajax = factory();
    }
}(this, function () {
    'use strict';

    function addAntiForgeryToken(data) {
        var token = document.getElementsByName("__RequestVerificationToken")[0];
        if (token) {
            data.__RequestVerificationToken = token.value;
        }
    }

    function doneHandler(response) {
        if (response != undefined) {
            if (response.wasdViolated) {
                window.location = response.url;
            }
        }
    }

    function failHander(response) {
        if (!["abort", "Bad Request"].includes(response.statusText)) {
            window.location = "/Home/Error";
        }
    }

    function addTimestampToUrl(url) {
        var timestamp = new Date().getTime();
        if (url.indexOf("?") > -1) {
            url += "&_=" + timestamp;
        } else {
            url += "?_=" + timestamp;
        }

        return url;
    }

    return {
        get: function (url, data) {
            url = addTimestampToUrl(url);
            var xhr = $.get(url, data);

            xhr.done(doneHandler).fail(failHander);
            return xhr;
        },
        post: function (url, data) {
            url = addTimestampToUrl(url);
            data = data || {};

            addAntiForgeryToken(data);
            var xhr = $.post(url, data);

            xhr.done(doneHandler).fail(failHander);
            return xhr;
        },
        postWithFiles: function (url, files, data) {
            url = addTimestampToUrl(url);
            var formData = new FormData();

            for (var i = 0; i < files.length; i++) {
                formData.append("files", files[i]);
            }
            formData.append("data", JSON.stringify(data));
            var antiForgeryData = {};
            addAntiForgeryToken(antiForgeryData);
            if (antiForgeryData.__RequestVerificationToken) {
                formData.append("__RequestVerificationToken", antiForgeryData.__RequestVerificationToken);
            }

            var options = {
                url: url,
                data: formData,
                type: "POST",
                contentType: false,
                processData: false
            };

            var xhr = $.ajax(options);
            xhr.done(doneHandler).fail(failHander);

            return xhr;
        }
    };
}));
