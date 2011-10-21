/// <reference path="..\jquery-1.6.2.js" />
/// <reference path="..\knockout-1.2.1.js" />
/// <reference path="..\common.js" />

var reportsModel = {};
reportsModel.Refresh = function () {
    $.postErr(getReportsUrl, function (res) {
        ko.mapping.updateFromJS(reportsModel, res);
    });
};
reportsModel.Load = function (callback) {
    $.postErr(getReportsUrl, function (res) {
        var mapping = {
            'items': {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                }
            }
        };
        reportsModel = ko.mapping.fromJS(res, mapping);
        ko.applyBindings(reportsModel, $("#report_table")[0]);
        callback();
    });
};


$(document).ready(function () {
    reportsModel.Load(function () {
        var data = new google.visualization.DataTable();
        data.addColumn('string', lang.Category);
        data.addColumn('number', 'Amount');
        ko.utils.arrayForEach(reportsModel.items(), function (item) {
            data.addRow([item.CategoryName(), item.Amount()]);
        });

        var chart = new google.visualization.PieChart($('#chart_div')[0]);
        chart.draw(data, { width: 450, height: 300, title: 'Spending by Category', is3D: true });
    });
});