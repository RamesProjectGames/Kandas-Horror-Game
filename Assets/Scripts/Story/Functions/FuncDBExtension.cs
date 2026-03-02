using Dialogue.Functions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Dialogue.Functions
{
    public abstract class FuncDBExtension
    {
        public static void Extend(FunctionsDatabase db)
        {

        }

        public static FunctionParams ConvertArgsToParams(string[] args) => new FunctionParams(args);
    }
}

namespace TestingPurposes
{
    public class TestFunction : FuncDBExtension
    {
        new public static void Extend(FunctionsDatabase db)
        {
            db.AddFunction("Teleport", new Action<string[]>(TeleportObject));
            db.AddFunction("Move", new Action<string[]>(MoveObject));
            db.AddFunction("Wait", new Func<string, IEnumerator>(Wait));
            db.AddFunction("Poultry", new Action(PrintPoultry));
            db.AddFunction("Objective", new Action<string>(CompleteObjective));
            //db.AddFunction("SetUpEnding", new Action());

        }

        private static IEnumerator Wait(string arg)
        {
            if(float.TryParse(arg, out float duration))
            {
                yield return new WaitForSeconds(duration);
            }
        }

        private static void TeleportObject(string[] args)
        {
            float x, y, z;
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: GameObject.Find("Player").transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: GameObject.Find("Player").transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: GameObject.Find("Player").transform.position.z);
            GameObject.Find(args[0]).TryGetComponent(out MovableObjects mo);
            if (mo != null)
            {
                mo.StartCoroutine(mo.Teleport(new Vector3(x, y, z)));
            }
            else
            {
                GameObject.Find(args[0]).transform.position = new Vector3(x, y, z);
            }
        }

        private static void MoveObject(string[] args)
        {
            float x, y, z;
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: GameObject.Find("Player").transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: GameObject.Find("Player").transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: GameObject.Find("Player").transform.position.z);
            GameObject.Find(args[0]).TryGetComponent(out MovableObjects mo);
            if (mo != null)
            {
                mo.StartCoroutine(mo.Move(new Vector3(x, y, z)));
            }
        }

        private static void PrintPoultry()
        {
            Debug.Log("Poultry printed from functions");
        }

        private static void CompleteObjective(string arg)
        {
            ObjectiveManager.Instance.CompleteObjective(arg);
        }
    }
}
