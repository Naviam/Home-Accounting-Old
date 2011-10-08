jQuery.postErr = function (url, data, callback, type) {
    // shift arguments if data argument was omited
    if ($.isFunction(data)) {
        type = type || callback;
        callback = data;
        data = null;
    }
    if (data == null)
        data = {};
    if (url == 'getDicts') {
        callback(Dicts);
    }
    if (url == 'updateTag') {
        var res = { Id: 10, UserId: 20 };
        callback(res);
    }
    if (url == 'delTag') {
        callback(data);
    }


    if (url == 'getAccounts') {
        callback(Accounts);
    }

    if (url == 'getTrans') {
        callback(Trans);
    }
};
function debug(s) {
    console.log(s);
}
function askToUser(text, callback) {
    callback();
}
