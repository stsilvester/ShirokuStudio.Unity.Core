using System.Collections.Generic;
using System.Linq;

namespace ShirokuStudio.Core
{
    public interface IHashCode
    {
        int HashCode { get; }
    }

    public static class IHashCodeExtensions
    {
        public static bool EqualsHashCode(this IHashCode self, IHashCode other)
        {
            return self.HashCode == other.HashCode;
        }

        public static int CalculateHashCode(this IEnumerable<IHashCode> objects)
        {
            int hash = 17;
            foreach (var obj in objects.Where(o => o != null))
                hash = hash * 31 + obj.HashCode;

            return hash;
        }

        public static int CalculateHashCode(this IEnumerable<object> objects)
        {
            int hash = 17;
            var i = 1;
            foreach (var obj in objects)
                hash = hash * 31 + (obj?.GetHashCode() ?? 23 * i++);
            return hash;
        }
    }
}