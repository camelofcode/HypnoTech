function MerchCategoryViewModel(data) {
    "use strict";
    var self = this;

    function findItemAtPositon(position) {
        for (var i = 0; i < self.subcategoryTypes().length; i++) {
            if (self.subcategoryTypes()[i].order() === position) {
                return self.subcategoryTypes()[i];
            }
        }
        return undefined;
    }

    function moveToPosition(itemToMove, position) {
        var itemToSwap = findItemAtPositon(position);
        itemToSwap.order(itemToMove.order());
        itemToMove.order(position);

        self.subcategoryTypes.sort(function (i1, i2) {
            if (i1 > i2) {
                return 1;
            } else {
                return -1;
            }
        });
    }

    self.name = ko.observable(data.name);
    self.order = ko.observable(data.order);

    self.addName = ko.observable("");

    if (data.subcategoryTypes) {
        self.subcategoryTypes = ko.observableArray([]);

        for (var i = 0; i < data.subcategoryTypes.length; i++) {
            self.subcategoryTypes.push(new MerchCategoryViewModel(data.subcategoryTypes[i]));
        }

        self.orderedSubcategories = ko.pureComputed(function () {
            return _.sortBy(self.subcategoryTypes(), function (item) {
                return item.order();
            });
        });

        self.add = function () {
            if (self.addName().trim() === "") {
                return;
            }

            var newItem = {
                name: self.addName(),
                order: self.subcategoryTypes().length
            };
            self.subcategoryTypes.push(new MerchCategoryViewModel(newItem));
            self.addName("");
        };

        self.canMoveUp = function (item) {
            return item && item.order() > 0;
        };

        self.canMoveDown = function (item) {
            return item && item.order() < self.subcategoryTypes().length - 1;
        };

        self.moveUp = function (item) {
            if (self.canMoveUp(item)) {
                moveToPosition(item, item.order() - 1);
            }
        };

        self.moveDown = function (item) {
            if (self.canMoveDown(item)) {
                moveToPosition(item, item.order() + 1);
            }
        };

        self.remove = function (itemToRemove) {
            ajax.get("/Settings/Types/AllowDeleteMerchSubcategoryType", itemToRemove.getJson()).done(function (result) {
                if (result.canRemove) {
                    var order = itemToRemove.order();
                    self.subcategoryTypes.remove(itemToRemove);

                    var itemsToUpdate = _.filter(self.subcategoryTypes(),
                        function (item) {
                            return item.order() > order;
                        });

                    _.each(itemsToUpdate,
                        function (item) {
                            item.order(item.order() - 1);
                        });
                } else {
                    messages.danger("Cannot remove '" +
                        itemToRemove.name() +
                        "' because it has merch assigned to it. Move all merch to a different subcategory before removing");
                }
            });

        };
    }

    self.getJson = function () {
        var json = {
            name: self.name(),
            order: self.order()
        };
        if (self.subcategoryTypes) {
            json.subcategoryTypes = [];
            for (var j = 0; j < self.orderedSubcategories().length; j++) {
                json.subcategoryTypes.push(self.subcategoryTypes()[j].getJson());
            }
        }

        return json;
    };
}

