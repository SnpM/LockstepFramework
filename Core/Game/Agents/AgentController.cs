using System;
using UnityEngine;
using Lockstep.Data;
using System.Collections.Generic;

namespace Lockstep
{
    public sealed class AgentController
    {
        
        public static string[] AgentCodes;
        private static readonly Dictionary<string,ushort> CodeIndexMap = new Dictionary<string, ushort>();
        
        public static Dictionary<string,FastStack<LSAgent>> CachedAgents;
        
        public static readonly bool[] GlobalAgentActive = new bool[MaxAgents * 4];
        public static readonly LSAgent[] GlobalAgents = new LSAgent[MaxAgents * 4];
        
        private static readonly FastStack<ushort> OpenGlobalIDs = new FastStack<ushort>();

        public static ushort PeakGlobalID { get; private set; }

        public const int MaxAgents = 512;
        private static readonly Dictionary<string,AgentInterfacer> CodeInterfacerMap = new Dictionary<string, AgentInterfacer>();

        public static void Setup()
        {
            
            AgentInterfacer[] agentInters = (LSDatabaseManager.CurrentDatabase as DefaultLSDatabase).AgentData;
            AgentCodes = new string[agentInters.Length];
            
            CachedAgents = new Dictionary<string,FastStack<LSAgent>>(agentInters.Length);
            
            for (int i = 0; i < agentInters.Length; i++)
            {
                AgentInterfacer interfacer = agentInters [i];
                string agentCode = interfacer.Name;
                AgentCodes [i] = agentCode;
                
                CachedAgents.Add(agentCode, new FastStack<LSAgent>(2));
                CodeInterfacerMap.Add(agentCode, interfacer);
                CodeIndexMap.Add(agentCode, (ushort)i);
            }
        }

        
        public static AgentInterfacer GetAgentInterfacer(string agentCode)
        {
            return AgentController.CodeInterfacerMap [agentCode];
        }

        public static ushort GetAgentCodeIndex(string agentCode)
        {
            return CodeIndexMap [agentCode];
            
        }

        public static string GetAgentCode(ushort id)
        {
            return AgentCodes [id];
        }

        public static bool IsValidAgentCode(string code)
        {
            return CodeInterfacerMap.ContainsKey(code);
        }

        public static void Initialize()
        {
            InstanceManagers.FastClear();
            GlobalAgentActive.Clear();
            OpenGlobalIDs.FastClear();
            PeakGlobalID = 0;
            foreach (FastStack<LSAgent> cache in CachedAgents.Values)
            {
                for (int j = 0; j < cache.Count; j++)
                {
                    cache.innerArray [j].SessionReset();
                }
            }
            
        }

        
        public static void Deactivate()
        {
            for (int i = 0; i < PeakGlobalID; i++)
            {
                if (GlobalAgentActive [i])
                {
                    DestroyAgent(GlobalAgents [i], true);
                }
            }
        }

        private static ushort GenerateGlobalID()
        {
            if (OpenGlobalIDs.Count > 0)
            {
                return OpenGlobalIDs.Pop();
            }
            return PeakGlobalID++;
        }

        public static bool TryGetAgentInstance(int globalID, out LSAgent returnAgent)
        {
            if (GlobalAgentActive [globalID])
            {
                returnAgent = GlobalAgents [globalID];
                return true;
            }
            returnAgent = null;
            return false;
        }

        
        
        public static void Simulate()
        {
            for (int iterator = 0; iterator < PeakGlobalID; iterator++)
            {
                if (GlobalAgentActive [iterator])
                {
                    GlobalAgents [iterator].Simulate();
                }
            }
        }

        public static void LateSimulate()
        {
            for (int i = 0; i < PeakGlobalID; i++)
            {
                if (GlobalAgentActive [i])
                    GlobalAgents [i].LateSimulate();
            }
        }

        public static void Visualize()
        {
            for (int iterator = 0; iterator < PeakGlobalID; iterator++)
            {
                if (GlobalAgentActive [iterator])
                {
                    GlobalAgents [iterator].Visualize();
                }
            }
        }

