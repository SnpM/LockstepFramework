using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep.UI;
using Lockstep.Data;

namespace Lockstep
{
    public class RTSInterfacingHelper : BehaviourHelper
    {
        public static GUIManager GUIManager;

        private static AbilityDataItem _currentInterfacer;

        public static AbilityDataItem CurrentInterfacer
        {
            get { return _currentInterfacer; }
            set
            {
                if (value.IsNotNull())
                {
                    IsGathering = true;
                }
                _currentInterfacer = value;
            }
        }

        public static AbilityDataItem QuickPos;
        public static AbilityDataItem QuickTarget;


        private static bool _isGathering;
        public static bool IsGathering {
            get {return _isGathering;}
            private set {
                SelectionManager.IsGathering = value;
                _isGathering = value;
            }
        }

        [SerializeField]
        private GUIStyle _boxStyle;

        static void Setup()
        {

            QuickPos = AbilityDataItem.FindInterfacer("Move");
            QuickTarget = AbilityDataItem.FindInterfacer("Scan");

            Setted = true;
        }

        static bool Setted = false;

        protected override void OnInitialize()
        {
            if (!Setted)
                Setup();
            SelectionManager.Initialize();

            RTSInterfacing.Initialize();
            IsGathering = false;
            CurrentInterfacer = null;
        }

        static Command curCom;

        protected override void OnVisualize()
        {
            SelectionManager.Update();


            RTSInterfacing.Visualize();

            if (IsGathering)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    IsGathering = false;
                    return;
                }

                if (Input.GetMouseButtonDown(0) || CurrentInterfacer.InformationGather == InformationGatherType.None)
                {
                    ProcessInterfacer(CurrentInterfacer);
                }
            } else
            {
                if (Selector.MainSelectedAgent != null)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        //LSAgent target;
                        if (RTSInterfacing.MousedAgent.IsNotNull() &&
						    Selector.MainSelectedAgent.GetAbility<Scan>() != null)
                        {
                            ProcessInterfacer((QuickTarget));
                        } else
                        {
                            ProcessInterfacer((QuickPos));
                        }
                    }
                }
            }
        }

        public static Command GetProcessInterfacer(AbilityDataItem facer)
        {

            if (facer == null)
            {
                Debug.LogError("Interfacer does not exist. Can't generate command.");
                return null;
            }
            switch (facer.InformationGather)
            {
                case InformationGatherType.Position:
                    curCom = new Command(facer.ListenInputID);
					curCom.Add<Vector2d>(RTSInterfacing.GetWorldPosD(Input.mousePosition));
                    break;
                case InformationGatherType.Target:
                    curCom = new Command(facer.ListenInputID);
                    if (RTSInterfacing.MousedAgent.IsNotNull())
                    {
                        curCom.SetData<DefaultData>(new DefaultData(DataType.UShort, RTSInterfacing.MousedAgent.LocalID));
                    }
                    break;
                case InformationGatherType.PositionOrTarget:
                    curCom = new Command(facer.ListenInputID);
                    if (RTSInterfacing.MousedAgent.IsNotNull())
                    {
                        curCom.Add<DefaultData>(new DefaultData(DataType.UShort, RTSInterfacing.MousedAgent.GlobalID));
                    } else
                    {
                        curCom.Add<Vector2d>(RTSInterfacing.GetWorldPosD(Input.mousePosition));
                    }
                    break;
                case InformationGatherType.None:
                    curCom = new Command(facer.ListenInputID);
                    break;
            }

            return curCom;
        }
        public static void ProcessInterfacer (AbilityDataItem facer) {
            Command com = GetProcessInterfacer (facer);
            Send(com);
        }


        protected virtual void OnGUI()
        {
            if (_boxStyle == null) return;
            this.DrawBox(_boxStyle);
        }

        protected virtual void DrawBox (GUIStyle style)
        {
            SelectionManager.DrawBox(style);
        }

        private static void Send(Command com)
        {
            IsGathering = false;
            PlayerManager.SendCommand(com);
        }
    }
}