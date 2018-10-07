﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CoinExchangeWatcher
{
    public class DbHelper
    {
        public static void CreateDb(string dbName)
        {
            if (File.Exists(dbName))
                return;
            SQLiteConnection.CreateFile(dbName);
            string sqlString = "CREATE TABLE TransData (CoinType TEXT NOT NULL,Height INTEGER NOT NULL,Txid TEXT NOT NULL,Address TEXT NOT NULL,Value REAL NOT NULL,ConfirmCount INTEGER NOT NULL,UpdateTime TEXT NOT NULL);" +
                               "CREATE TABLE Address (CoinType TEXT NOT NULL,Address TEXT NOT NULL,DateTime TEXT NOT NULL)";
            SQLiteConnection conn = new SQLiteConnection();
            conn.ConnectionString = "DataSource = " + dbName;
            conn.Open();           
            SQLiteCommand cmd = new SQLiteCommand(conn)
            {
                CommandText = sqlString
            };
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        /// <summary>
        /// 保存监控地址
        /// </summary>
        /// <param name="json"></param>
        public static void SaveAddress(JObject json)
        {
            var sql = $"insert into Address (CoinType,Address,DateTime) values ('{json["type"]}','{json["address"]}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:SS")}')";
            ExecuteSql(sql);
        }

        /// <summary>
        /// 保存交易信息
        /// </summary>
        /// <param name="transRspList"></param>
        public static void SaveTransInfo(List<TransResponse> transRspList)
        {
            StringBuilder sbSql = new StringBuilder();
            foreach (var tran in transRspList)
            {
                if (tran.confirmcount == 1)
                {
                    sbSql.Append(
                        $"insert into TransData (CoinType,Height,Txid,Address,Value,ConfirmCount,UpdateTime) values ('{tran.coinType}',{tran.height},'{tran.txid}','{tran.address}',{tran.value},{tran.confirmcount},'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:SS")}');");
                }
                else
                {
                    sbSql.Append(
                        $"update TransData set ConfirmCount={tran.confirmcount},UpdateTime='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:SS")}';");
                }

            }
            ExecuteSql(sbSql.ToString());
        }

        public static List<string> GetEthAddr()
        {
            throw new NotImplementedException();
        }

        public static int GetBtcIndex()
        {
            throw new NotImplementedException();
        }

        public static List<string> GetBtcAddr()
        {
            throw new NotImplementedException();
        }

        public static int GetEthIndex()
        {
            throw new NotImplementedException();
        }

        private static void ExecuteSql(string sql)
        {
            SQLiteConnection conn = new SQLiteConnection("Data Source = MonitorData.db");
            conn.Open();
            //事务操作
            SQLiteTransaction trans = conn.BeginTransaction();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.Transaction = trans;
            cmd.CommandText = sql.ToString();
            try
            {
                cmd.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex)
            {
                File.WriteAllText("saveErrLog.txt", ex.ToString());
                trans.Rollback();
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
