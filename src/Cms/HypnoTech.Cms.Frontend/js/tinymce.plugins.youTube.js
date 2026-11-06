// ReSharper disable InconsistentNaming
typeof tinymce !== 'undefined' && tinymce.PluginManager.add('youtube', function (editor) {
    var uploading = false;

    function openDialog() {
        $("#tinymce-youtube-form").off("submit");
        $("#tinymce-youtube").modal("show");

        $("#tinymce-youtube").off("hide.bs.modal");
        $("#tinymce-youtube").off("hidden.bs.modal");
        $('#tinymce-youtube').on('hide.bs.modal', function (e) {
            if (uploading) {
                e.preventDefault();
                e.stopImmediatePropagation();
                return false;
            }
        });
        $('#tinymce-youtube').on('hidden.bs.modal', function (e) {
            document.getElementById("tinymce-youtube-url").value = null;
        });


        $("#tinymce-youtube-form").on("submit", function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            var url = document.getElementById("tinymce-youtube-url").value;
            var slug = "";
            var match = url.match(/^.*(?:(?:youtu\.be\/|v\/|vi\/|u\/\w\/|embed\/)|(?:(?:watch)?\?v(?:i)?=|\&v(?:i)?=))([^#\&\?]*).*/);
            if (match && match[1]) {
                slug = match[1];
            } else {
                alert("YouTube Url not valid");
                return;
            }

            var img = "<p><div class=\"video-wrapper\"><iframe allowfullscreen=\"allowfullscreen\" src=\"https://www.youtube.com/embed/" + slug + "?enablejsapi=1&amp;version=3&amp;playerapiid=ytplayer\"></iframe></div><p>";
            editor.insertContent(img);
            uploading = false;
            $("#tinymce-youtube").modal("hide");
        });
    }

    editor.ui.registry.addButton('youTube', {
        tooltip: 'Embed Youtube Video',
        icon: 'embed',
        onAction: function () {
            // Open window
            openDialog();
        }
    });
});
