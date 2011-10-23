/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />

var reportsModel = {};
var rep_req = {};
reportsModel.Load = function () {
    $.postErr(getReportsUrl, function (res) {
        var mapping = {
            'items': {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                }
            }
        };
        reportsModel = ko.mapping.fromJS(res, mapping);
        //props
        reportsModel.request = {};
        rep_req = reportsModel.request;
        rep_req.selectedCurrency = ko.observable(reportsModel.currencyId());
        rep_req.selectedCurrency.subscribe(function () { reportsModel.Refresh(); });
        rep_req.selectedMenu = ko.observable(reportsModel.selectedMenu());
        //rep_req.selectedMenu.subscribe(function () { reportsModel.Refresh(); });
        rep_req.selectedSubMenu = ko.observable(reportsModel.selectedSubMenu());
        rep_req.selectedSubMenu.subscribe(function (val) { reportsModel.Refresh(); });
        reportsModel.graphType = ko.observable(0);
        reportsModel.graphType.subscribe(function () { reportsModel.fillChart(); });
        //
        //funcs
        reportsModel.Refresh = function () {
            $.postErr(getReportsUrl, ko.toJS(reportsModel.request), function (res) {
                ko.mapping.updateFromJS(reportsModel, res);
                reportsModel.fillChart();
            });
        };
        reportsModel.endAmount = ko.dependentObservable(function () {
            var total = 0;
            for (var i = 0; i < this.items().length; i++)
                total += parseInt(this.items()[i].Amount());
            return total;
        }, reportsModel);
        reportsModel.getChartTitle = function () {
            var title = '';
            if (rep_req.selectedMenu() == 0 && rep_req.selectedSubMenu() == 0)
                title = 'Spending by Category';
            if (rep_req.selectedMenu() == 0 && rep_req.selectedSubMenu() == 1)
                title = 'Spending by Merchant';
            if (rep_req.selectedMenu() == 0 && rep_req.selectedSubMenu() == 2)
                title = 'Spending by Tag';
            if (rep_req.selectedMenu() == 1 && rep_req.selectedSubMenu() == 0)
                title = 'Income by Category';
            if (rep_req.selectedMenu() == 1 && rep_req.selectedSubMenu() == 1)
                title = 'Income by Merchant';
            if (rep_req.selectedMenu() == 1 && rep_req.selectedSubMenu() == 2)
                title = 'Income by Tag';
            return title;
        }
        reportsModel.fillChart = function () {
            var data = new google.visualization.DataTable();
            data.addColumn('string', lang.Category);
            data.addColumn('number', 'Amount');
            ko.utils.arrayForEach(this.items(), function (item) {
                data.addRow([item.Name(), item.Amount()]);
            });
            $('#chart_p').hide();
            $('#chart_b').hide();
            if (this.graphType() == 0) {
                chart.draw(data, { width: 550, height: 300, title: this.getChartTitle(), is3D: true });
                $('#chart_p').show();
            }
            else {
                chart_b.draw(data, { width: 550, height: 300, title: this.getChartTitle() });
                $('#chart_b').show();
            }
        }
        //
        ko.applyBindings(reportsModel, $("#report_page")[0]);
        menuModel.selectedMenu(menuModel.menu[rep_req.selectedMenu()]);
        menuModel.selectedSubMenu(menuModel.menu[rep_req.selectedMenu()].subMenu[rep_req.selectedSubMenu()]);
        menuModel.selectedMenu.subscribe(function (val) {
            var need_update = rep_req.selectedSubMenu() == 0 && rep_req.selectedMenu() != val.id;
            rep_req.selectedMenu(val.id);
            menuModel.selectedSubMenu(menuModel.menu[val.id].subMenu[0]);
            if (need_update) rep_req.selectedSubMenu.valueHasMutated(); 
        });
        menuModel.selectedSubMenu.subscribe(function (val) { rep_req.selectedSubMenu(val.id); });
        ko.applyBindings(menuModel, $("#rep_menu")[0]);
        reportsModel.fillChart();
    });
};

var menuModel = {
    menu: [
            { id: 0, caption: 'Spending', subMenu: [{ id: 0, caption: 'By Category' }, { id: 1, caption: 'By Merchant' }, { id: 2, caption: 'By Tag'}] },
            { id: 1, caption: 'Income', subMenu: [{ id: 0, caption: 'By Category' }, { id: 1, caption: 'By Merchant' }, { id: 2, caption: 'By Tag'}] }
        ],
    selectedMenu: ko.observable(),
    selectedSubMenu: ko.observable()
};

var chart = {};
var chart_b = {};
$(document).ready(function () {
    chart = new google.visualization.PieChart($('#chart_p')[0]);
    chart_b = new google.visualization.BarChart($('#chart_b')[0]);
    reportsModel.Load();
});