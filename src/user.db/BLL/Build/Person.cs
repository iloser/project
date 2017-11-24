using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using user.Model;

namespace user.BLL
{

    public partial class Person
    {

        protected static readonly user.DAL.Person dal = new user.DAL.Person()
        {
            Field = new List<string>()
        };
        protected static readonly int itemCacheTimeout;

        static Person()
        {
            if (!int.TryParse(RedisHelper.Configuration["user_BLL_ITEM_CACHE:Timeout_Person"], out itemCacheTimeout))
                int.TryParse(RedisHelper.Configuration["user_BLL_ITEM_CACHE:Timeout"], out itemCacheTimeout);
        }

        #region delete, update, insert

        public static int Delete(int Id)
        {
            if (itemCacheTimeout > 0) RemoveCache(GetItem(Id));
            return dal.Delete(Id);
        }

        public static int Update(PersonInfo item)
        {
            if (itemCacheTimeout > 0) RemoveCache(item);
            return dal.Update(item);
        }
        public static user.DAL.Person.SqlUpdateBuild UpdateDiy(int Id)
        {
            return UpdateDiy(null, Id);
        }
        public static user.DAL.Person.SqlUpdateBuild UpdateDiy(PersonInfo item, int Id)
        {
            if (itemCacheTimeout > 0) RemoveCache(item != null ? item : GetItem(Id));
            return new user.DAL.Person.SqlUpdateBuild(item, Id);
        }
        /// <summary>
        /// 用于批量更新
        /// </summary>
        public static user.DAL.Person.SqlUpdateBuild UpdateDiyDangerous
        {
            get { return new user.DAL.Person.SqlUpdateBuild(); }
        }

        public static PersonInfo Insert(int? Id, int? Age, string Name)
        {
            return Insert(new PersonInfo
            {
                Id = Id,
                Age = Age,
                Name = Name
            });
        }
        public static PersonInfo Insert(PersonInfo item)
        {
            item = dal.Insert(item);
            if (itemCacheTimeout > 0) RemoveCache(item);
            return item;
        }
        private static void RemoveCache(PersonInfo item)
        {
            if (item == null) return;
            RedisHelper.Remove(string.Concat("user_BLL_Person_", item.Id));
        }
        #endregion

        public static PersonInfo GetItem(int Id)
        {
            if (itemCacheTimeout <= 0) return Select.WhereId(Id).ToOne();
            string key = string.Concat("user_BLL_Person_", Id);
            string value = RedisHelper.Get(key);
            if (!string.IsNullOrEmpty(value))
                try { return PersonInfo.Parse(value); } catch { }
            PersonInfo item = Select.WhereId(Id).ToOne();
            if (item == null) return null;
            RedisHelper.Set(key, item.Stringify(), itemCacheTimeout);
            return item;
        }

        public static List<PersonInfo> GetItems()
        {
            return Select.ToList();
        }
        public static PersonSelectBuild Select
        {
            get { return new PersonSelectBuild(dal); }
        }
    }
    public partial class PersonSelectBuild : SelectBuild<PersonInfo, PersonSelectBuild>
    {
        public PersonSelectBuild ColId
        {
            get
            {
                this._dals[0].Field.Add("a.`id`");
                return this;
            }
        }
        public PersonSelectBuild ColName
        {
            get
            {
                this._dals[0].Field.Add("a.`name`");
                return this;
            }
        }
        public PersonSelectBuild ColAge
        {
            get
            {
                this._dals[0].Field.Add("a.`age`");
                return this;
            }
        }
        public PersonSelectBuild WhereId(params int[] Id)
        {
            return this.Where1Or("a.`id` = {0}", Id);
        }
        public PersonSelectBuild WhereAge(params int?[] Age)
        {
            return this.Where1Or("a.`age` = {0}", Age);
        }
        public PersonSelectBuild WhereName(params string[] Name)
        {
            return this.Where1Or("a.`name` = {0}", Name);
        }
        public PersonSelectBuild WhereNameLike(params string[] Name)
        {
            if (Name == null || Name.Where(a => !string.IsNullOrEmpty(a)).Any() == false) return this;
            return this.Where1Or(@"a.`name` LIKE {0}", Name.Select(a => "%" + a + "%").ToArray());
        }
        protected new PersonSelectBuild Where1Or(string filterFormat, Array values)
        {
            return base.Where1Or(filterFormat, values) as PersonSelectBuild;
        }
        public PersonSelectBuild(IDAL dal) : base(dal, SqlHelper.Instance) { }
    }
}