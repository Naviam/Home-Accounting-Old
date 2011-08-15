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