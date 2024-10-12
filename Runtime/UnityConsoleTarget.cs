using System;
using NLog;
using NLog.Targets;
using UnityEngine;

namespace KC
{
        [Target("UnityConsole")]
        public sealed class UnityConsoleTarget : TargetWithLayout
        {
            protected override void Write(LogEventInfo logEvent)
            {
                //string logMessage = this.Layout.Render(logEvent);
                switch (logEvent.Level.Ordinal)
                {
                    case 0:
                    case 1:
                    case 2:
                        Debug.Log(logEvent.Message);
                        break;
                    case 3:
                        Debug.LogWarning(logEvent.Message);
                        break;
                    case 4:
                        Debug.LogError(logEvent.Message);
                        break;
                    case 5:
                        Debug.LogException(new Exception(logEvent.Message));
                        break;
                }
            }
        }
    
}