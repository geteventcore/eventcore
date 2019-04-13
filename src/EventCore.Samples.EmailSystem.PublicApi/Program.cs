﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace EventCore.Samples.EmailSystem.PublicApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
				WebHost.CreateDefaultBuilder(args)
						.UseStartup<Startup>()
						.UseUrls("http://localhost:95001/");
	}
}
