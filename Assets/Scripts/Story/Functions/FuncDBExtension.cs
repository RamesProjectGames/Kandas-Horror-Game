using Dialogue.Functions;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace Dialogue.Functions
{
    public abstract class FuncDBExtension
    {
        public static void Extend(FunctionsDatabase db)
        {

        }
    }
}

namespace TestingPurposes
{
    public class TestFunction : FuncDBExtension
    {
        new public static void Extend(FunctionsDatabase db)
        {
            db.AddFunction("Teleport", new Action<string, float, float, float>(TeleportObject));
            db.AddFunction("Move", new Action<string, float, float, float>(MoveObject));
            db.AddFunction("Poultry", new Action(PrintPoultry));
            //db.AddFunction("SetUpEnding", new Action());

        }

        private static void TeleportObject(string objectName, float x, float y, float z)
        {
            GameObject.Find(objectName).transform.position = new Vector3(x, y, z);
        }

        private static void MoveObject(string objectName, float x, float y, float z)
        {
            GameObject.Find(objectName).TryGetComponent(out NavMeshAgent objectAgent);
            objectAgent?.SetDestination(new Vector3(x, y, z));
        }

        private static void PrintPoultry()
        {
            Debug.Log("Poultry printed from functions");
        }
    }
}
