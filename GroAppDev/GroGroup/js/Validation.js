// JScript File

//Date Binding

function MonthBinding(date) {
    if (date != "" && date != null) {
        var inputdate = new Date(parseInt(date.substr(6)));
        var InputDateFormat = inputdate.getMonth() + 1 + "/" + inputdate.getDate() + "/" + inputdate.getFullYear();
        var input = inputdate.getFullYear() + "-" + ("0" + (inputdate.getMonth() + 1)).slice(-2);
        return input;
    }
}

function DateBinding(date) {
    if (date != null && date != "") {
        var inputdate = new Date(parseInt(date.substr(6)));
        //var InputDateFormat = inputdate.getMonth() + 1 + "/" + inputdate.getDate() + "/" + inputdate.getFullYear();
        var input = inputdate.getFullYear() + "-" + ("0"+(inputdate.getMonth() +1)).slice(-2) + "-" +("0"+ inputdate.getDate()).slice(-2);
        return input;
    }
}

function DateBindingByCountry(date, cntry) {
    if (date != null && date != "") {
        var input;
        var inputdate = new Date(parseInt(date.substr(6)));
        if(cntry == "India")
            input = ("0" + inputdate.getDate()).slice(-2) + "/" + ("0" + (inputdate.getMonth() + 1)).slice(-2) + "/" + inputdate.getFullYear();
        else
            input = ("0" + (inputdate.getMonth() + 1)).slice(-2) + "/" + ("0" + inputdate.getDate()).slice(-2) + "/" + inputdate.getFullYear();
        return input;
    }
}

function ReverseDateBinding(date) {
    if (date != null && date != "") {
        var spdt = date.split("/");
        var inputdate = new Date(spdt[2],spdt[1],spdt[0]);
        var input = ("0" + (inputdate.getMonth() + 1)).slice(-2) + "/" + ("0" + inputdate.getDate()).slice(-2) + "/" + inputdate.getFullYear();
        return input;
    }
    else{
        return date;
    }
}

