using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using NRTA.Core;
using System.Threading;
using OpenQA.Selenium.Support.Extensions;
using System.IO;

namespace NTRA.App
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            string chromePath = @"Chrome\App\Google Chrome\chrome.exe";
            if (File.Exists(chromePath))
            {
                Console.WriteLine("检测到便携版Chrome，使用便携版");
                options.BinaryLocation = chromePath;
            }
            else
            {
                Console.WriteLine("未检测到便携版Chrome，尝试使用本机自带Chrome");
            }
            options.AddArgument("disable-web-security");
            
            var webDriver = new ChromeDriver(Environment.CurrentDirectory, options);
            var executor = new Executor(null, webDriver);
            Console.WriteLine("打开考试系统页面...");
            webDriver.Navigate().GoToUrl("http://js.nrta.gov.cn/account/login/1");
            Console.WriteLine("等待进入答题页面...");
            while (true)
            {
                if (!executor.CheckExamPage())
                {
                    Thread.Sleep(1000);
                    continue;
                }
                Console.WriteLine("检测到答题页面，开始填写答案...");
                executor.FillWebAnswer();
                Console.WriteLine("填写答案完毕！为避免被查出，请手动选择标绿色的选项");
                break;
            }
            Console.WriteLine("按任意键退出");
            Console.ReadKey();
            Console.WriteLine("关闭浏览器");
            webDriver.Close();
            webDriver.Dispose();
            return;
        }
    }
}
