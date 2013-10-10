using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KasCRUD
{
    public class GenericMethods
    {
        private string assemblyName = "";
        private string namespaceName = "";

        public GenericMethods(string assemblyName, string namespaceName)
        {
            //if (String.IsNullOrEmpty(namespaceName)) throw new System.ArgumentException("Parameter must not be null", "namespaceName");
            this.assemblyName = assemblyName;
            this.namespaceName = namespaceName;
        }

        /// <summary>
        /// Calls a method in a given Class
        /// </summary>
        /// <param name="assemblyName">i.e System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;</param>
        /// <param name="namespaceName">The name space of the class</param>
        /// <param name="typeName">The class name</param>
        /// <param name="methodName">Method name in the class must be public static</param>
        /// <param name="mParams">Should be in Object[]</param>
        /// <returns>The returned object is in dynamic form</returns>
        public dynamic invokeStaticMethod(string className, string methodName, object[] mParams)
        {
            // Get the Type for the class
            Type type = getType(className);

            if (type == null) return new CRUDError { ErrorMessage = className + " Entity Not Found", FromMethod = "invokeStaticMethod(string typeName, string methodName, object[] mParams)" };
            dynamic returnedObject = null;
            try
            {
                returnedObject = type.InvokeMember(
                                            methodName,
                                            BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                                            null,
                                            null,
                                            mParams);
            }
            catch (Exception e)
            {
                return new CRUDError { ErrorMessage = e.Message, FromMethod = "invokeStaticMethod(string typeName, string methodName, object[] mParams)" };
            }

            return returnedObject;
        }

        /// <summary>
        /// Calls a method in a given class instance
        /// </summary>
        /// <param name="assemblyName">i.e System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;</param>
        /// <param name="namespaceName">The name space of the class</param>
        /// <param name="typeName">The class name</param>
        /// <param name="methodName">Method name in the class must be public</param>
        /// <param name="mParams">Should be in Object[], if no param pass null</param>
        /// <returns>The returned object is in dynamic form</returns>
        public dynamic invokeInstanceMethod(string className, string methodName, object[] mParams)
        {

            Type type = getType(className);
            if (type == null) return new CRUDError { ErrorMessage = className + " Entity Not Found", FromMethod = "invokeInstanceMethod(string typeName, string methodName, object[] mParams)" };
            dynamic instance = Activator.CreateInstance(type);
            MethodInfo method = null;
            if (mParams != null)
            {
                Type[] paramTypes = mParams.Select(p => p.GetType()).ToArray();
                method = type.GetMethod(methodName, paramTypes);
            }
            else
            {
                method = type.GetMethod(methodName);
            }
            if (method == null) return new CRUDError { ErrorMessage = methodName + " Method Not Found", FromMethod = "invokeInstanceMethod(string typeName, string methodName, object[] mParams)" };

            dynamic returnedObject = null;
            try
            {
                returnedObject = method.Invoke(instance, mParams);
            }
            catch (Exception e)
            {
                return new CRUDError { ErrorMessage = e.Message, FromMethod = "invokeInstanceMethod(string typeName, string methodName, object[] mParams)" };
            }

            return returnedObject;
        }

        public dynamic invokeMethodFromInstance(object instance, string instanceMethodName, object[] mParams)
        {
            Type type = instance.GetType();
            if (type == null) return new CRUDError { ErrorMessage = "Entity Not Found", FromMethod = "invokeMethodFromInstance(object instance, string instanceMethodName, object[] mParams)" };
            MethodInfo method = null;
            if (mParams != null)
            {
                Type[] paramTypes = mParams.Select(p => p.GetType()).ToArray();
                method = type.GetMethod(instanceMethodName, paramTypes);
            }
            else
            {
                method = type.GetMethod(instanceMethodName);
            }

            if (method == null) return new CRUDError { ErrorMessage = instanceMethodName + " Method Not Found", FromMethod = "invokeMethodFromInstance(object instance, string instanceMethodName, object[] mParams)" };

            dynamic returnedObject = null;
            try
            {
                returnedObject = method.Invoke(instance, mParams);
            }
            catch (Exception e)
            {
                return new CRUDError { ErrorMessage = e.Message, FromMethod = "invokeMethodFromInstance(object instance, string instanceMethodName, object[] mParams)" };
            }

            return returnedObject;
        }

        public dynamic invokeMethodFromInstance(object instance, string inerInstanceName, string inerInstanceMethodName, object[] mParams)
        {
            dynamic responce;
            try
            {
                dynamic inerInstance = instance.GetType().GetProperty(inerInstanceName).GetValue(instance);
                responce = invokeMethodFromInstance(inerInstance, inerInstanceMethodName, mParams);
            }
            catch (Exception e)
            {
                return new CRUDError { ErrorMessage = e.Message, FromMethod = "invokeMethodFromInstance(object instance, string inerInstanceName, string inerInstanceMethodName, object[] mParams)" };
            }
            return responce;
        }

        public dynamic createInstance(string className)
        {

            Type type = getType(className);
            if (type == null) return new CRUDError { ErrorMessage = className + " Entity Not Found", FromMethod = "createInstance(string tableName)" };
            dynamic instance = Activator.CreateInstance(type);
            return instance;
        }

        public dynamic createInstanceWithEntity(string className, dynamic entity)
        {
            Type type = getType(className);
            if (type == null) return new CRUDError { ErrorMessage = className + " Entity Not Found", FromMethod = "createInstanceWithEntity(string tableName, dynamic entity)" };
            JToken jToken = JToken.FromObject(entity);
            dynamic returnedObject = JsonConvert.DeserializeObject(jToken.ToString(), type);

            return returnedObject;
        }

        public dynamic getInnerInstanceName(object instance, string inerInstanceName)
        {
            dynamic res = null;
            try
            {
                res = instance.GetType().GetProperty(inerInstanceName).GetValue(instance);
            }
            catch (Exception e)
            {
                return new CRUDError { ErrorMessage = inerInstanceName + " Entity Not Found", FromMethod = "getInerInstanceName(object instance, string inerInstanceName)" };
            }

            return res;
        }

        public dynamic getInnerStaticProperty(string className, string inerInstanceName)
        {
            Type type;
            if (String.IsNullOrEmpty(assemblyName)) type = Type.GetType(namespaceName + "." + className);
            else if (String.IsNullOrEmpty(namespaceName)) type = Type.GetType(className + "," + assemblyName);
            else type = Type.GetType(namespaceName + "." + className + "," + assemblyName);

            var propertyInfo = type.GetProperty(inerInstanceName);
            dynamic value = propertyInfo.GetValue(null, null);
            return value;
        }

        public dynamic getType(string className)
        {
            Type type;
            if (String.IsNullOrEmpty(assemblyName) && String.IsNullOrEmpty(namespaceName)) type = Type.GetType(className);
            else if (String.IsNullOrEmpty(namespaceName) && !String.IsNullOrEmpty(assemblyName)) type = Type.GetType(className + "," + assemblyName);
            else if (String.IsNullOrEmpty(assemblyName) && !String.IsNullOrEmpty(namespaceName)) type = Type.GetType(namespaceName + "." + className);
            else type = Type.GetType(namespaceName + "." + className + "," + assemblyName);

            return type;
        }

    }
}