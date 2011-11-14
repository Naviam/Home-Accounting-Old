/// <reference path="jquery-1.6.2.js" />
var unitTest = false;
jQuery.postErr = function (url, data, callback, type) {
    // shift arguments if data argument was omited
    if ($.isFunction(data)) {
        type = type || callback;
        callback = data;
        data = null;
    }
    if (data == null)
        data = {};
    if (typeof pageContext != 'undefined' && pageContext != null) {
        if (type == 'json' && typeof data === "string") {
            var ins = ',"pageContext":' + JSON.stringify(pageContext) + '}';
            data = data.substr(0, data.length - 1) + ins;
        }
        else data.pageContext = pageContext;
    }
    var contentType = type == 'json' ? "application/json; charset=utf-8" : "application/x-www-form-urlencoded";
    $.ajax({
        type: "POST",
        url: url,
        data: data,
        complete: function () {
            unblockWindow();
            //            $("#saving").hide();
            //            $("#loading").hide();
        },
        success: callback,
        contentType: contentType,

        dataType: type,
        error: function (xmlHttpRequest, textStatus, errorThrown) {
            parseSiteError(xmlHttpRequest);
        }
    });
};
$('input.enterastab, select.enterastab, textarea.enterastab').live('keydown', function (e) {
    if (e.keyCode == 13) {
        var focusable = $('input,select,button,textarea').filter(':visible');
        $(this).trigger("change");
        focusable.eq(focusable.index(this) + 1).focus();
        return false;
    }
});
function askToUser(text, callback) {
    if (confirm(text)) callback();
}
function parseSiteError(request) {
    if (request.status == 500) {
        var errorObj = JSON.parse(request.responseText);
        showSiteError(errorObj.Text);
    }
    else {
        showSiteError(request.responseText);
    }
}
function showSiteError(errorText) {
    alert(errorText);
}
function debug(s) {
    console.log(s);
}
function viewport() {
	return [$(document).width(), $(document).height()]; 
}
function blockWindow() {
    // get the div
    hld = $(".win_blocker");
    if (!hld.length) {
        hld = $('<div/>').attr("class", 'win_blocker');
        $("body").append(hld);
    }
	var size = viewport();
    hld.css({
        top: 0,
        left: 0,
        width: size[0],
        height: size[1]
    });
    hld.show();
}
function unblockWindow() {
    $('.win_blocker').hide();
}
function addCommas(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}
String.prototype.capitalize = function () {
    return this.charAt(0).toUpperCase() + this.slice(1);
}
$.format = function (source, params) {
    if (arguments.length == 1)
        return function () {
            var args = $.makeArray(arguments);
            args.unshift(source);
            return $.format.apply(this, args);
        };
    if (arguments.length > 2 && params.constructor != Array) {
        params = $.makeArray(arguments).slice(1);
    }
    if (params.constructor != Array) {
        params = [params];
    }
    $.each(params, function (i, n) {
        source = source.replace(new RegExp("\\{" + i + "\\}", "g"), n);
    });
    return source;
};
//!!!!!Date format
//now.format("m/dd/yy");
// Returns, e.g., 6/09/07
// Can also be used as a standalone function
//dateFormat(now, "dddd, mmmm dS, yyyy, h:MM:ss TT");
// Saturday, June 9th, 2007, 5:46:21 PM
var dateFormat = function () {
    var token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g,
		timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g,
		timezoneClip = /[^-+\dA-Z]/g,
		pad = function (val, len) {
		    val = String(val);
		    len = len || 2;
		    while (val.length < len) val = "0" + val;
		    return val;
		};

    // Regexes and supporting functions are cached through closure
    return function (date, mask, utc) {
        var dF = dateFormat;

        // You can't provide utc if you skip other args (use the "UTC:" mask prefix)
        if (arguments.length == 1 && Object.prototype.toString.call(date) == "[object String]" && !/\d/.test(date)) {
            mask = date;
            date = undefined;
        }

        // Passing date through Date applies Date.parse, if necessary
        date = date ? new Date(date) : new Date;
        if (isNaN(date)) throw SyntaxError("invalid date");

        mask = String(dF.masks[mask] || mask || dF.masks["default"]);

        // Allow setting the utc argument via the mask
        if (mask.slice(0, 4) == "UTC:") {
            mask = mask.slice(4);
            utc = true;
        }

        var _ = utc ? "getUTC" : "get",
			d = date[_ + "Date"](),
			D = date[_ + "Day"](),
			m = date[_ + "Month"](),
			y = date[_ + "FullYear"](),
			H = date[_ + "Hours"](),
			M = date[_ + "Minutes"](),
			s = date[_ + "Seconds"](),
			L = date[_ + "Milliseconds"](),
			o = utc ? 0 : date.getTimezoneOffset(),
			flags = {
			    d: d,
			    dd: pad(d),
//			    ddd: dF.i18n.dayNames[D],
//			    dddd: dF.i18n.dayNames[D + 7],
			    m: m + 1,
			    mm: pad(m + 1),
//			    mmm: dF.i18n.monthNames[m],
//			    mmmm: dF.i18n.monthNames[m + 12],
			    yy: String(y).slice(2),
			    yyyy: y,
			    h: H % 12 || 12,
			    hh: pad(H % 12 || 12),
			    H: H,
			    HH: pad(H),
			    M: M,
			    MM: pad(M),
			    s: s,
			    ss: pad(s),
			    l: pad(L, 3),
			    L: pad(L > 99 ? Math.round(L / 10) : L),
			    t: H < 12 ? "a" : "p",
			    tt: H < 12 ? "am" : "pm",
			    T: H < 12 ? "A" : "P",
			    TT: H < 12 ? "AM" : "PM"
//			    Z: utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""),
//			    o: (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4),
//			    S: ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 != 10) * d % 10]
			};

        return mask.replace(token, function ($0) {
            return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1);
        });
    };
} ();
// Some common format strings
dateFormat.masks = {
    "default": "ddd mmm dd yyyy HH:MM:ss",
    enDateTime: "mm/dd/yyyy h:MM:ss TT",
    ruDateTime: "dd.mm.yyyy HH:MM:ss",
    enDate: "mm/dd/yyyy",
    ruDate: "dd.mm.yyyy"
//    shortDate: "m/d/yy",
//    mediumDate: "mmm d, yyyy",
//    longDate: "mmmm d, yyyy",
//    fullDate: "dddd, mmmm d, yyyy",
//    shortTime: "h:MM TT",
//    mediumTime: "h:MM:ss TT",
//    longTime: "h:MM:ss TT Z",
//    isoDate: "yyyy-mm-dd",
//    isoTime: "HH:MM:ss",
//    isoDateTime: "yyyy-mm-dd'T'HH:MM:ss",
//    isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'"
};
// For convenience...
Date.prototype.format = function (mask, utc) {
    return dateFormat(this, mask, utc);
};
// Internationalization strings
//dateFormat.i18n = {
//    dayNames: [
//		"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
//		"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
//	],
//    monthNames: [
//		"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
//		"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
//	]
//};
var inputCssError = 'input-validation-error';
var lang = Naviam.JavaScript;
lang.firstDay = 0;
if (lang.culture == 'ru') {
    dateFormat.masks["default"] = dateFormat.masks.ruDate;
    lang.firstDay = 1;
}
if (lang.culture == 'en')
    dateFormat.masks["default"] = dateFormat.masks.enDate;
//var MonthName = [lang.January, lang.February, lang.March, lang.April, lang.May, lang.June, lang.July,
//    lang.August, lang.September, lang.October, lang.November, lang.December];
//var WeekDayName2 = [lang.MondayS, lang.TuesdayS, lang.WednesdayS, lang.ThursdayS, lang.FridayS, lang.SaturdayS, lang.SundayS];
