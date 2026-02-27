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
            db.AddFunction("Teleport", new Func<string[], IEnumerator>(TeleportObject));
            db.AddFunction("Move", new Func<string[], IEnumerator>(MoveObject));
            db.AddFunction("Poultry", new Action(PrintPoultry));
            //db.AddFunction("SetUpEnding", new Action());

        }

        private static IEnumerator TeleportObject(string[] args)
        {
            float x, y, z;
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: GameObject.Find("Player").transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: GameObject.Find("Player").transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: GameObject.Find("Player").transform.position.z);
            GameObject.Find(args[0]).TryGetComponent(out CharacterController cc);
            if (cc != null)
            {
                cc.enabled = false;
            }
            yield return new WaitForEndOfFrame();
            GameObject.Find(args[0]).TryGetComponent(out NavMeshAgent na);
            if (na != null)
            {
                na.Warp(new Vector3(x, y, z));
            }
            else
            {
                GameObject.Find(args[0]).transform.position = new Vector3(x, y, z);
            }
            yield return new WaitForEndOfFrame();
            if (cc != null)
            {
                cc.enabled = true;
            }
        }

        private static IEnumerator MoveObject(string[] args)
        {
            yield return new WaitForEndOfFrame();
            float x, y, z;
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: GameObject.Find("Player").transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: GameObject.Find("Player").transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: GameObject.Find("Player").transform.position.z);
            GameObject.Find(args[0]).TryGetComponent(out NavMeshAgent objectAgent);
            objectAgent?.SetDestination(new Vector3(x, y, z));
            yield return new WaitForEndOfFrame();
        }

        private static void PrintPoultry()
        {
            Debug.Log("Poultry printed from functions");
        }
    }
}