function numberWithCommas(x) {
    if (x == null || x == "") return "";
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function DateBindingGrid(date) {
    if (date != "" && date != null) {
        var inputdate = new Date(parseInt(date.substr(6)));
        var InputDateFormat = inputdate.getMonth() + 1 + "/" + inputdate.getDate() + "/" + inputdate.getFullYear();
        return InputDateFormat;
    }
}

function DefaultDateBinding(date) {
    if (date != null && date != "") {
        var date1 = date.substring(0, date.length - 11);
        var spdt = date1.split("/");
        var inputdate = new Date(spdt[2], spdt[0], spdt[1]);
        var input = inputdate.getFullYear() + "-" + ("0" + (inputdate.getMonth())).slice(-2) + "-" + ("0" + inputdate.getDate()).slice(-2);
        return input;
    }
}

//function DateTimeBindingGrid(date) {
//    if (date != "" && date != null) {
//        var inputdate = new Date(parseInt(date.substr(6)));
//        var dt = inputdate.toLocaleString();
//        return dt;
//    }
//}
//function DateTimeBindingList(date) {
//    if (date != "" && date != null) {
//        var inputdate = new Date(date);
//        var dt = inputdate.toLocaleString();
//        return dt;
//    }
//}

function GridDateTime(date) {
    if (date != "" && date != null) {
        var inputdate = new Date(parseInt(date.substr(6)));
        var input = inputdate.getMonth() + 1 + "/" + inputdate.getDate() + "/" + inputdate.getFullYear() + " " + inputdate.getHours() + ":" + inputdate.getMinutes();
        return input;
    }
}

function DateTimeBindingGrid(date) {
    if (date != "" && date != null) {
        var inputdate = new Date(parseInt(date.substr(6)));
        var dt = inputdate.toLocaleString("en-US", { timeZone: "America/New_York" });
        return dt;
    }
}
function DateTimeBindingList(date) {
    if (date != "" && date != null) {
        var inputdate = new Date(date);
        var dt = inputdate.toLocaleString("en-US", { timeZone: "America/New_York" });
        return dt;
    }
}

function DateBind(date) {
    if (date != "" && date != null) {
        var inputdate = new Date(date);
        var InputDateFormat = ("0" + (inputdate.getMonth() + 1)).slice(-2) + "/" + ("0" + inputdate.getDate()).slice(-2) + "/" + inputdate.getFullYear();
        return InputDateFormat;
    }
}

function DateBindList(date) {
    if (date != "" && date != null) {
        var inputdate = new Date(date);
        var input = inputdate.getFullYear() + "-" + ("0" + (inputdate.getMonth() + 1)).slice(-2) + "-" + ("0" + inputdate.getDate()).slice(-2);
        return input;
    }
}

function toFixedFormate(item) {
    if (item == "" || item=="NaN")
        item = "0.00";
    else
        item = parseFloat(item).toFixed(2);
    return item;
}

function convert12to24(timeStr) {
    var meridian = timeStr.substr(timeStr.length - 2).toLowerCase(); 
    var hours = timeStr.substr(0, timeStr.indexOf(':'));
    var minutes = timeStr.substr(timeStr.indexOf(':') + 1, 2);
    if (meridian == 'pm') {
        if (hours != 12) {
            hours = hours * 1 + 12;
        }
        else {
            hours = (minutes != '00') ? '12' : '12';
        }
    }

    return hours + ':' + minutes;
}
function convert24to12(timeStr) {

    if (timeStr == "" || timeStr == null) return null;
    var hours = timeStr.substr(0, timeStr.indexOf(':'));
    var minutes = timeStr.substr(timeStr.indexOf(':') + 1, 2);
    var h = hours % 12;
    if (h === 0) h = 12;
    return (h < 10 ? "0" + h : h) +":"+ minutes + (hours < 12 ? 'AM' : 'PM');
}

function GetTime(timeStr) {

    if (timeStr == "" || timeStr == null) return null;
    var DBDate = new Date(parseInt(timeStr.substr(6)));
    var datetime = ("0" + DBDate.getHours()).slice(-2) + ":" + ("0" + DBDate.getMinutes()).slice(-2);
    return datetime;
}
function SetTime(timeStr) {

    if (timeStr == "" || timeStr == null) return null;
    var currentdate = new Date();
    var datetime = currentdate.getFullYear() + "-" + (currentdate.getMonth() + 1) + "-" + currentdate.getDate() + " " + timeStr;
    return datetime;
}


// Message alert box (MsgBox)

function alert(title, content, parent, subparent) {

            $.msgBox({
                title: title,
                content:content,
                parent: parent,
                subparent : subparent
            });

}

function AlertMessage(title, content) {
    $.msgBox({
        title: title,
        content: content,
    });
}

 function alertinfo(title, content, parent, subparent) {
            $.msgBox({
                title: title,
                content: content,
                parent: parent,
                subparent : subparent,
                type: "info"
            });

        }

   function alerterror(title, content, parent, subparent) {
            $.msgBox({
                title: title,
                content: content,
                parent: parent,
                subparent : subparent,
                type: "error",
                buttons: [{ value: "Ok"}],
            });

        }
  function alertconfirm(title, content, func, parent, subparent) {
           $.msgBox({
               title: title,
               content: content,
               parent: parent,
               subparent : subparent,
               type: "confirm",
               buttons: [{ value: "Yes" }, { value: "No" }],
               success: function (result) {
               if (result == "Yes") {
                  func();
               }
             }
         });
         
  }

  function alertconfirmyesno(title, content, func, Nofunc, parent, subparent) {
      $.msgBox({
          title: title,
          content: content,
          parent: parent,
          subparent: subparent,
          type: "confirm",
          buttons: [{ value: "Yes" }, { value: "No" }],
          success: function (result) {
              if (result == "Yes") {
                  func();
              }
              else if (result == "No") {
                  Nofunc();
              }
          }
      });

  }


  function AlertConfirmMessage(title, content, func) {
      $.msgBox({
          title: title,
          content: content,
          type: "confirm",
          buttons: [{ value: "Yes" }, { value: "No" }],
          success: function (result) {
              if (result == "Yes") {
                  func();
              }
          }
      });

  }

  function AlertConfirmMessage(title, content, func, func1) {
      $.msgBox({
          title: title,
          content: content,
          type: "confirm",
          buttons: [{ value: "Yes" }, { value: "No" }],
          success: function (result) {
              if (result == "Yes") {
                  func();
              }
              else if (result == "No") {
                  func1();
              }
          }
      });

  }


function fireEvent(obj, evt) {

    var fireOnThis = obj;
    if (document.createEvent) {
        var evObj = document.createEvent('MouseEvents');
        evObj.initEvent(evt, true, false);
        fireOnThis.dispatchEvent(evObj);
    } else if (document.createEventObject) {
        fireOnThis.fireEvent('on' + evt);
    }
}

//input value accepted number only
var ch = false;
var nums = false;
function isNumberKey(evt) {
    var charCode = ((evt.which) ? evt.which : event.keyCode);

    if (charCode == 46 && nums == true) { //&& ch == false
        ch = true;
        return true;
    }
    else {
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        } else {
            nums = true;
            return true;
        }
    }
}

