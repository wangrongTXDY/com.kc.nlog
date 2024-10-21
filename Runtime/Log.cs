using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Filters;
using NLog.Layouts;
using NLog.Targets;
using UnityEngine;

namespace KC
{
    /// <summary>
    /// Log组件
    /// </summary>
    public class Log : Singleton<Log>, ISingletonAwake
    {
        private LogType _logType;
        private LoggingConfiguration _loggingConfiguration;

        
        public string ConfigPath { get; set; }

        public LogLevel LogLevel { get; set; }

        public void Awake()
        {
            _loggingConfiguration = new LoggingConfiguration();
            LogManager.Configuration = _loggingConfiguration;
            ConfigPath = Path.Combine(Application.persistentDataPath, "log");
            LogLevel = LogLevel.Editor;
            Application.logMessageReceived += ApplicationOnLogMessageReceived;
            
            RegisterLogger("UnityApplication");
        }
        
        public void RegisterLogger(string name,FileTarget fileTarget = null)
        {
            var target = _loggingConfiguration.FindTargetByName<FileTarget>(name);
            if (target != null)
            {
                return;
            }

            var fTarget = fileTarget ?? LogTargetHelper.GetDefaultTarget();
            SetLevelTarget(name, fTarget);
            var allTarget = SetAllTarget(name);

            var rule = _loggingConfiguration.FindRuleByName(name);
            if (rule == null)
            {
                rule = new LoggingRule(name,NLog.LogLevel.Off, fTarget)
                {
                    RuleName = name
                };
                rule.Targets.Add(allTarget);
                SetLogLevel(LogLevel,rule);
                _loggingConfiguration.AddRule(rule);
            }
            
            LogManager.Configuration = _loggingConfiguration;
        }

