
/* normal sort */
var count = 0, no = 0;
function sort() {
    ///table sort order
    //var child;
    $('th').each(function (column) {
        
        $('th').css({ "cursor": "pointer" });
        $('td').css({ "cursor": "default" });
        $(this).addClass('sortable').click(function () {
            //alert("i");
           // $("#loading").fadeIn();

            var findSortKey = function ($cell) {
                return $cell.find('.sort-key').text().toUpperCase() + ' ' + $cell.text().toUpperCase();
               // alert(findSortKey);
            };
            var sortDirection = $(this).is('.sorted-asc') ? -1 : 1;
            var $rows = $(this).parent().parent().parent().find('tbody tr').get();
            var bob = 0;
            //loop through all the rows and find
            $.each($rows, function (index, row) {
                row.sortKey = findSortKey($(row).children('td').eq(column));
              
            });            

            var self = this;
            var $buttons = $('.sort');
            idx = $buttons.index(this);

            $rows.sort(function (a, b) {
                //alert(a.sortKey);
               // if (a.innerText != "")
                   // {
                    var $obj1 = $(a).find('td').eq(idx),
                    $obj2 = $(b).find('td').eq(idx),
                     value1, value2;
                //}
                //else
                //{
                //    var $obj1 = $(a).find(':checkbox'),
                //                $obj2 = $(b).find(':checkbox'),
                //                value1, value2;
                //}
               
                count++;
                if (a.sortKey == b.sortKey) {
                    no++;
                    //alert("s");
                    return 0;
                }
                else {
                    if ($.isNumeric((a.sortKey)) == true) {

                        //numeracy check
                        if (parseFloat(a.sortKey) < parseFloat(b.sortKey)) {
                            return -sortDirection;
                        }
                        if (parseFloat(a.sortKey) > parseFloat(b.sortKey)) {
                            return sortDirection;
                        }
                    }
                    else if ((a.sortKey).split(":").length - 1 == 1) {
                        //time check
                        var res = (a.sortKey).replace("PM", " PM").replace("AM", " AM");
                        var ser = (b.sortKey).replace("PM", " PM").replace("AM", " AM");
                        a = new Date('1970/01/01 ' + res);
                        b = new Date('1970/01/01 ' + ser);
                        return a < b ? -sortDirection : a > b ? sortDirection : 0;
                    }
                    else if ((a.sortKey).split(" ").length - 1 >= 90) {
                        //checkbox value
                        if ($("input").is(":checkbox")) {
                            value1 = $obj1.find(":checkbox")[0].checked;
                            value2 = $obj2.find(":checkbox")[0].checked;
                            return value1 < value2 ? -sortDirection : value1 > value2 ? sortDirection : 0;
                        }
                        else {
                            value1 = $('td:eq(' + column + ')', a).text();
                            value2 = $('td:eq(' + column + ')', b).text();
                            return value1 < value2 ? -sortDirection : value1 > value2 ? sortDirection : 0;
                        }
                    }
                    //else if()
                    //{
                    //    //if ($a.is(':checked') && !$b.is(':checked'))
                    //    //    return -1;
                    //    //else if (!$a.is(':checked') && $b.is(':checked'))
                    //    //    return 1;
                    //}
                    else {
                        //alphabetic check
                        if (a.sortKey < b.sortKey) {
                            return -sortDirection;
                        }
                        if (a.sortKey > b.sortKey) {
                            return sortDirection;
                        }
                    }
                    return 0;
                }
            });


            //add the rows in the correct order to the bottom of the table
            $.each($rows, function (index, row) {
                if (count == no) {
                    //alert("s");
                }
                else {
                    $('tbody').append(row);
                    row.sortKey = null;
                }
            });

            $('th').removeClass('sorted-asc sorted-desc');
            $('th i').removeClass('icon-white icon-chevron-down icon-white icon-chevron-up');
            //$('th img').css("display", "none");
            var $sortHead = $('th').filter(':nth-child(' + (column + 1) + ')');
            //$("." + child).hide();
            var parents = $(this).prevAll().find('th i').context.childNodes[1].className;
            if (count != no) {
                if (sortDirection == 1) {
                    //child = parents;
                    $sortHead.addClass('sorted-asc');
                    $("." + parents).addClass('icon-white icon-chevron-up');
                    //$("." + parents).css("float", "right");
                    $("." + parents).css("margin-top", "-4px");
                    //$("." + parents).show();
                }
                else {
                    //child = parents;
                    $sortHead.addClass('sorted-desc');
                    $("." + parents).addClass('icon-white icon-chevron-down');
                    // $("." + parents).css("float", "right");
                    $("." + parents).css("margin-top", "-4px");
                    //$("." + parents).show();
                }
            }
            count = ""; no = "";
            //$("." + child).hide();
            $('td').removeClass('sorted').filter(':nth-child(' + (column + 1) + ')').addClass('sorted');
           // $("#loading").fadeOut();

        });
    });

}

