﻿using System;
using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class TransactionsDataAdapter
    {
        private const string CacheKey = "userTrans";

        public static List<Transaction> GetTestTransactions(int recordsCount, int? companyId)
        {
            var res = new List<Transaction>(recordsCount);
            for (int i = 0; i < recordsCount; i++)
                res.Add(new Transaction()
                {
                    Description = "Test" + i.ToString() + "_" + companyId,
                    Category = "Category" + new Random(3).Next(recordsCount).ToString(),
                    Amount = 100.20M,
                    Id = i,
                    Date = DateTime.Now
                });
            return res;
        }
        public static IEnumerable<Transaction> GetTransactions(int? companyId) { return GetTransactions(companyId, null, false); }
        public static IEnumerable<Transaction> GetTransactions(int? companyId, int? languageId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetList<Transaction>(CacheKey, companyId, languageId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                //res = GetTestTransactions(7, companyId);
                res = new List<Transaction>();
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    using (var cmd = holder.Connection.CreateSPCommand("get_transactions"))
                    {
                        cmd.Parameters.AddWithValue("@id_company", companyId);
                        cmd.Parameters.AddWithValue("@id_language", languageId.ToDbValue());
                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    res.Add(new Transaction(reader));
                            }
                        }
                        catch (SqlException e)
                        {
                            cmd.AddDetailsToException(e);
                            throw;
                        }
                    }
                }
                //save to cache
                cache.SetList(CacheKey, res, companyId, languageId);
            }
            return res;
        }
        public static Transaction GetTransaction(int? id, int? companyId) { return GetTransaction(id, companyId, null, false); }
        public static Transaction GetTransaction(int? id, int? companyId, int? languageId, bool forceSqlLoad)
        {
            var cache = new CacheWrapper();
            var res = cache.GetFromList(CacheKey, new Transaction { Id = id }, companyId);
            if (res == null || forceSqlLoad)
            {
                //load from DB
                //TODO: check that trans belongs to company
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    using (var cmd = holder.Connection.CreateSPCommand("get_transactions"))
                    {
                        cmd.Parameters.AddWithValue("@id_transaction", id);
                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                res = new Transaction(reader);
                            }
                        }
                        catch (SqlException e)
                        {
                            cmd.AddDetailsToException(e);
                            throw;
                        }
                    }
                }
                //res = new Transaction() { Description = "Test", Category = "Dinner", Amount = 100.20M, Id = 1, Date = DateTime.Now };
                //save to cache
                if (res == null) // not found in cache->add
                    cache.AddToList<Transaction>(CacheKey, res, companyId);
                else
                    cache.UpdateList(CacheKey, res, companyId);
            }
            return res;
        }

        private static int InsertUpdate(Transaction entity, int? companyId, int? languageId, DbActionType action)
        {
            var cache = new CacheWrapper();
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = action == DbActionType.Insert ? "add_transaction" : "update_transaction";
                var cmd = holder.Connection.CreateSPCommand(commName);
                try
                {
                    cmd.AddEntityParameters(entity, action);
                    cmd.ExecuteNonQuery();
                    if (action == DbActionType.Insert)
                        entity.Id = cmd.GetRowIdParameter();
                    res = cmd.GetReturnParameter();
                }
                catch (SqlException e)
                {
                    cmd.AddDetailsToException(e);
                    throw;
                }
            }
            if (res == 0)
            {
                if (action == DbActionType.Insert)
                    cache.AddToList(CacheKey, entity, companyId, languageId);
                if (action == DbActionType.Update)
                    //if ok - update cache
                    cache.UpdateList(CacheKey, entity, companyId, languageId);
            }
            return res;
        }

        public static int Insert(Transaction entity, int? companyId, int? languageId)
        {
            return InsertUpdate(entity, companyId, languageId, DbActionType.Insert);
        }

        public static int Update(Transaction entity, int? companyId, int? languageId)
        {
            //TODO: check that trans belongs to company
            return InsertUpdate(entity, companyId, languageId, DbActionType.Update);
        }

        //we need to provide full object (not only id) to delete from redis (restrict of redis)
        public static int Delete(Transaction trans, int? companyId, int? languageId)
        {
            var res = -1;
            //TODO: check that trans belongs to company
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("del_transaction"))
                {
                    try
                    {
                        cmd.AddCommonParameters(trans.Id);
                        cmd.ExecuteNonQuery();
                        res = cmd.GetReturnParameter();
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
            if (res == 0)
            {
                //if ok - remove from cache
                new CacheWrapper().RemoveFromList(CacheKey, trans, companyId, languageId);
            }
            return res;
        }

        //public static IEnumerable<Transaction> GetTransactions(int? userId) { return GetTransactions(userId, false); }
        //public static IEnumerable<Transaction> GetTransactions(int? userId, bool forceSqlLoad)
        //{
        //    //CacheWrapper.UpdateList<Transaction>(CacheKey, new Transaction() { Description = "Test3", Category = "Dinner3", Amount = 140, Id = 1 }, userId);
        //    List<Transaction> res = CacheWrapper.GetList<Transaction>(CacheKey, userId);
        //    if (res == null || forceSqlLoad)
        //    {
        //        //load from DB
        //        res = new List<Transaction>();
        //        res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100.20M, Id = 1, Date = DateTime.Now });
        //        res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100, Id = 2 });
        //        res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100, Id = 3 });
        //        res.Add(new Transaction() { Description = "Test", Category = "Food", Amount = 100, Id = 4 });
        //        res.Add(new Transaction() { Description = "Test2", Category = "Food", Amount = 120, Id = 5 });
        //        res.Add(new Transaction() { Description = "Test2", Category = "Food", Amount = 120, Id = 6 });
        //        res.Add(new Transaction() { Description = "Test2", Category = "Food", Amount = 120, Id = 7 });
        //        //save to cache
        //        CacheWrapper.SetList<Transaction>(CacheKey, res, userId);
        //        //CacheWrapper.UpdateList<Transaction>(CacheKey, new Transaction() { Description = "Test3", Category = "Dinner3", Amount = 140, Id = 1 }, userId);
        //        //CacheWrapper.Set<List<Transaction>>(CacheKey, res, userId);
        //    }
        //    return res;
        //}

        //public static Transaction GetTransaction(int? id, int? userId) { return GetTransaction(id, userId, false); }
        //public static Transaction GetTransaction(int? id, int? userId, bool forceSqlLoad)
        //{
        //    Transaction res = CacheWrapper.GetFromList<Transaction>(CacheKey, new Transaction() { Id = id }, userId);
        //    if (res == null || forceSqlLoad)
        //    {
        //        //load from DB
        //        //TODO: on db side - check that trans belongs to user
        //        res = new Transaction() { Description = "Test", Category = "Dinner", Amount = 100.20M, Id = 1, Date = DateTime.Now };
        //        //save to cache
        //        if (res == null) // not found in cache->add
        //            CacheWrapper.AddToList<Transaction>(CacheKey, res, userId);
        //        else
        //            CacheWrapper.UpdateList<Transaction>(CacheKey, res, userId);
        //    }
        //    return res;
        //}

        //public static int Insert(Transaction trans, int? userId)
        //{
        //    int res = -1;
        //    //insert to db
        //    res = 0;
        //    byte[] randomNumber = new byte[2]; 
        //    RNGCryptoServiceProvider Gen = new RNGCryptoServiceProvider();
        //    Gen.GetBytes(randomNumber);
        //    int rand = Convert.ToInt32(randomNumber[1] * 256 + randomNumber[0]);
        //    trans.Id = rand;
        //    if (res == 0)
        //    {
        //        //if ok - save to cache
        //        CacheWrapper.AddToList<Transaction>(CacheKey, trans, userId);
        //    }
        //    return res;
        //}

    }
}
