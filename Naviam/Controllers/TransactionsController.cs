using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Naviam.Data;
using Naviam.DAL;
using Naviam.WebUI.Resources;

using Naviam.WebUI.Models;
using Naviam.Domain.Concrete;
using System.Resources;
using System.Web.Script.Serialization;

namespace Naviam.WebUI.Controllers
{
    //[Authorize(Roles="admin")]
    public class TransactionsController : BaseController
    {
        private readonly TransactionsRepository _transRepository;
        private readonly CategoriesRepository _categoriesRepository;
        
        public TransactionsController(): this(null, null)
        {
        }

        public TransactionsController(TransactionsRepository transRepository, CategoriesRepository categoriesRepository)
        {
            _transRepository = transRepository ?? new TransactionsRepository();
            _categoriesRepository = categoriesRepository ?? new CategoriesRepository();
        }
        
        public class FilterHolder
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
        }

        public class Head
        {
            public string Field { get; set; }
            public string Text { get; set; }
            public int Columns = 1;
            public int Size = 1;
        }

        public class Paging
        {
            public int PageSize = 10;
            public int Page { get; set; }
            public int PagesCount { get; set; }
            public int RowsOnPage { get; set; }
            public int RowsCount { get; set; }
            public string Filter { get; set; }
            public string SortField { get; set; }
            public int SortDirection { get; set; }

