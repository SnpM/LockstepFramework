using UnityEngine;
using System.Collections;
using Lockstep;
using System.Collections.Generic;
using System;
public abstract class BehaviourHelper
{
	
	private static FastList<BehaviourHelper> behaviourHelpers = new FastList<BehaviourHelper> ();
	private static HashSet<Type> createdTypes = new HashSet<Type> ();
	
	public static void GlobalSetup ()
	{
		Register <MovementGroupHandler> ();
		Register <ScanGroupHandler> ();
		for (int i = 0; i < behaviourHelpers.Count; i++) {
			behaviourHelpers[i].Setup ();
		}
	}
    private static void Register<THelper> () where THelper:BehaviourHelper, new(){
        BehaviourHelper helper = new THelper() as BehaviourHelper;
		if (createdTypes.Contains (helper.GetType ())) return;
		createdTypes.Add (helper.GetType ());
		behaviourHelpers.Add (helper);
	}
	
	public static void GlobalInitialize ()
	{
		for (int i = 0; i < behaviourHelpers.Count; i++) {
			behaviourHelpers [i].Initialize ();
		}
	}
	
	public static void GlobalSimulate ()
	{
		for (int i = 0; i < behaviourHelpers.Count; i++) {
			behaviourHelpers [i].Simulate ();
		}
	}

    public static void GlobalLateSimulate () {
        for (int i = 0; i < behaviourHelpers.Count; i++) {
            behaviourHelpers[i].LateSimulate ();
        }
    }
	
	public static void GlobalVisualize ()
	{
		for (int i = 0; i < behaviourHelpers.Count; i++) {
			behaviourHelpers [i].Visualize ();
		}
	}
	
	public static void GlobalExecute (Command com)
	{
		for (int i = 0; i < behaviourHelpers.Count; i++) {
			if (behaviourHelpers [i].ListenInput == com.LeInput)
				behaviourHelpers [i].Execute (com); 
		}
	}
	
	public static void GlobalDeactivate ()
	{
		for (int i = 0; i < behaviourHelpers.Count; i++) {
			behaviourHelpers [i].Deactivate ();
		}
	}
	

	public abstract InputCode ListenInput { get;}
	
	private void Setup ()
	{
		OnSetup ();
	}
	
	protected virtual void OnSetup ()
	{
	}
	
	private void Initialize ()
	{
		OnInitialize ();
	}
	
	protected virtual void OnInitialize ()
	{
	}
	
	private void Simulate ()
	{
		OnSimulate ();
	}
	
	protected virtual void OnSimulate ()
	{
	}
    private void LateSimulate () {
        OnLateSimulate ();
    }
    protected virtual void OnLateSimulate (){

    }
	
	private void Visualize ()
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