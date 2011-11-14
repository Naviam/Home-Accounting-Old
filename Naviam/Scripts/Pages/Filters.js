/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />
var filterModel = {}; 
//props
filterModel.items = ko.observableArray();
//methods
filterModel.toString = function () {
    return ko.toJSON(this.items);
};
filterModel.Add = function (key, value, showName, showValue, type) {
    var fItem = ko.utils.arrayFirst(this.items(), function (item) {
        return item.Name == key;
    });
    if (!fItem)
        this.items.push({ Name: key, Value: ko.observable(value), Type: type, ShowName: showName, ShowValue: ko.observable(showValue) });
    else {
        fItem.Value(value);
        fItem.Type = type;
        fItem.ShowName = showName;
        fItem.ShowValue(showValue);
    }
};
filterModel.Clear = function () {
    this.items([]);
}
filterModel.removeItem = function (item) {
    if (item) {
        ko.utils.arrayRemoveItem(filterModel.items, item);
        if (item.Name == 'TagId') {
            catModel.selectedTag(null);
            catModel.editedTag(null);
        }
        transModel.ReloadPage();
    }
}
filterModel.removeAll = function () {
    filterModel.Clear();
    transModel.ReloadPage();
}
filterModel.deleteByKey = function (key) {
    var fItem = ko.utils.arrayFirst(this.items(), function (item) {
        return item.Name == key;
    });
    if (fItem)
        ko.utils.arrayRemoveItem(this.items, fItem);
}