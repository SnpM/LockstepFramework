using UnityEngine;
using System.Collections;
using Lockstep.UI;
using Lockstep.Data;

namespace Lockstep
{
    public class RTSInterfacingHelper : InterfacingHelper
    {
        public static GUIManager GUIManager;

        private static AbilityInterfacer _currentInterfacer;

        public static AbilityInterfacer CurrentInterfacer
        {
            get { return _currentInterfacer;}
            set
            {
                if (value .IsNotNull())
                {
                    IsGathering = true;
                }
                _currentInterfacer = value;
            }
        }

        private static AbilityInterfacer QuickPos;
        private static AbilityInterfacer QuickTarget;

        public static bool IsGathering { get; private set; }

        static void Setup()
        {
            QuickPos = AbilityInterfacer.FindInterfacer ("Move");
            QuickTarget = AbilityInterfacer.FindInterfacer("Scan");

            Setted = true;
        }
        static bool Setted = false;

        protected override void OnInitialize()
        {
            if (!Setted)
                Setup ();
            SelectionManager.Initialize();

            RTSInterfacing.Initialize();
            IsGathering = false;
            CurrentInterfacer = null;
        }

        static Command curCom;
        protected override void OnVisualize()
        {
            SelectionManager.Update();

            if (CommandManager.sendType == SendState.None)
                return;
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
                if (Selector.MainSelectedAgent != null) {
                    if (Input.GetMouseButtonDown(1))
                {
                    LSAgent target;
                    if (RTSInterfacing.MousedAgent.IsNotNull() &&
                        PlayerManager.GetAllegiance(RTSInterfacing.MousedAgent) == AllegianceType.Enemy && 
                        Selector.MainSelectedAgent.Scanner != null)
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

        private static void ProcessInterfacer(AbilityInterfacer facer)
        {
            switch (facer.InformationGather)
            {
                case InformationGatherType.Position:
                    curCom = new Command(facer.ListenInputID);
                    curCom.Add<Vector2d> ( RTSInterfacing.GetWorldPosD(Input.mousePosition));
                    break;
                case InformationGatherType.Target:
                    curCom = new Command(facer.ListenInputID);
                    if (RTSInterfacing.MousedAgent .IsNotNull())
                    {
                        curCom.SetData<DefaultData> (new DefaultData(DataType.UShort,RTSInterfacing.MousedAgent.LocalID));
                    }
                    break;
                case InformationGatherType.PositionOrTarget:
                    curCom = new Command(facer.ListenInputID);
                    if (RTSInterfacing.MousedAgent .IsNotNull())
                    {
                        curCom.Add<DefaultData> (new DefaultData(DataType.UShort,RTSInterfacing.MousedAgent.GlobalID));
                    } else
                    {
                        curCom.Add<Vector2d> (RTSInterfacing.GetWorldPosD(Input.mousePosition));
                    }
                    break;
                case InformationGatherType.None:
                    curCom = new Command(facer.ListenInputID);
                    break;
            }

            Send(curCom);
        }

        public void DrawGUI () {

        }
        protected virtual void OnDrawGUI () {
            SelectionManager.DrawBox(GUIStyle.none);
        }
        
        private static void Send(Command com)
        {
            IsGathering = false;
            PlayerManager.SendCommand(com);
        }
    }
}