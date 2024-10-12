using System;
using System.IO;
using System.Text;
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
        private FileTarget _defaultFileTarget;
        private readonly LoggingConfiguration _loggingConfiguration = new LoggingConfiguration();

        public string ConfigPath { get; set; }

        public LogLevel LogEnableDetail { get; set; }

        public void Awake()
        {
            ConfigPath = Path.Combine(Application.persistentDataPath, "log");
            LogEnableDetail = LogLevel.Dev;
            RegisterLogger("All");
#if UNITY_EDITOR
            UnityLogConfig();
#endif
        }

        public void Init(FileTarget fileTarget)
        {
            _defaultFileTarget = fileTarget;
        }

        public void RegisterLogger(string name)
        {
            var levelRule = _loggingConfiguration.FindRuleByName(name);
            if (levelRule == null)
            {
                var levelTarget = GetLevelTarget(name, ConfigPath);
                var allTarget = GetAllTarget(name, ConfigPath);
                var allRule = new LoggingRule("*", NLog.LogLevel.Off, allTarget);
                levelRule = new LoggingRule(name, NLog.LogLevel.Off, levelTarget);
                SetLogLevel(LogEnableDetail, levelRule);
                SetLogLevel(LogEnableDetail, allRule);
                _loggingConfiguration.AddTarget(levelTarget);
                _loggingConfiguration.AddTarget(allTarget);
                _loggingConfiguration.AddRule(levelRule);
                _loggingConfiguration.AddRule(allRule);
                LogManager.Configuration = _loggingConfiguration;
            }
        }

        public void AddUnityLog()
        {
            
        }

        public FileTarget GetDefaultTarget()
        {
            return new FileTarget()
            {
                Header = "-------",
                Footer = "--------",
                Encoding = Encoding.UTF8,
                LineEnding = LineEndingMode.CRLF, //设置行结束符模式（CRLF
                KeepFileOpen = false, //保持文件打开以提高性能
                ConcurrentWrites = false, //禁用并发写入
                OpenFileCacheTimeout = 30, //打开文件缓存超时时间（秒）
                OpenFileCacheSize = 6, //打开文件缓存大小
                OpenFileFlushTimeout = 10, //打开文件刷新超时时间（秒）
                AutoFlush = true, //自动刷新缓冲区
                CleanupFileName = true, //清理文件名
                ArchiveAboveSize = 10240000, //当文件大小超过此值时进行归档（字节)
                MaxArchiveFiles = 10, //最大归档文件数
                MaxArchiveDays = 10, //最大归档天数
                ArchiveFileName = "archive.${level:uppercase=true}.{#}.txt", //归档文件名模式
                ArchiveNumbering = ArchiveNumberingMode.Date, // 归档编号模式（按日期）
                ArchiveEvery = FileArchivePeriod.Month, //归档周期（每月
                ArchiveOldFileOnStartup = false, //启动时归档旧文件
                ArchiveOldFileOnStartupAboveSize = 1000000, //启动时归档超过指定大小的旧文件（字节)
                ReplaceFileContentsOnEachWrite = false, //每次写入时替换文件内容
                EnableFileDelete = true, //启用文件删除
                ConcurrentWriteAttempts = 20000, //并发写入尝试次数
            };
        }

        public FileTarget GetLevelTarget(string ruleName, string path)
        {
            var target = _loggingConfiguration.FindTargetByName<FileTarget>(ruleName);
            if (target != null)
            {
                return target;
            }

            target = _defaultFileTarget;
            target.Name = ruleName;
            target.FileName = Path.Combine(path, ruleName, "${level:uppercase=true}-${shortdate}.log");
            target.Layout =
                new SimpleLayout(
                    "${longdate} [${callsite}(${callsite-filename:includeSourcePath=False}:${callsite-linenumber})] - ${message} ${exception:format=ToString}");
            return target;
        }

        public FileTarget GetAllTarget(string loggerName, string path)
        {
            var targetName = $"{loggerName} - all";
            var target = _loggingConfiguration.FindTargetByName<FileTarget>(targetName);
            if (target != null)
            {
                return target;
            }

            target = _defaultFileTarget;
            target.Name = targetName;
            target.FileName = Path.Combine(path, loggerName, "all-${shortdate}.log");
            target.Layout =
                new SimpleLayout(
                    "${longdate} [${uppercase:${level}}] [${callsite}(${callsite-filename:includeSourcePath=False}:${callsite-linenumber})] - ${message} ${exception:format=ToString}");
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

#if UNITY_EDITOR
        public LogLevel UnityLogEnableDetail { get; set; } = LogLevel.Trace;

        private void UnityLogConfig()
        {
            var target = new UnityConsoleTarget
            {
                Name = "UnityConsole",
                Layout = "${longdate} ${level} ${message}"
            };
            _loggingConfiguration.AddTarget(target);
            var rule = new LoggingRule("*", NLog.LogLevel.Off, target);
            SetLogLevel(UnityLogEnableDetail, rule);
            _loggingConfiguration.LoggingRules.Add(rule);
            LogManager.Configuration = _loggingConfiguration;
        }
#endif
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