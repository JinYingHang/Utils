using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqlSugerLib
{
    public class SqlSugarHelper //不能是泛型类
    {
       private static SqlSugarScope SqlSugarScope;
        public static SqlSugarScope Db {
            get {
                if (SqlSugarScope == null) {
                    SqlSugarScope = new SqlSugarScope(new ConnectionConfig() {
                        ConnectionString = "Server=.xxxxx",//连接符字串
                        DbType = DbType.SqlServer,//数据库类型
                        IsAutoCloseConnection = true //不设成true要手动close
                    },
                     db => {
                         db.Aop.OnLogExecuting = (sql, pars) => {
                             Console.WriteLine(sql);//输出sql,查看执行sql 性能无影响
                         };
                     });
                }
                return SqlSugarScope;
            }
        }
    }
}
