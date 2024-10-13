using System;
using System.IO;
using NLog;
using NLog.Config;
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

                var aTarget = _loggingConfiguration.FindTargetByName<FileTarget>("All");
                var aaTarget = _loggingConfiguration.FindTargetByName<FileTarget>("All - All");
                if (aTarget != null)
                {
                    rule.Targets.Add(aTarget);
                }
                if (aaTarget != null)
                {
                    rule.Targets.Add(aaTarget);
                }
                
                SetLogLevel(LogLevel,rule);
                _loggingConfiguration.AddRule(rule);
            }
            
            LogManager.Configuration = _loggingConfiguration;
        }

        public void AddUnityLog(UnityConsoleTarget unityConsoleTarget = null)
        {
            var target = unityConsoleTarget ?? LogTargetHelper.GetDefaultUnityConsoleTarget();
            _loggingConfiguration.AddTarget(target);
            _loggingConfiguration.AddRule(new LoggingRule("*",NLog.LogLevel.Trace,target));
            LogManager.Configuration = _loggingConfiguration;
        }

        public void AddAllLog()
        {
            SetLevelTarget("All",LogTargetHelper.GetDefaultTarget());
            SetAllTarget("All");
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