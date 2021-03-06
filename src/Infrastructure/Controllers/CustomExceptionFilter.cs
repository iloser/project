﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class CustomExceptionFilter : Attribute, IExceptionFilter {
	private ILogger _logger = null;
	private IConfigurationRoot _cfg = null;
	private IHostingEnvironment _env = null;

	public CustomExceptionFilter (ILogger<CustomExceptionFilter> logger, IConfigurationRoot cfg, IHostingEnvironment env) {
		_logger = logger;
		_cfg = cfg;
		_env = env;
	}

	public void OnException (ExceptionContext context) {
		//在这里记录错误日志，context.Exception 为异常对象
		context.Result = APIReturn.失败.SetMessage(context.Exception.Message); //返回给调用方
		context.ExceptionHandled = true;
	}
}