using UnityEngine;
using System.Collections;
using Lockstep;
using System.Collections.Generic;
using System;

public abstract class BehaviourHelper : MonoBehaviour, IBehaviourHelper
{

    public BehaviourHelper()
    {

    }
    
    [SerializeField]
    private int priority = 0;

    private static FastList<BehaviourHelper> behaviourHelpers = new FastList<BehaviourHelper>();
    private static HashSet<Type> createdTypes = new HashSet<Type>();
    private HashSet<ushort> ListenInputs = new HashSet<ushort>();
    
    public virtual ushort ListenInput
    {
        get { return 0; }
    }

    protected void RegisterListenInput(ushort input)
    {
        if(input != 0)
        {
            ListenInputs.Add(input);
        }
    }

    protected void UnregisterListenInput(ushort input)
    {
        if (input != 0)
        {
            ListenInputs.Remove(input);
        }
    }

    public void Initialize()
    {
        RegisterListenInput(ListenInput);
        OnInitialize();
    }

    protected virtual void OnInitialize()
    {
    }

    public void LateInitialize()
    {
        this.OnLateInitialize();
    }

    protected virtual void OnLateInitialize()
    {

    }

    public void Simulate()
    {
        OnSimulate();
    }

    protected virtual void OnSimulate()
    {
    }

    public void LateSimulate()
    {
        OnLateSimulate();
    }

    protected virtual void OnLateSimulate()
    {

    }

    public void Visualize()
    {
        OnVisualize();
    }

    protected virtual void OnVisualize()
    {
    }

    /// <summary>
    /// Validation on whether this behaviour can execute with this command
    /// Used by the BehaviourHelperManager to check if this behaviour should execute
    /// </summary>
    /// <param name="com"></param>
    /// <returns></returns>
    public virtual bool CanExecuteOnCommand(Command com)
    {
        return ListenInputs.Contains(com.InputCode);
    }

    public void Execute(Command com)
    {
        OnExecute(com);
    }

    protected virtual void OnExecute(Command com)
    {
    }

    public void RawExecute (Command com) {
        OnRawExecute (com);
    }
    protected virtual void OnRawExecute (Command com) {

    }

    public void GameStart () {
        OnGameStart ();
    }

    protected virtual void OnGameStart () {

    }

    public void Deactivate()
    {
        OnDeactivate();
    }

    protected virtual void OnDeactivate()
    {

    }

    /// <summary>
    /// Priority allows this behaviour helper to run ahead or behind other helpers in case that's needed.
    /// It can be set to 0, which is the default priority
    /// </summary>
    public int Priority
    {
        get { return priority; }
        set
        {
            Priority = value;
        }
    }
}