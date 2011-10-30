/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />
/// <reference path="~/Scripts/Pages/Transactions.js" />
function loadAccounts() {
    $.postErr(getAccountsUrl, function (res) {
        //        var childItem = function (data) {
        //            ko.mapping.fromJS(data, {}, this);
        //            /*var fItem = ko.utils.arrayFirst(item.Subitems(), function (item) {
        //                return item.Id() == data.CurrencyId;
        //            });
        //            //var catItem = catModel.itemById(data.CategoryId);
        //            var name = fItem != null ? fItem.NameShort() : '';
        //            this.Currency = ko.observable(name);*/
        //        }
        var mapping = {
            'items': {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                }
                //                , create: function (options) {
                //                    return new childItem(options.data, {}, this);
                //                }
            }
        };
        accountsModel = ko.mapping.fromJS(res, mapping);
        var editAccount = {};
        ko.mapping.fromJS(ko.mapping.toJS(res.accountTemplate), {}, editAccount);
        accountsModel.move_items = ko.observableArray();
        accountsModel.ExchangeItems = ko.observableArray();
        accountsModel.selectedItem = ko.observable(null);
        accountsModel.items.splice(0, 0, { Name: ko.observable(lang.All), Id: ko.observable(null), Balance: ko.observable(null), Currency: ko.observable(null), FinInstitutionName: ko.observable(null) });
        accountsModel.selectedItem(accountsModel.items()[0]);
        accountsModel.selectedItem.subscribe(function (newValue) {
            if (newValue != null) {
                catModel.selectedTag(null);
                filterModel.Clear();
                pageContext.accountId = newValue.Id();
                transModel.ReloadPage();
                accountsModel.hideEdit(false);
                var regExp = new RegExp('(accId)=([^&]*)', 'g');
                var form = $('#upload_statement').parents('form')[0];
                form.action = form.action.replace(regExp, '$1=' + pageContext.accountId);
                accountsModel.RecalcExchangeItems();
            }
        });
        accountsModel.RecalcExchangeItems = function () {
            var currValue = this.selectedItem();
            if (currValue.Id() == null) return;
            accountsModel.ExchangeItems(ko.utils.arrayFilter(accountsModel.items(), function (item) {
                return item.Id() != currValue.Id() && item.Id() != null && item.CurrencyId() != currValue.CurrencyId();
            }));
        };
        accountsModel.Refresh = function () {
            $.postErr(getAccountsUrl, function (res) {
                ko.mapping.updateFromJS(accountsModel, res);
                accountsModel.items.splice(0, 0, { Name: ko.observable(lang.All), Id: ko.observable(null), Balance: ko.observable(null), Currency: ko.observable(null), FinInstitutionName: ko.observable(null) });
            });
        };
        accountsModel.currencyById = function (id) {
            var fItem = ko.utils.arrayFirst(this.currItems(), function (item) {
                return item.Id() == id;
            });
            return fItem != null ? fItem.NameShort() : '';
        };
        accountsModel.getById = function (id) {
            return ko.utils.arrayFirst(this.items(), function (item) {
                return item.Id() == id;
            });
        };
        accountsModel.getFinById = function (id) {
            return ko.utils.arrayFirst(this.finInst(), function (item) {
                return item.Id() == id;
            });
        };
        accountsModel.getTypeById = function (id) {
            return ko.utils.arrayFirst(this.typesItems(), function (item) {
                return item.Id() == id;
            });
        };
        accountsModel.fillMoveItems = function (accId, transId, op) {
            var curItem = this.getById(accId);
            this.move_items(ko.utils.arrayFilter(this.items(), function (item) {
                return item.Id() != accId && item.Id() != null && item.CurrencyId() == curItem.CurrencyId();
            }));
            this.accOp = {};
            this.accOp.transId = transId;
            this.accOp.op = op;
        };
        accountsModel.hideEdit = function (show) {
            var elem = $("#account_edit")[0];
            var trans_elem = $("#transGrid");
            $(elem).find('form').validator({ lang: lang.culture }).data("validator").reset();
            if (!show) {
                $("#account_edit").overlay().close();
                //trans_elem.slideDown('slow');
                //$(elem).slideUp('slow');

                //$(elem).hide();
                //trans_elem.show();
            }
            else {
                $("#account_edit").overlay().load();
                //trans_elem.slideUp('slow');
                //$(elem).slideDown('slow');

                //$(elem).show();
                //trans_elem.hide();
            }
        };
        accountsModel.passToEdit = function (item) {
            //!!! Fill InstitutionId to populate list of types
            editAccount.FinInstitutionId(item.FinInstitutionId());
            //!!! then we can get right type id
            ko.mapping.fromJS(ko.mapping.toJS(item), {}, editAccount);
            var elem = $("#account_edit")[0];
            editAccount.Save = function () {
                if (editAccount === this) {
                    if ($(elem).find('form').data("validator").checkValidity()) {
                        if (this.Id() == null) //add
                            this.Balance(this.InitialBalance);
                        else
                            this.Balance(item.Balance() + (this.InitialBalance() - item.InitialBalance()));
                        $.postErr(updateAccountUrl, ko.mapping.toJS(this), function (res) {
                            if (editAccount.Id() != null)
                                ko.mapping.fromJS(res, {}, item);
                            else {
                                item = ko.mapping.fromJS(res);
                                accountsModel.items.splice(1, 0, item);
                            }
                            item.FinInstitutionName(accountsModel.getFinById(item.FinInstitutionId()).Name());
                            accountsModel.hideEdit(false);
                        });
                    }
                }
            };
            accountsModel.hideEdit(true);
        };
        accountsModel.addItem = function () {
            this.passToEdit(this.accountTemplate);
        };
        accountsModel.editItem = function (item) {
            accountsModel.passToEdit(item);
        };
        accountsModel.getTypes = function (finId) {
            var finInst = this.getFinById(ko.utils.unwrapObservable(finId));
            var $this = this;
            if (finInst) {
                var fin_acc_types = ko.utils.arrayFilter(this.finLinks(), function (item) {
                    return finInst.TypeId() == item.FinanceTypeId();
                });
                return ko.utils.arrayMap(fin_acc_types, function (item) {
                    return $this.getTypeById(item.AccountTypeId());
                });
            }
        };
        accountsModel.deleteItemById = function (id) {
            id = ko.utils.unwrapObservable(id);
            var curItem = this.getById(id);
            if (curItem) this.deleteItem(curItem);
        }
        accountsModel.deleteItem = function (item) {
            askToUser(lang.DeleteAccount, function () {
                $.postErr(deleteAccountUrl, { id: item.Id() }, function (res) {
                    accountsModel.hideEdit(false);
                    ko.utils.arrayRemoveItem(accountsModel.items, item);
                    if (accountsModel.selectedItem() == item)
                        accountsModel.selectedItem(accountsModel.items()[0]);
                    else
                        transModel.ReloadPage();
                });
            });
        };
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
        };
        ko.applyBindings(accountsModel, $("#accounts")[0]);
        ko.applyBindings(accountsModel, $("#accounts_move")[0]);
        ko.applyBindings(editAccount, $("#account_edit")[0]);
        $("#account_edit").overlay({ mask: { color: '#fff', opacity: 0.5, loadSpeed: 200 }, closeOnClick: true, closeIcon: true, 'onClose': function () { accountsModel.hideEdit(false); } });

        loadTransactions();
    });
}

