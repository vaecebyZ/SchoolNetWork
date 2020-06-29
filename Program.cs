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
        static readonly string BanListFilePath = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/BanList.data");
        static readonly string filename = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/UidPwd.data");
        static void Main(string[] args)
        {
            Console.WriteLine("==========WELCOME==============");
            //http://172.16.0.2/drcom/login?callback=dr1003&DDDDD=20183170535%40cmcc&upass=05251196&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&v=7051
            Listen(ListenHost);
        }

        private static void Listen(string ListenHost)//心跳
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
            else if (pingReply.Status == IPStatus.TimedOut)
            {

                Console.WriteLine("超时");
                Thread.Sleep(3000);
                bool isSeame = false;
                if (isFiretLoad)//首次加载
                {               
                    sr = File.OpenText(filename);//读取文件                   
                    isFiretLoad = false;
                }
                sfw = File.OpenText(BanListFilePath);
                string nextline;
                while ((nextline = sr.ReadLine()) != null)//读取一排数据
                {
                    string banlist;

                    while((banlist = sfw.ReadLine()) != null)
                    {
                        if(nextline == banlist)
                        {
                            Console.WriteLine("该账号存在与黑名单中:"+nextline);
                            isSeame = true;
                            break;
                        }
   
                    }
                    if (isSeame)
                    {
                        isSeame = false;
                        continue;
                    }
                    sfw.Close();
                    Console.WriteLine("=====================");
                    Console.WriteLine("正在尝试：");
                    Console.WriteLine(nextline);
                    Console.WriteLine();
                    string[] str = nextline.Split(' ');//空格分开密码账号
                    Login(str[0], str[1]);
                    //Console.WriteLine(str[0]);uid
                    //Console.WriteLine(str[1]);pwd
                }
            }
            else
            {
               Console.WriteLine("失败");
            }
        }

        public static void Login(string uid, string pwd)
        {
            var client = new RestClient("http://172.16.0.2/drcom");//使用RestSharp实现
            var request = new RestRequest(string.Format("login?callback=dr1003&DDDDD={0}%40cmcc&upass={1}&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&v=7051", uid, pwd), DataFormat.Json);
            var response = client.Get(request);
            var jcode = Regex.Replace(response.Content, @"(.*\()(.*)(\).*)", "$2"); //去掉小括号()
            Console.WriteLine(jcode);
            var result = JsonConvert.DeserializeObject<JObject>(jcode.ToString());
            if (result["result"].ToString() == "1")
            {
                Console.WriteLine("登录成功");
            }else if(result["result"].ToString() == "0")
            {
                Console.WriteLine("登录失败正在寻找原因");
                if(result["msga"].ToString() == "inuse, login again")
                {
                    Console.WriteLine("===========================");
                    Console.WriteLine("正确的账号信息,该账号将不会进入黑名单");
                    Console.WriteLine(result["uid"].ToString());
                }
                else
                {
                    Console.WriteLine("用户名或者密码问题,该条记录会被加入黑名单");
                    //using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
                    //{
                    //    string content = uid+" "+pwd+"\n";//向文本文件Demo.txt中写入的内容为"123456789"
                    //    byte[] data = Encoding.ASCII.GetBytes(content);//使用ASCII码将字符串转换为字节数据，所以一个字符占用一个字节
                    //    fileStream.Write(data, 0, data.Length);
                    //}
                    using (StreamWriter sw = new StreamWriter(BanListFilePath, true))
                    {
                        string content = uid + " " + pwd;
                        sw.WriteLine(content);
                    }
                }
            }
            else
            {
                Console.WriteLine("致命的未知错误");
                Console.ReadLine();
            }
            Listen(ListenHost);
        }
    }
}
