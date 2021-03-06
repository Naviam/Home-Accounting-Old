﻿/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />
//JSON.stringify
var transModel = {
    paging: { Page: 1, SortField: 'Date', SortDirection: 1, Filter: '' }
};
var transEdit = {};
ko.bindingHandlers.category = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //handle the field changing
        var checkElem = function () {
            var observable = valueAccessor();
            var val = $(element).val();
            //find category
            var item = catModel.Search(val);
            if (typeof item != 'undefined' && item != null)
                observable(item.Name());
            else
                $(element).val(observable()); //restore old
        };
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
    //localStorage.setItem("transFilter", null);
    filterModel.items(ko.utils.parseJson(localStorage.getItem("transFilter")));
    transModel.paging.Filter = filterModel.toString();
    var pSize = localStorage.getItem("transPageSize");
    transModel.paging.PageSize = pSize ? pSize : 50;
    $.postErr(getTransUrl, transModel.paging, function (res) {
        var childItem = function (data) {
            ko.mapping.fromJS(data, { 'include': ["RenameDescription", "RenameCategory"] }, this);
            var catItem = catModel.itemById(data.CategoryId);
            var catName = catItem != null ? catItem.Name() : '';
            this.Category = ko.observable(catName);
            this.Currency = ko.observable(accountsModel.currencyById(this.CurrencyId()));
            this.RenameDescription = ko.observable(false);
            this.RenameCategory = ko.observable(false);
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
        transModel = ko.mapping.fromJS(res, mapping);
        ko.mapping.fromJS(ko.mapping.toJS(res.transTemplate), {}, transEdit);
        transEdit.RenameDescription = ko.observable(false);
        transEdit.RenameCategory = ko.observable(false);
        transModel.paging.Page.subscribe(function (newValue) {
            transModel.ReloadPage();
        });
        transModel.selectedItem = ko.observable(null);
        transModel.selectedRow = ko.observable(null);
        transModel.ReloadPage = function () {
            if (this.DescrSub != null)
                this.DescrSub.dispose();
            localStorage.setItem("transFilter", ko.toJSON(filterModel.items));
            transModel.paging.Filter = filterModel.toString();
            $.postErr(getTransUrl, transModel.paging, function (res) {
                ko.mapping.updateFromJS(transModel, res);
                transModel.selectedItem(null);
                if (transDlg) transDlg.close();
                //$('#edit_row').hide();
            });
        }; //transModel.showCalendar = function (event, item) {
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
        //}
        transModel.SetPageSize = function (pageSize) {
            transModel.paging.PageSize(pageSize);
            localStorage.setItem("transPageSize", pageSize);
            this.ReloadPage();
        };
        transModel.ShowRenameDesc = function () {
            var item = this.selectedItem();
            return item != null && transEdit.Description() != item.Description() && item.Merchant() != null;
        };
        transModel.ShowRenameCat = function () {
            var item = this.selectedItem();
            return item != null && transEdit.CategoryId() != item.CategoryId() && item.Merchant() != null;
        };
        transModel.RenameToDesc = function () {
            var item = this.selectedItem();
            return item != null ? item.Description() : "";
        };
        transModel.RenameToCat = function () {
            var item = this.selectedItem();
            return item != null ? item.Category() : "";
        };
        transModel.removeFilters = function () {
            filterModel.Clear();
            this.ReloadPage();
        };
        transModel.ShowDialog = function () {
            var row = this.selectedRow();
            var frm = $("#transDlg");
            frm.css({ width: row.width() });
            var conf = transDlg.getConf();
            //conf.top = row.offset().top + row.height();
            //conf.left = row.offset().left - 5;
            var tblOffset = row.parents('table').offset();
            conf.top = tblOffset.top + row[0].offsetTop + row.height() - pageYOffset;
            conf.left = tblOffset.left - 5 - pageXOffset;
            var dd = conf.top + frm.height() - $(window).height();
            frm.overlay().load();
            if (dd > 0)
                $(window).scrollTop(dd + pageYOffset + 50);
        };
        transModel.GetNewItem = function () {
            var date = new Date();
            var fItem = ko.mapping.fromJS(ko.mapping.toJS(transModel.transTemplate));
            fItem.Date('/Date(' + date.getTime() + ')/');
            fItem.Category = ko.observable();
            fItem.Currency = ko.observable();
            fItem.CurrencyId = ko.observable(accountsModel.selectedItem().CurrencyId);
            fItem.AccountId = ko.observable(accountsModel.selectedItem().Id);
            fItem.RenameDescription = ko.observable(false);
            fItem.RenameCategory = ko.observable(false);
            return fItem;
        };
        transModel.Add = function () {
            var fItem = ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == null;
            });
            if (fItem != null) return;
            transDlg.close();
            this.items.splice(0, 0, this.GetNewItem());
            var row = $('#transGrid table tr:eq(1)');
            this.GoToEdit(null, this.items()[0], row);
            ko.mapping.fromJS(ko.mapping.toJS(transModel.selectedItem()), {}, transEdit);
            this.ShowDialog();
        };
        transModel.CancelAdd = function () {
            var fItem = ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == null;
            });
            ko.utils.arrayRemoveItem(this.items, fItem);
        };
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
            if (item != this.selectedItem()) this.selectedItem(item);
            if (event != null) {
                row = $(event.currentTarget);
                var tblOffset = row.parents('table').offset();
                var area = $("#trans_actions");
                //center align
                var x = tblOffset.left + row.width() / 2 - area.width() / 2;
                area.css({ left: x, top: tblOffset.top + row[0].offsetTop + row.height() });
                area.show();
            }
            this.selectedRow(row);
            $(row.find('[name="Category"]')).autocomplete(catModel.Suggest(), {
                minChars: 1
                //,matchContains: true //if minChars>1
            , delay: 10
            });
        }; //obj.date = eval(obj.date.replace(/\//g,'')) -- to convert the download datestring after json to a javascript Date
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
                transModel.SaveItem(sItem, reloadPage);
            }
        };
        transModel.SaveItem = function (sItem, reloadPage) {
            if (sItem) {
                var itemToSave = ko.mapping.toJS(sItem);
                var needRefreshRules = false;
                if (sItem.RenameDescription() || sItem.RenameCategory())
                    needRefreshRules = true;
                sItem.RenameDescription(false);
                sItem.RenameCategory(false);
                $.postErr(updateTransUrl, itemToSave, function (res) {
                    //transModel.selectedItem().Id(res.Id);
                    var amount = res.amount;
                    amount = res.trans.Direction == 0 ? -amount : amount;
                    accountsModel.addAmount(res.trans.AccountId, amount);
                    if (reloadPage)
                        transModel.ReloadPage();
                    if (needRefreshRules)
                        rulesModel.Refresh();
                });
            }
        };
        transModel.ShowEdit = function (event) {
            //this.selectedRow($(event.currentTarget).parents('tr'));
            ko.mapping.fromJS(ko.mapping.toJS(transModel.selectedItem()), {}, transEdit);
            this.ShowDialog();
        };
        transModel.Delete = function (item) {
            askToUser(lang.DeleteTrans, function () {
                transModel.DeleteItem(item);
            });
        }
        transModel.DeleteById = function (id) {
            id = ko.utils.unwrapObservable(id);
            var fItem = this.getById(id);
            if (fItem) this.Delete(fItem);
        }
        transModel.DeleteItem = function (item, callback) {
            $.postErr(delTransUrl, { id: item.Id() }, function (res) {
                var amount = item.Direction() == 0 ? item.Amount() : -item.Amount();
                accountsModel.addAmount(item.AccountId(), amount);
                ko.utils.arrayRemoveItem(transModel.items, item);
                transModel.selectedItem(null);
                transDlg.close();
                if (callback) callback();
            });
        };
        transModel.getById = function (id) {
            var fItem = ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == id;
            });
            return fItem;
        }
        transModel.ShowTransfer = function (id, event, op) {
            id = ko.utils.unwrapObservable(id);
            var fItem = this.getById(id);
            if (fItem) {
                accountsModel.fillMoveItems(fItem.AccountId(), fItem.Id(), op);
                if (accountsModel.move_items().length > 0) {
                    var offset = $(event.currentTarget).offset();
                    var area = $("#accounts_move");
                    area.css({ left: offset.left + $(event.currentTarget).width() - 5, top: offset.top + 15 });
                    area.show();
                }
            }
        };
        transModel.Transfer = function (item) {
            $("#accounts_move").hide();
            var fItem = this.getById(accountsModel.accOp.transId);
            if (fItem) {
                var newO = ko.mapping.toJS(fItem);
                newO.Id = null;
                newO.AccountId = item.Id();
                newO.CurrencyId = item.CurrencyId();
                if (accountsModel.accOp.op == 'trans') {
                    newO.Direction = newO.Direction == 0 ? 1 : 0;
                    this.SaveItem(newO, true);
                }
                if (accountsModel.accOp.op == 'move') {
                    this.DeleteItem(fItem, function () {
                        newO.Date = '/Date(' + new Date().getTime() + ')/';
                        transModel.SaveItem(newO, true);
                    });
                }
            }
        };
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
            this.paging.SortField(val);
            this.ReloadPage();
        };
        transModel.ShowExchange = function () {
            var hld = $('#exchangeDialog');
            accountsModel.RecalcExchangeItems();
            if (hld.html() == '') {
                $.postErr(getExchangeDlg, function (res) {
                    hld.html(res);
                    hld.overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true, closeIcon: true });
                    hld.overlay().load();
                });
            }
            else
                hld.overlay().load();
        };
        transModel.searchByKey = function (key, val, dKey, dVal, type) {
            var currItem = transModel.selectedItem();
            if (currItem != null) {
                filterModel.Clear();
                filterModel.Add(key, val, dKey, dVal, type);
                accountsModel.selectedItem(null);
                catModel.selectedTag(null);
                catModel.editedTag(null);
                pageContext.accountId = null;
                transModel.ReloadPage();
            }
        }
        transModel.Search = function () {
            var val = $('#search_box').val();
            if (val)
                val = val.trim();
            //remove no need
            filterModel.deleteByKey('CategoryId');
            if (val == '')
                filterModel.Clear()
            else
                if (val.substr(0, lang.Merchant.length + 2) == lang.Merchant + ': ')
                    filterModel.Add('Merchant', val.substr(lang.Merchant.length + 2, val.length), lang.FindMerchant, val.substr(lang.Merchant.length + 2, val.length));
                else
                    if (val.substr(0, lang.Description.length + 2) == lang.Description + ': ')
                        filterModel.Add('Description', val.substr(lang.Description.length + 2, val.length), lang.FindDescription, val.substr(lang.Description.length + 2, val.length));
                    else
                        if (val.substr(0, lang.Category.length + 2) == lang.Category + ': ')
                            filterModel.Add('Category', val.substr(lang.Category.length + 2, val.length), lang.FindCategory, val.substr(lang.Category.length + 2, val.length));
                        else
                            if (val.substr(0, lang.Tag.length + 2) == lang.Tag + ': ')
                                filterModel.Add('TagName', val.substr(lang.Tag.length + 2, val.length), lang.FindTag, val.substr(lang.Tag.length + 2, val.length));
                            else
                                filterModel.Add('ByString', val, lang.FindText, val);
            accountsModel.selectedItem(null);
            pageContext.accountId = null;
            transModel.ReloadPage();
        };
        transModel.ShowSplit = function (id) {
            var hld = $('#splitDialog');
            id = ko.utils.unwrapObservable(id);
            var fItem = this.getById(id);
            //debug(id);
            if (fItem) {
                if (hld.html() == '') {
                    $.postErr(getSplitDlg, function (res) {
                        hld.html(res);
                        splitModel.setInitial(id, fItem.Description(), fItem.Category(), fItem.Amount(), fItem.Merchant(), fItem.Currency(), fItem.Date(), fItem.Direction());
                        hld.overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true, closeIcon: true });
                        hld.overlay().load();
                    });
                }
                else {
                    splitModel.setInitial(id, fItem.Description(), fItem.Category(), fItem.Amount(), fItem.Merchant(), fItem.Currency(), fItem.Date(), fItem.Direction());
                    hld.overlay().load();
                }
            }
        }
        ko.applyBindings(transModel, $("#transGrid")[0]);
        ko.applyBindings(transModel, $("#filter_area")[0]);
        ko.applyBindings(filterModel, $("#display_filter")[0]);
        ko.applyBindings(transEdit, $("#transDlg")[0]);
        $('#search_box').autocomplete(findSuggestUrl, {
            minChars: 3
            , delay: 10
            , maxItemsToShow: 10
        });
        if (!unitTest) unblockWindow();
    });
}
$(document).ready(function () {
    if (!unitTest) blockWindow();
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
        var selItem = transModel.selectedItem();
        //if (transEdit.Id() != null) {
        //restore inline editing props
        transEdit.Amount(selItem.Amount());
        //transEdit.Category(selItem.Category());
        transEdit.Description(selItem.Description());
        //transEdit.Merchant(selItem.Merchant())
        transEdit.Date(selItem.Date());
        //}
        ko.mapping.fromJS(ko.mapping.toJS(transEdit), {}, selItem);
        var item = transModel.selectedItem();
        //console.log(item);
        if (item.Amount() <= 0)
            am.addClass(inputCssError);
        if (item.Category() == null)
            cat.addClass(inputCssError);
        if (item.Amount() <= 0 || item.Category() == null)
            return e.stopImmediatePropagation();
        transModel.Save(transEdit.Id() == null);
    });
    transDlg = $("#transDlg").overlay({ fixed: false }).data('overlay');
    if (transDlg)
        transDlg.onBeforeClose(function (e) {
            transModel.CancelAdd();
        });
    //$("#transDlg").overlay({ fixed: false });
    //loadTransactions();
    //Gategories
    $.postErr(getDicts, function (res) {
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
        catModel.tagNameToAdd = ko.observable("");
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
        };
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
        };
        catModel.deleteItem = function (item) {
            askToUser(lang.DeleteCategory, function () {
                $.postErr(delCatUrl, { id: item.Id() }, function (res) {
                    if (res != null) {
                        ko.utils.arrayRemoveItem(catModel.editItem().Subitems, item);
                        catModel.assignMenu();
                    }
                });
            });
        };
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
        };
        //Tags
        catModel.selectedTag = ko.observable(null);
        catModel.prevSelectedTag = null;
        catModel.editedTag = ko.observable(null);
        catModel.selectedTag.subscribe(function (newValue) {
            if (newValue != null && newValue != catModel.prevSelectedTag && newValue.Id() != null) {
                filterModel.Clear();
                filterModel.Add('TagId', newValue.Id(), lang.FindTag, newValue.Name());
                accountsModel.selectedItem(null);
                catModel.editedTag(null);
                pageContext.accountId = null;
                transModel.ReloadPage();
            }
            catModel.prevSelectedTag = newValue;
        });
        catModel.EditTags = function () {
            var hld = $('#tag_edit_area');
            if (hld.html() == '') {
                $.postErr(getTagsEditDlg, function (res) {
                    hld.html(res);
                    hld.overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true });
                    ko.applyBindings(catModel, hld[0]);
                    hld.overlay().load();
                });
            }
            else
                hld.overlay().load();
        };
        catModel.addTag = function () {
            var fItem = ko.utils.arrayFirst(catModel.tags(), function (item) {
                return item.Id() == null;
            });
            if (fItem != null) { catModel.editedTag(fItem); return; }
            catModel.tags.splice(0, 0, { Name: ko.observable(null), Id: ko.observable(null), UserId: ko.observable(null) });
            catModel.editedTag(catModel.tags()[0]);
        };
        catModel.editTag = function (item) {
            $.postErr(updateTagUrl, item, function (res) {
                item.Id(res.Id);
                item.UserId(res.UserId);
                catModel.editedTag(null);
            });
        };
        catModel.deleteTag = function (item) {
            askToUser(lang.DeleteTag, function () {
                $.postErr(delTagUrl, { id: item.Id() }, function (res) {
                    if (res != null) {
                        ko.utils.arrayRemoveItem(catModel.tags, item);
                    }
                });
            });
        };
        catModel.cancelAddTag = function () {
            var fItem = ko.utils.arrayFirst(catModel.tags(), function (item) {
                return item.Id() == null;
            });
            ko.utils.arrayRemoveItem(catModel.tags, fItem);
            catModel.editedTag(null);
        };
        catModel.AddTag = function () {
            if (this.tagNameToAdd()) {
                var v = { Name: this.tagNameToAdd() };
                var $this = this;
                $.postErr(updateTagUrl, v, function (res) {
                    var koNew = ko.mapping.fromJS(res);
                    $this.tags.push(koNew);
                    $this.tagNameToAdd("");
                });
            }
        };
        //End tags
        catModel.AssignCategory = function (item) {
            $("#cat_menu").hide();
            if (item.Id() == null)
                return this.EditCategories(item.parent);
            this.inputCat.val(item.Name()).change();
            //            if (transModel.selectedItem() != null) {
            //                transModel.selectedItem().Category(item.Name());
            //            }
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
        catModel.ShowCategories = function (btn) {
            var menu = $("#cat_menu");
            if (menu.css("display") != "none") {
                menu.hide();
                return;
            }
            var input = $(btn).parent().find('[name="Category"]');
            this.inputCat = input;
            var yPos = input.offset().top - menu.height();
            if (yPos < 0) yPos = 0;
            menu.css({ top: yPos, left: input.offset().left });
            menu.width(input.width());
            $("#cat_menu ul").width(input.width());
            menu.slideDown();
        };
        catModel.assignMenu = function () {
            ddsmoothmenu.init({ mainmenuid: "cat_menu", //menu DIV id
                orientation: 'v', //Horizontal or vertical menu: Set to "h" or "v"
                classname: 'ddsmoothmenu-v' //class added to menu's outer DIV
            });
        };
        ko.applyBindings(catModel, $("#cat_menu")[0]);
        ko.applyBindings(catModel, $("#tags")[0]);
        if (!unitTest) catModel.assignMenu();
        $("#cat_menu").hide();
        loadAccounts();
    });
    $("#cat_menu").hover(function () {
    }, function () {
        $("#cat_menu").hide();
        $("#cat_menu ul li ul").hide();
    });
});
