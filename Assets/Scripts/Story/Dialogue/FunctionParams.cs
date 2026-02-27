using System.Collections.Generic;
using UnityEngine;

public class FunctionParams
{
    private const char paramSplit = '^';

    private Dictionary<string, string> parameters = new Dictionary<string, string>();
    
    public FunctionParams(string[] paramArray)
    {
        for (int i = 0; i < paramArray.Length; i++)
        {
            if (paramArray[i].StartsWith(paramSplit))
            {
                string pName = paramArray[i];
                string pValue = "";

                if (i + 1 < paramArray.Length && !paramArray[i + 1].StartsWith(paramSplit))
                {
                    pValue = paramArray[i + 1];
                    i++;
                }

                parameters.Add(pName, pValue);
            }
        }
    }

    public bool TryGetValue<T>(string parameterName, out T value, T defaultValue = default(T)) => TryGetValue(new string[] { parameterName }, out value, defaultValue);

    public bool TryGetValue<T>(string[] parameterName, out T value, T defaultValue = default(T))
    {
        foreach (string param in parameterName)
        {
            if (parameters.TryGetValue(param, out string paramValue))
            {
                if (TryCastParam(paramValue, out value))
                {
                    return true;
                }
            }
        }

        value = defaultValue;
        return false;
    }

    private bool TryCastParam<T>(string paramValue, out T value)
    {
        if(typeof(T) == typeof(bool))
        {
            if(bool.TryParse(paramValue, out bool boolValue))
            {
                value = (T)(object)boolValue;
                return true;
            }
        }
        else if (typeof(T) == typeof(int))
        {
            if (int.TryParse(paramValue, out int intValue))
            {
                value = (T)(object)intValue;
                return true;
            }
        }
        else if (typeof(T) == typeof(float))
        {
            if (float.TryParse(paramValue, out float floatValue))
            {
                value = (T)(object)floatValue;
                return true;
            }
        }
        else if (typeof(T) == typeof(string))
        {
            value = (T)(object)paramValue;
            return true;
        }

        value = default(T);
        return false;
    }
}
