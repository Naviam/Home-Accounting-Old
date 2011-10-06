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
ko.bindingHandlers.notEmpty = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //handle the field changing
        var checkElem = function () {
            var observable = valueAccessor();
            var val = $(element).val();
            var correct = val != null && val != '';
            if (correct)
                observable(val);
            else
                $(element).val(observable()); //restore old
        };
        ko.utils.registerEventHandler(element, "change", checkElem);
    },
    update: function (element, valueAccessor) {
        $(element).val(ko.utils.unwrapObservable(valueAccessor()));
    }
};