function toggle(obj1, obj2) {
    var ele = obj1;
    var text = obj2;
    if (ele.style.display == "block") {
        ele.style.display = "none";
    }
    else {
        ele.style.display = "block";
    }
} 


function numbersonly(obj, e, decimal) {
    var key;
    var keychar;
    var phoneVal = obj.value;


    switch (phoneVal.length) {
        case 3:
            obj.value = phoneVal + "-";
            break;
        case 7:
            obj.value = phoneVal + "-";
            break;
    }

    if (window.event) {
        key = window.event.keyCode;
    }
    else if (e) {
        key = e.which;
    }
    else {
        return true;
    }
    keychar = String.fromCharCode(key);


    if ((key == null) || (key == 0) || (key == 9) || (key == 13) || (key == 27)) {
        return true;
    }

    else if (key == 8) {
        obj.value = "";
        return true;
    }
    else if ((("0123456789").indexOf(keychar) > -1)) {
        return true;
    }
    else if (decimal && (keychar == ".")) {
        return false;
    }
    else
        return false;
}

function ssnonly(obj, e, decimal) {
    var key;
    var keychar;
    var SSNVal = obj.value;


    switch (SSNVal.length) {
        case 3:
            obj.value = SSNVal + "-";
            break;
        case 6:
            obj.value = SSNVal + "-";
            break;
    }

    if (window.event) {
        key = window.event.keyCode;
    }
    else if (e) {
        key = e.which;
    }
    else {
        return true;
    }
    keychar = String.fromCharCode(key);


    if ((key == null) || (key == 0) || (key == 9) || (key == 13) || (key == 27)) {
        return true;
    }

    else if (key == 8) {
        obj.value = "";
        return true;
    }
    else if ((("0123456789").indexOf(keychar) > -1)) {
        return true;
    }
    else if (decimal && (keychar == ".")) {
        return false;
    }
    else
        return false;
}




function validateNumeric(obj, e, decimal) {
    if (window.event) {
        key = window.event.keyCode;
    }
    else if (e) {
        key = e.which;
    }
    else {
        return true;
    }
    keychar = String.fromCharCode(key);


    if ((key == null) || (key == 0) || (key == 9) || (key == 13) || (key == 27)) {
        return true;
    }
    //if (keychar == 'x') {
    //    if (phoneVal.length == 12)
    //        obj.value = phoneVal + "X";

    //}
    //if (keychar == 'X') {
    //    if (phoneVal.length == 12)
    //        obj.value = phoneVal + "X";

    //}
    else if (key == 8) {
        obj.value = "";
        return true;
    }
    else if ((("0123456789").indexOf(keychar) > -1)) {
        return true;
    }
    else if (decimal && (keychar == ".")) {
        return false;
    }
    else
        return false;
}



