using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NRTA.Core.Models;
using NRTA.Core.Data;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Interactions;

namespace NRTA.Core
{
    public class Executor
    {
        public static string SpiderJsonConfigFilename = "SpiderConfig.json";
        public static string UserListFilename = "SpiderUsers.json";

        private readonly IConfiguration _configuration;
        private readonly IWebDriver _webDriver;
        private readonly IWait<IWebDriver> _webWait;
        private readonly NRTADbContext _dbContext;
        private const string registeredUsersSection = "RegigteredUsers";
        public Executor(IConfiguration configuration,IWebDriver webDriver)
        {
            _configuration = configuration;
            _webDriver = webDriver;
            _webWait = new WebDriverWait(_webDriver,TimeSpan.FromSeconds(20));
            _dbContext = new NRTADbContext();
        }

        public void RunSpider(string[] args)
        {
            Console.WriteLine("请选择要进行的操作\r\n1.注册账号(默认)\r\n2.爬取");
            var operationInput = ReadLineWithDefault("1");
            if (operationInput == "1")
            {
                Register(1, 50);
            }
            else
            {
                Crawl();
            }
            //Crawl();
        }

        /// <summary>
        /// 读取输入文本，若输入为空则返回默认值
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string ReadLineWithDefault(string defaultString)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                input = defaultString;
            return input;
        }
        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="defaultUsername"></param>
        /// <param name="defaultPassword"></param>
        /// <param name="realNames"></param>
        public void Register(string defaultUsername, string defaultPassword, List<string> realNames,int startUsernameNumber,int endUsernameNumber)
        {
            Console.WriteLine("注册信息:");
            Console.WriteLine("用户名: " + defaultUsername);
            Console.WriteLine("密码: " + defaultPassword);
            Console.WriteLine("姓名列表: " + string.Join(" ", realNames));

            List<User> users = GetConfigurationUsers();
            string[] existedUsernames = users.Select(u => u.Username).ToArray();
            var rand = new Random();
            for (int i = startUsernameNumber; i < endUsernameNumber + 1; i++)
            {
                var user = new User
                {
                    Username = defaultUsername + string.Format("{0:000}", i),
                    Password = defaultPassword,
                    RealName = realNames[rand.Next(realNames.Count)]
                };
                if (existedUsernames.Contains(user.Username))
                {
                    Console.WriteLine("用户名: {0} 已存在，跳过", user.Username);
                    continue;
                }
                Console.WriteLine("尝试注册用户 {0}", user);
                _webDriver.Navigate().GoToUrl("http://js.nrta.gov.cn/account/register/1");
                //填写用户信息
                var regInfoDiv = _webWait.Until(d => d.FindElement(By.Id("examRegStep1")));
                var usernameInput = regInfoDiv.FindElement(By.CssSelector(".uname input"));
                var realnameInput = regInfoDiv.FindElement(By.CssSelector(".name input"));
                var passwordInput = regInfoDiv.FindElement(By.CssSelector(".password input"));
                var confirmInput = regInfoDiv.FindElement(By.CssSelector(".password_confirm input"));
                usernameInput.SendKeys(user.Username);
                realnameInput.SendKeys(user.RealName);
                passwordInput.SendKeys(user.Password);
                confirmInput.SendKeys(user.Password);
                var nextStepBtn = regInfoDiv.FindElement(By.ClassName("btn-next-step"));
                nextStepBtn.Click();
                StringBuilder depPath = new StringBuilder();
                //选择部门
                var regDepDiv = _webWait.Until(d => d.FindElement(By.Id("examRegStep2")));
                regDepDiv.FindElements(By.CssSelector(".select.dep .current-option")).Last().Click();
                Thread.Sleep(500);
                regDepDiv.FindElements(By.CssSelector(".select.dep .dep-option")).Last().Click();
                Thread.Sleep(500);
                string[] options =
                {
                    "国家广播电视总局",
                    "直属单位",
                    null,
                    null
                };
                foreach (var option in options)
                {
                    var depName = selectDep(regDepDiv, option);
                    depPath.Append(depName + " ");
                }
                regDepDiv.FindElement(By.CssSelector(".form-row.sort-row.field1 input")).SendKeys("办公室");
                regDepDiv.FindElement(By.CssSelector(".btn-finish")).Click();
                Thread.Sleep(2000);
                if (!string.IsNullOrEmpty(_webDriver.FindElement(By.CssSelector(".finish-text .uname")).Text))
                    Console.WriteLine("注册成功");
                else
                    Console.WriteLine("注册失败");
                user.Department = depPath.ToString();
                users.Add(user);
            }
            Console.WriteLine("保存账号信息");
            SaveUsers(users);
        }
        //从配置文件中读取用户，如果未读取到则返回空列表
        private List<User> GetConfigurationUsers()
        {
            var users = _configuration.GetSection(registeredUsersSection).Get<List<User>>();
            if (users == null)
                users = new List<User>();
            return users;
        }

