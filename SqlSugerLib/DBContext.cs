using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace SqlSugarHelper
{
    public class DbContext<T> where T : class, new()
    {
        //用来处理事务多表查询和复杂的操作
        public SqlSugarClient Db;

        //用来处理事务多表查询和复杂的操作
        public SimpleClient<T> CurrentDb { get { return new SimpleClient<T>(Db); } }

        public DbContext() {
            try {
                Db = new SqlSugarClient(new ConnectionConfig() {
                    ConnectionString = Config.ReadConfigValue("ConnectionString"),
                    DbType = (DbType)int.Parse(Config.ReadConfigValue("DbType")),
                    InitKeyType = InitKeyType.Attribute,
                    IsAutoCloseConnection = true,
                });
                Db.Aop.OnLogExecuting = (sql, pars) => {
                    Console.WriteLine(sql + "\r\n" + Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                };
            }
            catch (Exception) {
                throw;
            }
        }


        public virtual List<T> GetList() {
            try {
                return CurrentDb.GetList();
            }
            catch (Exception) {
                throw;
            }
        }

        public virtual bool Delete(dynamic id) {
            try {
                return CurrentDb.Delete(id);
            }
            catch (Exception) {
                throw;
            }
        }

        public virtual bool Update(T obj) {
            try {
                return CurrentDb.Update(obj);
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
