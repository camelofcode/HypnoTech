function ImageUploadViewModel(imagePath, allowedTypes, inputId) {
    "use strict";
    var self = this;
    self.inputId = inputId;
    self.imagePath = ko.observable(imagePath);
    self.hasRemovedFile = ko.observable(false);
    self.file = ko.observable(null);
    self.blob = ko.observable(null);

    self.image = ko.pureComputed(function() {
        return self.blob() || self.imagePath();
    });

    self.hasImage = ko.pureComputed(function () {
        return self.blob() !== null ||  (self.imagePath() !== null && self.imagePath() !== "");
    });

    self.hasAddedFile = ko.pureComputed(function () {
        return self.blob() !== null;
    });

    self.removeImage = function () {
        validation.remove(self.inputId);
        if (self.imagePath() !== null && self.imagePath() !== "") {
            self.hasRemovedFile(true);
        }
        self.imagePath(null);
        self.file(null);
        self.blob(null);
    };

    var reader = new FileReader();
    reader.onload = function(e) {
        self.blob(e.target.result);
    };

    self.file.subscribe(function (file) {
        if (file) {
            validation.remove(self.inputId);
            if (!allowedTypes.includes(file.type)) {
                self.removeImage();
                validation.invalid(self.inputId, "File must be one of the following types: " + allowedTypes.toString());
            } else {
                validation.remove(self.inputId);
                reader.readAsDataURL(file);
            }
        } 
    });
}