        /// <summary>
        /// 读取配置文件信息并注册用户
        /// </summary>
        /// <param name="startUsernameNumber"></param>
        /// <param name="endUsernameNumber"></param>
        public void Register(int startUsernameNumber, int endUsernameNumber)
        {
            var registerUserInfo = _configuration.GetSection("RegisterUserInfo");
            var defaultUsername = registerUserInfo["Username"];
            var defaultPassword = registerUserInfo["DefaultPassword"];
            var realNames = registerUserInfo.GetSection("RealNames").Get<List<string>>();
            Register(defaultUsername, defaultPassword, realNames, startUsernameNumber, endUsernameNumber);
        }
        /// <summary>
        /// 注册用户时选择部门
        /// </summary>
        /// <param name="regDepDiv"></param>
        /// <param name="optionText"></param>
        /// <returns></returns>
        private static string selectDep(IWebElement regDepDiv, string optionText = null)
        {
            var lastSelect = regDepDiv.FindElements(By.CssSelector(".select.dep")).Last();
            lastSelect.FindElement(By.CssSelector(".current-option")).Click();
            Thread.Sleep(500);
            if (string.IsNullOrEmpty(optionText))
            {
                Random rand = new Random();
                var options = lastSelect.FindElements(By.CssSelector(".dep-option .option"))
                    .Where(e=>e.Text!="监管中心");
                var option = options.ElementAt(rand.Next(options.Count()));
                optionText = option.Text;
                option.Click();
            }
            else
            {
                lastSelect.FindElements(By.CssSelector(".dep-option .option"))
                    .Where(e => e.Text == optionText).First().Click();
            }
            Thread.Sleep(500);
            return optionText;
        }
        /// <summary>
        /// 将已注册的账号信息保存到json文件中
        /// </summary>
        /// <param name="users"></param>
        public void SaveUsers(List<User> users)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            JsonSerializer jsonSerializer = new JsonSerializer();
            JObject jObject = new JObject();
            jObject.Add(registeredUsersSection, JArray.FromObject(users));
            jsonSerializer.Serialize(writer, jObject);
            StreamWriter wtyeu = new StreamWriter(UserListFilename);
            wtyeu.Write(sw);
            wtyeu.Flush();
            wtyeu.Close();
        }
        /// <summary>
        /// 爬取并保存题库
        /// </summary>
        /// <param name="users"></param>
        public void Crawl(List<User> users)
        {
            string examIdToCrawl = null;
            foreach (var user in users)
            {
                string url = "http://js.nrta.gov.cn/account/login/1";
                _webDriver.Navigate().GoToUrl(url);
                Actions action = new Actions(_webDriver);
                Console.WriteLine("打开网页 "+ url);
                var loginForm = _webWait.Until(d => d.FindElement(By.Id("loginForm")));
                Console.WriteLine("登录用户: " + user.Username);
                loginForm.FindElement(By.Id("username")).SendKeys(user.Username);
                loginForm.FindElement(By.Id("userTypePwd")).SendKeys(user.Password);
                loginForm.FindElement(By.Id("loginBtn")).Click();
                _webWait.Until(d => d.Url == "http://js.nrta.gov.cn/exam/");
                var examItemWrapper = _webWait.Until(d => d.FindElement(By.ClassName("item-exam-wrapper")));
                var examItems = examItemWrapper.FindElements(By.ClassName("item-exam"));
                if (examItems.Count == 0)
                {
                    Console.WriteLine("当前用户没有考试，换下一个用户");
                    _webDriver.FindElement(By.ClassName("character-img")).Click();
                    _webWait.Until(d => d.FindElement(By.Id("logoutBtn")).Displayed);
                    _webDriver.FindElement(By.Id("logoutBtn")).Click();
                    _webWait.Until(d => d.FindElement(By.Id("confirmLogoutBtn")).Displayed);
                    _webDriver.FindElement(By.Id("confirmLogoutBtn")).Click();
                    Thread.Sleep(1000);
                    continue;
                }
                if (examItems.Count == 1)
                {
                    examIdToCrawl = examItems[0].FindElement(By.ClassName("btn-item-exam")).GetAttribute("data-id");
                }
                //选择爬取哪个考试
                if (string.IsNullOrEmpty(examIdToCrawl))
                {
                    Console.WriteLine("发现{0}个考试，请选择:",examItems.Count);
                    printExamItems(examItems);
                    int input = ConsoleExtensions.Input("", 0, examItems.Count - 1);
                    examIdToCrawl = examItems[input].FindElement(By.ClassName("btn-item-exam")).GetAttribute("data-id");
                }
                Console.WriteLine("进入考试");
                var examItemToCraw = examItems
                    .Where(e => e.FindElement(By.ClassName("btn-item-exam")).GetAttribute("data-id") == examIdToCrawl)
                    .First();
                var examBtnToCraw = examItems.Select(e => e.FindElement(By.ClassName("btn-item-exam")))
                    .Where(e => e.GetAttribute("data-id") == examIdToCrawl).First();
                action.MoveToElement(examItemToCraw).Perform();
                _webWait.Until(d => examBtnToCraw.Displayed);
                examBtnToCraw.Click();
                Thread.Sleep(1000);
                //交卷
                _webDriver.FindElement(By.Id("endExamBtn")).Click();
                Thread.Sleep(500);
                _webDriver.FindElement(By.Id("confirmEndExamBtn")).Click();
                //Thread.Sleep(5000);
                _webWait.Until(d => d.Url.Contains("/result/inquire?examResultsId"));
                Thread.Sleep(1000);
                var lookAnswerBtn = _webWait.Until(d => d.FindElement(By.CssSelector(".footer-tab-2 a")));
                lookAnswerBtn.Click();
                _webWait.Until(d => d.Url.Contains("/exam/exam_check?"));
                //Thread.Sleep(3000);
                _webWait.Until(d => d.FindElement(By.ClassName("menu-item-score")));
                //读取考试题
                var questions = _webWait.Until(d => d.FindElements(By.CssSelector(".question-content")));
                Console.WriteLine("读取到{0}个试题",questions.Count);
                int count = saveQuestions(questions);
                Console.WriteLine("保存{0}个试题", count);
                _webDriver.FindElement(By.Id("logoutBtn")).Click();
                _webWait.Until(d => d.FindElement(By.Id("confirmLogoutBtn")).Displayed);
                _webDriver.FindElement(By.Id("confirmLogoutBtn")).Click();
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 爬取并保存题库
        /// </summary>
        public void Crawl()
        {
            var users = GetConfigurationUsers();
            Crawl(users);
        }

        private void printExamItems(ReadOnlyCollection<IWebElement> examItems)
        {
            foreach (var item in examItems)
            {
                string title = item.FindElement(By.ClassName("item-title")).Text;
                Console.WriteLine("{0}. {1}", examItems.IndexOf(item), title);
            }
        }

        private int saveQuestions(ReadOnlyCollection<IWebElement> questions)
        {
            int count = 0;
            foreach (var question in questions)
            {
                string id = question.GetAttribute("id");
                string subject = question.FindElement(By.ClassName("exam-question")).Text;
                subject = Regex.Replace(subject, @"^\d+\.\s*", "");
                var answers = question.FindElements(By.CssSelector(".answers .select"));
                string rightAnswer = question.FindElement(By.ClassName("question-ans-right")).Text;
                ExamQuestion dbQuestion = new ExamQuestion
                {
                    Id = id,
                    Subject = subject,
                    Answer=rightAnswer
                };
                foreach (var answer in answers)
                {
                    string answerText = answer.Text;
                    string answerPrefix = Regex.Match(answerText, @"^[A-Z]").Value;
                    typeof(ExamQuestion).GetProperty(answerPrefix.ToUpper()).SetValue(dbQuestion,answerText);
                }
                if (_dbContext.Questions.Find(id) != null)
                    continue;
                _dbContext.Questions.Add(dbQuestion);
                count++;
            }
            _dbContext.SaveChanges();
            return count;
        }
        /// <summary>
        /// 检查是否在答题页面
        /// </summary>
        /// <returns></returns>
        public bool CheckExamPage()
        {
            if (!_webDriver.Url.Contains("js.nrta.gov.cn/exam/exam_start"))
                return false;
            try
            {
                _webDriver.FindElement(By.ClassName("questions-content"));
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            return true;
        }

        public void FillWebAnswer()
        {
            var questions = _webDriver.FindElements(By.ClassName("question-content"));
            foreach (var question in questions)
            {
                string id = question.GetAttribute("id");
                ExamQuestion dbQuestion = _dbContext.Questions.Find(id);
                if (dbQuestion == null)
                    continue;
                string letters = "ABCDEFGH";
                string answer = dbQuestion.Answer;
                if (answer.StartsWith("\""))
                {
                    string[] subAnswer = answer.Split(',').Select(s => s.Trim('"')).ToArray();
                    var answerInputs = question.FindElements(By.CssSelector(".answers textarea"));
                    foreach (var input in answerInputs)
                    {
                        int index = answerInputs.IndexOf(input);
                        input.SendKeys(subAnswer[index]);
                    }
                }
                else
                {
                    foreach (char letter in answer)
                    {
                        int num = letters.IndexOf(letter) + 1;
                        string optionId = "#" + id + num.ToString();
                        //_webDriver.FindElement(By.Id(id + num.ToString())).Click();
                        string js = string.Format("$('{0}').parent().css('background-color','#99CC66');", optionId);
                        _webDriver.ExecuteJavaScript(js);
                    }
                }
            }
        }
    }
}
