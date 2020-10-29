using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using NRTA.Core;

namespace NRTA.Spider
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddJsonFile(Executor.SpiderJsonConfigFilename)
                .AddJsonFile(Executor.UserListFilename, true)
                .Build();
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("disable-web-security");
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            var webDriver = new ChromeDriver(options);
            var executor = new Executor(configuration, webDriver);
            executor.RunSpider(args);

            Console.ReadKey();
            Console.WriteLine("关闭浏览器");
            webDriver.Close();
            webDriver.Dispose();
            return;
        }
        
    }
}
