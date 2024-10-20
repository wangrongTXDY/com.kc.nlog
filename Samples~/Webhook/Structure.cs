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
}