        public static void DestroyAgent(LSAgent agent, bool Immediate = false)
        {
            GlobalAgentActive [agent.GlobalID] = false;
            
            AgentController leController = agent.Controller;
            leController.LocalAgentActive [agent.LocalID] = false;
            leController.OpenLocalIDs.Add(agent.LocalID);
            OpenGlobalIDs.Add(agent.GlobalID);
            
            agent.Deactivate(Immediate);
            
        }

        public static void CacheAgent(LSAgent agent)
        {
            CachedAgents [agent.MyAgentCode].Add(agent);
        }

        private static void UpdateDiplomacy(AgentController newCont)
        {
            for (int i = 0; i < InstanceManagers.Count; i++)
            {
                InstanceManagers [i].DiplomacyFlags.AddAt(AllegianceType.Neutral, newCont.ControllerID);
            }
        }

        
        public static int GetStateHash()
        {
            int operationToggle = 0;
            int hash = LSUtility.PeekRandom(int.MaxValue);
            for (int i = 0; i < PeakGlobalID; i++)
            {
                if (GlobalAgentActive [i])
                {
                    LSAgent agent = GlobalAgents [i];
                    int n1 = agent.Body.Position.GetStateHash() + agent.Body.Rotation.GetStateHash();
                    switch (operationToggle)
                    {
                        case 0:
                            hash ^= n1;
                            break;
                        case 1:
                            hash += n1;
                            break;
                        default:
                            hash ^= n1 * 3;
                            break;
                    }
                    operationToggle++;
                    if (operationToggle >= 2)
                    {
                        operationToggle = 0;
                    }
                    if (agent.Body.IsNotNull())
                    {
                        hash ^= agent.Body.Position.GetStateHash();
                        hash ^= agent.Body.Rotation.GetStateHash();
                    }
                }
            }
            
            
            return hash;
        }

        public static FastList<AgentController> InstanceManagers = new FastList<AgentController>();
        public readonly FastBucket<LSAgent> SelectedAgents = new FastBucket<LSAgent>();

        public bool SelectionChanged { get; set; }

        public readonly LSAgent[] LocalAgents = new LSAgent[MaxAgents];
        public readonly bool[] LocalAgentActive = new bool[MaxAgents];

        public byte ControllerID { get; private set; }

        public ushort PeakLocalID { get; private set; }

        public int PlayerIndex { get; set; }

        public bool HasTeam { get; private set; }

        public Team MyTeam { get; private set; }

        private readonly FastBucket<AllegianceType> DiplomacyFlags = new FastBucket<AllegianceType>();
        private readonly FastStack<ushort> OpenLocalIDs = new FastStack<ushort>();

        internal AgentController()
        {
            OpenLocalIDs.FastClear();
            PeakLocalID = 0;
            ControllerID = (byte)InstanceManagers.Count;
            
            for (int i = 0; i < InstanceManagers.Count; i++)
            {
                DiplomacyFlags.AddAt(AllegianceType.Neutral, InstanceManagers [i].ControllerID);
            }
            
            InstanceManagers.Add(this);
            UpdateDiplomacy(this);
            DiplomacyFlags [ControllerID] = AllegianceType.Friendly;
        }

        public void AddToSelection(LSAgent agent)
        {
            SelectedAgents.Add(agent);
            SelectionChanged = true;
        }

        public void RemoveFromSelection(LSAgent agent)
        {
            SelectedAgents.Remove(agent);
            SelectionChanged = true;
        }

        private Selection previousSelection;

