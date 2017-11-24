using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Swashbuckle.AspNetCore.Swagger {
	public class FormDataOperationFilter : IOperationFilter {
		public void Apply(Operation operation, OperationFilterContext context) {
			var actattrs = context.ApiDescription.ActionAttributes();
			if (actattrs.OfType<HttpPostAttribute>().Any() ||
				actattrs.OfType<HttpPutAttribute>().Any())
				operation.Consumes = new[] { "multipart/form-data" };
		}
	}

	public static class SwashbuckleSwaggerExtensions {
		public static IServiceCollection AddCustomizedSwaggerGen(this IServiceCollection services) {
			services.AddSwaggerGen(options => {
				options.SwaggerDoc("v1", new Info {
					Version = "v1",
					Title = "WebAPI",
					Description = "项目webapi接口说明",
					TermsOfService = "None",
					Contact = new Contact { Name = "duoyi", Email = "", Url = "http://duoyi.com" },
					License = new License { Name = "duoyi", Url = "http://duoyi.com" }
				});
				options.IgnoreObsoleteActions();
				//options.IgnoreObsoleteControllers(); // 类、方法标记 [Obsolete]，可以阻止【Swagger文档】生成
				options.DescribeAllEnumsAsStrings();
				//options.IncludeXmlComments(AppContext.BaseDirectory + @"/Admin.xml"); // 使用前需开启项目注释 xmldoc
				options.OperationFilter<FormDataOperationFilter>();
			});
			return services;
		}
	}
}