/* search screen  sort */
var count = 0, no = 0;
function searchsort() {
    //table search sort order
    $('th').each(function (column) {
        $('th').css({ "cursor": "pointer" });
        $('td').css({ "cursor": "default" });
        $(this).addClass('sortable').click(function ( e) {

            // $("#loading").fadeIn();
            var findSortKey = function ($cell) {
                return $cell.find('.sort-key').text().toUpperCase() + ' ' + $cell.text().toUpperCase();
            };

            var sortDirection = $(this).is('.sorted-asc') ? -1 : 1;
            var $rows = $(this).parent().parent().parent().find('tbody tr').get();
            var bob = 0;

            //loop through all the rows and find
            $.each($rows, function (index, row) {
                row.sortKey = findSortKey($(row).children('td').eq(column));

            });

            var self = this;
            var $buttons = $('.sort');
            idx = $buttons.index(this);

            $rows.sort(function (a, b) {
                //alert(a.sortKey);
                var $obj1 = $(a).find('td').eq(idx),
                $obj2 = $(b).find('td').eq(idx),
                value1, value2;
                count++;
                if (a.sortKey == b.sortKey) {
                    no++;
                    //alert("s");
                    return 0;
                }
                else {
                    if ($.isNumeric((a.sortKey)) == true) {

                        //numeracy check
                        if (parseFloat(a.sortKey) < parseFloat(b.sortKey)) {
                            return -sortDirection;
                        }
                        if (parseFloat(a.sortKey) > parseFloat(b.sortKey)) {
                            return sortDirection;
                        }
                    }
                    else if ((a.sortKey).split(":").length - 1 == 1) {
                        //time check
                        var res = (a.sortKey).replace("PM", " PM").replace("AM", " AM");
                        var ser = (b.sortKey).replace("PM", " PM").replace("AM", " AM");
                        a = new Date('1970/01/01 ' + res);
                        b = new Date('1970/01/01 ' + ser);
                        return a < b ? -sortDirection : a > b ? sortDirection : 0;
                    }
                    else if ((a.sortKey).split(" ").length - 1 >= 90) {
                        //((a.sortKey).split(" ").length - 1);
                        value1 = $obj1.find("input")[0].checked;
                        value2 = $obj2.find("input")[0].checked;
                        return value1 < value2 ? -sortDirection : value1 > value2 ? sortDirection : 0;
                    }
                    else {
                        //alphabetic check
                        if (a.sortKey < b.sortKey) {
                            return -sortDirection;
                        }
                        if (a.sortKey > b.sortKey) {
                            return sortDirection;
                        }
                        return 0;
                    }

                }
            });
            //add the rows in the correct order to the bottom of the table
            $.each($rows, function (index, row) {
                if (count == no) {
                    //alert("s");
                }
                else {
                    //alert("ss");
                    $(' tbody').append(row);
                    row.sortKey = null;
                }
            });
            
            //identify the collumn sort order
            //$('th').removeClass('sorted-asc sorted-desc');

            //var $sortHead = $(' th').filter(' :nth-child(' + (column + 1) + ')');
            //sortDirection == 1 ? $sortHead.addClass('sorted-asc') : $sortHead.addClass('sorted-desc');

            ////identify the collum to be sorted by
            //$('td').removeClass('sorted').filter(' :nth-child(' + (column + 1) + ')').addClass('sorted');


            //identify the collumn sort order
            $('th').removeClass('sorted-asc sorted-desc');
            $sortHead = $('th').filter(':nth-child(' + (column + 1) + ')');
            if (count != no) {
                if (sortDirection == 1) {

                    $sortHead.addClass('sorted-asc');
                    $('th i').css("display", "none");
                    $sortHead.append($('<i>', { class: 'icon-white icon-chevron-up' }))
                    //test = "hide";
                    $('th i').css("float", "right");
                    $('th i').css("margin-top", "-4px");
                }
                else {
                    $sortHead.addClass('sorted-desc');
                    $('th i').css("display", "none");
                    $sortHead.append($('<i>', { class: 'icon-white icon-chevron-down' }))
                    //demo = "hide";
                    $('th i').css("float", "right");
                    $('th i').css("margin-top", "-4px");
                }
            }
            count = ""; no = "";
            $('td').removeClass('sorted').filter(':nth-child(' + (column + 1) + ')').addClass('sorted');
           
            return false;
            //e.stopPropagation();
        });
    });
}