function MerchTypesViewModel(data) {
    "use strict";
    var self = this;

    self.categoryTypes = ko.observableArray([]);
    self.addName = ko.observable("");

    self.orderedCategories = ko.pureComputed(function () {
        return _.sortBy(self.categoryTypes(), function (item) {
            return item.order();
        });
    });

    for (var i = 0; i < data.length; i++) {
        self.categoryTypes.push(new MerchCategoryViewModel(data[i]));
    }

    self.discard = function () {
        window.location.reload();
    };

    self.saveChanges = function () {
        var categoriesData = {
            types: [],
            __RequestVerificationToken: document.getElementsByName("__RequestVerificationToken")[0].value
        };

        for (var i = 0; i < self.orderedCategories().length; i++) {
            categoriesData.types.push(self.orderedCategories()[i].getJson());
        }

        ajax.post("/Settings/Types/UpdateMerchCategoryTypes", categoriesData).done(function () {
            window.location.reload();
        });
    };

    self.add = function () {
        if (self.addName().trim() === "") {
            return;
        }

        var newItem = {
            name: self.addName(),
            order: self.categoryTypes().length,
            subcategoryTypes: []
        };
        self.categoryTypes.push(new MerchCategoryViewModel(newItem));
        self.addName("");
    };

    self.canMoveUp = function (item) {
        return item && item.order() > 0;
    };

    self.canMoveDown = function (item) {
        return item && item.order() < self.categoryTypes().length - 1;
    };

    function findItemAtPositon(position) {
        for (var i = 0; i < self.categoryTypes().length; i++) {
            if (self.categoryTypes()[i].order() === position) {
                return self.categoryTypes()[i];
            }
        }
        return undefined;
    }

    function moveToPosition(itemToMove, position) {
        var itemToSwap = findItemAtPositon(position);
        itemToSwap.order(itemToMove.order());
        itemToMove.order(position);

        self.categoryTypes.sort(function (i1, i2) {
            if (i1 > i2) {
                return 1;
            } else {
                return -1;
            }
        });
    }

    self.moveUp = function (item) {
        if (self.canMoveUp(item)) {
            moveToPosition(item, item.order() - 1);
        }
    };

    self.moveDown = function (item) {
        if (self.canMoveDown(item)) {
            moveToPosition(item, item.order() + 1);
        }
    };

    self.remove = function (itemToRemove) {
        ajax.get("/Settings/Types/AllowDeleteMerchCategoryType", itemToRemove.getJson()).done(function (result) {
            if (result.canRemove) {
                var order = itemToRemove.order();
                self.categoryTypes.remove(itemToRemove);

                var itemsToUpdate = _.filter(self.categoryTypes(),
                    function (item) {
                        return item.order() > order;
                    });

                _.each(itemsToUpdate,
                    function (item) {
                        item.order(item.order() - 1);
                    });
            } else {
                messages.danger("Cannot remove '" +
                    itemToRemove.name() +
                    "' because it has merch assigned to it. Move all merch to a different category before removing");
            }
        });

    };
}

function FlatTypeViewModel(data) {
    "use strict";
    var self = this;

    self.name = ko.observable(data.name);
    self.order = ko.observable(data.order);

    self.getJson = function () {
        var json = {
            name: self.name(),
            order: self.order()
        };

        return json;
    };
}

function LinkTypesViewModel(data) {
    "use strict";
    var self = this;

    self.linkTypes = ko.observableArray([]);
    self.addName = ko.observable("");

    self.orderedLinks = ko.pureComputed(function () {
        return _.sortBy(self.linkTypes(), function (item) {
            return item.order();
        });
    });

    for (var i = 0; i < data.length; i++) {
        self.linkTypes.push(new MerchCategoryViewModel(data[i]));
    }

    self.discard = function () {
        window.location.reload();
    };

    self.saveChanges = function () {
        var linksData = {
            types: [],
            __RequestVerificationToken: document.getElementsByName("__RequestVerificationToken")[0].value
        };

        for (var i = 0; i < self.orderedLinks().length; i++) {
            linksData.types.push(self.orderedLinks()[i].getJson());
        }

        ajax.post("/Settings/Types/UpdateLinkTypes", linksData).done(function () {
            window.location.reload();
        });
    };

    self.add = function () {
        if (self.addName().trim() === "") {
            return;
        }

        var newItem = {
            name: self.addName(),
            order: self.linkTypes().length,
            sublinkTypes: []
        };
        self.linkTypes.push(new MerchCategoryViewModel(newItem));
        self.addName("");
    };

    self.canMoveUp = function (item) {
        return item && item.order() > 0;
    };

    self.canMoveDown = function (item) {
        return item && item.order() < self.linkTypes().length - 1;
    };

    function findItemAtPositon(position) {
        for (var i = 0; i < self.linkTypes().length; i++) {
            if (self.linkTypes()[i].order() === position) {
                return self.linkTypes()[i];
            }
        }
        return undefined;
    }

    function moveToPosition(itemToMove, position) {
        var itemToSwap = findItemAtPositon(position);
        itemToSwap.order(itemToMove.order());
        itemToMove.order(position);

        self.linkTypes.sort(function (i1, i2) {
            if (i1 > i2) {
                return 1;
            } else {
                return -1;
            }
        });
    }

    self.moveUp = function (item) {
        if (self.canMoveUp(item)) {
            moveToPosition(item, item.order() - 1);
        }
    };

    self.moveDown = function (item) {
        if (self.canMoveDown(item)) {
            moveToPosition(item, item.order() + 1);
        }
    };

    self.remove = function (itemToRemove) {
        ajax.get("/Settings/Types/AllowDeleteLinkType", itemToRemove.getJson()).done(function (result) {
            if (result.canRemove) {
                var order = itemToRemove.order();
                self.linkTypes.remove(itemToRemove);

                var itemsToUpdate = _.filter(self.linkTypes(),
                    function (item) {
                        return item.order() > order;
                    });

                _.each(itemsToUpdate,
                    function (item) {
                        item.order(item.order() - 1);
                    });
            } else {
                messages.danger("Cannot remove '" +
                    itemToRemove.name() +
                    "' because it has links assigned to it. Move all links to a different category before removing");
            }
        });

    };
}

