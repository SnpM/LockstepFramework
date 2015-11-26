 using UnityEngine;
using System.Collections;
using Lockstep.Data;
using Lockstep;
using System;
public class BuildNode : CerealBehaviour, IMousable {

    Vector3 IMousable.WorldPosition {
        get {
            return transform.position;
        }
    }
    float IMousable.MousableRadius { get {return (float)(int)this._nodeType / 2;}}

	[SerializeField]
	private Vector2dHeight _localPos;

	[SerializeField]
	private AgentCode[] _spawnableAgents;
	public AgentCode[] SpawnableAgents {get {return _spawnableAgents;}}

    [SerializeField]
    private BuildNodeType _nodeType;
    public BuildNodeType NodeType {get{return _nodeType;}}

	public bool CanBuild {get {return Builder != null && Builder.Disabled == false && Occupied == false;}}
	public bool Occupied {get ;private set;}
	public ushort ID {get; private set;}
	private Build Builder;
	public LSAgent Agent {get {return Builder .IsNotNull () ? Builder.Agent : null;}}
	public void Setup (ushort id, Build build)
	{
		ID = id;
		Builder = build;
	}
	public void Initialize () {
		Occupied = false;
	}
	public void SendBuild (int creationIndex ) {
		Command com = new Command (InputCode.Build, Agent.Controller.ControllerID);
		com.Target = ID;
		com.Count = creationIndex;
		com.Select = new Selection();
		LSUtility.bufferAgents.FastClear ();
		LSUtility.bufferAgents.Add (Builder.Agent);
		com.Select.Serialize (LSUtility.bufferAgents);
		CommandManager.SendCommand (com);
	}

	public void Execute (Command com)
	{
		if (CanBuild == false) return;
		LSAgent agent = Agent.BuildChild (SpawnableAgents[com.Count], _localPos.ToOrientedVector2d (), _localPos.Height);
		agent.onDeactivation += OnChildDie;
		Occupied = true;
	}

    Action<LSAgent> _onChildDie;
    Action<LSAgent> _OnChildDie {get {return _onChildDie ?? (_onChildDie = OnChildDie);}}
	private void OnChildDie (LSAgent agent) {
		agent.onDeactivation -= OnChildDie;
		//Occupied = false;
	}
#if UNITY_EDITOR

    protected override void OnAfterSerialize  () {
		Vector3 localPos = transform.root.InverseTransformPoint (transform.position);
		_localPos = new Vector2dHeight(localPos);

		new UnityEditor.SerializedObject (this).ApplyModifiedProperties ();
	}
#endif
}
