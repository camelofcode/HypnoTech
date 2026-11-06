function PhotoMetaDataViewModel(listName, index, name, value) {
    "use strict";
    var self = this;

    self.name = ko.pureComputed(function () {
        return listName + "[" + index() + "]." + name[0].toUpperCase() + name.slice(1);
    });
    self.value = value;
}

function PhotoViewModel(listName, item, index, imagePathName, parent) {
    "use strict";
    var self = this;
    self.index = ko.observable(index);
    self.properties = ko.observableArray([]);
    self.propertyNames = [];
    for (var propertyName in item) {
        if (item.hasOwnProperty(propertyName)) {
            self.propertyNames.push(propertyName);
            self[propertyName] = ko.observable(item[propertyName] || "");
            if (propertyName !== "blob") {
                self.properties.push(new PhotoMetaDataViewModel(listName, self.index, propertyName, self[propertyName]));
            }
        }
    }
    if (!self.blob) {
        self.blob = ko.observable(null);
    }

    self.image = ko.pureComputed(function () {
        return self.blob() || self[imagePathName]();
    });

    self.hasImage = ko.pureComputed(function () {
        return self.blob() !== null || (self[imagePathName]() !== null && self[imagePathName]() !== "");
    });

    self.hasAddedFile = ko.pureComputed(function () {
        return self.blob() !== null;
    });
}

function PhotoGalleryViewModel(listMemberName, items, imagePathName, allowedTypes, template) {
    "use strict";
    var self = this;

    var objectTemplate = template;

    self.listMemberName = listMemberName;
    self.items = ko.observableArray([]);
    self.files = ko.observable(null);

    if (items) {
        for (var i = 0; i < items.length; i++) {
            self.items.push(new PhotoViewModel(self.listMemberName, items[i], i, imagePathName, self));
        }
    }

    self.addedFiles = ko.observableArray([]);

    function readBlob(file) {
        var reader = new FileReader();
        reader.onload = function (e) {
            objectTemplate.blob = e.target.result;
            var index = self.items().length;
            self.addedFiles.push(self.listMemberName + "_" + index);
            self.items.push(new PhotoViewModel(self.listMemberName, objectTemplate, index, imagePathName, self));
        };
        reader.readAsDataURL(file);
    }

    self.removedFiles = ko.observableArray([]);
    self.removeFile = function (item) {
        if (item.imagePath()) {
            self.removedFiles.push(item.imagePath());
        } else {
            self.addedFiles.remove(self.listMemberName + "_" + item.index());
            self.addedFiles.valueHasMutated();
        }
        self.items.remove(item);
        for (var j = 0; j < self.items().length; j++) {
            var currentIndex = self.items()[j].index();
            var addedFilesIndex = self.addedFiles.indexOf(self.listMemberName + "_" + currentIndex);
            if (addedFilesIndex > -1) {
                self.addedFiles()[addedFilesIndex] = self.listMemberName + "_" + j;
                self.addedFiles.valueHasMutated();
            }
            self.items()[j].index(j);
        }
    };

    self.files.subscribe(function (files) {
        if (files) {
            for (var j = 0; j < files.length; j++) {
                var file = files[j];

                if (allowedTypes.includes(file.type)) {
                    readBlob(file);
                }
            }
        }
    });
}