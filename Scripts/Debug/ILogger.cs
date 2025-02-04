using System;

namespace ShirokuStudio.Core
{
    public interface ILogger
    {
        void Debug(string message);

        void Debug(string message, UnityEngine.Object context);

        void Log(string message);

        void Log(string message, UnityEngine.Object context);

        void Warning(string message);

        void Warning(string message, UnityEngine.Object context);

        void Error(string message);

        void Error(string message, UnityEngine.Object context);

        void Error(Exception ex);

        void Error(Exception ex, UnityEngine.Object context);

        void Error(Exception ex, string message);

        void Error(Exception ex, string message, UnityEngine.Object context);

        void Assert(bool condition, string message);
    }

    public interface ILogger<T> : ILogger
    { }
}