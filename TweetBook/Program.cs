﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TweetBook.Data;

namespace TweetBook
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
           var host = CreateWebHostBuilder(args).Build();

            using (var service = host.Services.CreateScope())
            {
                var dbContext = service.ServiceProvider.GetRequiredService<DataContext>();

                await dbContext.Database.MigrateAsync();
            }

            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
