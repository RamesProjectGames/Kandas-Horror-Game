using UnityEngine;
using Dialogue;
using System.Reflection;
using System;
using System.Linq;
using System.Collections;

namespace Dialogue.Functions
{
    public class DialogueFunctionManager : MonoBehaviour
    {
        public static DialogueFunctionManager Instance { get; private set; }
        public static Coroutine process = null;
        public static bool isRunningFunction => process != null;

        private FunctionsDatabase db;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            db = new FunctionsDatabase();

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] extTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(FuncDBExtension))).ToArray();

            foreach (Type extension in extTypes)
            {
                MethodInfo extendMethod = extension.GetMethod("Extend");
                extendMethod.Invoke(null, new object[] { db });
            }
        }

        public Coroutine Execute(string functionName, params string[] args)
        {
            Delegate function = db.GetFunction(functionName);

            if(function == null)
                return null;
            
            return StartFunction(functionName, function, args);
        }

        private Coroutine StartFunction(string functionName, Delegate function, string[] args)
        {
            StopCurrentFunction();

            process = StartCoroutine(RunningFunction(function, args));
            return process;
        }

        private void StopCurrentFunction()
        {
            if(process != null)
                StopCoroutine(process);

            process = null;
        }

        private IEnumerator RunningFunction(Delegate function, string[] args)
        {
            yield return WaitingForFunctionCompletion(function, args);
        }

        private IEnumerator WaitingForFunctionCompletion(Delegate function, string[] args)
        {
            //Function with no argument
            if (function is Action)
                function.DynamicInvoke();
            //Function with one argument
            else if (function is Action<string>)
                function.DynamicInvoke(args[0]);
            //Function with multiple arguments
            else if (function is Action<string[]>)
                function.DynamicInvoke((object)args);
            //Coroutine with no argument
            else if (function is Func<IEnumerator>)
                yield return ((Func<IEnumerator>) function)();
            //Coroutine with one argument
            else if (function is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)function)(args[0]);
            //Coroutine with multiple arguments
            else if (function is Func<string[], IEnumerator>)
                yield return ((Func<string[], IEnumerator>)function)(args);
        }
    }
}