        public void AddUnityLog(UnityConsoleTarget unityConsoleTarget = null)
        {
            var target = unityConsoleTarget ?? LogTargetHelper.GetDefaultUnityConsoleTarget();
            _loggingConfiguration.AddTarget(target);
            NLog.LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().FilterDynamicIgnore(evt => evt.FormattedMessage?.Length > 100).WriteToFile("log.txt");
            });
            var rule = new LoggingRule("*", NLog.LogLevel.Trace, target)
            {
                RuleName = "UnityConsole",
                Filters =
                {
                    new ConditionBasedFilter()
                    {
                        Condition ="'${logger}'!='UnityApplication'",
                        Action = FilterResult.Log
                    }
                }
            };
            _loggingConfiguration.AddRule(rule);
            LogManager.Configuration = _loggingConfiguration;
        }

        public void AddAllLog()
        {
            if (_loggingConfiguration.FindRuleByName("All") != null)
            {
                return;
            }
            var fTarget = LogTargetHelper.GetDefaultTarget();
            SetLevelTarget("All",fTarget);
            fTarget.Layout =
                new SimpleLayout(
                    "${longdate} [${logger}] [${callsite}(${callsite-filename:includeSourcePath=False}:${callsite-linenumber})] - ${message} ${exception:format=ToString}");
            var allTarget = SetAllTarget("All");
            allTarget.Layout =
                new SimpleLayout(
                    "${longdate} [${logger}] [${lowercase:${level}}] [${callsite}(${callsite-filename:includeSourcePath=False}:${callsite-linenumber})] - ${message} ${exception:format=ToString}");
            var rule = new LoggingRule("*", NLog.LogLevel.Trace, fTarget)
            {
                RuleName = "All"
            };
            rule.Targets.Add(allTarget);
            _loggingConfiguration.AddRule(rule);
            LogManager.Configuration = _loggingConfiguration;
        }
        
        public void Remove(string name)
        {
            _loggingConfiguration.RemoveRuleByName(name);
            _loggingConfiguration.RemoveTarget(name);
            _loggingConfiguration.RemoveTarget($"{name} - All");
            LogManager.Configuration = _loggingConfiguration;
        }

        /// <summary>
        /// 移除All目录Log,要移除所有Log请使用Clear()
        /// </summary>
        public void RemoveAllLog()
        {
            if (_loggingConfiguration.FindRuleByName("All") == null)
            {
                return;
            }
            _loggingConfiguration.RemoveRuleByName("All");
            _loggingConfiguration.RemoveTarget("All");
            _loggingConfiguration.RemoveTarget("All - All");
            LogManager.Configuration = _loggingConfiguration;
        }

        public void Clear()
        {
            _loggingConfiguration = new LoggingConfiguration();
            LogManager.Configuration = _loggingConfiguration;
        }

        public void Reset()
        {
            Awake();
        }

        public FileTarget GetLevelTarget(string ruleName)
        {
            return _loggingConfiguration.FindTargetByName<FileTarget>(ruleName);
        }
        
        public FileTarget GetAllTarget(string loggerName)
        {
            return _loggingConfiguration.FindTargetByName<FileTarget>($"{loggerName} - All");
        }

        private void SetLevelTarget(string targetName,FileTarget fileTarget)
        {
            fileTarget.Name = targetName;
            fileTarget.FileName = Path.Combine(ConfigPath, targetName, "${level:lowercase=true}-${shortdate}.log");
            fileTarget.Layout =
                new SimpleLayout(
                    "${longdate} [${callsite}(${callsite-filename:includeSourcePath=False}:${callsite-linenumber})] - ${message} ${exception:format=ToString}");
            _loggingConfiguration.AddTarget(fileTarget);
        }

        private FileTarget SetAllTarget(string loggerName)
        {
            var targetName = $"{loggerName} - All";
            var target = _loggingConfiguration.FindTargetByName<FileTarget>(targetName);
            if (target != null)
            {
                return target;
            }

            target = LogTargetHelper.GetDefaultTarget();
            target.Name = targetName;
            target.FileName = Path.Combine(ConfigPath, loggerName, "All-${shortdate}.log");
            target.Layout =
                new SimpleLayout(
                    "${longdate} [${lowercase:${level}}] [${callsite}(${callsite-filename:includeSourcePath=False}:${callsite-linenumber})] - ${message} ${exception:format=ToString}");
            _loggingConfiguration.AddTarget(target);
            return target;
        }

        private void SetLogLevel(LogLevel detail, LoggingRule rule)
        {
            if (detail.HasFlag(LogLevel.Off))
            {
                return;
            }

            if (detail.HasFlag(LogLevel.Trace))
            {
                rule.EnableLoggingForLevel(NLog.LogLevel.Trace);
            }

            if (detail.HasFlag(LogLevel.Debug))
            {
                rule.EnableLoggingForLevel(NLog.LogLevel.Debug);
            }

            if (detail.HasFlag(LogLevel.Info))
            {
                rule.EnableLoggingForLevel(NLog.LogLevel.Info);
            }

            if (detail.HasFlag(LogLevel.Warn))
            {
                rule.EnableLoggingForLevel(NLog.LogLevel.Warn);
            }

            if (detail.HasFlag(LogLevel.Error))
            {
                rule.EnableLoggingForLevel(NLog.LogLevel.Error);
            }

            if (detail.HasFlag(LogLevel.Fatal))
            {
                rule.EnableLoggingForLevel(NLog.LogLevel.Fatal);
            }
        }
        
        private void ApplicationOnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            if (type is not (LogType.Error or LogType.Exception or LogType.Warning))
            {
                return;
            }
            
            if (condition.StartsWith("Exception: "))
            {
                //这里只会处理被动异常,所以主动消息不处理
                return;
            }
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(condition);
            stringBuilder.AppendLine(stacktrace);
            var message = stringBuilder.ToString();
            
            switch (type)
            {
                case LogType.Error:
                    LogManager.GetLogger("UnityApplication").Error(message);
                    break;
                case LogType.Warning:
                    LogManager.GetLogger("UnityApplication").Warn(message);
                    break;
                case LogType.Exception:
                    LogManager.GetLogger("UnityApplication").Fatal(message);
                    break;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            Application.logMessageReceived -= ApplicationOnLogMessageReceived;
            Clear();
        }
    }

    /// <summary>
    /// Log开启类型
    /// </summary>
    [Flags]
    public enum LogLevel : byte
    {
        Trace = 1 << 0,
        Debug = 1 << 1,
        Info = 1 << 2,
        Warn = 1 << 3,
        Error = 1 << 4,
        Fatal = 1 << 5,
        Off = 1 << 6,

        Editor = Trace | Debug | Info | Warn | Error | Fatal,

        Dev = Info | Warn | Error,

        Release = Error
    }
}