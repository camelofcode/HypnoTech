function CheckBoxInputViewModel(id, hiddenWhenChecked, hiddenWhenUnchecked) {
    "use strict";

    function hide(elementId) {
        document.getElementById(elementId).parentNode.style.display = "none";
    }

    function show(elementId) {
        document.getElementById(elementId).parentNode.style.display = "";
    }

    var checkbox = document.getElementById(id);

    checkbox.addEventListener("change", function() {
        if (this.checked) {
            hiddenWhenChecked.forEach(elementId => hide(elementId));
            hiddenWhenUnchecked.forEach(elementId => show(elementId));
        } else {
            hiddenWhenChecked.forEach(elementId => show(elementId));
            hiddenWhenUnchecked.forEach(elementId => hide(elementId));
        }
    });
}