/* screen with double table  sort */
var count = 0, no = 0;
function doublesort() {

    $('.tablesort th').each(function (column) {
        //alert('ss');
        $('.tablesort th').css({ "cursor": "pointer" });
        $('.tablesort td').css({ "cursor": "default" });
        $(this).addClass('sortable').click(function () {
            // alert('ss');
            //$("#loading").fadeIn();
            var findSortKey = function ($cell) {
                return $cell.find('.sort-key').text().toUpperCase() + ' ' + $cell.text().toUpperCase();
                // alert(findSortKey);
            };
            var sortDirection = $(this).is('.sorted-asc') ? -1 : 1;
            var $rows = $(".tablesort").find('tbody tr').get();
            var bob = 0;
            //loop through all the rows and find
            $.each($rows, function (index, row) {
                row.sortKey = findSortKey($(row).children('td').eq(column));
                // alert(row.sortKey);
            });
            var self = this;
            var $buttons = $('.sort');
            idx = $buttons.index(this);
            //alert(idx);
            //compare and sort the rows alphabetically or numerically
            $rows.sort(function (a, b) {
                //alert(a.sortKey);
                var $obj1 = $(a).find('td').eq(idx),
                $obj2 = $(b).find('td').eq(idx),
                value1, value2;
                count++;
                if (a.sortKey == b.sortKey) {
                    no++;
                    //alert("s");
                    return 0;
                }
                else {
                    if (a.sortKey.trim() == "" && b.sortKey.trim() == "") {
                        return -sortDirection;
                    }
                    if ($.isNumeric((a.sortKey.replace(/\,/g, '').trim())) == true && $.isNumeric((b.sortKey.replace(/\,/g, '').trim())) == true) {
                        //numeracy check
                        if (parseFloat(a.sortKey.replace(/\,/g, '').trim()) < parseFloat(b.sortKey.replace(/\,/g, '').trim())) {
                            return -sortDirection;
                        }
                        if (parseFloat(a.sortKey.replace(/\,/g, '').trim()) > parseFloat(b.sortKey.replace(/\,/g, '').trim())) {
                            return sortDirection;
                        }
                    }
                    else if ((a.sortKey).split(":").length - 1 == 1) {
                        //time check
                        var res = (a.sortKey).replace("PM", " PM").replace("AM", " AM");
                        var ser = (b.sortKey).replace("PM", " PM").replace("AM", " AM");
                        a = new Date('1970/01/01 ' + res);
                        b = new Date('1970/01/01 ' + ser);
                        return a < b ? -sortDirection : a > b ? sortDirection : 0;
                    }
                    else if ((a.sortKey).split(" ").length - 1 >= 90) {
                        value1 = $obj1.find("input")[0].checked;
                        value2 = $obj2.find("input")[0].checked;
                        return value1 < value2 ? -sortDirection : value1 > value2 ? sortDirection : 0;
                    }
                    else if (isValidDate(a.sortKey) && isValidDate(b.sortKey)) {
                        if (new Date(a.sortKey) < new Date(b.sortKey)) {
                            return -sortDirection;
                        }
                        if (new Date(a.sortKey) > new Date(b.sortKey)) {
                            return sortDirection;
                        }
                    }
                    else {
                        //alphabetic check
                        if (a.sortKey.replace(/\,/g, '').trim() < b.sortKey.replace(/\,/g, '').trim()) {
                            return -sortDirection;
                        }
                        if (a.sortKey.replace(/\,/g, '').trim() > b.sortKey.replace(/\,/g, '').trim()) {
                            return sortDirection;
                        }
                    }
                    return 0;
                }
            });

            //add the rows in the correct order to the bottom of the table
            $.each($rows, function (index, row) {
                if (count == no) {
                    //alert("s");
                }
                else {
                    $('.tablesort tbody').append(row);
                    //$('.tablesorting tbody').append();
                    row.sortKey = null;
                }
            });

            //identify the collumn sort order
            $('.tablesort th').removeClass('sorted-asc sorted-desc');
            $('.tablesort th i').removeClass('icon-white icon-chevron-down icon-white icon-chevron-up');

            var $sortHead = $('.tablesort th').filter('.tablesort :nth-child(' + (column + 1) + ')');
            var parents = $(this).prevAll().find('th i').context.childNodes[1].className;
            //sortDirection == 1 ? $sortHead.addClass('sorted-asc') : $sortHead.addClass('sorted-desc');
            if (count != no) {
                if (sortDirection == 1) {
                    //child = parents;
                    $sortHead.addClass('sorted-asc');
                    $("." + parents).addClass('icon-white icon-chevron-up');
                    //$("." + parents).css("float", "right");
                    $("." + parents).css("margin-top", "-4px");
                    //$("." + parents).show();
                }
                else {
                    //child = parents;
                    $sortHead.addClass('sorted-desc');
                    $("." + parents).addClass('icon-white icon-chevron-down');
                    //$("." + parents).css("float", "right");
                    $("." + parents).css("margin-top", "-4px");
                    //$("." + parents).show();
                }
            }
            count = ""; no = "";
            //$("#loading").fadeOut();
            //identify the collum to be sorted by
            $('.tablesort td').removeClass('sorted').filter('.tablesort :nth-child(' + (column + 1) + ')').addClass('sorted');
        });
    });
}


