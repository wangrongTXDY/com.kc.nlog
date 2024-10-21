using System;

namespace KC
{
    [Serializable]
    public struct FeiShu
    {
        public string msg_type;

        public FeiShuContent content;
    }

    [Serializable]
    public struct FeiShuContent
    {
        public string text;
    }
    
    //-------------------------
    
    [Serializable]
    public struct DingDing
    {
        public string msgtype;

        public DingDingContent text;
    }

    [Serializable]
    public struct DingDingContent
    {
        public string content;
    }
    
    //-------------------------------
    
    [Serializable]
    public struct WeChat
    {
        public string msgtype;

        public WeChatContent text;
    }

    [Serializable]
    public struct WeChatContent
    {
        public string content;
    }
}