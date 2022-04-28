/*
jQuery.msgBox plugin 
Copyright 2011, Halil İbrahim Kalyoncu
License: BSD
modified by Oliver Kopp, 2012.
 * added support for configurable image paths
 * a new msgBox can be shown within an existing msgBox
*/

/*
contact :

halil@ibrahimkalyoncu.com
koppdev@googlemail.com

*/

// users may change this variable to fit their needs
var msgBoxImagePath = "../images/";

jQuery.msgBox = msg;
function msg (options) {
    var isShown = false;
    var typeOfValue = typeof options;
    var defaults = {
        content: (typeOfValue == "string" ? options : "Message"),
        title: "Warning",
        type: "alert",
        autoClose: false,
        timeOut: 0,
        showButtons: true,
        parent: false,
        subparent: false,
        buttons: [{ value: "Ok"}],
        inputs: [{ type: "text", name:"userName", header: "User Name" }, { type: "password",name:"password", header: "Password"}],
        success: function (result) { },
        beforeShow: function () { },
        afterShow: function () { },
        beforeClose: function () { },
        afterClose: function () { },
        opacity: 0.5
    };
    options = typeOfValue == "string" ? defaults : options;
    if (options.type != null) {
        switch (options.type) {
            case "alert":
                options.title = options.title == null ? "Warning" : options.title;
                break;
            case "info":
                options.title = options.title == null ? "Information" : options.title;
                break;
            case "error":
                options.title = options.title == null ? "Error" : options.title;
                break;
            case "confirm":
                options.title = options.title == null ? "Confirmation" : options.title;
                options.buttons = options.buttons == null ? [{ value: "Yes" }, { value: "No" }, { value: "Cancel"}] : options.buttons;
                break;
            case "prompt":
                options.title = options.title == null ? "Log In" : options.title;
                options.buttons = options.buttons == null ? [{ value: "Login" }, { value: "Cancel"}] : options.buttons;
                break;
            default:
                image = "alert.png";
        }
    }
    options.timeOut = options.timeOut == null ? (options.content == null ? 500 : options.content.length * 70) : options.timeOut;
    options = $.extend(defaults, options);
    if (options.autoClose) {
        setTimeout(hide, options.timeOut);
    }
    var image = "";
    switch (options.type) {
        case "alert":
            image = "alert.png";
            break;
        case "info":
            image = "info.png";
            break;
        case "error":
            image = "error.png";
            break;
        case "confirm":
            image = "confirm.png";
            break;
        default:
            image = "alert.png";
    }
    
    var divId = "msgBox" + new Date().getTime();
    
    var divMsgBoxId = divId; 
    var divMsgBoxContentId = divId+"Content"; 
    var divMsgBoxImageId = divId+"Image";
    var divMsgBoxButtonsId = divId+"Buttons";
    var divMsgBoxBackGroundId = divId+"BackGround";
    
    var buttons = "";
    $(options.buttons).each(function (index, button) {
        buttons += "<input class=\"msgButton\" type=\"button\" name=\"" + button.value + "\" value=\"" + button.value + "\" />";
    });

    var inputs = "";
    $(options.inputs).each(function (index, input) {
        var type = input.type;
        if (type=="checkbox" || type =="radiobutton") {
            inputs += "<div class=\"msgInput\">" +
            "<input type=\"" + input.type + "\" name=\"" + input.name + "\" "+(input.checked == null ? "" : "checked ='"+input.checked+"'")+" value=\"" + (typeof input.value == "undefined" ? "" : input.value) + "\" />" +
            "<text>"+input.header +"</text>"+
            "</div>";
        }
        else {
            inputs += "<div class=\"msgInput\">" +
            "<span class=\"msgInputHeader\">" + input.header + "<span>" +
            "<input type=\"" + input.type + "\" name=\"" + input.name + "\" value=\"" + (typeof input.value == "undefined" ? "" : input.value) + "\" />" +
            "</div>";
        }
    });

    var divBackGround = "<div id=" + divMsgBoxBackGroundId + " class=\"msgBoxBackGround\"></div>";
    var divTitle = "<div class=\"msgBoxTitle\">" + options.title + "</div>";
    var divContainer = "<div class=\"msgBoxContainer\"><div id=" + divMsgBoxImageId + " class=\"msgBoxImage\"><img src=\"" + msgBoxImagePath + image + "\"/></div><div id=" + divMsgBoxContentId + " class=\"msgBoxContent\"><p><span>" + options.content + "</span></p></div></div>";
    var divButtons = "<div id=" + divMsgBoxButtonsId + " class=\"msgBoxButtons\">" + buttons + "</div>";
    var divInputs = "<div class=\"msgBoxInputs\">" + inputs + "</div>";

    var divMsgBox; 
    var divMsgBoxContent; 
    var divMsgBoxImage;
    var divMsgBoxButtons;
    var divMsgBoxBackGround;

    if (options.subparent) {

        if (options.type == "prompt") {
            window.parent.window.parent.$("html").append(divBackGround + "<div id=" + divMsgBoxId + " class=\"msgBox\">" + divTitle + "<div>" + divContainer + (options.showButtons ? divButtons + "</div>" : "</div>") + "</div>");
            divMsgBox = window.parent.window.parent.$("#" + divMsgBoxId);
            divMsgBoxContent = window.parent.window.parent.$("#" + divMsgBoxContentId);
            divMsgBoxImage = window.parent.window.parent.$("#" + divMsgBoxImageId);
            divMsgBoxButtons = window.parent.window.parent.$("#" + divMsgBoxButtonsId);
            divMsgBoxBackGround = window.parent.window.parent.$("#" + divMsgBoxBackGroundId);

            divMsgBoxImage.remove();
            divMsgBoxButtons.css({ "text-align": "center", "margin-top": "5px" });
            divMsgBoxContent.css({ "width": "100%", "height": "100%" });
            divMsgBoxContent.html(divInputs);
        }
        else {
            window.parent.window.parent.$("html").append(divBackGround + "<div id=" + divMsgBoxId + " class=\"msgBox\">" + divTitle + "<div>" + divContainer + (options.showButtons ? divButtons + "</div>" : "</div>") + "</div>");
            divMsgBox = window.parent.window.parent.$("#" + divMsgBoxId);
            divMsgBoxContent = window.parent.window.parent.$("#" + divMsgBoxContentId);
            divMsgBoxImage = window.parent.window.parent.$("#" + divMsgBoxImageId);
            divMsgBoxButtons = window.parent.window.parent.$("#" + divMsgBoxButtonsId);
            divMsgBoxBackGround = window.parent.window.parent.$("#" + divMsgBoxBackGroundId);
        }

        var width = divMsgBox.width();
        var height = divMsgBox.height();
        var windowHeight = $(window.parent.window.parent).height();
        var windowWidth = $(window.parent.window.parent).width();
    }

    else if (options.parent) {

        if (options.type == "prompt") {
            window.parent.$("html").append(divBackGround + "<div id=" + divMsgBoxId + " class=\"msgBox\">" + divTitle + "<div>" + divContainer + (options.showButtons ? divButtons + "</div>" : "</div>") + "</div>");
            divMsgBox = window.parent.$("#" + divMsgBoxId);
            divMsgBoxContent = window.parent.$("#" + divMsgBoxContentId);
            divMsgBoxImage = window.parent.$("#" + divMsgBoxImageId);
            divMsgBoxButtons = window.parent.$("#" + divMsgBoxButtonsId);
            divMsgBoxBackGround = window.parent.$("#" + divMsgBoxBackGroundId);

            divMsgBoxImage.remove();
            divMsgBoxButtons.css({ "text-align": "center", "margin-top": "5px" });
            divMsgBoxContent.css({ "width": "100%", "height": "100%" });
            divMsgBoxContent.html(divInputs);
        }
        else {
            window.parent.$("html").append(divBackGround + "<div id=" + divMsgBoxId + " class=\"msgBox\">" + divTitle + "<div>" + divContainer + (options.showButtons ? divButtons + "</div>" : "</div>") + "</div>");
            divMsgBox = window.parent.$("#" + divMsgBoxId);
            divMsgBoxContent = window.parent.$("#" + divMsgBoxContentId);
            divMsgBoxImage = window.parent.$("#" + divMsgBoxImageId);
            divMsgBoxButtons = window.parent.$("#" + divMsgBoxButtonsId);
            divMsgBoxBackGround = window.parent.$("#" + divMsgBoxBackGroundId);
        }

        var width = divMsgBox.width();
        var height = divMsgBox.height();
        var windowHeight = $(window.parent).height();
        var windowWidth = $(window.parent).width();

    }
    else if (!options.subaprent && !options.parent) {

        if (options.type == "prompt") {
            $("html").append(divBackGround + "<div id=" + divMsgBoxId + " class=\"msgBox\">" + divTitle + "<div>" + divContainer + (options.showButtons ? divButtons + "</div>" : "</div>") + "</div>");
            divMsgBox = $("#" + divMsgBoxId);
            divMsgBoxContent = $("#" + divMsgBoxContentId);
            divMsgBoxImage = $("#" + divMsgBoxImageId);
            divMsgBoxButtons = $("#" + divMsgBoxButtonsId);
            divMsgBoxBackGround = $("#" + divMsgBoxBackGroundId);

            divMsgBoxImage.remove();
            divMsgBoxButtons.css({ "text-align": "center", "margin-top": "5px" });
            divMsgBoxContent.css({ "width": "100%", "height": "100%" });
            divMsgBoxContent.html(divInputs);
        }
        else {
            $("html").append(divBackGround + "<div id=" + divMsgBoxId + " class=\"msgBox\">" + divTitle + "<div>" + divContainer + (options.showButtons ? divButtons + "</div>" : "</div>") + "</div>");
            divMsgBox = $("#" + divMsgBoxId);
            divMsgBoxContent = $("#" + divMsgBoxContentId);
            divMsgBoxImage = $("#" + divMsgBoxImageId);
            divMsgBoxButtons = $("#" + divMsgBoxButtonsId);
            divMsgBoxBackGround = $("#" + divMsgBoxBackGroundId);
        }

        var width = divMsgBox.width();
        var height = divMsgBox.height();
        var windowHeight = $(window).height();
        var windowWidth = $(window).width();

    }
    

    var top = windowHeight / 2 - height / 2;
    var left = windowWidth / 2 - width / 2;

    show();

    function show() {
        if (isShown) {
            return;
        }
        divMsgBox.css({ opacity: 0, top: top - 50, left: left });
        divMsgBox.css("background-image", "url('"+msgBoxImagePath+"msgBoxBackGround.png')");
        divMsgBoxBackGround.css({ opacity: options.opacity });
        divMsgBox.animate({ opacity: 1, "top": top, "left": left }, 200);
        setTimeout(options.afterShow, 200);
        isShown = true;
        options.beforeShow();

        if (options.subparent) {
            divMsgBoxBackGround.css({ "width": $(window.parent.window.parent.document).width(), "height": getDocHeight(window.parent.window.parent.document) });
            window.parent.window.parent.$(divMsgBoxId + "," + divMsgBoxBackGroundId).fadeIn(0);
            $(window.parent.window.parent).bind("resize", function (e) {
                var width = divMsgBox.width();
                var height = divMsgBox.height();
                var windowHeight = $(window.parent.window.parent).height();
                var windowWidth = $(window.parent.window.parent).width();

                var top = windowHeight / 2 - height / 2;
                var left = windowWidth / 2 - width / 2;

                divMsgBox.css({ "top": top, "left": left });
            });
        }
        else if (options.parent) {
            divMsgBoxBackGround.css({ "width": $(window.parent.document).width(), "height": getDocHeight(window.parent.document) });
            window.parent.$(divMsgBoxId + "," + divMsgBoxBackGroundId).fadeIn(0);
            $(window.parent).bind("resize", function (e) {
                var width = divMsgBox.width();
                var height = divMsgBox.height();
                var windowHeight = $(window.parent).height();
                var windowWidth = $(window.parent).width();

                var top = windowHeight / 2 - height / 2;
                var left = windowWidth / 2 - width / 2;

                divMsgBox.css({ "top": top, "left": left });
            });
        }
        else if (!options.subaprent && !options.parent) {

            divMsgBoxBackGround.css({ "width": $(document).width(), "height": getDocHeight(document) });
           $(divMsgBoxId + "," + divMsgBoxBackGroundId).fadeIn(0);
            $(window).bind("resize", function (e) {
                var width = divMsgBox.width();
                var height = divMsgBox.height();
                var windowHeight = $(window).height();
                var windowWidth = $(window).width();

                var top = windowHeight / 2 - height / 2;
                var left = windowWidth / 2 - width / 2;

                divMsgBox.css({ "top": top, "left": left });
            });

        }
    }

    function hide() {
        if (!isShown) {
            return;
        }
        options.beforeClose();
        divMsgBox.animate({ opacity: 0, "top": top - 50, "left": left }, 200);
        divMsgBoxBackGround.fadeOut(300);
        setTimeout(function () { divMsgBox.remove(); divMsgBoxBackGround.remove(); }, 300);
        setTimeout(options.afterClose, 300);
        isShown = false;
    }

    function getDocHeight(doc) {
        var D = doc;
        return Math.max(
        Math.max(D.body.scrollHeight, D.documentElement.scrollHeight),
        Math.max(D.body.offsetHeight, D.documentElement.offsetHeight),
        Math.max(D.body.clientHeight, D.documentElement.clientHeight));
    }

    function getFocus() {
    	divMsgBox.fadeOut(200).fadeIn(200);
    }


    if (options.subparent) {

        window.parent.window.parent.$("input.msgButton").click(function (e) {
            e.preventDefault();
            var value = window.parent.window.parent.$(this).val();
            if (options.type != "prompt") {
                options.success(value);
            }
            else {
                var inputValues = [];
                window.parent.window.parent.$("div.msgInput input").each(function (index, domEle) {
                    var name = window.parent.window.parent.$(this).attr("name");
                    var value = window.parent.window.parent.$(this).val();
                    var type = window.parent.window.parent.$(this).attr("type");
                    if (type == "checkbox" || type == "radiobutton") {
                        inputValues.push({ name: name, value: value, checked: $(this).attr("checked") });
                    }
                    else {
                        inputValues.push({ name: name, value: value });
                    }
                });
                options.success(value, inputValues);
            }
            hide();
        });

    }
    else if (options.parent) {

        window.parent.$("input.msgButton").click(function (e) {
            e.preventDefault();
            var value = window.parent.$(this).val();
            if (options.type != "prompt") {
                options.success(value);
            }
            else {
                var inputValues = [];
                window.parent.$("div.msgInput input").each(function (index, domEle) {
                    var name = window.parent.$(this).attr("name");
                    var value = window.parent.$(this).val();
                    var type = window.parent.$(this).attr("type");
                    if (type == "checkbox" || type == "radiobutton") {
                        inputValues.push({ name: name, value: value, checked: $(this).attr("checked") });
                    }
                    else {
                        inputValues.push({ name: name, value: value });
                    }
                });
                options.success(value, inputValues);
            }
            hide();
        });


    }
    else if(!options.subaprent && !options.parent) {

        $("input.msgButton").click(function (e) {
            e.preventDefault();
            var value = $(this).val();
            if (options.type != "prompt") {
                options.success(value);
            }
            else {
                var inputValues = [];
                $("div.msgInput input").each(function (index, domEle) {
                    var name = $(this).attr("name");
                    var value = $(this).val();
                    var type = $(this).attr("type");
                    if (type == "checkbox" || type == "radiobutton") {
                        inputValues.push({ name: name, value: value, checked: $(this).attr("checked") });
                    }
                    else {
                        inputValues.push({ name: name, value: value });
                    }
                });
                options.success(value, inputValues);
            }
            hide();
        });


    }


   

    divMsgBoxBackGround.click(function (e) {
        if (!options.showButtons || options.autoClose) {
            hide();
        }
        else {
            getFocus();
        }
    });
};
