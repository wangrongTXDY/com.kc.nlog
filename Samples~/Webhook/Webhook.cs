using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;
using UnityEngine;
using UnityEngine.Profiling;

namespace KC
{
    public class Webhook : Singleton<Webhook>, ISingletonAwake
    {
         private List<string> _webhookUrls;
         private List<string> _exceptions;
         
         public void Awake()
         {
             _exceptions = new List<string>();
             _webhookUrls = new List<string>();
            Application.logMessageReceived += ApplicationOnLogMessageReceived;
         }

         private void ApplicationOnLogMessageReceived(string condition, string stacktrace, LogType type)
         {
             var stringBuilder = new StringBuilder();
             stringBuilder.AppendLine(condition);
             stringBuilder.AppendLine(stacktrace);
             var message = stringBuilder.ToString();
            
             if (_exceptions.Contains(message))
             {
                 return;
             }
             _exceptions.Add(message);

             foreach (var webhookUrl in _webhookUrls)
             {
                 SendAsync(webhookUrl, message);
             }
         }

         public void RegisterUrl(string url)
         {
             if (_webhookUrls.Contains(url))
             {
                 return;
             }
             _webhookUrls.Add(url);
         }
         

        private async void SendAsync(string webhookUrl,string message)
        {
            using var httpClient = new HttpClient();
        
            await httpClient.PostAsync(webhookUrl, new StringContent(WebhookHelper.GetFeiShuContent(message), Encoding.UTF8, "application/json"));
        }

    }
}