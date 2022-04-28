(function ($) {
    SetScreenRestrictions = function (userAdd, userEdit, userDelete) {
        var AddRemoved=false;
        var EditRemoved = false;
        if ($("#btnAdd").length && userAdd != 'True') {
            $("#btnAdd").remove();
            AddRemoved = true;
        }
        if ($("#btnEdit").length && userEdit != 'True') {
            $("#btnEdit").remove();
            EditRemoved = true;
        }
        if ($("#btnDelete").length && userDelete != 'True') {
            $("#btnDelete").remove();
        }
        if (!$("#btnAdd").length && !$("#btnEdit").length) {
            if (AddRemoved == true || EditRemoved == true) {
                $("#btnSave").remove();
                $("#btnCancel").remove();
            }
        }
    }
})(jQuery);