﻿using System;
using System.Collections.Generic;
using Naviam.Data;
using System.Data.SqlClient;

namespace Naviam.DAL
{
    public class TransactionsDataAdapter
    {

        public static List<Transaction> GetTestTransactions(int recordsCount, int? companyId)
        {
            var res = new List<Transaction>(recordsCount);
            for (int i = 0; i < recordsCount; i++)
                res.Add(new Transaction()
                {
                    Description = "Test" + i.ToString() + "_" + companyId,
                    CategoryId = new Random(19).Next(recordsCount),
                    Amount = 100.20M,
                    Id = i,
                    Date = DateTime.Now
                });
            return res;
        }

        public static List<Transaction> GetTransactions(int? companyId)
        {
            var res = new List<Transaction>();
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("transactions_get"))
                {
                    cmd.Parameters.AddWithValue("@id_company", companyId);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var trans = new Transaction(reader);
                                res.Add(trans);
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
            return res;
        }

        public static Transaction GetTransaction(int? id, int? companyId)
        {
            Transaction res = null;
            //TODO: check that trans belongs to company
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("transactions_get"))
                {
                    cmd.Parameters.AddWithValue("@id_company", companyId);
                    cmd.Parameters.AddWithValue("@id_transaction", id);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
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
            return res;
        }

        public static int InsertUpdate(Transaction entity, int? companyId, DbActionType action)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                var commName = action == DbActionType.Insert ? "transaction_create" : "web.transaction_update";
                var cmd = holder.Connection.CreateSPCommand(commName);
                try
                {
                    cmd.AddEntityParameters(entity, action);
                    if (action == DbActionType.Update)
                        cmd.Parameters.AddWithValue("@id_company", companyId);
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
            return res;
        }

        public static int BatchInsert(List<Transaction> list, int? companyId, DbActionType action)
        {
            var res = -1;
            var commName = action == DbActionType.Insert ? "transaction_create" : "web.transaction_update";
            using (var scope = new System.Transactions.TransactionScope())
            {
                using (var holder = SqlConnectionHelper.GetConnection())
                {
                    var cmd = holder.Connection.CreateSPCommand(commName);
                    //SqlTransaction transaction = holder.Connection.BeginTransaction("BatchInsertTransactions");
                    //cmd.Transaction = transaction;
                    try
                    {
                        foreach (var entity in list)
                        {
                            cmd.Parameters.Clear();
                            cmd.AddEntityParameters(entity, action);
                            if (action == DbActionType.Update)
                                cmd.Parameters.AddWithValue("@id_company", companyId);
                            cmd.ExecuteNonQuery();
                            if (action == DbActionType.Insert)
                                entity.Id = cmd.GetRowIdParameter();
                            int l_res = cmd.GetReturnParameter();
                            if (l_res != 0)
                                return l_res;
                        }
                        //transaction.Commit();
                        scope.Complete();
                        res = 0;
                    }
                    catch (SqlException e)
                    {
                        //transaction.Rollback();
                        cmd.AddDetailsToException(e);
                        throw;
                    }
                }
            }
            return res;
        }

        public static int Delete(int? id, int? companyId)
        {
            var res = -1;
            using (var holder = SqlConnectionHelper.GetConnection())
            {
                using (var cmd = holder.Connection.CreateSPCommand("transaction_delete"))
                {
                    try
                    {
                        cmd.AddCommonParameters(id);
                        cmd.Parameters.AddWithValue("@id_company", companyId);
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
