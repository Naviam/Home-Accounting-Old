/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
//JSON.stringify
var transModel = {
    paging: { Page: 1, SortField: 'Date', SortDirection: 1 }
};
ko.numericObservable = function (initialValue) {
    var _actual = ko.observable(initialValue);
    var result = ko.dependentObservable({
        read: function () {
            return _actual();
        },
        write: function (newValue) {
            var parsed = parseFloat(newValue);
            if (isNaN(parsed))
                _actual.valueHasMutated();
            _actual(isNaN(parsed) ? _actual() : parsed);
        }
    });
    return result;
};
ko.categoryObservable = function (initialValue) {
    var _actual = ko.observable(initialValue);
    var result = ko.dependentObservable({
        read: function () {
            return _actual();
        },
        write: function (newValue) {
            //find category
            var item = catModel.Search(newValue);
            if (item == null)
                _actual.valueHasMutated();
            _actual(item == null ? _actual() : item.Name());
        }
    });
    return result;
};
$(document).ready(function () {
    $.postErr(getTransUrl, transModel.paging, function (res) {
        var childItem = function (data) {
            ko.mapping.fromJS(data, {}, this);
            this.Amount = ko.numericObservable(data.Amount);
            this.Category = ko.categoryObservable(data.Category);
        }
        var mapping = {
            'items': {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                }
                    , create: function (options) {
                        return new childItem(options.data, {}, this);
                    }
            }
        }
        transModel = ko.mapping.fromJS(res, mapping);
        transModel.paging.Page.subscribe(function (newValue) {
            transModel.ReloadPage();
        });
        transModel.selectedRow = ko.observable(-1);
        transModel.currentItem = null;
        transModel.ReloadPage = function () {
            $.postErr(getTransUrl, transModel.paging, function (res) {
                ko.mapping.updateFromJS(transModel, res);
                transModel.selectedRow(-1);
                transModel.currentItem = null;
                $('#edit_row').hide();
            });
        }
        transModel.DescrSub = null;
        transModel.GoToEdit = function (event, item) {
            //manual edit row
            //                    var row = $(event.currentTarget)
            //                    var rowEdit = $('#edit_row');
            //                    rowEdit.css({ top: row.offset().top, width: row.width(), height: row.height() });
            //                    rowEdit.find('.edit_area').slideUp();
            //                    rowEdit.find('[name="Description"]').val(item.Description());
            //                    rowEdit.show();
            //**********

            //console.log(event);
            //item.Amount = ko.numericObservable(item.Amount());
            item.FullRow = ko.dependentObservable(function () {
                return this.Id() + "_" + this.Description() + "_" + this.Category() + "_" + this.Amount();
            }, item);
            this.selectedRow(item.Id());
            this.currentItem = item;
            if (this.DescrSub != null)
                this.DescrSub.dispose();
            this.DescrSub = item.FullRow.subscribe(function (newValue) {
                transModel.Save();
            });
        }
        //obj.date = eval(obj.date.replace(/\//g,'')) -- to convert the download datestring after json to a javascript Date
        //obj.date = "\\/Date(" + obj.date.getTime() + ")\\/" --to convert a javascript date to microsoft json:
        transModel.Save = function () {
            if (this.currentItem != null) {
                var item = catModel.Search(this.currentItem.Category());
                if (item != null)
                    this.currentItem.CategoryId(item.Id());
                //transModel.currentItem.Date = transModel.currentItem.Date().replace('/Date(', '\\/Date(').replace(')/', ')\\/');
                //                    $.postErr(updateTransUrl, ko.toJSON(transModel.currentItem), function (res) {
                //                    }, 'json');
                $.postErr(updateTransUrl, transModel.currentItem, function (res) {
                });
                //console.log(transModel.currentItem.Id());
            }
        }
        transModel.ShowEditArea = function (btn) {
            var editArea = $(btn).parent().find('.edit_area');
            //TODO: load data into form
            editArea.slideDown();
        }
        transModel.ShowCategories = function (btn) {
            var menu = $("#cat_menu");
            var input = $(btn).parent().find('[name="Category"]');
            menu.css({ top: input.offset().top + 20, left: input.offset().left });
            menu.width(input.width());
            $("#cat_menu ul").width(input.width());
            menu.slideDown();
        }
        transModel.Sort = function (val) {
            var currSort = this.paging.SortField();
            if (currSort != null)
                if (val == currSort) {
                    if (this.paging.SortDirection() == 0)
                        this.paging.SortDirection(1);
                    else
                        this.paging.SortDirection(0);
                }
                else
                    this.paging.SortDirection(0);
            this.paging.SortField(val)
            this.ReloadPage();
        }
        ko.applyBindings(transModel, $("#transGrid")[0]);
    });
    //Gategories
    $.postErr(getCatUrl, function (res) {
        catModel = ko.mapping.fromJS(res);
        catModel.Search = function (search) {
            search = search.toLowerCase();
            if (!search) {
                return null;
            } else {
                for (var i = 0, j = this.items().length; i < j; i++) {
                    var item = this.items()[i];
                    if (item.Name().toLowerCase() == search)
                        return item;
                    var fItem = ko.utils.arrayFirst(item.Subitems(), function (item) {
                        return item.Name().toLowerCase() == search;
                    });
                    if (fItem != null)
                        return fItem;
                }
            }
        };
        catModel.AssignCategory = function (item) {
            if (transModel.currentItem != null) {
                transModel.currentItem.Category(item.Name());
                $("#cat_menu").hide();
            }
        };
        ko.applyBindings(catModel, $("#cat_menu")[0]);
        ddsmoothmenu.init({
            mainmenuid: "cat_menu", //menu DIV id
            orientation: 'v', //Horizontal or vertical menu: Set to "h" or "v"
            classname: 'ddsmoothmenu-v' //class added to menu's outer DIV
        })
        $("#cat_menu").hide();
    });
    $("#cat_menu").hover(function () {
    }, function () {
        $("#cat_menu").hide();
        $("#cat_menu ul li ul").hide();
    });
});
