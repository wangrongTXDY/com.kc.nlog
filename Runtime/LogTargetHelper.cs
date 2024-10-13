using System.Text;
using NLog.Targets;

namespace KC
{
    public static partial class LogTargetHelper
    {
        public static FileTarget GetDefaultTarget()
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
                ArchiveFileName = "archive.${level:uppercase=true}.{#}.log", //归档文件名模式
                ArchiveNumbering = ArchiveNumberingMode.Date, // 归档编号模式（按日期）
                ArchiveEvery = FileArchivePeriod.Month, //归档周期（每月
                ArchiveOldFileOnStartup = false, //启动时归档旧文件
                ArchiveOldFileOnStartupAboveSize = 1000000, //启动时归档超过指定大小的旧文件（字节)
                ReplaceFileContentsOnEachWrite = false, //每次写入时替换文件内容
                EnableFileDelete = true, //启用文件删除
                ConcurrentWriteAttempts = 20000, //并发写入尝试次数
            };
        }

        public static UnityConsoleTarget GetDefaultUnityConsoleTarget()
        {
            return new UnityConsoleTarget
            {
                Name = "UnityConsole",
                Layout = "${longdate} ${level} ${message}"
            };
        }
    }
}