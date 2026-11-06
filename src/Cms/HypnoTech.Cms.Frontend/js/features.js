function NavBarLinkViewModel(data) {
    "use strict";
    var self = this;

    var original = "";
    self.editing = ko.observable(false);

    self.id = ko.observable(data.id);
    self.key = ko.observable(data.key);
    self.text = ko.observable(data.text);
    self.order = ko.observable(data.order);
    self.enabled = ko.observable(data.enabled);

    self.edit = function () {
        original = self.text();
        self.editing(true);
    };

    self.cancelEdit = function () {
        self.text(original);
        original = "";
        self.editing(false);
    };

    self.saveEdit = function () {
        if (self.text() === "") {
            self.text(original);
            original = "";
        }
        self.editing(false);
    };

    self.getJson = function() {
        return {
            id: self.id(),
            key: self.key(),
            text: self.text(),
            order: self.order(),
            enabled: self.enabled()
        };
    };
}

function FeaturesViewModel(links) {
    "use strict";
    var self = this;

    self.links = ko.observableArray([]);

    self.orderedLinks = ko.pureComputed(function() {
        return _.sortBy(self.links(), function (item) {
            return item.order();
        });
    });

    for (var i = 0; i < links.length; i++) {
        self.links.push(new NavBarLinkViewModel(links[i]));
    }

    self.discard = function () {
        window.location.reload();
    };

    self.saveChanges = function () {
        var featuresData = {
            links: [],
            __RequestVerificationToken: document.getElementsByName("__RequestVerificationToken")[0].value
        };

        for (var i = 0; i < self.orderedLinks().length; i++) {
            featuresData.links.push(self.orderedLinks()[i].getJson());
        }

        ajax.post("/Settings/Features/Update", featuresData).done(function() {
            window.location.reload();
        });
    };

    self.canMoveUp = function (item) {
        return item && item.order() > 0;
    };

    self.canMoveDown = function (item) {
        return item && item.order() < self.links().length - 1;
    };

    function findItemAtPositon(position) {
        for (var i = 0; i < self.links().length; i++) {
            if (self.links()[i].order() === position) {
                return self.links()[i];
            }
        }
        return undefined;
    }

    function moveToPosition(itemToMove, position) {
        var itemToSwap = findItemAtPositon(position);
        itemToSwap.order(itemToMove.order());
        itemToMove.order(position);

        self.links.sort(function(i1, i2) {
            if (i1 > i2) {
                return 1;
            } else {
                return -1;
            }
        });
    }

    self.moveUp = function(item) {
        if (self.canMoveUp(item)) {
            moveToPosition(item, item.order() - 1);
        }
    };

    self.moveDown = function (item) {
        if (self.canMoveDown(item)) {
            moveToPosition(item, item.order() + 1);
        }
    };


}