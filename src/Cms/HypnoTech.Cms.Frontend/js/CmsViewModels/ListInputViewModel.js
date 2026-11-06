function ListInputItemMetaDataViewModel(listName, index, name, value) {
    "use strict";
    var self = this;

    self.name = ko.pureComputed(function () {
        return listName + "[" + index() + "]." + name[0].toUpperCase() + name.slice(1);
    });
    self.value = value;
}

function ListInputItemViewModel(listName, item, index, parent, singularObjectName) {
    "use strict";
    var self = this;
    self.index = ko.observable(index);
    self.properties = ko.observableArray([]);
    self.propertyNames = [];
    for (var propertyName in item) {
        if (item.hasOwnProperty(propertyName)) {
            self.propertyNames.push(propertyName);
            self[propertyName] = ko.observable(item[propertyName] || "");
            if (item[propertyName] === 0) {
                self[propertyName](0);
            }
            self.properties.push(new ListInputItemMetaDataViewModel(listName, self.index, propertyName, self[propertyName]));
        }
    }

    self.showEditItem = function () {
        parent.isEdit(true);
        parent.templateHeaderText("Edit " + singularObjectName);

        for (var i = 0; i < self.propertyNames.length; i++) {
            var propertyName = self.propertyNames[i];
            self[propertyName + "Temp"] = self[propertyName]();
        }
        parent.selectedItem(self);
    };

    self.revert = function () {
        for (var i = 0; i < self.propertyNames.length; i++) {
            var propertyName = self.propertyNames[i];
            self[propertyName](self[propertyName + "Temp"]);
        }
    };
}

function ListInputViewModel(listMemberName, items, template, formTemplate, nameField, objectName, validators, isOrdered, orderPropertyName) {
    "use strict";
    var self = this;

    var objectTemplate = template;
    var nameProperty = nameField;
    var singularObjectName = objectName;
    var ordered = isOrdered;
    var orderProperty = orderPropertyName;

    self.validators = validators;

    self.listMemberName = listMemberName;
    self.items = ko.observableArray([]);

    self.selectedItem = ko.observable();
    self.template = ko.observable(formTemplate);

    if (items) {
        for (var i = 0; i < items.length; i++) {
            self.items.push(new ListInputItemViewModel(self.listMemberName, items[i], i, self, singularObjectName));
        }
    }

    self.itemToDeleteName = ko.pureComputed(function () {
        return self.selectedItem()[nameProperty]();
    });

    self.templateHeaderText = ko.observable();

    self.showAddItem = function () {
        self.templateHeaderText("Add " + singularObjectName);
        self.selectedItem(new ListInputItemViewModel(self.listMemberName, objectTemplate, self.items().length, self));
    };

    self.showRemoveItem = function (item) {
        self.templateHeaderText("Remove " + singularObjectName);
        self.selectedItem(item);
    };

    self.isEdit = ko.observable(false);

    self.removeItem = function () {
        self.items.remove(self.selectedItem());
        for (var j = 0; j < self.items().length; j++) {
            self.items()[j].index(j);
        }
    };

    function validateSelectedItem() {
        var isValid = true;
        for (var j = 0; j < self.selectedItem().propertyNames.length; j++) {
            var propertyName = self.selectedItem().propertyNames[j];
            var property = self.selectedItem()[propertyName];

            var validator = _.find(validators, { name: propertyName });
            if (validator) {
                validation.remove(validator.id);
                if (validator.isRequired && (property() === null || (typeof property() === "string" && property().trim() === ""))) {
                    validation.invalid(validator.id, validator.displayName + (validator.isPlural ? " are required" : " is required"));
                    isValid = false;
                }
                else if (validator.validateUrl && property() !== null) {
                    if (property().startsWith("mailto:")) {
                        var email = property().replace("mailto:", "");
                        if (/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(email) === false) {
                            validation.invalid(validator.id, validator.displayName + " is not a valid Email Address");
                            isValid = false;
                        }
                    } else {
                        if (/https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/.test(property()) === false) {
                            validation.invalid(validator.id, validator.displayName + " is not a valid Url");
                            isValid = false;
                        }
                    }
                }
            }
        }
        return isValid;
    }

    if (ordered) {
        function findItemAtPositon(position) {
            for (var i = 0; i < self.items().length; i++) {
                if (self.items()[i][orderProperty]() === position) {
                    return self.items()[i];
                }
            }
            return undefined;
        }

        function moveToPosition(itemToMove, position) {
            var itemToSwap = findItemAtPositon(position);
            itemToSwap[orderProperty](itemToMove[orderProperty]());
            itemToMove[orderProperty](position);

            self.items.sort(function (i1, i2) {
                if (i1 > i2) {
                    return 1;
                } else {
                    return -1;
                }
            });
        }

        self.orderedItems = ko.pureComputed(function () {
            return _.sortBy(self.items(), function (item) {
                return item[orderProperty]();
            });
        });

        self.canMoveUp = function (item) {
            return item && item[orderProperty]() > 0;
        };

        self.canMoveDown = function (item) {
            return item && item[orderProperty]() < self.orderedItems().length - 1;
        };

        self.moveUp = function (item) {
            if (self.canMoveUp(item)) {
                moveToPosition(item, item[orderProperty]() - 1);
            }
        };

        self.moveDown = function (item) {
            if (self.canMoveDown(item)) {
                moveToPosition(item, item[orderProperty]() + 1);
            }
        };
    }

    self.saveItem = function () {
        if (validateSelectedItem()) {
            if (self.isEdit()) {
                self.isEdit(false);
                $('#addEditModal' + listMemberName).modal('hide');
            } else {
                self.items.push(self.selectedItem());
                $('#addEditModal' + listMemberName).modal('hide');
            }
        }
    };

    $('#addEditModal' + listMemberName).on('hidden.bs.modal', function () {
        if (self.isEdit()) {
            self.selectedItem().revert();
            self.isEdit(false);
        }
    });

    ko.applyBindings(self, $("#addEditModal" + listMemberName)[0]);
    ko.applyBindings(self, $("#deleteModal" + listMemberName)[0]);
}