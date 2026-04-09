using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue.Functions
{
    public class FunctionsDatabase
    {   
        private Dictionary<string, Delegate> database = new Dictionary<string, Delegate>();

        public bool HasFunction(string functionName) => database.ContainsKey(functionName);

        public void AddFunction(string functionName, Delegate function)
        {
            functionName = functionName.ToLower();

            if (!database.ContainsKey(functionName))
            {
                database.Add(functionName, function);
            }
            else
            {
                Debug.LogError($"Function {functionName} already exists in the database");
            }
        }
        public Delegate GetFunction(string functionName)
        {
            functionName = functionName.ToLower();

            if (!database.ContainsKey(functionName))
            {
                Debug.LogError($"Function {functionName} doesn't exist in the database");
                return null;
            }

            return database[functionName];
        }
    }
}
