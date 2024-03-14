using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AssetStudio;

public class LuaMethods
{
    public Delegate GetAnyMethod(string className, string methodName, object target = null)
    {
        var type = Type.GetType(className);
        if (type == null)
        {
            throw new Exception($"Class {className} not found.");
        }

        var method = type.GetMethod(methodName);
        if (method == null)
        {
            throw new Exception($"Method {methodName} not found in class {className}.");
        }

        var delegateType = GetDelegateType(method);
        var del = Delegate.CreateDelegate(delegateType, target, method);

        return del;
    }

    private Type GetDelegateType(MethodInfo methodInfo)
    {
        var types = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();
        if (methodInfo.ReturnType != typeof(void))
        {
            types.Add(methodInfo.ReturnType);
        }
        return Expression.GetDelegateType(types.ToArray());
    }
    
    public FileStream CreateFileStream(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public MemoryStream CreateMemoryStream()
    {
        return new MemoryStream();
    }
    
    public EndianBinaryReader CreateEndianBinaryReader(Stream stream)
    {
        return new EndianBinaryReader(stream);
    }
}