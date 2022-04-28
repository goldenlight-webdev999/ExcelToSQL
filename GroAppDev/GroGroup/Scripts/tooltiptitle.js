$(document).ready(function () {

    //Replaces data-rel attribute to rel.
    //We use data-rel because of w3c validation issue
    $('a[data-rel]').each(function () {
        $(this).attr('rel', $(this).data('rel'));
        //alert($(this).attr('rel', $(this).data('rel')));
        //alert($(this).data('rel'));
    });

    // tooltip sample
    if ($('.tooltipsample').length > 0)
        $('.tooltipsample').tooltip({ selector: "a[rel=tooltip]" });

    $('.popoversample').popover({ selector: 'a[rel=popover]', trigger: 'hover' });

    // tabbed widget
    $('.tabbedwidget').tabs();

});