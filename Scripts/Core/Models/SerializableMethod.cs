using ShirokuStudio.Core.Reflection;
using System;
using System.Reflection;
using UnityEngine;

namespace ShirokuStudio.Core.Models
{
    [Serializable]
    public class SerializableMethod
    {
        public MethodInfo MethodInfo
        {
            get => getMethodInfo();
            set => setMethodInfo(value);
        }

        private MethodInfo _methodInfo;

        public bool IsValid => MethodInfo != null;

        public string Name => methodName;

        [SerializeField]
        private string typeName;

        [SerializeField]
        private string methodName;

        [SerializeField]
        private int hash;

        public SerializableMethod()
        {
        }

        public SerializableMethod(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        private MethodInfo getMethodInfo()
        {
            if (_methodInfo != null
                && typeName == _methodInfo.DeclaringType.AssemblyQualifiedName
                && typeName == _methodInfo.Name)
                return _methodInfo;

            _methodInfo = null;

            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(methodName))
                return _methodInfo;

            _methodInfo = TypeCache.GetMethod(typeName, methodName, hash);
            return _methodInfo;
        }

        private void setMethodInfo(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            typeName = _methodInfo?.DeclaringType?.AssemblyQualifiedName;
            methodName = _methodInfo?.Name;
            hash = TypeCache.GetMethodHash(_methodInfo);
        }

        public override string ToString()
        {
            return $"{typeName}.{methodName}({hash}){(_methodInfo != null ? "O" : "X")}";
        }
    }
}