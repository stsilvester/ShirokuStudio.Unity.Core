using ShirokuStudio.Core.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ShirokuStudio.Core
{
    public static class CSVUtility
    {
        public struct Config
        {
            public char Seperator;
            public bool HasHeader;
            public char Scope;
            public Dictionary<string, string> HeaderMapping;

            public static Config Default = new Config()
            {
                HasHeader = true,
                Seperator = ',',
                Scope = '\''
            };
        }

        private class CSVReaderEnumerator : IEnumerator<string>
        {
            public string Input { get; }
            public Config Config { get; }
            public string Current { get; private set; }
            public bool NextRow { get; private set; }
            public int StartIndex { get; private set; }
            public int EndIndex { get; private set; }
            public bool HasNext => EndIndex < length;
            public int ColumnCount { get; private set; }
            private int cellLoad = 0;
            private const int max = 100;

            object IEnumerator.Current { get; }

            private int length;
            private StringBuilder stringBuilder = new StringBuilder();

            public CSVReaderEnumerator(string input, Config config)
            {
                Input = input.Replace("\r\n", "\n").Trim().TrimEnd('\n', '\r');
                length = Input.Length;
                Config = config;
            }

            public bool MoveNext()
            {
                if (!HasNext)
                    return false;

                stringBuilder.Clear();
                StartIndex = EndIndex;
                NextRow = false;
                var isScoped = false;
                var nextRow = false;
                for (var i = StartIndex; i < Input.Length; i++)
                {
                    EndIndex = i;
                    var cur = Input[EndIndex];
                    if (isScoped)
                    {
                        if (cur == Config.Scope)
                        {
                            if (EndIndex + 1 < Input.Length && Input[EndIndex + 1] == Config.Scope)
                            {
                                stringBuilder.Append(cur);
                                i++;
                            }
                            else
                            {
                                isScoped = false;
                            }
                        }
                        else
                        {
                            stringBuilder.Append(cur);
                        }
                    }
                    else if (cur == '\n')
                    {
                        nextRow = true;
                        break;
                    }
                    else if (cur == Config.Seperator)
                    {
                        break;
                    }
                    else if (cur == Config.Scope)
                    {
                        isScoped = true;
                    }
                    else if (i < Input.Length && cur != '\n')
                    {
                        stringBuilder.Append(cur);
                    }
                }
                EndIndex++;
                NextRow = nextRow;
                Current = stringBuilder.ToString();
                UnityEngine.Debug.Log($"next cell: {Current}, hasNext:{HasNext}[{StartIndex}-{EndIndex}], nextRow:{NextRow}");
                if (cellLoad++ > max)
                {
                    throw new Exception("overflow");
                }
                return true;
            }

            public void Reset()
            {
                StartIndex = 0;
                EndIndex = 0;
                NextRow = false;
            }

            public void Dispose()
            {
            }

            public string[] GetNextRow()
            {
                var result = new List<string>();
                if (ColumnCount == 0)
                {
                    while (MoveNext())
                    {
                        result.Add(Current);
                        if (NextRow)
                            break;
                    }
                    ColumnCount = result.Count;
                    return result.ToArray();
                }
                else
                {
                    return GetNextCells(ColumnCount);
                }
            }

            public string[] GetNextCells(int length)
            {
                var result = new List<string>();
                for (int i = 0; i < length; i++)
                {
                    if (MoveNext())
                    {
                        result.Add(Current);
                    }
                    else
                    {
                        result.Add(string.Empty);
                    }
                }
                return result.ToArray();
            }
        }

        public static string Serialize(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows, Config? config = null)
        {
            var output = new StringBuilder();
            if (config.Value.HasHeader)
            {
                writeRow(output, config.Value, headers);
            }
            foreach (var row in rows)
            {
                writeRow(output, config.Value, row);
            }
            return output.ToString();
        }

        public static List<string[]> Deserialize(string input, out string[] headers, Config? config = null)
        {
            headers = null;
            var e = new CSVReaderEnumerator(input, config ?? Config.Default);

            if (config.Value.HasHeader)
                headers = e.GetNextRow();

            var result = new List<string[]>();
            while (e.HasNext)
                result.Add(e.GetNextRow());
            return result;
        }

        public static string Serialize<T>(IEnumerable<T> values, Config? config = null, params Expression<Func<T, object>>[] memberGetters)
        {
            config ??= Config.Default;

            var output = new StringBuilder();

            var members = memberGetters.IsNullOrEmpty()
                ? FastCacher.GetMemberNames<T>().ToArray()
                : memberGetters
                    .Select(m => m.Body is MemberExpression mexp ? mexp.Member : m.Body is UnaryExpression uexp ? uexp.Operand is MemberExpression umexp ? umexp.Member : null : null)
                    .Select(m => m.Name).ToArray();

            var valueGetters = members.Select(FastCacher.GetGetter<T>);

            if (config.Value.HasHeader)
            {
                var map = members;
                if (config.Value.HeaderMapping != null)
                {
                    map = members.Select(m => config.Value.HeaderMapping.TryGetValue(m, out var c) ? c : m).ToArray();
                }
                writeRow(output, config.Value, values: map);
            }

            foreach (var item in values)
            {
                var cells = valueGetters.Select(g => g(item).ToString()).ToArray();
                writeRow(output, config.Value, cells);
            }
            return output.ToString();
        }

        private static void writeRow(StringBuilder output, Config config, IEnumerable<string> values)
        {
            var length = values.Count();
            var i = 0;
            foreach (var text in values)
            {
                if (text.Contains(config.Seperator) || text.Contains(config.Scope) || text.Contains('\n'))
                {
                    output.Append(config.Scope);
                    output.Append(text.Replace(config.Scope.ToString(), config.Scope + config.Scope.ToString()));
                    output.Append(config.Scope);
                }
                else
                    output.Append(text);

                if (i < length - 1)
                {
                    output.Append(config.Seperator);
                }
                i++;
            }
            output.AppendLine();
        }

        public static IEnumerable<T> Deserialize<T>(string input, Config? config = null, params Expression<Func<T, object>>[] memberGetters)
            where T : new()
        {
            var members = memberGetters.IsNullOrEmpty()
                ? FastCacher.GetMemberNames<T>().ToArray()
                : memberGetters.Select(m => m.Body is MemberExpression mexp ? mexp.Member : m.Body is UnaryExpression uexp ? uexp.Operand is MemberExpression umexp ? umexp.Member : null : null)
                    .Select(m => m.Name).ToArray();

            var e = new CSVReaderEnumerator(input, config ?? Config.Default);
            if (config.Value.HasHeader)
            {
                UnityEngine.Debug.Log("deserialize headers");
                var headers = e.GetNextRow();

                if (config.Value.HeaderMapping != null)
                {
                    headers = headers.Select(h => config.Value.HeaderMapping.FirstOrDefault(kv => kv.Value == h).Key ?? h).ToArray();
                }

                members = members.OrderBy(m => headers.IndexOf(m)).ToArray();
            }

            var dataList = new List<T>();
            while (e.HasNext)
            {
                var row = e.GetNextRow();
                var data = new T();
                for (int i = 0; i < row.Length; i++)
                    FastCacher.Set(data, members[i], row[i]);
                dataList.Add(data);
            }
            return dataList;
        }
    }
}