using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Palmmedia.ReportGenerator.Core.Common;
using UnityEngine;

namespace KC
{
    public class Webhook : Singleton<Webhook>, ISingletonAwake
    {

        private List<string> _exceptionFunctions;
        private uint _showLineCount;
       
        public void Awake()
        {
            Application.logMessageReceived += OnLog;
            _exceptionFunctions = new List<string>();
            _showLineCount= 10;
        }
        
        private void OnLog(string logString,string stackTrace,LogType logType)
        {
            if (logType is LogType.Error or LogType.Exception)
            {
                
                var mainInfo = GetExceptionMessageInfo(stackTrace);
                var exceptionStr = $"Function: {mainInfo.Item1}\r\n ErrorInfo: {logString}\r\n StackTrace: {stackTrace}";
                if (_exceptionFunctions.Contains(mainInfo.Item2))
                {
                    return;
                }
                        
                _exceptionFunctions.Add(mainInfo.Item2);
                Debug.Log(exceptionStr);
            }
        }
        
        /// <summary>
        /// 发送报错信息
        /// </summary>
        private async void SendExceptionInfo(string webhookUrl,string message)
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                msg_type = "text",
                content = new
                {
                    text = message
                }
            };

            var payloadJson = JsonSerializer.ToJsonString(payload);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Message sent successfully.");
            }
            else
            {
                Debug.Log($"Failed to send message. Status code: {response.StatusCode}");
            }
        }
        
        /// <summary>
        /// 打印Exception拼接信息
        /// </summary>
        /// <param name="stacktrace">堆栈</param>
        /// <returns></returns>
        private (string,string) GetExceptionMessageInfo(string stacktrace)
        {
            int count = 0;
            var stringBuilder = new StringBuilder();
            try
            {
                var allLineStr = stacktrace.Split("\n");
                var str = allLineStr[0];
                var infos = str.Split(".cs");

                if (infos.Length<2)
                {
                    while (count<_showLineCount)
                    {
                        count++;
                        infos = allLineStr[count].Split(".cs");
                        if (infos.Length>=2)
                        {
                            break;
                        }
                    }
                }
                var type =infos[0];
                var types = type.Split("/");
                var function = $"{types[^2]} / {types[^1]}";
                stringBuilder.Append(function);

                var lineStr = infos[^1];
                var line = lineStr.Split(":");
                stringBuilder.Append("  line：").Append((line[^1]));
                return (stringBuilder.ToString(),function);
            }
            catch (Exception)
            {
                return (stringBuilder.ToString(),"");
            }
        }
    }
}