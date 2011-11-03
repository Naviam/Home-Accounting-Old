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
        rep_req = reportsModel.request;
        rep_req.selectedCurrency.subscribe(function () { reportsModel.Refresh(); });
        rep_req.selectedTimeFrame.subscribe(function () { reportsModel.Refresh(); });
        //rep_req.selectedMenu.subscribe(function () { reportsModel.Refresh(); });
        rep_req.selectedSubMenu.subscribe(function (val) { reportsModel.Refresh(); });
        reportsModel.graphType = ko.observable(0);
        reportsModel.graphType.subscribe(function () { reportsModel.fillChart(); });
        //
        //funcs
        reportsModel.Refresh = function () {
            $.postErr(getReportsUrl, ko.toJS(rep_req), function (res) {
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
                title = lang.Spending + ' ' + lang.ByCategory;
            if (rep_req.selectedMenu() == 0 && rep_req.selectedSubMenu() == 1)
                title = lang.Spending + ' ' + lang.ByMerchant;
            if (rep_req.selectedMenu() == 0 && rep_req.selectedSubMenu() == 2)
                title = lang.Spending + ' ' + lang.ByTag;
            if (rep_req.selectedMenu() == 0 && rep_req.selectedSubMenu() == 3)
                title = lang.Spending + ' ' + lang.OverTime;
            if (rep_req.selectedMenu() == 1 && rep_req.selectedSubMenu() == 0)
                title = lang.Income + ' ' + lang.ByCategory;
            if (rep_req.selectedMenu() == 1 && rep_req.selectedSubMenu() == 1)
                title = lang.Income + ' ' + lang.ByMerchant;
            if (rep_req.selectedMenu() == 1 && rep_req.selectedSubMenu() == 2)
                title = lang.Income + ' ' + lang.ByTag;
            if (rep_req.selectedMenu() == 1 && rep_req.selectedSubMenu() == 3)
                title = lang.Income + ' ' + lang.OverTime;
            return title;
        }
        reportsModel.fillChart = function () {
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'key');
            data.addColumn('number', lang.Amount);
            ko.utils.arrayForEach(this.items(), function (item) {
                data.addRow([item.Name(), item.Amount()]);
            });
            $('#chart_p').hide();
            $('#chart_b').hide();
            $('#chart_c').hide();
            if (this.graphType() == 0) {
                chart.draw(data, { width: 1100, height: 300, title: this.getChartTitle(), is3D: true });
                $('#chart_p').show();
            }
            else {
                if (rep_req.selectedSubMenu() == 3) {
                    chart_c.draw(data, { width: 1100, height: 300, title: this.getChartTitle() });
                    $('#chart_c').show();
                } else {
                    chart_b.draw(data, { width: 1100, height: 300, title: this.getChartTitle() });
                    $('#chart_b').show();
                }
            }
        }
        //
        ko.applyBindings(reportsModel, $("#report_page")[0]);
        ko.applyBindings(reportsModel, $("#timeline")[0]);
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
            { id: 0, caption: lang.Spending, subMenu: [{ id: 0, caption: lang.ByCategory.capitalize() }, { id: 1, caption: lang.ByMerchant.capitalize() }, { id: 2, caption: lang.ByTag.capitalize() }, { id: 3, caption: lang.OverTime.capitalize()}] },
            { id: 1, caption: lang.Income, subMenu: [{ id: 0, caption: lang.ByCategory.capitalize() }, { id: 1, caption: lang.ByMerchant.capitalize() }, { id: 2, caption: lang.ByTag.capitalize() }, { id: 3, caption: lang.OverTime.capitalize()}] }
        ],
    selectedMenu: ko.observable(),
    selectedSubMenu: ko.observable()
};

var chart = {};
var chart_b = {};
$(document).ready(function () {
    chart = new google.visualization.PieChart($('#chart_p')[0]);
    chart_b = new google.visualization.BarChart($('#chart_b')[0]);
    chart_c = new google.visualization.ColumnChart($('#chart_c')[0]);
    reportsModel.Load();
});