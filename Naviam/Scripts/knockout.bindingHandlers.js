/// <reference path="jquery-1.6.2.js" />
/// <reference path="knockout-1.2.1.js" />

ko.bindingHandlers.msDateTime = {
    init: function (element, valueAccessor, allBindingsAccessor) {
    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (value != null) {
            var date = eval("new " + value.replace(/\//g, ''));
            var val = date.format();
            $(element).text(val);
        }
    }
};
ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor, context) {
        //initialize datepicker with some optional options
        var options = allBindingsAccessor().datepickerOptions || {};
        $(element).dateinput(options);
        //handle the field changing
        ko.utils.registerEventHandler(element, "change", function () {
            var observable = valueAccessor();
            //console.log();
            observable('/Date(' + $(element).data("dateinput").getValue().getTime() + ')/');
        });
        //ko.bindingHandlers.visible.init(element, valueAccessor, allBindingsAccessor, context);
    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (value != null) {
            var date = eval("new " + value.replace(/\//g, ''));
            $(element).data("dateinput").setValue(date);
        }
    }
};