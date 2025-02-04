using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Zenject;

namespace ShirokuStudio.Core
{
    //TODO: design nested scope so that can be used to collect logs inside nested scope,
    //resolve using DI with id to get parented-nested scope

    public sealed class DebugScope : IDisposable
    {
        public struct Config
        {
            public bool EnableStopWatch;
            public bool ResetStopWatchEachPrint;
            public bool PrependTimestamp;
            public Action<string> OutputHandler;
        }

        public static readonly Config DefaultConfig = new Config
        {
#if DEBUG
            EnableStopWatch = true,
            ResetStopWatchEachPrint = true,
            PrependTimestamp = true
#endif
        };

        private Stopwatch stopwatch;
        private Stopwatch stopwatch_all;

        private bool isDisposed = false;

        private List<Func<string, string>> compositeLogMessage;
        private Func<string, string> prepend;
        private Action<string> outputHandler;
        private event Action afterPrint;

        private StringBuilder sb = new();

        public DebugScope(Func<string, string> prepend, Config? config = null)
            : this(config)
        {
            this.prepend = prepend;
        }

        public DebugScope(string prefix, Config? config = null)
            : this(config)
        {
            prepend = (msg) => $"[{prefix}] {msg}";
        }

        public DebugScope(Config? config = null)
        {
            prepend ??= (msg) => msg;
#if DEBUG
            compositeLogMessage = new();
            var cfg = config ?? DefaultConfig;
            if (cfg.EnableStopWatch)
            {
                stopwatch = new Stopwatch();
                stopwatch_all = new Stopwatch();
                stopwatch.Start();
                stopwatch_all.Start();
                compositeLogMessage?.Add((msg) =>
                {
                    var cost = stopwatch.ElapsedMilliseconds;
                    stopwatch.Restart();
                    return $"{msg}(cost:{cost}ms)";
                });
            }

            if (cfg.ResetStopWatchEachPrint)
                afterPrint += () => stopwatch?.Restart();

            if (cfg.PrependTimestamp)
                compositeLogMessage?.Add((msg) => $"[{DateTime.Now:HH:mm:ss}] {msg}");

            outputHandler = cfg.OutputHandler;
#endif
        }

        [Conditional("DEBUG")]
        public void Log(string message)
        {
            message = prepend(message);
            message = processMessage(message);
            log(message);
            afterPrint?.Invoke();
        }

        public void Append(string message)
        {
            message = processMessage(message);
            sb.AppendLine(message);
        }

        private void log(string msg, UnityEngine.Object context = null)
        {
            UnityEngine.Debug.Log(msg, context);
            outputHandler?.Invoke(msg);
        }

        private string processMessage(string input)
        {
            compositeLogMessage.ForEach(func => input = func(input));
            return input;
        }

        public void Complete(string finish = null, UnityEngine.Object context = null)
        {
            if (sb.Length <= 0)
                return;

            if (string.IsNullOrWhiteSpace(finish))
                sb.AppendLine(processMessage(finish));

            if (stopwatch_all != null)
                sb.Append($"(total cost:{stopwatch_all.ElapsedMilliseconds}ms)");

            log(prepend("\n" + sb.ToString()), context);

            sb.Clear();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            if (sb.Length > 0)
                Complete();

            compositeLogMessage?.Clear();
            afterPrint = null;
            if (stopwatch is not null)
            {
                stopwatch.Stop();
                stopwatch = null;
            }
            if (stopwatch_all is not null)
            {
                stopwatch_all.Stop();
                stopwatch_all = null;
            }
        }
    }
}