﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

using ServiceStack.Redis;
using Naviam.Data;

namespace TstParser
{
    class Program
    {

        static void Main(string[] args)
        {
            //Can't load 900000-600000 from 1 key -> buffer to small
            //Can't save 1300000 to 1 key -> ?

            //50 mb
            //Took 3531.25ms to store (300000 records in 1 key)
            //Took 2953.125ms to load (300000 records from 1 key)
            //Removed all keys from db
            //Took 17765.625ms to store (300000 records in list)
            //Took 3140.625ms to load (300000 records from list)
            ///************************************
            //Took 5265.625ms to store (500000 records in 1 key)
            //Took 4921.875ms to load (500000 records from 1 key)
            //Removed all keys from db
            //Took 29531.25ms to store (500000 records in list)
            //Took 5359.375ms to load (500000 records from list)
            //tst redis
            int totalRecords = 300000;
            var trans = new List<Transaction>();
            for (int i = 0; i < totalRecords; i++)
            {
                trans.Add(new Transaction() { Description = "test Description _Description_" + i, Id = i });
            }

            var before = DateTime.Now;

            //TimeSpan exp = new TimeSpan(0, 2400, 0);
            using (var redisClient = new RedisClient())
            {
                var typedRedis = redisClient.GetTypedClient<List<Transaction>>();
                typedRedis.SetEntry("test", trans);
            }
            Console.WriteLine("Took {0}ms to store ({1} records in 1 key)", (DateTime.Now - before).TotalMilliseconds, totalRecords);

            before = DateTime.Now;
            using (var redisClient = new RedisClient())
            {
                var typedRedis = redisClient.GetTypedClient<List<Transaction>>();
                trans = typedRedis["test"];
            }
            Console.WriteLine("Took {0}ms to load ({1} records from 1 key)", (DateTime.Now - before).TotalMilliseconds, trans.Count);

            before = DateTime.Now;
            using (var redisClient = new RedisClient())
            {
                var typedRedis = redisClient.GetTypedClient<List<Transaction>>();
                trans = typedRedis["test"];
            }
            Console.WriteLine("Took {0}ms to load (1 item from 1 key): {1}", (DateTime.Now - before).TotalMilliseconds, trans[0].Description);

            using (var redisClient = new RedisClient())
            {
                redisClient.FlushAll();
            }
            Console.WriteLine("Removed all keys from db");

            using (var redisClient = new RedisClient())
            {
                var typedRedis = redisClient.GetTypedClient<Transaction>();
                var currStat = typedRedis.Lists["test"];
                //trans.ForEach(x => currStat.Add(x));
                currStat.AddRange(trans);
            }
            Console.WriteLine("Took {0}ms to store ({1} records in list)", (DateTime.Now - before).TotalMilliseconds, totalRecords);

            before = DateTime.Now;
            using (var redisClient = new RedisClient())
            {
                var typedRedis = redisClient.GetTypedClient<Transaction>();
                var currStat = typedRedis.Lists["test"];
                trans = currStat.GetAll();
            }
            Console.WriteLine("Took {0}ms to load ({1} records from list)", (DateTime.Now - before).TotalMilliseconds, trans.Count);

            before = DateTime.Now;
            using (var redisClient = new RedisClient())
            {
                var typedRedis = redisClient.GetTypedClient<Transaction>();
                var currStat = typedRedis.Lists["test"];
                var tr = new Transaction() { Id = 100 };
                var res = currStat[currStat.IndexOf(tr)];
                Console.WriteLine("Took {0}ms to load (1 item from 1 key): {1}", (DateTime.Now - before).TotalMilliseconds, res.Description);
            }

            using (var redisClient = new RedisClient())
            {
                redisClient.FlushAll();
            }
            Console.WriteLine("Removed all keys from db");

            /*Console.WriteLine("trying to fill all memory");
            using (var redisClient = new RedisClient())
            {
                var typedRedis = redisClient.GetTypedClient<Transaction>();
                var currStat = typedRedis.Lists["test"];
                int i = 0;
                while (true)
                {
                    var tr = new Transaction() { Description = "test Description _Description_" + i, Id = i };
                    currStat.Add(tr);
                    i++;
                }
            }*/

            //Load of 300000:
            //simple-4900ms
            //list-4600ms
            
            //Save of 300000:
            //simple-3200ms
            //list-15300ms
            Console.ReadLine();
            return;
        }
    }
}
