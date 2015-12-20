using UnityEngine;
using System.Collections;
using Lockstep;
using System.Collections.Generic;
using System;
public abstract class BehaviourHelper : MonoBehaviour, IBehaviourHelper
{

    public BehaviourHelper () {

    }

	private static FastList<BehaviourHelper> behaviourHelpers = new FastList<BehaviourHelper> ();
	private static HashSet<Type> createdTypes = new HashSet<Type> ();
	

	public virtual InputCode ListenInput {
        get {return InputCode.None;}
    }

	public void Initialize ()
	{
		OnInitialize ();
	}
	
	protected virtual void OnInitialize ()
	{
	}
    public void LateInitialize () {
        this.OnLateInitialize();
    }
    protected virtual void OnLateInitialize () {

    }
    public void Simulate ()
	{
		OnSimulate ();
	}
	
	protected virtual void OnSimulate ()
	{
	}
    public void LateSimulate () {
        OnLateSimulate ();
    }
    protected virtual void OnLateSimulate (){

    }
	
    public void Visualize ()
	{
		OnVisualize ();
	}
	
	protected virtual void OnVisualize ()
	{
	}
	
    public void Execute (Command com)
	{
		OnExecute (com);
	}
	
	protected virtual void OnExecute (Command com)
	{
	}
	
    public void Deactivate ()
	{
			OnDeactivate ();
	}
	
	protected virtual void OnDeactivate ()
	{
		
		
	}
}