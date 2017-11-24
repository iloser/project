using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using user.Model;

namespace user.DAL
{

    public partial class Person : IDAL
    {
        #region transact-sql define
        public string Table { get { return TSQL.Table; } }
        public List<string> Field { get; set; }
        public string Sort { get { return TSQL.Sort; } }
        internal class TSQL
        {
            internal static readonly string Table = "`person`";
            //internal static readonly string Field = "a.`id`, a.`age`, a.`name`";
            internal static readonly string Sort = "a.`id`";
            public static readonly string Delete = "DELETE FROM `person` WHERE ";
            public static readonly string Insert = "INSERT INTO `person`(`id`, `age`, `name`) VALUES(?id, ?age, ?name)";
        }
        #endregion

        #region common call
        protected static MySqlParameter GetParameter(string name, MySqlDbType type, int size, object value)
        {
            MySqlParameter parm = new MySqlParameter(name, type, size);
            parm.Value = value;
            return parm;
        }
        protected static MySqlParameter[] GetParameters(PersonInfo item)
        {
            return new MySqlParameter[] {
                GetParameter("?id", MySqlDbType.Int32, 11, item.Id),
                GetParameter("?age", MySqlDbType.Int32, 11, item.Age),
                GetParameter("?name", MySqlDbType.VarChar, 45, item.Name)};
        }
        public PersonInfo GetItem(IDataReader dr)
        {
            int index = -1;
            return GetItem(dr, ref index) as PersonInfo;
        }
        public object GetItem(IDataReader dr, ref int index)
        {
            PersonInfo item = new PersonInfo();
            for (var i = 0; i < this.Field.Count; i++)
                if (!dr.IsDBNull(++index))
                {
                    var name = dr.GetName(index);
                    name = name.Substring(0, 1).ToUpper() + name.Substring(1);
                    item[name] = dr.GetValue(index);
                }
            return item;
        }
        #endregion

        public int Delete(int Id)
        {
            return SqlHelper.ExecuteNonQuery(string.Concat(TSQL.Delete, "`id` = ?id"),
                GetParameter("?id", MySqlDbType.Int32, 11, Id));
        }

        public int Update(PersonInfo item)
        {
            return new SqlUpdateBuild(null, item.Id.Value)
                .SetAge(item.Age)
                .SetName(item.Name).ExecuteNonQuery();
        }
        #region class SqlUpdateBuild
        public partial class SqlUpdateBuild
        {
            protected PersonInfo _item;
            protected string _fields;
            protected string _where;
            protected List<MySqlParameter> _parameters = new List<MySqlParameter>();
            public SqlUpdateBuild(PersonInfo item, int Id)
            {
                _item = item;
                _where = SqlHelper.Addslashes("`id` = {0}", Id);
            }
            public SqlUpdateBuild() { }
            public override string ToString()
            {
                if (string.IsNullOrEmpty(_fields)) return string.Empty;
                if (string.IsNullOrEmpty(_where)) throw new Exception("防止 user.DAL.Person.SqlUpdateBuild 误修改，请必须设置 where 条件。");
                return string.Concat("UPDATE ", TSQL.Table, " SET ", _fields.Substring(1), " WHERE ", _where);
            }
            public int ExecuteNonQuery()
            {
                string sql = this.ToString();
                if (string.IsNullOrEmpty(sql)) return 0;
                return SqlHelper.ExecuteNonQuery(sql, _parameters.ToArray());
            }
            public SqlUpdateBuild Where(string filterFormat, params object[] values)
            {
                if (!string.IsNullOrEmpty(_where)) _where = string.Concat(_where, " AND ");
                _where = string.Concat(_where, "(", SqlHelper.Addslashes(filterFormat, values), ")");
                return this;
            }
            public SqlUpdateBuild WhereExists<T>(SelectBuild<T> select)
            {
                return this.Where($"EXISTS({select.ToString("1")})");
            }
            public SqlUpdateBuild WhereNotExists<T>(SelectBuild<T> select)
            {
                return this.Where($"NOT EXISTS({select.ToString("1")})");
            }

            public SqlUpdateBuild Set(string field, string value, params MySqlParameter[] parms)
            {
                if (value.IndexOf('\'') != -1) throw new Exception("user.DAL.Person.SqlUpdateBuild 可能存在注入漏洞，不允许传递 ' 给参数 value，若使用正常字符串，请使用参数化传递。");
                _fields = string.Concat(_fields, ", ", field, " = ", value);
                if (parms != null && parms.Length > 0) _parameters.AddRange(parms);
                return this;
            }
            public SqlUpdateBuild SetAge(int? value)
            {
                if (_item != null) _item.Age = value;
                return this.Set("`age`", $"?age_{_parameters.Count}",
                    GetParameter($"?age_{_parameters.Count}", MySqlDbType.Int32, 11, value));
            }
            public SqlUpdateBuild SetAgeIncrement(int value)
            {
                if (_item != null) _item.Age += value;
                return this.Set("`age`", $"ifnull(`age`, 0) + ?age_{_parameters.Count}",
                    GetParameter($"?age_{_parameters.Count}", MySqlDbType.Int32, 11, value));
            }
            public SqlUpdateBuild SetName(string value)
            {
                if (_item != null) _item.Name = value;
                return this.Set("`name`", $"?name_{_parameters.Count}",
                    GetParameter($"?name_{_parameters.Count}", MySqlDbType.VarChar, 45, value));
            }
        }
        #endregion

        public PersonInfo Insert(PersonInfo item)
        {
            SqlHelper.ExecuteNonQuery(TSQL.Insert, GetParameters(item));
            return item;
        }

    }
}