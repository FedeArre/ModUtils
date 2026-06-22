using HarmonyLib;
using SimplePartLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[HarmonyPatch]
internal static class ExceptionHook
{
    private static readonly string[] TargetTypeNames =
    {
        "MainCarProperties",
        "CarProperties",
        "Partinfo",
        "DISABLER",
        "FLUID",
    };

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        const BindingFlags methodFlags = BindingFlags.Instance |
                                         BindingFlags.Static |
                                         BindingFlags.Public |
                                         BindingFlags.NonPublic |
                                         BindingFlags.DeclaredOnly;

        foreach (var typeName in TargetTypeNames)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type == null)
            {
                Debug.LogWarning(
                    "[DeveloperExceptionTracer] Could not find type: " +
                    typeName);
                continue;
            }

            foreach (var targetType in EnumerateTypeAndNestedTypes(type))
            {
                foreach (var method in targetType.GetMethods(methodFlags))
                {
                    if (ShouldPatchMethod(method))
                        yield return method;
                }
            }
        }
    }

    private static IEnumerable<Type> EnumerateTypeAndNestedTypes(Type root)
    {
        yield return root;

        const BindingFlags nestedFlags = BindingFlags.Public |
                                         BindingFlags.NonPublic;

        foreach (var nested in root.GetNestedTypes(nestedFlags))
        {
            foreach (var child in EnumerateTypeAndNestedTypes(nested))
                yield return child;
        }
    }

    private static bool ShouldPatchMethod(MethodBase method)
    {
        if (method == null)
            return false;

        if (method.IsAbstract)
            return false;

        if (method.ContainsGenericParameters)
            return false;

        if (method.GetMethodBody() == null)
            return false;

        // Skip property accessors - extremely hot and almost never the meaningful source of an exception
        if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            return false;

        return true;
    }

    [HarmonyFinalizer]
    private static Exception Finalizer(
        Exception __exception,
        object __instance,
        MethodBase __originalMethod)
    {
        if (!ModMain.DevUIEnabled.Checked)
            return __exception;

        if (__exception == null)
            return null;

        ExceptionTracerUtil.LogException(
            __originalMethod,
            __instance,
            __exception);

        return __exception;
    }
}

internal static class ExceptionTracerUtil
{
    private static readonly HashSet<string> Seen = new HashSet<string>();

    public static void ClearSeen()
    {
        lock (Seen)
            Seen.Clear();
    }

    public static void LogException(
        MethodBase originalMethod,
        object instance,
        Exception exception)
    {
        var owner = ResolveOwner(instance);
        var key = BuildKey(originalMethod, owner, exception);

        if (!ShouldLog(key))
            return;

        var sb = new StringBuilder();

        sb.AppendLine("[ExpandedException]");
        sb.AppendLine(
            "Method: " +
            originalMethod.DeclaringType?.FullName +
            "." +
            originalMethod.Name);
        sb.AppendLine("Exception type: " + exception.GetType().FullName);
        sb.AppendLine("Message: " + exception.Message);
        sb.AppendLine("Patched instance: " + DescribeObject(instance));
        sb.AppendLine("Resolved owner: " + DescribeObject(owner));

        if (owner != null)
        {
            sb.AppendLine("Null fields on resolved owner:");
            sb.AppendLine(DumpNullFields(owner));
        }

        AppendInnerExceptions(sb, exception);

        sb.AppendLine("Exception:");
        sb.AppendLine(exception.ToString());

        if (owner is UnityEngine.Object unityOwner)
            Debug.LogError(sb.ToString(), unityOwner);
        else
            Debug.LogError(sb.ToString());
    }

    private static void AppendInnerExceptions(
        StringBuilder sb,
        Exception exception)
    {
        var depth = 0;
        var current = exception.InnerException;

        while (current != null)
        {
            depth++;
            sb.AppendLine(
                "InnerException[" +
                depth +
                "]: " +
                current.GetType().FullName +
                " - " +
                current.Message);
            current = current.InnerException;
        }
    }

