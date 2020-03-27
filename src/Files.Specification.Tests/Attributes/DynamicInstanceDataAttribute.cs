namespace Files.Specification.Tests.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class DynamicInstanceDataAttribute : Attribute, ITestDataSource
    {

        public string DynamicDataSourceName { get; }

        public DynamicDataSourceType DynamicDataSourceType { get; }

        public DynamicInstanceDataAttribute(
            string dynamicDataSourceName,
            DynamicDataSourceType dynamicDataSourceType = DynamicDataSourceType.Property
        )
        {
            DynamicDataSourceName = dynamicDataSourceName;
            DynamicDataSourceType = dynamicDataSourceType;
        }

        public IEnumerable<object?[]> GetData(MethodInfo testMethodInfo)
        {
            var testClass = GetTestClassInstance(testMethodInfo);
            
            if (DynamicDataSourceType == DynamicDataSourceType.Property)
            {
                var property = testClass.GetType().GetProperty(DynamicDataSourceName);
                var value = (IEnumerable<object?[]>)property.GetValue(testClass);
                return value;
            }
            else
            {
                var method = testClass.GetType().GetMethod(DynamicDataSourceName);
                return (IEnumerable<object?[]>)method.Invoke(testClass, null);
            }
        }

        private static object GetTestClassInstance(MethodInfo testMethodInfo)
        {
            return Activator.CreateInstance(testMethodInfo.ReflectedType);
        }

        public string? GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data is object)
            {
                return $"{methodInfo.Name}({string.Join(", ", data)})";
            }
            return null;
        }

    }

}
