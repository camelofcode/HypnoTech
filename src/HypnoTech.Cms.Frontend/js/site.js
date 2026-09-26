// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    // Sidebar Toggle Script.
    $("#menu-toggle").click(function (e) {
        e.preventDefault();
        $("#wrapper").toggleClass("toggled");

        Cookies.set("sidebartoggled", $("#wrapper").hasClass("toggled") ? "yes" : "no");
    });

    $('[data-toggle="tooltip"]').tooltip()

    setTimeout(function () {
        $("[role='alert']").fadeTo(500, 500).slideUp(500);
    }, 3000);

    var hash = window.location.hash;
    hash && $('ul.nav a[href="' + hash + '"]').tab('show');

    if ($(".nav-tabs").length > 0) {
        $('.nav-tabs a').click(function (e) {
            $(this).tab('show');
            var scrollmem = $('body').scrollTop();
            window.location.hash = this.hash;
            $('html,body').scrollTop(scrollmem);
        });

        var hash = window.location.hash;
        $('.nav-tabs a[href="' + hash + '"]').tab('show');
    }
})();
