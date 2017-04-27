using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;
using System.Collections.Generic;
using Lockstep.Data;
namespace Lockstep
{
    public class DefaultSaver : EnvironmentSaver
    {
        [SerializeField]
        private EnvironmentBodyInfo[] _environmentBodies;
        public EnvironmentBodyInfo[] EnvironmentBodies {get {return _environmentBodies;}}
        [SerializeField]
        private EnvironmentObject[] _environmentObjects;
        public EnvironmentObject[] EnvironmentObjects {get {return _environmentObjects;}}

		[SerializeField]
		private EnvironmentAgentInfo [] _environmentAgents;
		public EnvironmentAgentInfo [] EnvironmentAgents {
			get {
				return _environmentAgents;
			}
		}

        protected override void OnSave () {
            SaveBodies ();
            SaveObjects ();
			SaveAgents ();

        }

        protected override void OnApply () {
            foreach (EnvironmentObject obj in EnvironmentObjects) {
                obj.Initialize();
            }
        }

        protected override void OnLateApply()
        {
            foreach (EnvironmentBodyInfo info in EnvironmentBodies) {
                info.Body.Initialize(info.Position,info.Rotation);
            }

            foreach (EnvironmentObject obj in EnvironmentObjects) {
                obj.LateInitialize();
            }
			foreach (var agentInfo in EnvironmentAgents) {
				AgentController.DefaultController.AddAgent (agentInfo.Agent);
				agentInfo.Agent.Setup (AgentController.GetAgentInterfacer (agentInfo.AgentCode));
				agentInfo.Agent.Initialize (agentInfo.Position.ToVector2d(), agentInfo.Rotation);
				agentInfo.Agent.Body.HeightPos = agentInfo.Position.z;
			}
        }

        void SaveBodies () {
            UnityLSBody[] allBodies = GameObject.FindObjectsOfType<UnityLSBody> ();
            FastList<EnvironmentBodyInfo> bodiesBuffer = new FastList<EnvironmentBodyInfo>();
            foreach (UnityLSBody body in allBodies) {
                if (IsAgent(body)) continue;
                Vector3d pos = new Vector3d(body.transform.position);
                Vector2d rot = Vector2d.CreateRotation(body.transform.eulerAngles.y * Mathf.Deg2Rad);
                EnvironmentBodyInfo bodyInfo = new EnvironmentBodyInfo(
                    body,
                    pos,
                    rot
                );
                bodiesBuffer.Add(bodyInfo);
            }

            _environmentBodies = bodiesBuffer.ToArray();
        }
        void SaveObjects () {
            EnvironmentObject[] allObjects = GameObject.FindObjectsOfType<EnvironmentObject> ();
            FastList<EnvironmentObject> objectBuffer = new FastList<EnvironmentObject>();

            foreach (EnvironmentObject obj in allObjects) {
                if (IsAgent(obj)) continue;
                objectBuffer.Add(obj);
            }
            _environmentObjects = objectBuffer.ToArray();
			for (int i = 0; i < _environmentObjects.Length; i++)
			{
				_environmentObjects[i].Save();
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(_environmentObjects[i]);
#endif
			}
        }

		void SaveAgents ()
		{
			#if UNITY_EDITOR
			LSAgent [] allAgents = GameObject.FindObjectsOfType<LSAgent> ();
			FastList<EnvironmentAgentInfo> agentsBuffer = new FastList<EnvironmentAgentInfo> ();
			
			IAgentDataProvider database;

			if (LSDatabaseManager.TryGetDatabase<IAgentDataProvider> (out database)) {
				var agentData = database.AgentData;
				Dictionary<GameObject, string> prefabCodeMap = new Dictionary<GameObject, string> ();
				foreach (var item in agentData) {
					prefabCodeMap.Add (item.GetAgent ().gameObject, item.Name);
				}
				foreach (var agent in allAgents) {
					GameObject prefab = UnityEditor.PrefabUtility.GetPrefabParent (agent.gameObject) as GameObject;
					string agentCode;
					if (prefabCodeMap.TryGetValue (prefab, out agentCode)) {
					
						Vector3d pos = new Vector3d (agent.transform.position);
						Vector2d rot = Vector2d.CreateRotation (agent.transform.eulerAngles.y * Mathf.Deg2Rad);
						EnvironmentAgentInfo agentInfo = new EnvironmentAgentInfo (
							agentCode,
							agent,
							pos,
							rot
						);
						agentsBuffer.Add (agentInfo);
					} else {
						Debug.LogError (agent + " does not exist in 'Agents' database");
					}
				}
				this._environmentAgents = agentsBuffer.ToArray ();
			} else {
				Debug.LogError ("No database");
			}
			#endif
		}
        static bool IsAgent (object obj) {
            MonoBehaviour mb = obj as MonoBehaviour;
            if (mb.IsNull()) return false;
            return mb.GetComponent<LSAgent>().IsNotNull();
        }

    }
}