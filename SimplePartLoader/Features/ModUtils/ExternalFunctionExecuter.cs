using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class ExternalFunctionExecuter : MonoBehaviour
    {
        private bool CorroutineCooldown = false;
        private List<Action> actions = new List<Action>();

        public void AddFunction(Action action)
        {
            actions.Add(action);

            if(!CorroutineCooldown)
            {
                CorroutineCooldown = true;
                StartCoroutine(ExecuteDelayed());
            }
        }

        IEnumerator ExecuteDelayed()
        {
            List<Action> currentFunctions = actions.ToList();
            actions.Clear();

            yield return 0; // Wait 1 frame
            CorroutineCooldown = false;

            if (actions.Count != 0)
            {
                StartCoroutine(ExecuteDelayed()); // Make sure that if a function was added on the frame of wait, it gets still counted
            }

            foreach(Action action in currentFunctions)
            {
                try
                {
                    action.Invoke();
                }
                catch(Exception ex)
                {
                    CustomLogger.AddLine("DelayExecuteFunc", ex);
                }
            }
        }
    }
}
