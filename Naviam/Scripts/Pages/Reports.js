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
        reportsModel.isTimeFrameSelected = function (time) {
            return rep_req.selectedTimeFrameStart() <= time && rep_req.selectedTimeFrameEnd() >= time;
        };
        reportsModel.changeTimeFrame = function (ev, newVal) {
            var oldS = rep_req.selectedTimeFrameStart();
            var oldE = rep_req.selectedTimeFrameEnd();
            var newS = newVal;
            var newE = newVal;
            if ((ev.ctrlKey || ev.shiftKey) && newVal > oldS) newS = oldS;
            if ((ev.ctrlKey || ev.shiftKey) && newVal < oldE) newE = oldE;
            rep_req.selectedTimeFrameStart(newS);
            rep_req.selectedTimeFrameEnd(newE);
            var needR = rep_req.selectedTimeFrame() == 4 && (newS != oldS || newE != oldE);
            rep_req.selectedTimeFrame(4);
            if (needR) reportsModel.Refresh();
        };
        reportsModel.Refresh = function () {
            $.postErr(getReportsUrl, ko.toJS(rep_req), function (res) {
                ko.mapping.updateFromJS(reportsModel, res);
                reportsModel.setupDragTime();
                reportsModel.fillChart();
            });
        };
        reportsModel.endAmount = ko.dependentObservable(function () {
            var total = 0;
            for (var i = 0; i < this.items().length; i++)
                total += parseInt(this.items()[i].Amount());
            return total;
        }, reportsModel);
        reportsModel.endAmount2 = ko.dependentObservable(function () {
            var total = 0;
            for (var i = 0; i < this.items().length; i++)
                total += parseInt(this.items()[i].Amount2());
            return total;
        }, reportsModel);
        reportsModel.selectTable = function (item) {
            if (rep_req.selectedMenu() == 2) return;
            if (rep_req.selectedSubMenu() == 0)
                filterModel.Add('CategoryId', item.Id(), lang.FindCategory, item.Name(), "int");
            if (rep_req.selectedSubMenu() == 1)
                filterModel.Add('Merchant', item.Name(), lang.FindMerchant, item.Name());
            if (rep_req.selectedSubMenu() == 2)
                filterModel.Add('TagName', item.Name(), lang.FindTag, item.Name());
            //add type
            if (rep_req.selectedMenu() == 0)
                filterModel.Add('Direction', '0', lang.FindDirection, lang.Spending);
            if (rep_req.selectedMenu() == 1)
                filterModel.Add('Direction', '1', lang.FindDirection, lang.Income);
            //add time period
            var dStart = '' + (rep_req.selectedTimeFrameStart() - 1);
            var dEnd = '' + (rep_req.selectedTimeFrameEnd() - 1);
            var dateS = new Date(dStart.substr(0, 4), dStart.substr(4, 2));
            var dateE = new Date(dEnd.substr(0, 4), dEnd.substr(4, 2));
            //get last day of month
            dateE.setMonth(dateE.getMonth() + 1);
            dateE.setDate(dateE.getDate() - 1);
            filterModel.Add('BetweenDate', rep_req.selectedTimeFrameStart() + '' + rep_req.selectedTimeFrameEnd(), lang.FindBetweenDate, dateS.format() + ' - ' + dateE.format());
            localStorage.setItem("transFilter", ko.toJSON(filterModel.items));
            window.location = transUrl;
        };
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
            if (rep_req.selectedMenu() == 2 && rep_req.selectedSubMenu() == 3)
                title = lang.NetIncome;
            return title;
        }
        reportsModel.fillChart = function () {
            var net = false;
            if (rep_req.selectedMenu() == 2 && rep_req.selectedSubMenu() == 3)
                net = true;
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'key');
            if (net) {
                this.graphType(1);
                data.addColumn('number', lang.Income);
                data.addColumn('number', lang.Spending);
                ko.utils.arrayForEach(this.items(), function (item) {
                    data.addRow([item.Name(), item.Amount(), -item.Amount2()]);
                });
            }
            else {
                data.addColumn('number', lang.Amount);
                ko.utils.arrayForEach(this.items(), function (item) {
                    data.addRow([item.Name(), item.Amount()]);
                });
            }
            $('#chart_p').hide();
            $('#chart_b').hide();
            $('#chart_c').hide();
            if (this.graphType() == 0) {
                chart.draw(data, { width: 1100, height: 300, title: this.getChartTitle(), is3D: true });
                $('#chart_p').show();
            }
            else {
                if (rep_req.selectedSubMenu() == 3) {
                    chart_c.draw(data, { width: 1100, height: 300, title: this.getChartTitle(), isStacked: true });
                    $('#chart_c').show();
                } else {
                    chart_b.draw(data, { width: 1100, height: 300, title: this.getChartTitle() });
                    $('#chart_b').show();
                }
            }
        };
        reportsModel.isDragged = false;
        reportsModel.setupDragTime = function () {
            //time frame drag
            $('.time_frame_cell')
            .mouseover(function (e) {
                e.preventDefault();
                if (reportsModel.isDragged) {
                    var val = $(e.target).attr('data_val');
                    if (!val) //on span
                        val = $(e.target).parent().attr('data_val');
                    if (val) {
                        e.shiftKey = true;
                        reportsModel.changeTimeFrame(e, val);
                    }
                }
            })
            .mousedown(function (e) {
                e.preventDefault();
                reportsModel.isDragged = true;
            })
            .mouseup(function (e) {
                reportsModel.isDragged = false;
            });
        };
        //
        ko.applyBindings(reportsModel, $("#report_page")[0]);
        ko.applyBindings(reportsModel, $("#timeline")[0]);
        menuModel.selectedMenu(menuModel.menu[rep_req.selectedMenu()]);
        menuModel.selectedSubMenu(menuModel.menu[rep_req.selectedMenu()].subMenu[rep_req.selectedSubMenu()]);
        menuModel.selectedMenu.subscribe(function (val) {
            var need_update = rep_req.selectedSubMenu() == menuModel.menu[val.id].subMenu[0].id && rep_req.selectedMenu() != val.id; //need to force refresh
            rep_req.selectedMenu(val.id);
            menuModel.selectedSubMenu(menuModel.menu[val.id].subMenu[0]);
            if (need_update) rep_req.selectedSubMenu.valueHasMutated();
        });
        menuModel.selectedSubMenu.subscribe(function (val) { rep_req.selectedSubMenu(val.id); });
        ko.applyBindings(menuModel, $("#rep_menu")[0]);
        reportsModel.setupDragTime();
        reportsModel.fillChart();
    });
};

var menuModel = {
    menu: [
            { id: 0, caption: lang.Spending, subMenu: [{ id: 0, caption: lang.ByCategory.capitalize() }, { id: 1, caption: lang.ByMerchant.capitalize() }, { id: 2, caption: lang.ByTag.capitalize() }, { id: 3, caption: lang.OverTime.capitalize()}] },
            { id: 1, caption: lang.Income, subMenu: [{ id: 0, caption: lang.ByCategory.capitalize() }, { id: 1, caption: lang.ByMerchant.capitalize() }, { id: 2, caption: lang.ByTag.capitalize() }, { id: 3, caption: lang.OverTime.capitalize()}] },
            { id: 2, caption: lang.NetIncome, subMenu: [{ id: 3, caption: lang.OverTime.capitalize() }] }
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