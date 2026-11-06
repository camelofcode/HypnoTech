//site.js
(function () {
    function getRoot() {
        if (window.location.port.length > 0) {
            return window.location.protocol + "//" + window.location.hostname + ":" + window.location.port + "/";
        } else {
            return window.location.protocol + "//" + window.location.hostname + "/";
        }
    }

    var root = getRoot();
    var useHash = false; // Defaults to: false
    var hash = '#'; // Defaults to: '#'
    var router = new Navigo(root, useHash, hash);

    var spaInitialised = false;

    function executePageScript(script) {
        if (script) {
            if (window[script] && window[script].execute) {
                window[script].execute();
            }
        }
    }

    function getPage(url) {
        if (!spaInitialised) {
            spaInitialised = true;
            return;
        }

        var request = new XMLHttpRequest();

        request.open('GET', url, true);
        request.setRequestHeader("X-Partial-Request", "");
        request.onload = function () {
            if (request.status >= 200 && request.status < 400 || request.status === 404) {
                // Success!
                var data = JSON.parse(request.responseText);
                window.scrollTo(0, 0);
                document.title = data.title + " - " + document.getElementById("site-name").value;
                document.querySelector('link[rel="canonical"]').setAttribute("href", data.canonicalUrl);
                document.querySelector('meta[name="Description"]').setAttribute("content", data.description);
                document.getElementById("content").innerHTML = data.html;
                router.updatePageLinks();
                executePageScript(data.script);
                events.trigger("page-loaded");
            } else {
                // We reached our target server, but it returned an error              
                window.location = url;

            }
        };

        request.onerror = function () {
            // There was a connection error of some sort
            window.location = url;
        };

        request.send();
    }

    function buildUrl(params, query) {
        var url = "";
        var i = 0;
        var pathId = "path" + 1;

        while (params[pathId]) {
            i += 1;
            pathId = "path" + i;
            if (params[pathId]) {
                url += "/" + params[pathId];
            }
        }

        if (query) {
            url += '?' + query;
        }
        if (url === "") {
            url = "/";
        }

        return url;
    }

    router
        .on({
            '/:path1': function (params, query) {
                getPage(buildUrl(params, query));
            },
            '/:path1/:path2': function (params, query) {
                getPage(buildUrl(params, query));
            },
            '/:path1/:path2/:path3': function (params, query) {
                getPage(buildUrl(params, query));
            },
            '/:path1/:path2/:path3/:path4': function (params, query) {
                getPage(buildUrl(params, query));
            },
            '/:path1/:path2/:path3/:path4/:path5': function (params, query) {
                getPage(buildUrl(params, query));
            },
            '*': function (params, query) {
                getPage(buildUrl([], query));
            }
        })
        .resolve(getRoot());

    $(document).ready(function () {
        //Execute page script on document load if exists
        var script = document.getElementById("page-script").value;
        executePageScript(script);
    });
}());