//**** allow text n numbers ( No special characters ) ****//

function alphanumeric_only(e) {

    var keycode;
    if (window.event) keycode = window.event.keyCode;

    else if (event) keycode = event.keyCode;
    else if (e) keycode = e.which;

    else return true; if ((keycode >= 48 && keycode <= 57) || (keycode >= 65 && keycode <= 90) || (keycode >= 97 && keycode <= 122)) {

        return true;
    }

    else {

        return false;
    }

    return true;
}


//**** TIME FORMAT ******//

function isTime(obj, evt) {


    var phoneVal = obj.value;

    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }


    else {
        switch (phoneVal.length) {
            case 0:
                if (charCode > 50) {
                    return false;
                }
                break;
            case 1:
                if (phoneVal == 2) {
                    if (charCode > 51) {
                        return false;
                    }
                }
                break;
            case 2:
                obj.value = phoneVal + ":";
                if (charCode > 53) {
                    return false;
                }

                break;

            case 3:
                if (charCode > 53) {
                    return false;
                }
                break;
        }


        return true;
    }
}


/***DATE FORMAT(mm/dd/yyyy)***/

function isDate(obj, evt) {


    var phoneVal = obj.value;

    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }


    else {
        switch (phoneVal.length) {
            case 0:
                if (charCode > 49) {
                    return false;
                }
                break;
            case 1:
                if (phoneVal == 1) {
                    if (charCode > 50) {
                        return false;
                    }
                }

                break;
            case 2:
                
                if (charCode > 51) {
                    return false;
                }
                obj.value = phoneVal + "/";
                break;

            case 4:
               
                var a = phoneVal.substr(3, 3);
             
                if (a == 3) {
                    if (charCode > 49) {
                        return false;
                    }
                }
                break;
            case 5:
                obj.value = phoneVal + "/";
                break;
          
        }


        return true;
    }
}


//**** Only Numbers *****//

function numbers(e) {

    var keycode;
    if (window.event) keycode = window.event.keyCode;

    else if (event) keycode = event.keyCode;
    else if (e) keycode = e.which;

    else return true; if ((keycode >= 48 && keycode <= 57)) {

        return true;
    }

    else {

        return false;
    }

    return true;
}

/***Taxpayer Phone Validate)***/

function taxphone(obj, evt) {


    var phoneVal = obj.value;

    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }


    else {
        switch (phoneVal.length) {
           
            case 3:

                obj.value = phoneVal + "-";
                break;

           

        }


        return true;
    }
}



//**** Only Text *****//


function onlytext(e) {

    var keycode;
    if (window.event) keycode = window.event.keyCode;

    else if (event) keycode = event.keyCode;
    else if (e) keycode = e.which;

    else return true; if ((keycode >= 65 && keycode <= 90) || (keycode >= 97 && keycode <= 122) || (keycode == 32)) {

        return true;
    }

    else {

        return false;
    }

    return true;
}

//**** Numbers with decimal *****//
var dec = false;
function decimalnumbers(e) {

    var keycode;
    if (window.event) keycode = window.event.keyCode;

    else if (event) 
    keycode = event.keyCode;
    else if (e)
     keycode = e.which;

    else 
    return true; 
    if ((keycode >= 48 && keycode <= 57) || keycode == 46)
     {
      /*  if(keycode == 46 && dec == false)
        {
            dec = true;
            return true;
        }
        else  if(keycode == 46 && dec == true)
        {
            return false;
        } */
        return true;
    }

    else
     {

        return false;
    }

    return true;
}




//** Validation Email **//

