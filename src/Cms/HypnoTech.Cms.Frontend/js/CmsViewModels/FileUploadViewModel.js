function FileUploadViewModel(filePath, allowedTypes, inputId) {
    "use strict";
    var self = this;
    self.inputId = inputId;
    self.filePath = ko.observable(filePath);
    self.hasRemovedFile = ko.observable(false);
    self.uploadedFile = ko.observable(null);
    self.blob = ko.observable(null);
    self.isAudio = ko.observable(filePath && filePath.endsWith("mp3"));
    self.newFileName = ko.observable(null);

    self.file = ko.pureComputed(function() {
        return self.blob() || self.filePath();
    });

    self.hasFile = ko.pureComputed(function () {
        return self.blob() !== null || (self.filePath() !== null && self.filePath() !== "");
    });

    self.hasAddedFile = ko.pureComputed(function () {
        return self.blob() !== null;
    });

    self.fileText = ko.pureComputed(function() {
        if (self.filePath() !== null && self.filePath() !== "") {
            return self.filePath();
        } else {
            return self.newFileName();
        }
    });

    self.removeFile = function () {
        validation.remove(self.inputId);
        if (self.filePath() !== null && self.filePath() !== "") {
            self.hasRemovedFile(true);
        }
        self.filePath(null);
        self.uploadedFile(null);
        self.blob(null);
    };

    var reader = new FileReader();
    reader.onload = function(e) {
        self.blob(e.target.result);
    };

    self.uploadedFile.subscribe(function (file) {
        if (file) {
            validation.remove(self.inputId);
            if (!allowedTypes.includes(file.type)) {
                self.removeFile();
                validation.invalid(self.inputId, "File must be one of the following types: " + allowedTypes.toString());
            } else {
                if (file.type === "audio/mp3") {
                    self.isAudio(true);
                }
                validation.remove(self.inputId);
                self.newFileName(file.name);
                reader.readAsDataURL(file);
            }
        } 
    });
}