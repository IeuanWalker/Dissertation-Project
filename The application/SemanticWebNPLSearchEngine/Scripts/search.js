$(document).ready(function () {
    $('#search-input').on("keypress", function (e) {
        if (e.keyCode == 13) {
            var search = $.trim($("#search-input").val());
            return false; // prevent the button click from happening
        }
    });
});