using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqlSugerLib
{
    internal class Test
    {
        public void Demo() {
            //建表
            //db.CodeFirst.InitTables<Student>(); 更多看文档迁移   

            //查询表的所有
            var list = SqlSugarHelper.Db.Queryable<Student>().ToList();

            //插入
            SqlSugarHelper.Db.Insertable(new Student() { SchoolId = 1, Name = "jack" }).ExecuteCommand();

            //更新
            SqlSugarHelper.Db.Updateable(new Student() { Id = 1, SchoolId = 2, Name = "jack2" }).ExecuteCommand();

            //删除
            SqlSugarHelper.Db.Deleteable<Student>().Where(it => it.Id == 1).ExecuteCommand();

        }
        //实体与数据库结构一样
        public class Student
        {
            //数据是自增需要加上IsIdentity 
            //数据库是主键需要加上IsPrimaryKey 
            //注意：要完全和数据库一致2个属性
            [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
            public int Id { get; set; }
            public int? SchoolId { get; set; }
            public string Name { get; set; }
        }
    }
}
