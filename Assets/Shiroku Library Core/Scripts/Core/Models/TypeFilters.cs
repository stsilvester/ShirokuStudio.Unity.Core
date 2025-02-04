using System;

namespace ShirokuStudio.Core.Models
{
    public interface ITypeFilter
    {
        TriState IsAbstract { get; set; }
        TriState IsInterface { get; set; }
        TriState IsStruct { get; set; }
        TriState IsGeneric { get; set; }

        public bool IsMatch(Type type)
        {
            var isAbstract = IsAbstract.ToBoolean();
            if (isAbstract.HasValue && type.IsAbstract != isAbstract.Value)
                return false;

            var isInterface = IsInterface.ToBoolean();
            if (isInterface.HasValue && type.IsInterface != isInterface.Value)
                return false;

            var isStruct = IsStruct.ToBoolean();
            if (isStruct.HasValue && type.IsValueType != isStruct.Value)
                return false;

            var isGeneric = IsGeneric.ToBoolean();
            if (isGeneric.HasValue && type.IsGenericType != isGeneric.Value)
                return false;

            return true;
        }
    }
}