function CmsListViewModel() {
    "use strict";
    var self = this;

    self.searchText = ko.observable(document.getElementById("search-text").value).extend({ rateLimit: { timeout: 500, method: "notifyWhenChangesStop" } });

    self.searchText.subscribe(function() {
        doSearch();
    });
    

    function navigate(sortColumn, direction, search) {
        var query = "?";
        if (search) {
            query += "search=" + search + "&";
        }
        if (sortColumn) {
            query += "sortColumn=" + sortColumn + "&";
        }
        if (sortColumn) {
            query += "direction=" + direction + "&";
        }
        query = query.slice(0, -1);

        var newUrl = window.location.href.split('?')[0] + query;
        window.location = newUrl;
    }
    function doSearch() {
        var sortElement = document.querySelector("[data-column-sort='Ascending']") ||
            document.querySelector("[data-column-sort='Descending']");

        if (sortElement) {
            navigate(sortElement.getAttribute("data-column-index"),
                sortElement.getAttribute("data-column-sort"),
                document.getElementById("search-text").value);
        } else {
            navigate(null, null, document.getElementById("search-text").value);
        }
    }


    document.addEventListener('click',
        function (event) {
            //Delete modal hookup
            if (event.target.matches('[data-target="#deleteModal"]') ||
                event.target.parentElement.matches('[data-target="#deleteModal"]')) {
                var id = event.target.getAttribute("data-id") || event.target.parentElement.getAttribute("data-id");
                var title = event.target.getAttribute("data-title") || event.target.parentElement.getAttribute("data-title");

                document.getElementById("item-title").innerText = title;

                var removeForm = document.getElementById("remove-form");
                removeForm.action = removeForm.getAttribute("data-action") + id;
            }
            //Sort order hookup
            else if (event.target.matches('[data-sort]')) {
                var index = event.target.getAttribute("data-column-index");
                var direction = event.target.getAttribute("data-column-sort");

                if (direction === "Descending" || direction === "Unsorted") {
                    direction = "Ascending";
                } else {
                    direction = "Descending";
                }

                navigate(index, direction, document.getElementById("search-text").value);
            }
            //Search button hookup
            else if (event.target.matches('#input-search') ||
                event.target.parentElement.matches('#input-search')) {
                doSearch();
            }
        },
        false);
}