function RecordTypesViewModel(data) {
    "use strict";
    var self = this;

    self.recordTypes = ko.observableArray([]);
    self.addName = ko.observable("");

    self.orderedRecords = ko.pureComputed(function () {
        return _.sortBy(self.recordTypes(), function (item) {
            return item.order();
        });
    });

    for (var i = 0; i < data.length; i++) {
        self.recordTypes.push(new MerchCategoryViewModel(data[i]));
    }

    self.discard = function () {
        window.location.reload();
    };

    self.saveChanges = function () {
        var recordsData = {
            types: [],
            __RequestVerificationToken: document.getElementsByName("__RequestVerificationToken")[0].value
        };

        for (var i = 0; i < self.orderedRecords().length; i++) {
            recordsData.types.push(self.orderedRecords()[i].getJson());
        }

        ajax.post("/Settings/Types/UpdateRecordTypes", recordsData).done(function () {
            window.location.reload();
        });
    };

    self.add = function () {
        if (self.addName().trim() === "") {
            return;
        }

        var newItem = {
            name: self.addName(),
            order: self.recordTypes().length,
            subrecordTypes: []
        };
        self.recordTypes.push(new MerchCategoryViewModel(newItem));
        self.addName("");
    };

    self.canMoveUp = function (item) {
        return item && item.order() > 0;
    };

    self.canMoveDown = function (item) {
        return item && item.order() < self.recordTypes().length - 1;
    };

    function findItemAtPositon(position) {
        for (var i = 0; i < self.recordTypes().length; i++) {
            if (self.recordTypes()[i].order() === position) {
                return self.recordTypes()[i];
            }
        }
        return undefined;
    }

    function moveToPosition(itemToMove, position) {
        var itemToSwap = findItemAtPositon(position);
        itemToSwap.order(itemToMove.order());
        itemToMove.order(position);

        self.recordTypes.sort(function (i1, i2) {
            if (i1 > i2) {
                return 1;
            } else {
                return -1;
            }
        });
    }

    self.moveUp = function (item) {
        if (self.canMoveUp(item)) {
            moveToPosition(item, item.order() - 1);
        }
    };

    self.moveDown = function (item) {
        if (self.canMoveDown(item)) {
            moveToPosition(item, item.order() + 1);
        }
    };

    self.remove = function (itemToRemove) {
        ajax.get("/Settings/Types/AllowDeleteRecordType", itemToRemove.getJson()).done(function (result) {
            if (result.canRemove) {
                var order = itemToRemove.order();
                self.recordTypes.remove(itemToRemove);

                var itemsToUpdate = _.filter(self.recordTypes(),
                    function (item) {
                        return item.order() > order;
                    });

                _.each(itemsToUpdate,
                    function (item) {
                        item.order(item.order() - 1);
                    });
            } else {
                messages.danger("Cannot remove '" +
                    itemToRemove.name() +
                    "' because it has records assigned to it. Move all records to a different category before removing");
            }
        });

    };
}