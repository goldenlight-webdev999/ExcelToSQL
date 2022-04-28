(function ($) {
    var obj;
    var tablesObj;
    var tableIdAndRow = [];
    var TrAttrStrArray = [];
    //Use To Hide First RoW On Onload
    pageonload = function (x) {
        tablesObj = $(x).find($("table"));
        GetEachTrAttrArray(tablesObj);
        $(x).find($("table tbody tr:first")).hide();
    }
    //Getting Each Table Row Attribute in a Array.
    GetEachTrAttrArray = function (TblsObj) {
        $.each(TblsObj, function (key, val) {
            var tblrowid = [];
            tblrowid["id"] = val.id;
            tblrowid["attr"] = getTrAttrStr(val.id);
            tableIdAndRow[key] = tblrowid;
            TrAttrStrArray[key] = tblrowid;
        });
    }

    function getTrAttrStr(x) {
        var attrArry = "";
        if ($("#" + x + " tbody tr").length > 0) {
            $.each($("#" + x + " tbody tr:first").get(0).attributes, function (key, val) {
                if (val.name == "class") {
                    attrArry += val.name + "='" + val.value.replace("success selected", "") + "'" + " ";
                }
                else if (val.name == "style") {
                    if(val.value.replace("display: none;", "")!="")
                    attrArry += val.name + "=" + val.value.replace("display: none;", "") + " ";
                }
                else {
                    attrArry += val.name + "='" + val.value + "'" + " ";
                }

            });
        }
        return attrArry;
    }
    // FormTblValArray

    //Used To Get Insert Type String Formate To Insert Record.
    GetInsertArray = function (x) {
        //alert($(x).find($(".textClss")).length);
        var FieldName = "", FieldVal = "", array = [];
        obj = $(x).find($(".TblVals"));
        $.each(obj, function (key, value) {

            if ($(value).attr("type") == "checkbox") {

                FieldName += $(value).attr("id") + ",";
                FieldVal += (value.checked == true ? 1 : 0) + ",";
            }
            else {
                if ($(value).val() != null && $(value).val() != "") {
                    FieldName += $(value).attr("id") + ",";
                    FieldVal += "'" + $(value).val() + "',";
                }
            }
        });
        FieldName = FieldName.substr(0, FieldName.length - 1);
        FieldVal = FieldVal.substr(0, FieldVal.length - 1);
        array["FieldName"] = FieldName; array["FieldVal"] = FieldVal;
        return array;
    };

    //Use To Get Object Type to insert,update using ENTF
    //GetInsertObjForENTF = function (x) {
    //    var ENTObj = "";
    //    obj = $(x).find($(".TblVals"));
    //    $.each(obj, function (key, value) {
    //        if ($(value).attr("type") == "checkbox") {
    //            ENTObj += $(value).attr("id") + ":" + (value.checked == true ? 1 : 0) + ",";
    //        }
    //        else {
    //            if ($(value).val() == null || $(value).val() != "") {
    //                ENTObj += $(value).attr("id") + ":" + $(value).val() + ",";
    //            }
    //        }
    //    });
    //    ENTObj = ENTObj.substr(0, FieldName.length - 1);
    //    return ENTObj;
    //};

    GetInsertObjForENTF = function (x) {
        var ENTObj = {};
        obj = $(x).find($(".TblVals"));
        $.each(obj, function (key, value) {
            if ($(value).attr("type") == "checkbox") {
                ENTObj[$(value).attr("id")] = (value.checked == true ? true : false);
            }
            else {
                if ($(value).val() != null && $(value).val() != "") {
                    ENTObj[$(value).attr("id")] = $(value).val();
                }
            }
        });
        return ENTObj;
    };
    //Use To Get Sql Update type string Formate for Update Recored.
    GetupdateStr = function (x) {
        var FieldStr = "";
        obj = $(x).find($(".TblVals"));
        $.each(obj, function (key, value) {
            if ($(value).attr("type") == "checkbox") {
                FieldStr += $(value).attr("id") + "=" + "'" + (value.checked == true ? 1 : 0) + "',";

            }
            else {
                if ($(value).val() != null && $(value).val() != "") {
                    FieldStr += $(value).attr("id") + "=" + "'" + $(value).val() + "',";
                }
            }
        });
        FieldStr = FieldStr.substr(0, FieldStr.length - 1);
        return FieldStr;
    };

    //Use Fill Grid For a Table Per Screen.
    FillTable = function (x, list) {
        // obj = $(x).find($("table tbody tr:first")).children();
        var attrArry = "";
        attrArry = TrAttrStrArray[0].attr;

        obj = $("table tbody tr:first").children();
        $("table tbody tr").remove();
        $.each(list, function (index, value) {
            //  if (key ==2) {
            var strattr = "";
            var rowtd = "";

            $.each(obj, function (key, col) {
                var colname = new Object()
                if ($(col).attr("colname") != "" && $(col).attr("colname") != undefined) {
                    colname = $(col).attr("colname");
                    col.innerHTML = value[colname];
                    rowtd += col.outerHTML;
                }
                else {
                    if ($(col).children().attr("type") == "checkbox") {
                        colname = $(col).children().attr("colname");
                        if (value[colname] == "True")
                            $(col).children().attr("checked", true);
                        else
                            $(col).children().attr("checked", false);
                        rowtd += col.outerHTML;
                    }
                }
                if (attrArry.indexOf($(col).attr("colname")) >= 0) {
                    var attrname = "'" + $(col).attr("colname") + "'";
                    strattr = attrArry.replace(new RegExp(attrname), "'"+value[colname]+"'");
                }

                   rowtd = rowtd.replace("$index", index + 1 + '' + key + 1);
            });
            if (strattr != "")
                strattr = strattr.replace("$index", index + 1);
            else
                strattr = attrArry.replace("$index", index + 1);

            strattr = strattr == "" ? 'class="clickableRow"' : (strattr + ' ' + 'class="clickableRow"');

            $("table tbody").append('<tr ' + strattr + ' >' + rowtd + '</tr>');
           
        });
    }

    //Use To FillTableById(Id,GridList) for Multi Table per Screen
    FillTableById = function (x, list) {
        // obj = $(x).find($("table tbody tr:first")).children();
        var attrArry = "";

        $.each(tablesObj, function (key, tblobj) {
            if (tblobj.id == x) {
                attrArry = TrAttrStrArray[key].attr;
            }
        });
      //  alert(attrArry);
        obj = $("#" + x + " tbody tr:first").children();
        $("#" + x + " tbody tr").remove();
        $.each(list, function (index, value) {
            var strattr = "";
            var rowtd = "";

            $.each(obj, function (key, col) {
                var colname = new Object()
                if ($(col).attr("colname") != "" && $(col).attr("colname") != undefined) {
                    colname = $(col).attr("colname");
                    col.innerHTML = value[colname];
                    rowtd += col.outerHTML;
                }
                else {
                    if ($(col).children().attr("type") == "checkbox") {
                        colname = $(col).children().attr("colname");
                        if (value[colname] == "True")
                            $(col).children().attr("checked", true);
                        else
                            $(col).children().attr("checked", false);
                        rowtd += col.outerHTML;
                    }
                }
                if (attrArry.indexOf($(col).attr("colname")) >= 0) {
                    var attrname = "'" + $(col).attr("colname") + "'";
                    strattr = attrArry.replace(new RegExp(attrname), "'"+value[colname]+"'");
                }
                    rowtd = rowtd.replace("$index", index + 1 + '' + key + 1);
            });
            if (strattr!="")
                strattr = strattr.replace("$index", index + 1);
            else
                strattr = attrArry.replace("$index", index + 1);
            $("#" + x + " tbody").append('<tr ' + strattr + ' >' + rowtd + '</tr>');
        });
    }

    //Use To Bind Row Value.
    SelectedRowBind = function (indexobj, self) {

        var GetBindVal = $(self).find(".BindValue");
        $.each(GetBindVal, function (key, val) {
            if ($("#" + val.id).attr("type") != "checkbox") {
                $("#" + val.id).val(indexobj[val.id]);
            }
            else {
                if (indexobj[val.id] == "True")
                    $("#" + val.id).attr("checked", true);
                else
                    $("#" + val.id).attr("checked", false);
            }
        });
    };
})(jQuery);