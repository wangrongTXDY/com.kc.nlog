using System.Net.Http;
using System.Text;
using UnityEngine;

namespace KC
{
    public static partial class WebhookHelper
    {
        public static string GetFeiShuContent(string message)
        {
            return JsonUtility.ToJson(new FeiShu()
            {
                msg_type = "text",
                content = new FeiShuContent()
                {
                    text = message
                }
            });
            //return $@"{{""msg_type"": ""text"", ""content"": {{ ""text"": ""{message}"" }}}}";
        }
    }
}