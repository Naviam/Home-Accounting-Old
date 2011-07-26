/// <reference path="jquery-1.6.2.js" />
jQuery.postErr = function (url, data, callback, type) {
    // shift arguments if data argument was omited
    if ($.isFunction(data)) {
        type = type || callback;
        callback = data;
        data = null;
    }
    $.ajax({
        type: "POST",
        url: url,
        data: data,
        complete: function () {
//            $("#saving").hide();
//            $("#loading").hide();
        },
        success: callback,
        dataType: type,
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (XMLHttpRequest.status == 500) {
                var errorObj = JSON.parse(XMLHttpRequest.responseText);
                showSiteError(errorObj.Text);
            }
            else {
                showSiteError(XMLHttpRequest.responseText);
            }
        }
    });
}
function showSiteError(errorText) {
    alert(errorText);
}