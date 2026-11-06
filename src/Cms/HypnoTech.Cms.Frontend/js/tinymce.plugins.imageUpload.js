// ReSharper disable InconsistentNaming
typeof tinymce !== 'undefined' && tinymce.PluginManager.add('imageUpload', function (editor) {
    var uploading = false;

    function showUploadingUI() {
        uploading = true;
        $("#tinymce-image-upload-form").hide();
        $("#tinymce-image-upload-uploading").show();
    }

    function hideUploadingUI() {
        uploading = false;
        $("#tinymce-image-upload-form").show();
        $("#tinymce-image-upload-uploading").hide();
    }

    function openDialog() {
        $("#tinymce-image-upload-uploading").hide();
        $("#tinymce-image-upload-form").off("submit");
        $("#tinymce-image-upload").modal("show");

        $("#tinymce-image-upload").off("hide.bs.modal");
        $("#tinymce-image-upload").off("hidden.bs.modal");
        $('#tinymce-image-upload').on('hide.bs.modal', function (e) {
            if (uploading) {
                e.preventDefault();
                e.stopImmediatePropagation();
                return false;
            }
        });
        $('#tinymce-image-upload').on('hidden.bs.modal', function (e) {
            document.getElementById("tinymce-image-upload-file").value = null;
            hideUploadingUI();
        });


        $("#tinymce-image-upload-form").on("submit", function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            var files = document.getElementById("tinymce-image-upload-file").files;
            showUploadingUI();

            ajax.postWithFiles("/Uploads/UploadRichTextImage", files).done(function (response) {
                if (response.url) {
                    var img = "<img src=\"" + response.url + "\" width=\"100%\">";
                    editor.insertContent(img);
                    hideUploadingUI();
                    uploading = false;
                    $("#tinymce-image-upload").modal("hide");
                    document.getElementById("tinymce-image-upload-file").value = null;
                } else {
                    alert(response.error);
                    hideUploadingUI();
                    uploading = false;
                }
            });
        });
    }

    editor.ui.registry.addButton('imageUpload', {
        tooltip: 'Add Image',
        icon: 'image',
        onAction: function () {
            // Open window
            openDialog();
        }
    });
});