var count = 0, no = 0;
function doublesort2() {

    $('.main-tablesort th').each(function (column) {
        //alert('ss');
        $('.main-tablesort th').css({ "cursor": "pointer" });
        $('.main-tablesort td').css({ "cursor": "default" });
        $(this).addClass('sortable').click(function () {
            // alert('ss');
            //$("#loading").fadeIn();
            var findSortKey = function ($cell) {
                return $cell.find('.sort-key').text().toUpperCase() + ' ' + $cell.text().toUpperCase();
                // alert(findSortKey);
            };
            var sortDirection = $(this).is('.sorted-asc') ? -1 : 1;
            var $rows = $(".main-tablesort").find('tbody tr').get();
            var bob = 0;
            //loop through all the rows and find
            $.each($rows, function (index, row) {
                row.sortKey = findSortKey($(row).children('td').eq(column));
                // alert(row.sortKey);
            });
            var self = this;
            var $buttons = $('.sort');
            idx = $buttons.index(this);
            //alert(idx);
            //compare and sort the rows alphabetically or numerically
            $rows.sort(function (a, b) {
                //alert(a.sortKey);
                var $obj1 = $(a).find('td').eq(idx),
                $obj2 = $(b).find('td').eq(idx),
                value1, value2;
                count++;
                if (a.sortKey == b.sortKey) {
                    no++;
                    //alert("s");
                    return 0;
                }
                else {
                    if ($.isNumeric((a.sortKey)) == true) {
                        //numeracy check
                        //alert("s");
                        if (parseFloat(a.sortKey) < parseFloat(b.sortKey)) {
                            //alert(parseFloat(b.sortKey));
                            return -sortDirection;
                        }
                        if (parseFloat(a.sortKey) > parseFloat(b.sortKey)) {
                            return sortDirection;
                        }
                    }
                    else if ((a.sortKey).split(":").length - 1 == 1) {
                        //time check
                        var res = (a.sortKey).replace("PM", " PM").replace("AM", " AM");
                        var ser = (b.sortKey).replace("PM", " PM").replace("AM", " AM");
                        a = new Date('1970/01/01 ' + res);
                        b = new Date('1970/01/01 ' + ser);
                        return a < b ? -sortDirection : a > b ? sortDirection : 0;
                    }
                    else if ((a.sortKey).split(" ").length - 1 >= 90) {
                        //alert((a.sortKey).split(" ").length - 1);
                        value1 = $obj1.find("input")[0].checked;
                        value2 = $obj2.find("input")[0].checked;
                        return value1 < value2 ? -sortDirection : value1 > value2 ? sortDirection : 0;
                    }
                    else {
                        //alphabetic check
                        //alert("ss");
                        if (a.sortKey < b.sortKey) {
                            return -sortDirection;
                        }
                        if (a.sortKey > b.sortKey) {
                            return sortDirection;
                        }
                    }
                    return 0;
                }
            });

            //add the rows in the correct order to the bottom of the table
            $.each($rows, function (index, row) {
                if (count == no) {
                    //alert("s");
                }
                else {
                    $('.main-tablesort tbody').append(row);
                    //$('.tablesorting tbody').append();
                    row.sortKey = null;
                }
            });

            //identify the collumn sort order
            $('.main-tablesort th').removeClass('sorted-asc sorted-desc');
            $('.main-tablesort th i').removeClass('icon-white icon-chevron-down icon-white icon-chevron-up');

            var $sortHead = $('.main-tablesort th').filter('.main-tablesort :nth-child(' + (column + 1) + ')');
            var parents = $(this).prevAll().find('th img').context.childNodes[1].className;
            //sortDirection == 1 ? $sortHead.addClass('sorted-asc') : $sortHead.addClass('sorted-desc');
            if (count != no) {
                if (sortDirection == 1) {
                    //child = parents;
                    $sortHead.addClass('sorted-asc');
                    $("." + parents).addClass('icon-white icon-chevron-up');
                    //$("." + parents).css("float", "right");
                    $("." + parents).css("margin-top", "-4px");
                    //$("." + parents).show();
                }
                else {
                    //child = parents;
                    $sortHead.addClass('sorted-desc');
                    $("." + parents).addClass('icon-white icon-chevron-down');
                    //$("." + parents).css("float", "right");
                    $("." + parents).css("margin-top", "-4px");
                    //$("." + parents).show();
                }
            }
            count = ""; no = "";
            //$("#loading").fadeOut();
            //identify the collum to be sorted by
            $('.main-tablesort td').removeClass('sorted').filter('.main-tablesort :nth-child(' + (column + 1) + ')').addClass('sorted');
        });
    });
}


