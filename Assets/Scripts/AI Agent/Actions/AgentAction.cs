using System;
using System.Collections;
using UnityEngine;

namespace IA.FSM
{
    public abstract class AgentAction
    {
        protected AIAgent _agent;

        public virtual void Init() { }
        public virtual IEnumerator ExecuteAction() { yield break; }
        public virtual void StopAction() { }


        protected AgentAction(AIAgent agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        }
    }
}