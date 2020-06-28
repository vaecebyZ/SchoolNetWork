using System.Net.Http;
using System;
using RestSharp;
using RestSharp.Authenticators;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;//暂停
using System.IO;//文件读取

namespace dotnet
{
    class Program
    {
        static bool isFiretLoad = true;
        static string ListenHost = "www.bing.com.cn";
        static StreamReader sr;
        static void Main(string[] args)
        {
            //http://172.16.0.2/drcom/login?callback=dr1003&DDDDD=20183170535%40cmcc&upass=05251196&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&v=7051
            Listen("www.bing.com.cn");
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
                if (isFiretLoad)//首次加载
                {
                    string filename = Path.GetFullPath(Environment.CurrentDirectory + "/Resources/UidPwd.data");
                    sr = File.OpenText(filename);//读取文件
                    isFiretLoad = false;
                }
                string nextline;
                while ((nextline = sr.ReadLine()) != null)//读取一排数据
                {
                    Console.WriteLine(nextline);
                    string[] str = nextline.Split(' ');//空格分开密码账号
                    Login(str[0],str[1]);
                    //Console.WriteLine(str[0]);uid
                    //Console.WriteLine(str[1]);pwd
                }
            }
            else
            {
                Console.WriteLine("失败");
            }
        }

        public static void Login(string uid,string pwd)
        {
            var client = new RestClient("http://172.16.0.2/drcom");//使用RestSharp实现
            var request = new RestRequest(string.Format("login?callback=dr1003&DDDDD={0}%40cmcc&upass={1}&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&v=7051",uid,pwd), DataFormat.Json);
            var response = client.Get(request);
            Console.WriteLine(response.Content);
            Listen(ListenHost);
        }

    }
}