function dynamicwidth() {
   // alert($(window).width());
   // $('.dynamicwidth').css('width', $(window).width() - 200 + 'px');
    $('.dynamicwidth').css('max-height', $(window).height() - 120 + 'px');
}

function dynamicheight() {
    //alert($(window).height()*58/100);
  
    $('.dynamicheight').css('max-height', $(window).height() - $(window).height() * 18 / 100 + 'px');
   
    //$('.dynamicmidheight').css('max-height', $(window).height() - $(window).height() * 30 / 100 + 'px');
}

function download(filename, text) {
    var pom = document.createElement('a');
    pom.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(text));
    pom.setAttribute('download', filename);

    if (document.createEvent) {
        var event = document.createEvent('MouseEvents');
        event.initEvent('click', true, true);
        pom.dispatchEvent(event);
    }
    else {
        pom.click();
    }
    $("#NotesDialog").dialog("close");

}

/* checked on single table  */
$('.check').live('click', function (event) {
    if (this.checked == false) {
        var rowindex = this.id;
        rowindex = rowindex.replace("chk_", "");
        rowindex++;
       // var $sortHead = $('th').filter(':nth-child(' + rowindex + ')');
       // $('td').removeClass('sorted');
        $('th').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');
        $('td').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');

        $('th').filter(':nth-child(' + rowindex + ')').removeClass('sortable');
        $('td').filter(':nth-child(' + rowindex + ')').removeClass('sortable');

        $('th').filter(':nth-child(' + rowindex + ')').addClass('hide');
        $('td').filter(':nth-child(' + rowindex + ')').addClass('hide');

        if ($('th').length == $('th.hide').length) {

            alert("warning", "Please select atleast one column");

            $('th').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');
            $('td').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');

            $('th').filter(':nth-child(' + rowindex + ')').removeClass('hide');
            $('td').filter(':nth-child(' + rowindex + ')').removeClass('hide');

            $('th').filter(':nth-child(' + rowindex + ')').addClass('sortable');
            $('td').filter(':nth-child(' + rowindex + ')').addClass('sortable');

            rowindex--;
            $("#chk_" + [rowindex]).attr('checked', true);

        }


    }
    else if (this.checked == true) {
        var rowindex = this.id;
        rowindex = rowindex.replace("chk_", "");
        rowindex++;
       // var $sortHead = $('th').filter(':nth-child(' + rowindex + ')');
        //$('td').removeClass('sorted');
        $('th').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');
        $('td').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');

        $('th').filter(':nth-child(' + rowindex + ')').removeClass('hide');
        $('td').filter(':nth-child(' + rowindex + ')').removeClass('hide');

        $('th').filter(':nth-child(' + rowindex + ')').addClass('sortable');
        $('td').filter(':nth-child(' + rowindex + ')').addClass('sortable');

    }
    else {

    }
});

