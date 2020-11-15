namespace Files.Specification.Tests.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Provides test data rows from an instance property or method of the test class,
    ///     initialized via <see cref="Activator.CreateInstance(Type)"/>.
    /// </summary>
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
            IEnumerable<object?[]>? result;

            if (DynamicDataSourceType == DynamicDataSourceType.Property)
            {
                var property = testClass.GetType().GetProperty(DynamicDataSourceName);
                result = (IEnumerable<object?[]>?)property?.GetValue(testClass);
            }
            else
            {
                var method = testClass.GetType().GetMethod(DynamicDataSourceName);
                result = (IEnumerable<object?[]>?)method?.Invoke(testClass, null);
            }

            return result ?? Enumerable.Empty<object?[]>();
        }

        private static object GetTestClassInstance(MethodInfo testMethodInfo)
        {
            if (testMethodInfo.ReflectedType is null ||
                Activator.CreateInstance(testMethodInfo.ReflectedType) is not object result)
            {
                throw new InvalidOperationException($"Creating an instance of type ${testMethodInfo.ReflectedType} failed.");
            }
            return result;
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
