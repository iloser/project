using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using user.BLL;
using user.Model;

namespace user.Module.Admin.Controllers {
	[Route("[controller]")]
	public class PersonController : BaseController {
		public PersonController(ILogger<PersonController> logger) : base(logger) { }

		[HttpGet]
		public ActionResult List([FromServices]IConfigurationRoot cfg, [FromQuery] string key, [FromQuery] int limit = 20, [FromQuery] int page = 1) {
			var select = Person.Select
				.Where(!string.IsNullOrEmpty(key), "a.name like {0}", string.Concat("%", key, "%"));
			long count;
			var items = select.Count(out count).Skip((page - 1) * limit).Limit(limit).ToList();
			ViewBag.items = items;
			ViewBag.count = count;
			return View();
		}

		[HttpGet(@"add")]
		public ActionResult Edit() {
			return View();
		}
		[HttpGet(@"edit")]
		public ActionResult Edit([FromQuery] int Id) {
			PersonInfo item = Person.GetItem(Id);
			if (item == null) return APIReturn.记录不存在_或者没有权限;
			ViewBag.item = item;
			return View();
		}

		/***************************************** POST *****************************************/
		[HttpPost(@"add")]
		[ValidateAntiForgeryToken]
		public APIReturn _Add([FromForm] int? Id, [FromForm] int? Age, [FromForm] string Name) {
			PersonInfo item = new PersonInfo();
			item.Id = Id;
			item.Age = Age;
			item.Name = Name;
			item = Person.Insert(item);
			return APIReturn.成功.SetData("item", item.ToBson());
		}
		[HttpPost(@"edit")]
		[ValidateAntiForgeryToken]
		public APIReturn _Edit([FromQuery] int Id, [FromForm] int? Age, [FromForm] string Name) {
			PersonInfo item = Person.GetItem(Id);
			if (item == null) return APIReturn.记录不存在_或者没有权限;
			item.Age = Age;
			item.Name = Name;
			int affrows = Person.Update(item);
			if (affrows > 0) return APIReturn.成功.SetMessage($"更新成功，影响行数：{affrows}");
			return APIReturn.失败;
		}

		[HttpPost("del")]
		[ValidateAntiForgeryToken]
		public APIReturn _Del([FromForm] int[] ids) {
			int affrows = 0;
			foreach (int id in ids)
				affrows += Person.Delete(id);
			if (affrows > 0) return APIReturn.成功.SetMessage($"删除成功，影响行数：{affrows}");
			return APIReturn.失败;
		}

        [HttpGet("test")]
        public APIReturn Get()
        {
            var users = Person.Select.ColName.ColId.InnerJoin<Person>("person", "name,id", "a.id=person.id").ToList();
            return APIReturn.成功.SetData("users",users) ;
        }
    }
}
