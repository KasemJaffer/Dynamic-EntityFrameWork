using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KasCRUD
{
    public static class JsonHelper
    {
        public static object ConvertToSimpleObject(object obj, params string[] paths)
        {
            if (obj == null)
                return null;

            var objType = obj.GetType();

            if (obj is String || objType.IsPrimitive)
                return obj;

            Dictionary<string, List<string>> pathsToCreate = new Dictionary<string, List<string>>();
            foreach (var p in paths)
            {
                List<string> pSplited = p.Split('.').ToList();
                string key = pSplited[0];
                pSplited.RemoveAt(0);
                if (pathsToCreate.ContainsKey(key) == false)
                    pathsToCreate.Add(key, new List<string>());

                if (pSplited.Count > 0)
                    pathsToCreate[key].Add(String.Join(".", pSplited));
            }

            List<string> createdPaths = new List<string>();


            List<PropertyInfo> properties = GetSimpleObjectProperties(objType).ToList();
            List<ExtensionMetadata> metadatas = properties.Select(p => new ExtensionMetadata { PropertyName = p.Name, PropertyType = p.PropertyType.AssemblyQualifiedName }).ToList();

            if (paths != null)
            {
                foreach (var path in pathsToCreate.Keys)
                {
                    var property = objType.GetProperty(path);

                    if (property != null)
                    {
                        var propertyType = property.PropertyType;
                        var propertyValue = property.GetValue(obj, null);
                        if ((propertyValue is String) == false && propertyValue is IEnumerable)
                            propertyType = typeof(ArrayList);
                        else
                            propertyType = typeof(object);

                        metadatas.Add(new ExtensionMetadata { PropertyName = path, PropertyType = propertyType.AssemblyQualifiedName });
                    }
                }
            }

            Type virtualType = ReflectionHelper.CreateVirtualType(objType.Name, metadatas);
            object virtualObject = ReflectionHelper.CreateVirtualObject(virtualType, properties.Select(p => new PropertyMetadata { PropertyName = p.Name, Value = p.GetValue(obj, null) }));
            var virtualObjectType = virtualObject.GetType();

            foreach (var path in pathsToCreate.Keys)
            {
                object pathObject = null;
                var property = objType.GetProperty(path);

                if (property != null)
                {
                    var propertyValue = property.GetValue(obj, null);

                    if ((propertyValue is String) == false && propertyValue is IEnumerable)
                    {
                        pathObject = new ArrayList();
                        var propertyValueEnumerator = propertyValue as IEnumerable;
                        foreach (var item in propertyValueEnumerator)
                        {
                            if (pathsToCreate[path].Count > 0)
                                (pathObject as ArrayList).Add(ConvertToSimpleObject(item, pathsToCreate[path].ToArray()));
                            else
                                (pathObject as ArrayList).Add(ConvertToSimpleObject(item));
                        }
                    }
                    else
                    {
                        if (pathsToCreate[path].Count > 0)
                            pathObject = ConvertToSimpleObject(propertyValue, String.Join(".", pathsToCreate[path].ToArray()));
                        else
                            pathObject = ConvertToSimpleObject(propertyValue);
                    }

                    virtualObjectType.GetProperty(path).SetValue(virtualObject, pathObject, null);
                }
            }

            return virtualObject;
        }

        public static IEnumerable<PropertyInfo> GetSimpleObjectProperties(Type modelType)
        {
            return modelType.GetProperties().Where(p => IsSimpleObjectProperty(p.PropertyType));
        }

        public static bool IsSimpleObjectProperty(Type propertyType)
        {

            return propertyType.IsPrimitive || simpleObjectProperties.Contains(propertyType);
           
        }

        private static Type[] simpleObjectProperties = new[] {
            typeof(string),
            typeof(decimal),
            typeof(float),
            typeof(double),
            typeof(bool),
            typeof(int?),
            typeof(int),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Nullable<int>),
            typeof(Nullable<bool>), 
            typeof(Nullable<float>), 
            typeof(Nullable<decimal>), 
            typeof(Nullable<double>), 
            typeof(Nullable<DateTime>),
            typeof(Nullable<DateTimeOffset>),
            typeof(Nullable<TimeSpan>),       
            typeof(Nullable<Guid>), 
        };
    }
}