            public IEnumerable<T> ApplyPaging<T>(IEnumerable<T> input)
            {
                if (input != null)
                {
                    IQueryable<T> q;
                    if (!String.IsNullOrEmpty(Filter) && Filter != "null")
                    {
                        q = input.AsQueryable();
                        input = q.Where(Filter).ToList();
                    }
                    if (!String.IsNullOrEmpty(SortField) && SortField != "null")
                    {
                        string sort = SortField;
                        q = input.AsQueryable();
                        if (SortDirection != 0)
                            sort = sort + " desc";
                        input = q.OrderBy(sort).ToList();
                    }
                    PagesCount = (int)Math.Ceiling((double)input.Count() / PageSize);
                    RowsCount = input.Count();
                    if (Page < 1) Page = 1;
                    if (Page > PagesCount) Page = PagesCount;
                    input = input.Skip((Page - 1) * PageSize).Take(PageSize).ToList();
                    RowsOnPage = input.Count();
                }
                return input;
            }
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetTransactions(Paging paging, PageContext pageContext)
        {
            var user = CurrentUser;
            var trans = _transRepository.GetTransactions(user.CurrentCompany);
            JavaScriptSerializer js = new JavaScriptSerializer();
            if (!String.IsNullOrEmpty(paging.Filter))
            {
                List<FilterHolder> flt = js.Deserialize<List<FilterHolder>>(paging.Filter);
                paging.Filter = "";
                var byTag = flt.FirstOrDefault(m => m.Name == "TagId");
                if (byTag != null)
                    trans = trans.Where(q => q.TagIds.Contains(byTag.Value));
                else //others filters
                {
                    foreach (var item in flt)
                    {
                        if (item.Name == "ByString" || item.Name == "Category")
                        {
                            var cats = GetCategories(user.Id);
                            var catsIds = cats.Where(m => m.Name.ToLower().Contains(item.Value.ToLower()));
                            //!
                            if (item.Name == "ByString")
                                trans = trans.Where(s => (s.Merchant != null && s.Merchant.ToLower().Contains(item.Value.ToLower())) || (s.Description != null && s.Description.ToLower().Contains(item.Value.ToLower()))).
                                    Union(from t in trans join c in catsIds on t.CategoryId equals c.Id select t).ToList();
                            else
                                trans = (from t in trans join c in catsIds on t.CategoryId equals c.Id select t).ToList();
                            //trans = from t in trans join c in catsIds on t.CategoryId equals c.Id into tmp from tr in tmp.DefaultIfEmpty() select t;
                        }
                        else
                        {
                            if (item.Name == "TagName")
                            {
                                //TODO: enable search by tags
                                var tags = TagsRepository.GetTags(user.Id);
                                var tagIds = tags.Where(m => m.Name.ToLower().Contains(item.Value.ToLower()));
                                //trans = (from t in trans join c in tagIds on t.t equals c.Id select t).ToList();
                            }
                            else
                            {
                                if (item.Type == "int")
                                    paging.Filter += String.Format("{0}=={1} and ", item.Name, item.Value);
                                else
                                    paging.Filter += String.Format("{0}==\"{1}\" and ", item.Name, item.Value);
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(paging.Filter))
                        paging.Filter = paging.Filter.Substring(0, paging.Filter.Length - 5);
                }
            }
            if (pageContext.AccountId != null)
            {
                paging.Filter = String.Format("AccountId={0}", pageContext.AccountId);
            }
            //var trans = TransactionsDataAdapter.GetTransactions(user.CurrentCompany);

            trans = paging.ApplyPaging(trans);

            var head = new List<Head>
                           {
                               new Head {Field = "Date", Text = DisplayNames.Date, Size = 75},
                               new Head {Field = "Description", Text = DisplayNames.Description, Size = 275},
                               new Head {Field = "Merchant", Text = DisplayNames.Merchant, Size = 250},
                               new Head {Field = "CategoryId", Text = DisplayNames.Category, Size = 175},
                               new Head {Field = "Amount", Text = DisplayNames.Amount, Size = 120}
                           };
            return Json(new { items = trans, paging, headItems = head, transTemplate = new Transaction() });
        }

        [HttpPost]
        public ActionResult UpdateTransaction(Transaction trans, PageContext pageContext)
        {
            var companyId = CurrentUser.CurrentCompany;
            var tags = Request.Form["TagIds[]"] as string;
            trans.TagIds = new List<string>();
            if (tags != null)
            {
                string[] tagsA = tags.Split(',');
                foreach (var item in tagsA)
                {
                    trans.TagIds.Add(item);
                }
            }
            //TryUpdateModel(trans);
            var amount = trans.Amount;
            if (trans.Id != null)
            {
                var updateTrans = TransactionsDataAdapter.GetTransaction(trans.Id, companyId);
                amount = (trans.Direction == updateTrans.Direction) ? -(updateTrans.Amount - trans.Amount) : trans.Amount * 2;
                _transRepository.Update(trans, companyId);
            }
            else
                _transRepository.Insert(trans, companyId);
            return Json(new { trans, amount });
        }

        [HttpPost]
        public ActionResult DeleteTransaction(int? id)
        {
            var user = CurrentUser;
            _transRepository.Delete(id, user.CurrentCompany);
            return Json(id);
        }

        [HttpPost]
        public ActionResult SplitTrans(TransactionsSplit splits)
        {
            var companyId = CurrentUser.CurrentCompany;
            var updateTrans = TransactionsDataAdapter.GetTransaction(splits.Id, companyId);

            if (updateTrans != null)
            {
                updateTrans.Amount = splits.EndAmount;
                _transRepository.Update(updateTrans, companyId);
                updateTrans = updateTrans.Clone();
                foreach (var item in splits.Items)
                {
                    updateTrans.Description = item.Description;
                    updateTrans.Merchant = item.Merchant;
                    updateTrans.CategoryId = item.CategoryId;
                    updateTrans.Amount = item.Amount;
                    _transRepository.Insert(updateTrans, companyId);
                }
            }
            return Json(updateTrans);
        }

        #region Additional dialogs

        [HttpPost]
        public ActionResult GetExchangeDialog()
        {
            return PartialView("_exchangeDialog");
        }

        [HttpPost]
        public ActionResult GetSplitDialog()
        {
            return PartialView("_splitDialog");
        }

        #endregion

        #region Categories

        private List<Category> GetCategories(int? userId)
        {
            var cats = _categoriesRepository.GetCategories(userId);
            //TODO: move Localize
            var rm = new ResourceManager(typeof(Resources.Enums));
            foreach (var item in cats)
            {
                var st = rm.GetString("c_" + item.Id.ToString());
                if (!String.IsNullOrEmpty(st))
                    item.Name = st;
            }
            //end Localize
            return cats;
        }

        [HttpPost]
        public ActionResult GetDicts()
        {
            var user = CurrentUser;
            var cats = GetCategories(user.Id);
            var tags = TagsRepository.GetTags(user.Id);
            var items = Categories.GetTree(cats);
            return Json(new { items, tags });
        }

        [HttpPost]
        public ActionResult GetCategoriesEditDialog()
        {
            return PartialView("_categoriesEdit");
        }

        [HttpPost]
        public ActionResult UpdateCategory(Category cat)
        {
            var user = CurrentUser;
            cat.UserId = user.Id;
            if (cat.Subitems.Count == 1 && cat.Subitems[0] == null)
                cat.Subitems.Clear();
            if (cat.Id != null)
                _categoriesRepository.Update(cat, cat.UserId);
            else
                _categoriesRepository.Insert(cat, cat.UserId);
            return Json(cat);
        }

        [HttpPost]
        public ActionResult DeleteCategory(int? id)
        {
            var user = CurrentUser;
            _categoriesRepository.Delete(id, user.Id);
            return Json(id);
        }

        #endregion

        #region Tags

        [HttpPost]
        public ActionResult GetTagsEditDialog()
        {
            return PartialView("_tagsEdit");
        }

        [HttpPost]
        public ActionResult UpdateTag(Tag tag)
        {
            var user = CurrentUser;
            tag.UserId = user.Id;
            if (tag.Id != null)
                TagsRepository.Update(tag, tag.UserId);
            else
                TagsRepository.Insert(tag, tag.UserId);
            return Json(tag);
        }

        [HttpPost]
        public ActionResult DeleteTag(int? id)
        {
            var user = CurrentUser;
            TagsRepository.Delete(id, user.Id);
            return Json(id);
        }

        #endregion

        [HttpGet]
        public string FindSuggest(string q)
        {
            if (String.IsNullOrEmpty(q)) return "";
            q = q.ToLower();
            var user = CurrentUser;
            var cats = GetCategories(user.Id);
            var tags = TagsRepository.GetTags(user.Id);
            string res = "";
            //categories
            foreach (var item in cats)
            {
                if (item.Name.ToLower().Contains(q))
                    res += Naviam.WebUI.Resources.JavaScript.Category + ": " + item.Name + "|" + item.Id.ToString() + "\n";
            }
            //tags
            //TODO: enable search by tags
            /*foreach (var item in tags)
            {
                if (item.Name.ToLower().Contains(q))
                    res += Naviam.WebUI.Resources.JavaScript.Tag + ": " + item.Name + "|" + item.Id.ToString() + "\n";
            }*/
            var trans = _transRepository.GetTransactions(user.CurrentCompany);
            //merchant and description
            foreach (var item in trans)
            {
                if (item.Merchant != null && item.Merchant.ToLower().Contains(q))
                    res += Naviam.WebUI.Resources.JavaScript.Merchant + ": " + item.Merchant + "|" + item.Id.ToString() + "\n";
                if (item.Description != null && item.Description.ToLower().Contains(q))
                    res += Naviam.WebUI.Resources.JavaScript.Description + ": " + item.Description + "|" + item.Id.ToString() + "\n";
            }
            return res;
        }
    }
}
