/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />
function loadAccounts() {
    $.postErr(getAccountsUrl, function (res) {
        //var childItem = function (data) {
        //ko.mapping.fromJS(data, {}, this);
        //                var fItem = ko.utils.arrayFirst(item.Subitems(), function (item) {
        //                    return item.Id() == data.CurrencyId;
        //                });
        //                //var catItem = catModel.itemById(data.CategoryId);
        //                var name = fItem != null ? fItem.NameShort() : '';
        //                this.Currency = ko.observable(name);
        //}
        var mapping = {
            'items': {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                }
                //                    , create: function (options) {
                //                        return new childItem(options.data, {}, this);
                //                    }
            }
        }

        accountsModel = ko.mapping.fromJS(res, mapping);
        accountsModel.move_items = ko.observableArray();
        accountsModel.ExchangeItems = ko.observableArray();
        accountsModel.selectedItem = ko.observable(null);
        accountsModel.items.splice(0, 0, { Name: ko.observable(lang.All), Id: ko.observable(null), Balance: ko.observable(null), Currency: ko.observable(null) });
        accountsModel.selectedItem(accountsModel.items()[0]);
        accountsModel.selectedItem.subscribe(function (newValue) {
            pageContext.accountId = newValue.Id();
            transModel.ReloadPage();
            accountsModel.hideEdit(false);
            var regExp = new RegExp('(accId)=([^&]*)', 'g');
            var form = $('#upload_statement').parents('form')[0];
            form.action = form.action.replace(regExp, '$1=' + pageContext.accountId);
            accountsModel.RecalcExchangeItems();
        });
        accountsModel.RecalcExchangeItems = function () {
            var currValue = this.selectedItem();
            if (currValue.Id() == null) return;
            accountsModel.ExchangeItems(ko.utils.arrayFilter(accountsModel.items(), function (item) {
                return item.Id() != currValue.Id() && item.Id() != null && item.CurrencyId() != currValue.CurrencyId();
            }));
        }
        accountsModel.Refresh = function () {
            $.postErr(getAccountsUrl, function (res) {
                ko.mapping.updateFromJS(accountsModel, res);
            });
        }
        accountsModel.currencyById = function (id) {
            var fItem = ko.utils.arrayFirst(this.currItems(), function (item) {
                return item.Id() == id;
            });
            return fItem != null ? fItem.NameShort() : '';
        }
        accountsModel.getById = function (id) {
            return ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == id;
            });
        }
        accountsModel.fillMoveItems = function (accId, transId, op) {
            var curItem = this.getById(accId);
            this.move_items(ko.utils.arrayFilter(this.items(), function (item) {
                return item.Id() != accId && item.Id() != null && item.CurrencyId() == curItem.CurrencyId();
            }));
            this.accOp = {};
            this.accOp.transId = transId;
            this.accOp.op = op;
        }
        accountsModel.hideEdit = function (show) {
            var elem = $("#account_edit")[0];
            var trans_elem = $("#transGrid");
            $(elem).find('form').validator({ lang: lang.culture }).data("validator").reset();
            if (!show) {
                $(elem).hide();
                trans_elem.show();
            }
            else {
                $(elem).show();
                trans_elem.hide();
            }
        }
        accountsModel.passToEdit = function (item, editItem) {
            var elem = $("#account_edit")[0];
            var trans_elem = $("#transGrid");
            item.Save = function () {
                if (item === this) {
                    if ($(elem).find('form').data("validator").checkValidity()) {
                        if (editItem == null) //add
                            item.Balance = item.InitialBalance;
                        else
                            item.Balance = editItem.Balance() + (item.InitialBalance - editItem.InitialBalance());
                        $.postErr(updateAccountUrl, this, function (res) {
                            if (editItem != null)
                                ko.mapping.fromJS(res, {}, editItem);
                            else {
                                koNew = ko.mapping.fromJS(res)
                                accountsModel.items.splice(1, 0, koNew);
                            }
                            accountsModel.hideEdit(false);
                        });
                    }
                }
            }
            ko.cleanNode(elem);
            ko.applyBindings(item, elem);
            accountsModel.hideEdit(true);
        }
        accountsModel.addItem = function () {
            var newItem = { Id: null, Name: null, InitialBalance: 0, Description: null, CurrencyId: null, TypeId: null };
            this.passToEdit(newItem, null);
        }
        accountsModel.editItem = function (item) {
            accountsModel.passToEdit(ko.mapping.toJS(item), item);
        }
        accountsModel.deleteItem = function (item) {
            askToUser(lang.DeleteAccount, function () {
                $.postErr(deleteAccountUrl, { id: item.Id() }, function (res) {
                    ko.utils.arrayRemoveItem(accountsModel.items, item);
                    if (accountsModel.selectedItem() == item)
                        accountsModel.selectedItem(accountsModel.items()[0]);
                    else
                        transModel.ReloadPage();
                });
            });
        }
        accountsModel.addAmount = function (id, amount) {
            if (amount != 0) {
                var fItem = ko.utils.arrayFirst(this.items(), function (item) {
                    return item.Id() == id;
                });
                if (fItem) {
                    fItem.Balance(fItem.Balance() + amount);
                    $.postErr(addAccountAmountUrl, { id: id, amount: amount }, function (res) {
                    });
                }
            }
        }
        ko.applyBindings(accountsModel, $("#accounts")[0]);
        ko.applyBindings(accountsModel, $("#accounts_move")[0]);

        loadTransactions();
    });
}