    private static bool ShouldLog(string key)
    {
        lock (Seen)
            return Seen.Add(key);
    }

    private static string BuildKey(
        MethodBase originalMethod,
        object owner,
        Exception exception)
    {
        var methodName =
            originalMethod.DeclaringType?.FullName +
            "." +
            originalMethod.Name;

        var ownerKey = GetObjectKey(owner);
        var exceptionType = exception.GetType().FullName;
        var message = exception.Message ?? "<no message>";
        var stack = exception.StackTrace ?? "<no stack>";

        return methodName +
               " | " +
               ownerKey +
               " | " +
               exceptionType +
               " | " +
               message +
               " | " +
               stack;
    }

    public static object ResolveOwner(object instance)
    {
        if (instance == null)
            return null;

        if (instance is Component || instance is GameObject)
            return instance;

        var flags = BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic;

        var ownerField = instance.GetType().GetField("<>4__this", flags);
        if (ownerField != null)
        {
            try
            {
                return ownerField.GetValue(instance) ?? instance;
            }
            catch
            {
                return instance;
            }
        }

        return instance;
    }

    private static string GetObjectKey(object obj)
    {
        if (obj == null)
            return "<null>";

        if (obj is Component component)
        {
            return component.GetType().FullName +
                   "@" +
                   GetTransformPath(component.transform);
        }

        if (obj is GameObject gameObject)
        {
            return "GameObject@" + GetTransformPath(gameObject.transform);
        }

        return obj.GetType().FullName;
    }

    private static string DescribeObject(object obj)
    {
        if (obj == null)
            return "<null>";

        if (obj is Component component)
        {
            return string.Format(
                "{0} on GameObject '{1}', path '{2}', scene '{3}'",
                component.GetType().FullName,
                component.gameObject.name,
                GetTransformPath(component.transform),
                component.gameObject.scene.name);
        }

        if (obj is GameObject gameObject)
        {
            return string.Format(
                "GameObject '{0}', path '{1}', scene '{2}'",
                gameObject.name,
                GetTransformPath(gameObject.transform),
                gameObject.scene.name);
        }

        if (obj is UnityEngine.Object unityObject)
            return obj.GetType().FullName + " '" + unityObject.name + "'";

        return obj.GetType().FullName;
    }

    private static string GetTransformPath(Transform transform)
    {
        if (transform == null)
            return "<no transform>";

        var sb = new StringBuilder(transform.name);

        while (transform.parent != null)
        {
            transform = transform.parent;
            sb.Insert(0, transform.name + "/");
        }

        return sb.ToString();
    }

    private static string DumpNullFields(object obj)
    {
        if (obj == null)
            return "<no object>";

        var sb = new StringBuilder();

        foreach (var field in EnumerateFields(obj.GetType()))
        {
            if (field.IsStatic)
                continue;

            if (field.FieldType.IsValueType &&
                Nullable.GetUnderlyingType(field.FieldType) == null)
            {
                continue;
            }

            object value;

            try
            {
                value = field.GetValue(obj);
            }
            catch (Exception ex)
            {
                sb.AppendLine(
                    field.DeclaringType?.Name +
                    "." +
                    field.Name +
                    " = <read failed: " +
                    ex.GetType().Name +
                    ">");
                continue;
            }

            var isNull = value == null;

            if (value is UnityEngine.Object unityValue && unityValue == null)
                isNull = true;

            if (isNull)
            {
                sb.AppendLine(
                    field.DeclaringType?.Name +
                    "." +
                    field.FieldType.Name +
                    " " +
                    field.Name +
                    " = <null>");
            }
        }

        if (sb.Length == 0)
            sb.AppendLine("<no null fields found>");

        return sb.ToString();
    }

    private static IEnumerable<FieldInfo> EnumerateFields(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance |
                                   BindingFlags.Public |
                                   BindingFlags.NonPublic |
                                   BindingFlags.DeclaredOnly;

        while (type != null)
        {
            foreach (var field in type.GetFields(flags))
                yield return field;

            type = type.BaseType;
        }
    }
}