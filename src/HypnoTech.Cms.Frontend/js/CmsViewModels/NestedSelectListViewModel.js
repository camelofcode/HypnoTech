function NestedSelectListViewModel(values, selectedItem, selectedChildItem) {
    "use strict";
    var self = this;

    self.options = ko.observableArray(values);
    self.childOptions = ko.observableArray([]);
    self.selectedOption = ko.observable(selectedItem || values[0].value);
    self.selectedChildOption = ko.observable(selectedChildItem || values[0].children[0].value);

    function setChildOptions() {
        for (var i = 0; i < self.options().length; i++) {
            var option = self.options()[i];
            if (option.value === self.selectedOption()) {
                self.childOptions(option.children);
                return;
            }
        }
        self.childOptions([]);
    }

    setChildOptions();

    self.selectedOption.subscribe(function() {
        setChildOptions();
    });
}