        public void Execute(Command com)
        {
            if (com.LeInput == InputCode.Spawn)
            {
                for (int i = 0; i < com.Count; i++)
                {
                    LSAgent agent = CreateAgent(GetAgentCode(com.Target), com.Position);
                }
                return;
            }
            
            
            if (com.HasGroupID)
            {
                /*var group = AgentGroupController.GetGroup(com.GroupID);
                if (group .IsNotNull ()) {
                    for (i = 0; i < group.Agents.Count; i++) {
                        group.Agents[i].Execute(com);
                    }
                }*/
            } else
            {
                if (com.HasSelect == false)
                    com.Select = previousSelection;
                previousSelection = com.Select;
            }
            
            BehaviourHelperManager.Execute(com);
            for (int i = 0; i < com.Select.selectedAgentLocalIDs.Count; i++)
            {
                ushort selectedAgentID = com.Select.selectedAgentLocalIDs [i];
                if (LocalAgentActive [selectedAgentID])
                {
                    LocalAgents [selectedAgentID].Execute(com);
                }
            }
        }

        public static Command GenerateSpawnCommand(AgentController cont, string agentCode, int count, Vector2d position)
        {
            Command com = new Command(InputCode.Spawn);
            com.ControllerID = cont.ControllerID;
            com.Position = position;
            com.Target = (ushort)AgentController.GetAgentCodeIndex(agentCode);
            com.Count = count;
            return com;
        }

        public LSAgent CreateAgent(string agentCode,
                                   Vector2d position = default(Vector2d))
        {
            Vector2d vec = new Vector2d(0, 1);
            if (!IsValidAgentCode(agentCode))
            {
                throw new System.ArgumentException(string.Format("Agent code '{0}' not found.", agentCode));
            }
            FastStack<LSAgent> cache = CachedAgents [agentCode];
            LSAgent curAgent = null;
            if (cache.IsNotNull() && cache.Count > 0)
            {
                curAgent = cache.Pop();
            } else
            {
                AgentInterfacer interfacer = AgentController.CodeInterfacerMap [agentCode];
                curAgent = GameObject.Instantiate(AgentController.CodeInterfacerMap [agentCode].Prefab).GetComponent<LSAgent>();
                curAgent.Setup(interfacer);
            }
            InitializeAgent(curAgent, position);
            return curAgent;
        }
        /*
        //Create agent from pre-existing template
        public LSAgent CreateAgent(GameObject agentObject,
                                   Vector2d position = default(Vector2d)) {
            curAgent = GameObject.Instantiate(agentObject).GetComponent<LSAgent>();
            curAgent.Setup(this, default(AgentCode));
            InitializeAgent (curAgent, position);

            return curAgent;
        }*/
        
        private void InitializeAgent(LSAgent agent,
                                      Vector2d position)
        {
            ushort localID = GenerateLocalID();
            LocalAgents [localID] = agent;
            LocalAgentActive [localID] = true;
            
            ushort globalID = GenerateGlobalID();
            GlobalAgentActive [globalID] = true;
            GlobalAgents [globalID] = agent;
            agent.Initialize(this, localID, globalID, position);
        }

        private ushort GenerateLocalID()
        {
            if (OpenLocalIDs.Count > 0)
            {
                return OpenLocalIDs.Pop();
            } else
            {
                return PeakLocalID++;
            }
        }

        public void SetAllegiance(AgentController otherController, AllegianceType allegianceType)
        {
            DiplomacyFlags [otherController.ControllerID] = allegianceType;
        }

        public AllegianceType GetAllegiance(AgentController otherController)
        {
            return HasTeam && otherController.HasTeam ? MyTeam.GetAllegiance(otherController) : DiplomacyFlags [otherController.ControllerID];
        }

        public AllegianceType GetAllegiance (byte controllerID) {
            if (HasTeam) {
                //TODO: Team allegiance
            }

            return DiplomacyFlags [controllerID];
        }

        public void JoinTeam(Team team)
        {
            MyTeam = team;
            HasTeam = true;
        }

        public void LeaveTeam()
        {
            HasTeam = false;
        }
    }

    [System.Flags]
    public enum AllegianceType : int
    {
        Neutral         = 1 << 1,
        Friendly        = 1 << 2,
        Enemy           = 1 << 3
    }
  
}