using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace user.BLL {

	public partial class RedisHelper : CSRedis.QuickHelperBase {
		public static IConfigurationRoot Configuration { get; internal set; }
		public static void InitializeConfiguration(IConfigurationRoot cfg) {
			Configuration = cfg;
			int port, poolsize, database;
			string ip, pass;
			if (!int.TryParse(cfg["ConnectionStrings:redis:port"], out port)) port = 6379;
			if (!int.TryParse(cfg["ConnectionStrings:redis:poolsize"], out poolsize)) poolsize = 50;
			if (!int.TryParse(cfg["ConnectionStrings:redis:database"], out database)) database = 0;
			ip = cfg["ConnectionStrings:redis:ip"];
			pass = cfg["ConnectionStrings:redis:pass"];
			Name = cfg["ConnectionStrings:redis:name"];
			Instance = new CSRedis.ConnectionPool(ip, port, poolsize);
			Instance.Connected += (s, o) => {
				CSRedis.RedisClient rc = s as CSRedis.RedisClient;
				if (!string.IsNullOrEmpty(pass)) rc.Auth(pass);
				if (database > 0) rc.Select(database);
			};
		}
	}

	//截至 1.2.6 版本仍然有 Timeout bug
	//public partial class RedisHelper : StackExchange.Redis.QuickHelperBase {
	//	public static IConfigurationRoot Configuration { get; internal set; }
	//	public static void InitializeConfiguration(IConfigurationRoot cfg) {
	//		Configuration = cfg;
	//		int port, poolsize, database;
	//		string ip, pass;
	//		if (!int.TryParse(cfg["ConnectionStrings:redis:port"], out port)) port = 6379;
	//		if (!int.TryParse(cfg["ConnectionStrings:redis:poolsize"], out poolsize)) poolsize = 50;
	//		if (!int.TryParse(cfg["ConnectionStrings:redis:database"], out database)) database = 0;
	//		ip = cfg["ConnectionStrings:redis:ip"];
	//		pass = cfg["ConnectionStrings:redis:pass"];
	//		Name = cfg["ConnectionStrings:redis:name"];
	//		Instance = new StackExchange.Redis.ConnectionMultiplexerPool($"{ip}:{port},password={pass},name={Name},defaultdatabase={database}", poolsize);
	//	}
	//}

	public static partial class BLLExtensionMethods {
		public static List<TReturnInfo> ToList<TReturnInfo>(this SelectBuild<TReturnInfo> select, int expireSeconds, string cacheKey = null) { return select.ToList(RedisHelper.Get, RedisHelper.Set, TimeSpan.FromSeconds(expireSeconds), cacheKey); }
	}
}