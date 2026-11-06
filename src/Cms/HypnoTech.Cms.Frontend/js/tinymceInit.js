(function () {
    if (typeof tinymce === 'undefined') {
        return;
    }

    tinymce.init({
        selector: 'textarea.tinymce',
        plugins: ['paste', 'link']
    });
})();
