/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />
//JSON.stringify
var transModel = {
    paging: { Page: 1, SortField: 'Date', SortDirection: 1 }
};
ko.bindingHandlers.amount = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //handle the field changing
        ko.utils.registerEventHandler(element, "change", function () {
            var observable = valueAccessor();
            var parsed = parseFloat($(element).val().replace(/[^\d.]+/g, ""))
            var correct = !isNaN(parsed) && (parsed > 0);
            if (correct)
                observable(parsed)
            else 
                $(element).val(observable()); //restore old
        });
    },
    update: function (element, valueAccessor) {
        var val = addCommas(ko.utils.unwrapObservable(valueAccessor()));
        $(element).val(val);
        $(element).text(val);
    }
};
ko.bindingHandlers.category = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //handle the field changing
        var checkElem = function () {
            var observable = valueAccessor();
            var val = $(element).val();
            //find category
            var item = catModel.Search(val);
            if (typeof item != 'undefined' && item != null)
                observable(item.Name())
            else
                $(element).val(observable()); //restore old
        }
        ko.utils.registerEventHandler(element, "change", checkElem);
        ko.utils.registerEventHandler(element, "result", checkElem);
    },
    update: function (element, valueAccessor) {
        $(element).val(ko.utils.unwrapObservable(valueAccessor()));
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
function loadTransactions() {
    $.postErr(getTransUrl, transModel.paging, function (res) {
        var childItem = function (data) {
            ko.mapping.fromJS(data, {}, this);
            var catItem = catModel.itemById(data.CategoryId);
            var catName = catItem != null ? catItem.Name() : '';
            this.Category = ko.observable(catName);
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
        transModel.selectedItem = ko.observable(null);
        transModel.selectedRow = ko.observable(null);
        transModel.editObj = null;
        transModel.ReloadPage = function () {
            if (this.DescrSub != null)
                this.DescrSub.dispose();
            $.postErr(getTransUrl, transModel.paging, function (res) {
                ko.mapping.updateFromJS(transModel, res);
                transModel.selectedItem(null);
                transDlg.close();
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
        transModel.ShowDialog = function () {
            var row = this.selectedRow();
            var frm = $("#transDlg");
            frm.css({ width: row.width() });
            var conf = transDlg.getConf();
            //conf.top = row.offset().top + row.height();
            //conf.left = row.offset().left - 5;
            conf.top = row.parents('table')[0].offsetTop + row[0].offsetTop + row.height();
            conf.left = -5;
            frm.overlay().load();
        }
        transModel.Add = function () {
            var fItem = ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == null;
            });
            if (fItem != null) return;
            transDlg.close();
            this.editObj = null;
            var date = new Date();
            this.items.splice(0, 0, { Id: ko.observable(null), Description: ko.observable(null), Category: ko.observable(null), CategoryId: ko.observable(null), Amount: ko.observable(0),
                Date: ko.observable('/Date(' + date.getTime() + ')/'), Direction: ko.observable(1), Notes: ko.observable(null), Merchant: ko.observable(null), Direction: ko.observable(0)
            });
            var row = $('#transGrid table tr:eq(1)');
            ko.applyBindings(this.items()[0], $("#transDlg")[0]);
            this.GoToEdit(null, this.items()[0], row);
            this.ShowDialog();
        }
        transModel.CancelAdd = function () {
            var fItem = ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == null;
            });
            ko.utils.arrayRemoveItem(this.items, fItem);
        }
        transModel.DescrSub = null;
        transModel.GoToEdit = function (event, item, row) {
            //manual edit row
            //                    
            //                    var rowEdit = $('#edit_row');
            //                    rowEdit.css({ top: row.offset().top, width: row.width(), height: row.height() });
            //                    rowEdit.find('.edit_area').slideUp();
            //                    rowEdit.find('[name="Description"]').val(item.Description());
            //                    rowEdit.show();
            //**********

            //console.log();
            if (item != this.selectedItem()) transDlg.close();
            if (this.DescrSub != null)
                this.DescrSub.dispose();
            if (item.Id() != null) {
                item.FullRow = ko.dependentObservable(function () {
                    return this.Description() + "_" + this.Category() + "_" + this.Amount() + this.Date();
                }, item);
                this.DescrSub = item.FullRow.subscribe(function (newValue) {
                    transModel.Save(false);
                });
            }
            this.selectedItem(item);
            this.editObj2 = ko.mapping.toJS(transModel.selectedItem());
            if (event != null)
                row = $(event.currentTarget)
            this.selectedRow(row);
            $(row.find('[name="Category"]')).autocomplete(catModel.Suggest(), {
                minChars: 1
                //,matchContains: true //if minChars>1
            , delay: 10
            });
        }
        //obj.date = eval(obj.date.replace(/\//g,'')) -- to convert the download datestring after json to a javascript Date
        //obj.date = "\\/Date(" + obj.date.getTime() + ")\\/" --to convert a javascript date to microsoft json:
        transModel.Save = function (reloadPage) {
            var sItem = transModel.selectedItem();
            if (sItem != null) {
                var cat = sItem.Category();
                if (cat != null) {
                    var item = catModel.Search(cat);
                    if (item != null)
                        sItem.CategoryId(item.Id());
                }
                //                    $.postErr(updateTransUrl, ko.toJSON(transModel.currentItem), function (res) {
                //                    }, 'json');
                $.postErr(updateTransUrl, ko.mapping.toJS(sItem), function (res) {
                    //transModel.selectedItem().Id(res.Id);
                    //debug(transModel.editObj2);
                    var amount = (sItem.Direction() == transModel.editObj2.Direction) ? -(transModel.editObj2.Amount - sItem.Amount()) : sItem.Amount();
                    if (transModel.editObj2.Amount == 0) //add
                        amount = sItem.Amount(); 
                    amount = sItem.Direction() == 0 ? -amount : amount;
                    accountsModel.addAmount(res.AccountId, amount);
                    if (reloadPage) transModel.ReloadPage();
                });
                //console.log(transModel.currentItem.Id());
            }
        }
        transModel.ShowEdit = function (event, item) {
            this.selectedRow($(event.currentTarget).parents('tr'));
            this.editObj = ko.mapping.toJS(transModel.selectedItem());
            ko.applyBindings(this.editObj, $("#transDlg")[0]);
            this.ShowDialog();
            //$("#edit_form").overlay().load();
        }
        transModel.DeleteItem = function (item) {
            $.postErr(delTransUrl, { id: item.Id() }, function (res) {
                var amount = item.Direction() == 0 ? item.Amount() : -item.Amount();
                accountsModel.addAmount(item.AccountId(), amount);
                ko.utils.arrayRemoveItem(transModel.items, item);
            });
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
}
$(document).ready(function () {
    //$("#edit_form").overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true });
    //$("#transDlg #cancel").click(function (e) { transModel.CancelAdd(); });
    $("#transDlg #ok").click(function (e) {
        //TODO: check input
        //tst
        var row = transModel.selectedRow();
        var am = row.find('[name="Amount"]');
        var cat = row.find('[name="Category"]');
        am.removeClass(inputCssError);
        cat.removeClass(inputCssError);
        if (transModel.editObj != null) {
            //restore inline editing props
            var selItem = transModel.selectedItem();
            transModel.editObj.Amount = selItem.Amount();
            transModel.editObj.Category = selItem.Category();
            transModel.editObj.Description = selItem.Description();
            transModel.editObj.Date = selItem.Date();
            ko.mapping.fromJS(transModel.editObj, {}, selItem);
        }
        var item = transModel.selectedItem();
        //console.log(item);
        if (item.Amount() <= 0)
            am.addClass(inputCssError);
        if (item.Category() == null)
            cat.addClass(inputCssError);
        if (item.Amount() <= 0 || item.Category() == null)
            return e.stopImmediatePropagation();
        transModel.Save(transModel.editObj == null);
    });
    transDlg = $("#transDlg").overlay({ fixed: false }).data('overlay');
    transDlg.onBeforeClose(function (e) {
        //console.log(e);
        transModel.CancelAdd();
    });
    //$("#transDlg").overlay({ fixed: false });
    //loadTransactions();
    //Gategories
    $.postErr(getCatUrl, function (res) {
        //        var mapping = {
        //            'items': {
        //                key: function (data) {
        //                    return ko.utils.unwrapObservable(data.Id);
        //                }
        //            }
        //        }
        catModel = ko.mapping.fromJS(res);
        for (var i = 0, j = catModel.items().length; i < j; i++) {
            var item = catModel.items()[i];
            item.Subitems.push({ Name: ko.observable(lang.EditCategories), Id: ko.observable(null), parent: item });
        }
        loadAccounts();
        catModel.editItem = ko.observable(null);
        catModel.itemById = function (id) {
            if (!id) {
                return null;
            } else {
                for (var i = 0, j = this.items().length; i < j; i++) {
                    var item = this.items()[i];
                    if (item.Id() == id)
                        return item;
                    var fItem = ko.utils.arrayFirst(item.Subitems(), function (item) {
                        return item.Id() == id;
                    });
                    if (fItem != null)
                        return fItem;
                }
            }
        };
        catModel.catNameToAdd = ko.observable("");
        catModel.AddSubitem = function () {
            if (this.catNameToAdd() && this.editItem()) {
                var v = { Name: this.catNameToAdd(), ParentId: this.editItem().Id() };
                var $this = this;
                $.postErr(updateCatUrl, v, function (res) {
                    var koNew = ko.mapping.fromJS(res);
                    $this.editItem().Subitems.splice($this.editItem().Subitems().length - 1, 0, koNew);
                    $this.catNameToAdd("");
                    $this.assignMenu();
                });
            }
        }
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
        catModel.editCat = function (item) {
            $.postErr(updateCatUrl, item, function (res) {
                catModel.assignMenu();
            });
        }
        catModel.deleteItem = function (item) {
            $.postErr(delCatUrl, { id: item.Id() }, function (res) {
                if (res != null) {
                    ko.utils.arrayRemoveItem(catModel.editItem().Subitems, item);
                    catModel.assignMenu();
                }
            });
        }
        catModel.EditCategories = function (item) {
            var hld = $('#cat_edit_area');
            if (item != null)
                this.editItem(item);
            if (hld.html() == '') {
                $.postErr(getCatEditDlg, function (res) {
                    hld.html(res);
                    hld.overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true });
                    ko.applyBindings(catModel, hld[0]);
                    hld.overlay().load();
                });
            }
            else
                hld.overlay().load();
        }
        catModel.AssignCategory = function (item) {
            $("#cat_menu").hide();
            if (item.Id() == null)
                return this.EditCategories(item.parent);
            if (transModel.selectedItem() != null) {
                transModel.selectedItem().Category(item.Name());
            }
        };
        catModel.Suggest = function () {
            var res = new Array();
            ko.utils.arrayForEach(this.items(), function (item) {
                res.push(item.Name());
                ko.utils.arrayForEach(item.Subitems(), function (item) {
                    res.push(item.Name());
                });
            });
            return res;
        };
        catModel.assignMenu = function () {
            ddsmoothmenu.init({ mainmenuid: "cat_menu", //menu DIV id
                orientation: 'v', //Horizontal or vertical menu: Set to "h" or "v"
                classname: 'ddsmoothmenu-v' //class added to menu's outer DIV
            })
        }
        ko.applyBindings(catModel, $("#cat_menu")[0]);
        catModel.assignMenu();
        $("#cat_menu").hide();
    });
    $("#cat_menu").hover(function () {
    }, function () {
        $("#cat_menu").hide();
        $("#cat_menu ul li ul").hide();
    });
});
