var unitTest = true;
var lang = Naviam.JavaScript;

pageContext = {};

var updateTagUrl = 'updateTag';
var delTagUrl = 'delTag';
var getDicts = 'getDicts';
var Dicts = { items: [{ Id:1, Name: 'test cat', Subitems: []}], tags: [{ Name: 'test tag', Id: 1, UserId: 1 }, { Name: 'test tag2', Id: 2, UserId: 1}] };

var getAccountsUrl = 'getAccounts';
var Accounts = { items: [{ Name: 'test acc', Id: 1}], currItems: [{ NameShort: 'currency', Id: 1}], accountTemplate: {} };

var getTransUrl = 'getTrans';
var Trans = { items: [{ Name: 'test trans', Id: 1, CurrencyId: 1, CategoryId: 1}], transTemplate: {}, paging: { Page: 1, SortField: 'Date', SortDirection: 1, Filter: ''} };
