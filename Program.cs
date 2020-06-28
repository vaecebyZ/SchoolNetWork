using System.Net.Http;
using System;
using RestSharp;
using RestSharp.Authenticators;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;//暂停

namespace dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            //var client = new RestClient("https://api.twitter.com/1.1");
            //Console.WriteLine("Hello World!");
            //http://172.16.0.2/drcom/login?callback=dr1003&DDDDD=20183170535%40cmcc&upass=05251196&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&v=7051
            //var url = "https://www.baidu.com";
            Login();
        }

        private static void Listen(string ListenHost)//监听
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
                Login();
            }
            else
            {
                Console.WriteLine("失败");
            }
        }

        public static void Login()
        {
            //using (var client = new HttpClient())//使用System.Net.Http;实现
            //{
            //    var responseString = client.GetStringAsync(url);
            //    Console.WriteLine(responseString);
            //}     
            var client = new RestClient("http://172.16.0.2/drcom");//使用RestSharp实现
            //var client = new RestClient("http://baidu.com");
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");
            var request = new RestRequest("login?callback=dr1003&DDDDD=20183170506%40cmcc&upass=3627918916&0MKKey=123456&R1=0&R3=0&R6=0&para=00&v6ip=&v=7051", DataFormat.Json);
            //var request = new RestRequest("index");
            var response = client.Get(request);
            Console.WriteLine(response.Content);
            var ListenHost = "www.bing.com.cn";
            Listen(ListenHost);

        }

    }
}
