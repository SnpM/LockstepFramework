using UnityEngine;
using System.Collections;
using Lockstep.UI;
using Lockstep.Data;

namespace Lockstep
{
    public static class InterfaceManager
    {
        public static GUIManager GUIManager;
        private static BuildNode _currentBuildTile;

        public static BuildNode CurrentBuildTile
        {
            get
            {
                return _currentBuildTile;
            }
            set
            {
                _currentBuildTile = value;
            }
        }

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

        public static void Setup()
        {
            QuickPos = AbilityInterfacer.FindInterfacer(Lockstep.Data.AbilityCode.Move);
            QuickTarget = AbilityInterfacer.FindInterfacer(Lockstep.Data.AbilityCode.Scan);
        }

        public static void Initialize()
        {
            Interfacing.Initialize();
            IsGathering = false;
            CurrentInterfacer = null;
        }

        static Command curCom;

        public static void Visualize()
        {
            if (CommandManager.sendType == SendState.None)
                return;
            Interfacing.Visualize();
            if (CurrentBuildTile .IsNotNull())
            {
                if (InputManager.GetInformationDown())
                {
                    if (GUIManager.CanInteract)
                    {
                        
                        GUIManager.InformationDown ();
                        CurrentBuildTile = null;
                    }
                }
            }

            BuildManager.Visualize ();

            if (IsGathering)
            {
                if (InputManager.GetQuickDown())
                {
                    IsGathering = false;
                    return;
                }

                if (InputManager.GetInformationDown() || CurrentInterfacer.InformationGather == InformationGatherType.None)
                {
                    ProcessInterfacer(CurrentInterfacer);
                }
            } else
            {
                if (Selector.MainSelectedAgent != null) {
                if (InputManager.GetQuickDown())
                {
                    LSAgent target;
                    if (Interfacing.MousedAgent.IsNotNull() &&
                        PlayerManager.GetAllegiance(Interfacing.MousedAgent) == AllegianceType.Enemy && 
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
                    curCom = new Command(facer.ListenInput);
                    curCom.Position = Interfacing.GetWorldPosD(Input.mousePosition);
                    break;
                case InformationGatherType.Target:
                    curCom = new Command(facer.ListenInput);
                    if (Interfacing.MousedAgent .IsNotNull())
                    {
                        curCom.Target = Interfacing.MousedAgent.LocalID;
                    }
                    break;
                case InformationGatherType.PositionOrTarget:
                    curCom = new Command(facer.ListenInput);
                    if (Interfacing.MousedAgent .IsNotNull())
                    {
                        curCom.Target = Interfacing.MousedAgent.GlobalID;
                    } else
                    {
                        curCom.Position = Interfacing.GetWorldPosD(Input.mousePosition);
                    }
                    break;
                case InformationGatherType.None:
                    curCom = new Command(facer.ListenInput);
                    break;
            }
            if (facer.MarkType != MarkerType.None)
            {
                Interfacing.ActivateMarkerOnMouse(facer.MarkType);
            }
            Send(curCom);
        }
        
        private static void Send(Command com)
        {
            IsGathering = false;
            PlayerManager.SendCommand(com);
        }
    }
}