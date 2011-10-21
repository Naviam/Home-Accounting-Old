/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />

var reportsModel = {};
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
        reportsModel.request.selectedCurrency = ko.observable(reportsModel.currencyId());
        reportsModel.request.selectedCurrency.subscribe(function (newValue) {
            reportsModel.Refresh();
        });
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
        reportsModel.fillChart = function () {
            var data = new google.visualization.DataTable();
            data.addColumn('string', lang.Category);
            data.addColumn('number', 'Amount');
            ko.utils.arrayForEach(reportsModel.items(), function (item) {
                data.addRow([item.CategoryName(), item.Amount()]);
            });

            chart.draw(data, { width: 450, height: 300, title: 'Spending by Category', is3D: true });
        }
        //
        ko.applyBindings(reportsModel, $("#report_table")[0]);
        reportsModel.fillChart();
    });
};


var chart = {};
$(document).ready(function () {
    chart = new google.visualization.PieChart($('#chart_div')[0]);
    reportsModel.Load();
});