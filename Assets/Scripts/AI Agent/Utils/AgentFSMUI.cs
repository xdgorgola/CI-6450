using UnityEngine;
using UnityEngine.UI;

public class AgentFSMUI : MonoBehaviour
{
    public AIAgent agent;
    public Text _fsmName;
    public Text _stateName;
    public Text _inTransition;

    public void Update()
    {
        if (agent is null)
            return;

        if (agent.FSM is null)
            return;

        _fsmName.text = agent.FSM.Name;
        _stateName.text = agent.FSM.CurrentState.Name;
        _inTransition.text = agent.FSM.InTransition ? "In transition" : "In state";
    }
}
