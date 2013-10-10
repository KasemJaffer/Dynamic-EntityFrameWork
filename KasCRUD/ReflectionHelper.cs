using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;


namespace KasCRUD
{
    public static class ReflectionHelper
    {
        public static string VirtualAssemblyName = "VirtualObject";

        public static Type CreateVirtualType(string className, IEnumerable<ExtensionMetadata> extensions)
        {
            AssemblyName assemblyName = new AssemblyName(VirtualAssemblyName);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name + ".dll");

            TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public);

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            foreach (var extension in extensions)
            {
                FieldBuilder field = typeBuilder.DefineField(extension.PropertyName.ToLower(), Type.GetType(extension.PropertyType), FieldAttributes.Private);

                PropertyBuilder property = typeBuilder.DefineProperty(extension.PropertyName, PropertyAttributes.HasDefault, Type.GetType(extension.PropertyType), Type.EmptyTypes);

                MethodBuilder propertyGetter = typeBuilder.DefineMethod("get_" + extension.PropertyName, getSetAttributes, Type.GetType(extension.PropertyType), Type.EmptyTypes);
                ILGenerator numberGetIL = propertyGetter.GetILGenerator();
                numberGetIL.Emit(OpCodes.Ldarg_0);
                numberGetIL.Emit(OpCodes.Ldfld, field);
                numberGetIL.Emit(OpCodes.Ret);

                MethodBuilder propertySetter = typeBuilder.DefineMethod("set_" + extension.PropertyName, getSetAttributes, null, new Type[] { Type.GetType(extension.PropertyType) });

                ILGenerator numberSetIL = propertySetter.GetILGenerator();
                numberSetIL.Emit(OpCodes.Ldarg_0);
                numberSetIL.Emit(OpCodes.Ldarg_1);
                numberSetIL.Emit(OpCodes.Stfld, field);
                numberSetIL.Emit(OpCodes.Ret);

                property.SetGetMethod(propertyGetter);
                property.SetSetMethod(propertySetter);
            }

            Type virtualType = typeBuilder.CreateType();

            return virtualType;
        }

        public static object CreateVirtualObject(Type type, IEnumerable<PropertyMetadata> properties)
        {
            object obj = Activator.CreateInstance(type);

            foreach (var property in properties)
            {
                PropertyInfo propertyInfo = type.GetProperty(property.PropertyName);
                propertyInfo.SetValue(obj, property.Value, null);
            }

            return obj;
        }
    }
}
