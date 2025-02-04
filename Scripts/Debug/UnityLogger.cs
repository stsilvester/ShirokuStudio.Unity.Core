using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace ShirokuStudio.Core
{
    public class UnityLogger<T> : ILogger<T>
    {
        public const int MaxTrace = 40;
        public readonly string Prefix = $"[{typeof(T).Name}]";

        private static string rootPath = Path.Combine(Application.dataPath);
        private static Regex pattern_trace = new Regex(@"\s*at (?<method>\S+\s\(.*\))\s?\[.+\]\s?in((.+(?<=/Assets/)(?<path>.+))|(.+)):(?<line>\d+)?", RegexOptions.Compiled);

        public void Debug(string message)
        {
#if DEBUG
            log(LogType.Log, message);
#endif
        }

        public void Debug(string message, UnityEngine.Object context)
        {
#if DEBUG
            log(LogType.Log, message, context: context);
#endif
        }

        public void Log(string message)
        {
            log(LogType.Log, message);
        }

        public void Log(string message, UnityEngine.Object context)
        {
            log(LogType.Log, message, context: context);
        }

        public void Warning(string message)
        {
            log(LogType.Warning, message);
        }

        public void Warning(string message, UnityEngine.Object context)
        {
            log(LogType.Warning, message, context: context);
        }

        public void Error(string message)
        {
            log(LogType.Error, message);
        }

        public void Error(string message, UnityEngine.Object context)
        {
            log(LogType.Error, message, context: context);
        }

        public void Error(Exception ex)
        {
            log(LogType.Error, ex);
        }

        public void Error(Exception ex, UnityEngine.Object context)
        {
            log(LogType.Error, ex, context: context);
        }

        public void Error(Exception ex, string message)
        {
            log(LogType.Error, ex, message);
        }

        public void Error(Exception ex, string message, UnityEngine.Object context)
        {
            log(LogType.Error, ex, message, 5, context);
        }

        private void log(LogType type, Exception ex, string message = null, int level = 4, UnityEngine.Object context = default)
        {
            log(type, $"{message ?? "ERROR!!!"} {ex.Message}", ex.StackTrace, level, context);
        }

        private void log(LogType type, string message, string stackTrace = null, int level = 3, UnityEngine.Object context = default)
        {
            var name = context ? $"@{context.name}" : "";
            message = message.Replace("{", "{{").Replace("}", "}}").Trim();
#if UNITY_EDITOR
            stackTrace ??= Environment.StackTrace;
            var lines = stackTrace.Split("\n");
            var src = pattern_trace.Match(lines.FirstOrDefault());
            if (lines.Length > level)
            {
                stackTrace = string.Join("\n", lines.Skip(level).Take(MaxTrace).Select(convertToStacktrace));
                if (lines.Length > MaxTrace)
                {
                    stackTrace += "\n(message truncated...)";
                }
                UnityDebug.LogFormat(type, LogOption.NoStacktrace, context, $"<b>{Prefix}</b>{name} {message}\n{stackTrace}");
            }
            else
            {
                UnityDebug.LogFormat(context, $"<b>{Prefix}</b>{name} {message}");
            }
#else
            try
            {
                UnityDebug.LogFormat(type, LogOption.None, context: context, format: $"{Prefix}{name} {message}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                UnityEngine.Debug.LogError($"{Prefix}{name} {message}");
            }
#endif
        }

        private string convertToStacktrace(string trace)
        {
            trace = trace.Replace("\\", "/");
            var match = pattern_trace.Match(trace);
            if (!match.Success)
                return trace;

            var method = match.Groups["method"].Value.Replace(rootPath, "");
            var path = "Assets/" + match.Groups["path"];
            var line = match.Groups["line"];
            var log = $"-{method} (at <a href=\"{path}\" line=\"{line}\">{path}:{line}</a>)";
            if (path.Contains("Assets/Scripts") == false)
            {
                log = $"<color=grey>{log}</color>";
            }
            return log;
        }

        public void Assert(bool condition, string message)
        {
            if (condition)
                return;

            log(LogType.Assert, message);
        }
    }
}