function ValidateEmail(inputText) {
    //var reg = /^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$/;
    var reg = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    //var reg = /^[A-Z|0-9][A-Z0-9._%+-]*[A-Z0-9]+@[A-Z|0-9][A-Z0-9._%+-]*([-][A-Z|0-9]+)*\.([A-Z][A-Z|0-9]*(\.[A-Z][A-Z|0-9]*)?)$/i;
    if (reg.test(inputText)) {
        return true;
    }
    else {
        return false;
    }
}

function myTrim(x) {
    return x.replace(/^\s+|\s+$/gm, '');
}

function isValidDate(dateString) {
    var dtRegex = new RegExp(/\b\d{1,2}[\/-]\d{1,2}[\/-]\d{4}\b/);
    return dtRegex.test(dateString);

}

function send_email(inputText) {
    var subject = "";

    if (inputText != null && inputText != "") {

        document.location.href = "mailto:" + inputText + "?subject=" + subject + "&body=" + subject;

    }

}

function isValidPeriod(dateString) {
    //var year = dateString.substr(0, 4);
    var month = dateString.substr(4, 2);
    //if ((year < 1900) || (year > new Date().getFullYear())) {
    //    return false;
    //}
    if ((month < 1) || (month > 12)) {
        return false;
    }

}

/* to accept year only for periods eg: 06-07  */
function periods(obj, e, decimal) {
    var keycode;
    if (window.event) keycode = window.event.keyCode;
    else if (event) keycode = event.keyCode;
    else if (e) keycode = e.which;
    else return true;
    if (keycode == 45) {
        return true;
    }
    else if ((keycode >= 48 && keycode <= 57)) {
        periodsplit(obj, e, decimal);
        return true;
    }
    else {
        return false;
    }
    return true;
}
function periodsplit(obj, e, decimal) {
    var periodVal = obj.value;
    switch (periodVal.length) {
        case 2:
            obj.value = periodVal + "-";
            break;
    }
}

/* to accept year only for periods eg: 06-07  */
function periods(obj, e, decimal) {
    var keycode;
    if (window.event) keycode = window.event.keyCode;
    else if (event) keycode = event.keyCode;
    else if (e) keycode = e.which;
    else return true;
    if (keycode == 45) {
        return true;
    }
    else if ((keycode >= 48 && keycode <= 57)) {
        periodsplit(obj, e, decimal);
        return true;
    }
    else {
        return false;
    }
    return true;
}
function periodsplit(obj, e, decimal) {
    var periodVal = obj.value;
    switch (periodVal.length) {
        case 2:
            obj.value = periodVal + "-";
            break;
    }
}

function numberswithdot(obj, e, decimal, pos) {
    var key;
    var keychar;
    var numVal = obj.value;
    var clsindex = numVal.indexOf(".");
    if (clsindex <= -1) {
        switch (numVal.length) {
            case pos:
                obj.value = numVal + ".";
                break;

        }
    }

    if (window.event) {
        key = window.event.keyCode;
    }
    else if (e) {
        key = e.which;
    }
    else {
        return true;
    }
    keychar = String.fromCharCode(key);


    if ((key == null) || (key == 0) || (key == 9) || (key == 13) || (key == 27)) {
        return true;
    }

    else if (key == 8) {
        obj.value = "";
        return true;
    }
    else if ((("0123456789").indexOf(keychar) > -1)) {
        return true;
    }
    else if (decimal && (keychar == ".") && clsindex > -1) {
        return false;
    }
    else if (decimal && (keychar == ".") && clsindex <= -1) {
        return true;
    }
    else
        return false;
}

function CCcheck(obj, evt) {

    var phoneVal = obj.value;

    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }


    else {
        switch (phoneVal.length) {

            case 4:

                obj.value = phoneVal + "-";
                break;
            case 9:

                obj.value = phoneVal + "-";
                break;
            case 14:

                obj.value = phoneVal + "-";
                break;

        }
        return true;
    }
}