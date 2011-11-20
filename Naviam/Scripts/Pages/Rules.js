/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />

var rulesModel = {isLoaded: false};
//props
//methods
rulesModel.Load = function (callback) {
    var $this = this;
    $.postErr(getRulesUrl, function (res) {
        if (!$this.isLoaded) {
            var childItem = function (data) {
                ko.mapping.fromJS(data, {}, this);
                var catItem = catModel.itemById(data.FieldValue);
                var catName = catItem != null ? catItem.Name() : '';
                this.Category = ko.observable(catName);
            };
            var mapping = {
                'items': {
                    key: function (data) {
                        return ko.utils.unwrapObservable(data.Id);
                    }
                    , create: function (options) {
                        return new childItem(options.data, {}, this);
                    }
                }
            };
            $this.srv = ko.mapping.fromJS(res, mapping);
            ko.applyBindings($this.srv, $('#rulesDialog')[0]);
            $this.isLoaded = true;
        } else {
            ko.mapping.updateFromJS($this.srv, res);
        }
        $('.suggestCat').autocomplete(catModel.Suggest(), { minChars: 1, delay: 10 });
        if (typeof callback != 'undefined')
            callback();
    });
};
rulesModel.Refresh = function () {
    if (!this.isLoaded) return;
    rulesModel.Load();
};
rulesModel.DeleteItem = function (item) {
    var $this = this;
    askToUser(lang.DeleteRule, function () {
        $.postErr(delRuleUrl, { id: item.Id() }, function (res) {
            if (res != null) {
                ko.utils.arrayRemoveItem($this.srv.items, item);
            }
        });
    });
}
rulesModel.EditItem = function (item) {
    var cat = item.Category();
    if (!item.Field() || !item.FieldTargetValue() || (!item.FieldValue() && item.Field() != 'id_category')) return;
    if (item.Field() == 'id_category') {
        if (!cat) return;
        var c_item = catModel.Search(cat);
        if (!c_item) return;
        item.FieldValue(c_item.Id());
    }
    $.postErr(updateRuleUrl, item, function (res) {
        item.Id(res.Id);
    });
}
rulesModel.AddItem = function () {
    var fItem = ko.utils.arrayFirst(this.items(), function (item) {
        return item.Id() == null;
    });
    if (fItem != null) return;
    fItem = ko.mapping.fromJS(ko.mapping.toJS(this.ruleTemplate));
    fItem.Category = ko.observable();
    fItem.UserId(1);
    this.items.push(fItem);
    $('.suggestCat').autocomplete(catModel.Suggest(), { minChars: 1, delay: 10 });
}

var dialogs = {};
dialogs.showDialog = function (holder, url, model) {
    var hld = $(holder);
    if (hld.html() == '') {
        $.postErr(url, function (res) {
            hld.html(res);
            model.Load(function () {
                hld.overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true, closeIcon: true });
                hld.overlay().load();
            });
        });
    }
    else
        hld.overlay().load();
}