/* checked on double table  */
$('.checked').live('click', function (event) {
    if (this.checked == false) {
        var rowindex = this.id;
        rowindex = rowindex.replace("chk_", "");
        rowindex++;
        //var $sortHead = $('.tablesort th').filter(':nth-child(' + rowindex + ')');
       // $('.tablesort td').removeClass('sorted');
        $('.tablesort th').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');
        $('.tablesort td').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');

        $('.tablesort th').filter('.tablesort :nth-child(' + rowindex + ')').removeClass('sortable');
        $('.tablesort td').filter('.tablesort :nth-child(' + rowindex + ')').removeClass('sortable');

        $('.tablesort th').filter('.tablesort :nth-child(' + rowindex + ')').addClass('hide');
        $('.tablesort td').filter('.tablesort :nth-child(' + rowindex + ')').addClass('hide');

        $('.innertable th').filter(':nth-child(' + rowindex + ')').removeClass('hide');
        $('.innertable td').filter(' :nth-child(' + rowindex + ')').removeClass('hide');


        if ($('.exportclass th').length == $('.tablesort th.hide').length) {

            alert("warning", "Please select atleast one column");

            $('.tablesort th').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');
            $('.tablesort td').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');

            $('.tablesort th').filter('.tablesort :nth-child(' + rowindex + ')').removeClass('hide');
            $('.tablesort td').filter(':nth-child(' + rowindex + ')').removeClass('hide');

            $('.tablesort th').filter('.tablesort :nth-child(' + rowindex + ')').addClass('sortable');
            $('.tablesort td').filter('.tablesort :nth-child(' + rowindex + ')').addClass('sortable');

            $('.innertable th').filter(':nth-child(' + rowindex + ')').removeClass('hide');
            $('.innertable td').filter(' :nth-child(' + rowindex + ')').removeClass('hide');

            rowindex--;
            $("#chk_" + [rowindex]).attr('checked', true);

        }

    }
    else if (this.checked == true) {
        var rowindex = this.id;
        rowindex = rowindex.replace("chk_", "");
        rowindex++;
        //var $sortHead = $('.tablesort th').filter('.tablesort :nth-child(' + rowindex + ')');
        //$('.tablesort td').removeClass('sorted');
        $('.tablesort th').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');
        $('.tablesort td').filter(':nth-child(' + rowindex + ')').removeClass('sorted sorted-asc sorted-desc');

        $('.tablesort th').filter('.tablesort :nth-child(' + rowindex + ')').removeClass('hide');
        $('.tablesort td').filter(':nth-child(' + rowindex + ')').removeClass('hide');

        $('.tablesort th').filter('.tablesort :nth-child(' + rowindex + ')').addClass('sortable');
        $('.tablesort td').filter('.tablesort :nth-child(' + rowindex + ')').addClass('sortable');

        $('.innertable th').filter(':nth-child(' + rowindex + ')').removeClass('hide');
        $('.innertable td').filter(' :nth-child(' + rowindex + ')').removeClass('hide');
    }
    else {

    }
});




