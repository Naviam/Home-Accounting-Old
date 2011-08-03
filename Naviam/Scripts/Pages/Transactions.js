/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
//JSON.stringify
var transModel = {
    paging: { Page: 1, SortField: 'Date', SortDirection: 1 }
};
ko.bindingHandlers.numeric = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //handle the field changing
        ko.utils.registerEventHandler(element, "change", function () {
            var observable = valueAccessor();
            var parsed = parseFloat($(element).val());
            var correct = !isNaN(parsed) && (parsed > 0);
            if (correct)
                observable(parsed)
            else
                $(element).val(observable()); //restore old
        });
    },
    update: function (element, valueAccessor) {
        $(element).val(ko.utils.unwrapObservable(valueAccessor()));
    }
};
ko.bindingHandlers.category = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //handle the field changing
        ko.utils.registerEventHandler(element, "change", function () {
            var observable = valueAccessor();
            var val = $(element).val();
            //find category
            var item = catModel.Search(val);
            if (item != null)
                observable(item.Name())
            else
                $(element).val(observable()); //restore old
        });
    },
    update: function (element, valueAccessor) {
        $(element).val(ko.utils.unwrapObservable(valueAccessor()));
    }
};
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
        var date = eval("new " + value.replace(/\//g, ''));
        $(element).data("dateinput").setValue(date);
    }
};
 //!!!replaced by bindingHandlers
//ko.numericObservable = function (initialValue) {
//    var _actual = ko.observable(initialValue);
//    var result = ko.dependentObservable({
//        read: function () {
//            return _actual();
//        },
//        write: function (newValue) {
//            var parsed = parseFloat(newValue);
//            var correct = !isNaN(parsed) && (parsed > 0);
//            if (!correct)
//                _actual.valueHasMutated();
//            else
//                _actual(parsed);
//        }
//    });
//    return result;
//};
//ko.categoryObservable = function (initialValue) {
//    var _actual = ko.observable(initialValue);
//    var result = ko.dependentObservable({
//        read: function () {
//            return _actual();
//        },
//        write: function (newValue) {
//            //find category
//            var item = catModel.Search(newValue);
//            if (item == null)
//                _actual.valueHasMutated();
//            _actual(item == null ? _actual() : item.Name());
//        }
//    });
//    return result;
//};
//!!!
$(document).ready(function () {
    $("#edit_form").overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true });
    $.postErr(getTransUrl, transModel.paging, function (res) {
        //!!!replaced by bindingHandlers
        //        var childItem = function (data) {
        //            ko.mapping.fromJS(data, {}, this);
        //            this.Amount = ko.numericObservable(data.Amount);
        //            this.Category = ko.categoryObservable(data.Category);
        //        }
        //!!!
        var mapping = {
            'items': {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                }
                //!!!replaced by bindingHandlers
                //                    , create: function (options) {
                //                       return new childItem(options.data, {}, this);
                //                    }
                //!!!
            }
        }
        transModel = ko.mapping.fromJS(res, mapping);
        transModel.paging.Page.subscribe(function (newValue) {
            transModel.ReloadPage();
        });
        transModel.selectedRow = ko.observable(-1);
        transModel.currentItem = null;
        transModel.ReloadPage = function () {
            if (this.DescrSub != null)
                this.DescrSub.dispose();
            $.postErr(getTransUrl, transModel.paging, function (res) {
                ko.mapping.updateFromJS(transModel, res);
                transModel.selectedRow(-1);
                transModel.currentItem = null;
                //$('#edit_row').hide();
            });
        }
        transModel.showCalendar = function (event, item) {
            //var input = $(event.currentTarget).parent().find('[name="date1"]');
            //           
            //            DisableBeforeToday = false;
            //            if (lang.culture == 'ru') {
            //                DateSeparator = '.';
            //                NewCssCal(input, item.Date, 'ddMMyyyy', 'arrow');
            //            }
            //            if (lang.culture == 'en') {
            //                DateSeparator = '/';
            //                NewCssCal(input, item.Date, 'MMddyyyy', 'arrow');
            //            }
        }
        transModel.Add = function () {
            var fItem = ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == null;
            });
            if (fItem != null) return;
            var date = new Date();
            this.items.splice(0, 0, { Id: ko.observable(null), Description: ko.observable(null), Category: ko.observable(null), CategoryId: ko.observable(null), Amount: ko.observable(0),
                Date: ko.observable('/Date(' + date.getTime() + ')/')
            });
            //this.currentItem = this.items[0];
            this.GoToEdit(null, this.items()[0]);
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

            //console.log();
            //$("#edit_form").overlay().close();
            item.FullRow = ko.dependentObservable(function () {
                return this.Description() + "_" + this.Category() + "_" + this.Amount() + this.Date();
            }, item);
            if (this.DescrSub != null)
                this.DescrSub.dispose();
            this.DescrSub = item.FullRow.subscribe(function (newValue) {
                transModel.Save();
            });
            this.currentItem = item;
            this.selectedRow(item.Id());
        }
        //obj.date = eval(obj.date.replace(/\//g,'')) -- to convert the download datestring after json to a javascript Date
        //obj.date = "\\/Date(" + obj.date.getTime() + ")\\/" --to convert a javascript date to microsoft json:
        transModel.Save = function () {
            if (this.currentItem != null) {
                var cat = this.currentItem.Category();
                if (cat != null) {
                    var item = catModel.Search(cat);
                    if (item != null)
                        this.currentItem.CategoryId(item.Id());
                }
                //transModel.currentItem.Date = transModel.currentItem.Date().replace('/Date(', '\\/Date(').replace(')/', ')\\/');
                //                    $.postErr(updateTransUrl, ko.toJSON(transModel.currentItem), function (res) {
                //                    }, 'json');
                $.postErr(updateTransUrl, ko.mapping.toJS(transModel.currentItem), function (res) {
                    transModel.currentItem.Id(res.Id);
                });
                //console.log(transModel.currentItem.Id());
            }
        }
        transModel.ShowEditArea = function (event, item) {
            //TODO: load data into form
            $("#edit_form").overlay().load();
        }
        transModel.ShowCategories = function (btn) {
            var menu = $("#cat_menu");
            if (menu.css("display") != "none") {
                menu.hide();
                return;
            }
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
            if (!search) {
                return null;
            } else {
                search = search.toLowerCase();
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
