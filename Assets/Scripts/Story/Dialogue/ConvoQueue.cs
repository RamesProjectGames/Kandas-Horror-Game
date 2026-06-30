using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class ConvoQueue
    {
        public Queue<Convo> convoQueue = new Queue<Convo>();

        public Convo top => convoQueue.Peek();

        public void Enqueue(Convo convo) => convoQueue.Enqueue(convo);
        public void EnqueuePrio(Convo convo)
        {
            Queue<Convo> queue = new Queue<Convo>();
            queue.Enqueue(convo);

            while (convoQueue.Count > 0)
            {
                queue.Enqueue(convoQueue.Dequeue());
            }

            convoQueue = queue;
        }

        public void Dequeue()
        {
            if (convoQueue.Count > 0)
            {
                convoQueue.Dequeue();
            }
        }

        public bool IsEmpty() => convoQueue.Count == 0;
    }
}