/*TableExport */
function GlobalExcelExport(table, ActivityName, ActivityInfo2, ActivityType, filename) {
    var i, j;
    var csv = "";
    if (table.children[0].innerText == "") {
        var table_headings = table.children[1].children[0].children;
        var table_body_rows = table.children[2].children;
    }
    else {

        var table_headings = table.children[0].children[0].children;
        var table_body_rows = table.children[1].children;
    }
    var heading;
    var headingsArray = [];
    var TblheadingsArray = [];
    for (i = 0; i < table_headings.length; i++) {

        if (table_headings[i].className != "hide") {
            heading = table_headings[i];
            if (heading.innerText == "") {
                headingsArray.push("Checkbox");
                TblheadingsArray.push({ header: "Checkbox" });
            }
            else {
                headingsArray.push('"' + heading.innerText + '"');
                TblheadingsArray.push({ header: table_headings[i].innerText });
            }
        }
    }
    csv += headingsArray.join(',') + "\n";
    var row;
    var columns;
    var column;
    var columnsArray;
    for (i = 0; i < table_body_rows.length; i++) {
        row = table_body_rows[i];
        columns = row.children;
        columnsArray = [];
        for (j = 0; j < columns.length; j++) {
            if (columns[j].className != "hide") {
                var column = columns[j];

                if (column.children.length > 0) {
                    if (columns[j].innerText == "") {
                        if (column.children[0].className == "check") {
                            columnsArray.push('"' + column.children[0].checked + '"');
                        }
                        else {
                            columnsArray.push('"' + column.children[0].value + '"');
                        }
                    }
                }
                else {
                    columnsArray.push('"' + column.textContent + '"');
                }

            }
        }
        //if (columnsArray.length > 1)
        csv += columnsArray.join(',') + "\n";
    }
   
    var get_date = new Date();
    var month = get_date.getMonth() + 1;
    var day = get_date.getDate();
    var CurDate =
        (month < 10 ? '0' : '') + month + '/' +
        (day < 10 ? '0' : '') + day + '/'
    + get_date.getFullYear();
    download(filename + CurDate + ".csv", csv);
}

function GlobalExcelExportsortable(table, ActivityName, ActivityInfo2, ActivityType, filename) {

        var i, j;
        var csv = "";
        if (table.children[0].innerText == "") {
            var table_headings = table.children[1].children[0].children;
            var table_body_rows = table.children[2].children;
        }
        else {

            var table_headings = table.children[0].children[0].children;
            var table_body_rows = table.children[1].children;
        }

        var heading;
        var headingsArray = [];
        var TblheadingsArray = [];
        for (i = 0; i < table_headings.length; i++) {

            var str = table_headings[i].className;
            if (str.includes("sortable") || !str.includes("hide")) {
                if (!str.includes("hide")) {
                    heading = table_headings[i];
                    if (heading.style.display == "none") {

                    }
                    else {
                        if (heading.innerText == "") {
                            headingsArray.push("Checkbox");
                            TblheadingsArray.push({ header: "Checkbox" });
                        }
                        else {
                            //headingsArray.push('"' + heading.innerText + '"');
                            headingsArray.push('"' + heading.innerText.replace(/\▴/g, '').replace(/\▾/g, '').trim() + '"');
                            TblheadingsArray.push({ header: table_headings[i].innerText });
                        }
                    }
                }
            }
        }

        csv += headingsArray.join(',') + "\n";

        var row;
        var columns;
        var column;
        var columnsArray;
        for (i = 0; i < table_body_rows.length; i++) {
            row = table_body_rows[i];
            columns = row.children;
            columnsArray = [];
            for (j = 0; j < columns.length; j++) {
                var str = columns[j].className;
                if (str.includes("sortable") || !str.includes("hide")) {
                    var column = columns[j];
                    if (column.style.display == "none") {

                    }
                    else {
                        if (column.children.length > 0) {
                            if (columns[j].innerText == "") {
                                if (column.children[0].className == "check") {
                                    columnsArray.push('"' + column.children[0].checked + '"');
                                }
                                else {
                                    //  if ($("input:text").type == "text")		
                                    columnsArray.push('"' + column.children[0].value + '"');
                                    //else		
                                    //    columnsArray.push('"' + column.children[0].src + '"');		
                                }
                            }
                        }

                        else {
                            columnsArray.push('"' + column.textContent.replace(/"/g, '""') + '"');
                        }
                    }
                }
                else if (str.includes("hide")) {
                }


            }
            //if (columnsArray.length > 1)
            csv += columnsArray.join(',') + "\n";
        }

        var get_date = new Date();
        var month = get_date.getMonth() + 1;
        var day = get_date.getDate();
        var CurDate =
            (month < 10 ? '0' : '') + month + '/' +
            (day < 10 ? '0' : '') + day + '/'
        + get_date.getFullYear();
        download(filename + CurDate + ".csv", csv);
    }

