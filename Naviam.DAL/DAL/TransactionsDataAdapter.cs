using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Npgsql;

using Naviam.Data;
using Naviam.Code;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class TransactionsDataAdapter
    {

        private const string CacheKey = "userTrans";

        public static IEnumerable<Transaction> GetTransactionsByCompany(int? companyId) { return GetTransactionsByCompany(companyId, false); }
        public static IEnumerable<Transaction> GetTransactionsByCompany(int? companyId, bool forceSqlLoad)
        {
            List<Transaction> res = CacheWrapper.GetList<Transaction>(CacheKey, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                //res = GetTestTransactions(7);
                using (SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(SqlConnectionHelper.ConnectionType.Naviam))
                {
                    using (SqlCommand cmd = holder.Connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.CommandText = "get_transactions";
                        cmd.Parameters.AddWithValue("@id_company", companyId);
                        try
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    res.Add(new Transaction(reader));
                            }
                        }
                        catch (SqlException e)
                        {
                            throw e;
                        }
                    }
                }
                //save to cache
                CacheWrapper.SetList<Transaction>(CacheKey, res, companyId);
            }
            return res;
        }

        public static List<Transaction> GetTestTransactions(int recordsCount)
        {
            List<Transaction> res = new List<Transaction>(recordsCount);
            for (int i = 0; i < recordsCount; i++)
                res.Add(new Transaction()
                    {
                        Description = "Test" + i.ToString(),
                        Category = "Category" + new Random(3).Next(recordsCount).ToString(),
                        Amount = 100.20M,
                        Id = i,
                        Date = DateTime.Now
                    });
            return res;
        }
        
        public static IEnumerable<Transaction> GetTransactions(int? userId) { return GetTransactions(userId, false); }
        public static IEnumerable<Transaction> GetTransactions(int? userId, bool forceSqlLoad)
        {
            //CacheWrapper.UpdateList<Transaction>(CacheKey, new Transaction() { Description = "Test3", Category = "Dinner3", Amount = 140, Id = 1 }, userId);
            List<Transaction> res = CacheWrapper.GetList<Transaction>(CacheKey, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                res = new List<Transaction>();
                res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100.20M, Id=1, Date = DateTime.Now });
                res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100, Id = 2 });
                res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100, Id = 3 });
                res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100, Id = 4 });
                res.Add(new Transaction() { Description = "Test2", Category = "Food", Amount = 120, Id = 5 });
                res.Add(new Transaction() { Description = "Test2", Category = "Food", Amount = 120, Id = 6 });
                res.Add(new Transaction() { Description = "Test2", Category = "Food", Amount = 120, Id = 7 });
                //save to cache
                CacheWrapper.SetList<Transaction>(CacheKey, res, userId);
                //CacheWrapper.UpdateList<Transaction>(CacheKey, new Transaction() { Description = "Test3", Category = "Dinner3", Amount = 140, Id = 1 }, userId);
                //CacheWrapper.Set<List<Transaction>>(CacheKey, res, userId);
            }
            return res;
        }

        public static Transaction GetTransaction(int? id, int? userId) { return GetTransaction(id, userId, false); }
        public static Transaction GetTransaction(int? id, int? userId, bool forceSqlLoad)
        {
            Transaction res = CacheWrapper.GetFromList<Transaction>(CacheKey, new Transaction() { Id = id }, userId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                //TODO: on db side - check that trans belongs to user
                res = new Transaction() { Description = "Test", Category = "Dinner", Amount = 100.20M, Id = 1, Date = DateTime.Now };
                //save to cache
                if (res == null) // not found in cache->add
                    CacheWrapper.AddToList<Transaction>(CacheKey, res, userId);
                else
                    CacheWrapper.UpdateList<Transaction>(CacheKey, res, userId);
            }
            return res;
        }

        public static int Insert(Transaction trans, int? userId)
        {
            int res = -1;
            //insert to db
            res = 0;
            if (res == 0)
            {
                //if ok - save to cache
                CacheWrapper.AddToList<Transaction>(CacheKey, trans, userId);
            }
            return res;
        }

        public static int Update(Transaction trans, int? userId)
        {
            int res = -1;
            //update db
            res = 0;
            if (res == 0)
            {
                //if ok - update cache
                CacheWrapper.UpdateList<Transaction>(CacheKey, trans, userId);
            }
            return res;
        }
        
        //we need to provide full object (not only id) to delete (restrict of redis)
        public static int Delete(Transaction trans, int? userId)
        {
            int res = -1;
            //delete from db
            res = 0;
            if (res == 0)
            {
                //if ok - remove from cache
                CacheWrapper.RemoveFromList<Transaction>(CacheKey, trans, userId);
            }
            return res;
        }
    }
}
