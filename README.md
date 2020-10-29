# 答题助手
答题活动助手，功能包括：  
- 批量注册账号
- 爬取题库
- 答题时标记答案
## 环境及配置
开发环境: .Net Core 3.1  
开发工具: Visual Studio 2019
## 项目结构
NRTA.Core 核心库  
NRTA.Spider 用于注册和爬取题库  
NRTA.App 用于答题  
## 使用方法
### 初始化环境及配置
1. 安装Entity Framework Core 工具  
   `dotnet tool install --global dotnet-ef`
2. 初始化数据库  
   `dotnet ef database update -p NRTA.Core -s NRTA.Spider`
3. 修改`NRTA.Spider`项目下的`SpiderConfig.json`文件:  
   DefaultPassword: 注册账户使用的密码
   Username: 注册账户的用户名前缀
   RealNames: 注册账户的用户姓名，可按照自己喜好添加
4. 生成各个项目
5. 将`NRTA.Spider`下的`exam.db`和`SpiderConfig.json`文件复制到`NRTA.Spider`的生成目录下。
### 注册账号
1. 运行`NRTA.Spider`。
2. 选择"注册账号"。
3. 等待程序运行，程序会将注册的账号信息存储为`SpiderUsers.json`。
### 爬取题库
1. 运行`NRTA.Spider`。
2. 选择"爬取"。
3. 等待程序运行，程序会自动登录、答题、记录答案，并将题目存储到`exam.db`中。
4. 若中间报错，或题目爬取不全，可再来一次。
5. 每次有新的答题都要爬一边，以更新题库
### 答题
1. 将`NRTA.Spider`运行目录下的`exam.db`复制到`NRTA.App`生成目录下。
2. 运行`NRTA.App`，在打开的浏览器中输入自己的账号密码登录系统，然后手动进入答题页面。
3. 等待程序识别题目。
4. 选择题正确选项会标绿，需手动点击答题；填空题会自动填好。
5. 确定无误后提交试卷，100分Get！