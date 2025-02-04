using System;
using ModestTree;

namespace ShirokuStudio.Siganls
{
    public readonly struct SignalID
    {
        public Type SignalType { get; }

        public SignalID(Type signalType)
        {
            SignalType = signalType;
        }

        public override string ToString()
        {
            return $"SignalID [Type={SignalType.PrettyName()}]";
        }

        public override bool Equals(object obj)
        {
            if (obj is SignalID id)
            {
                return id == this;
            }
            return false;
        }

        public static bool operator ==(SignalID a, SignalID b)
        {
            return a.SignalType == b.SignalType;
        }

        public static bool operator !=(SignalID a, SignalID b)
        {
            return a.SignalType != b.SignalType;
        }

        public override int GetHashCode()
        {
            return SignalType.GetHashCode();
        }
    }
}