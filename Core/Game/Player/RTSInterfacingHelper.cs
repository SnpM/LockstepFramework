using UnityEngine;
using Lockstep.Data;

namespace Lockstep
{
	public class RTSInterfacingHelper : BehaviourHelper
	{
		public static GUIManager GUIManager;

		private static AbilityDataItem _currentInterfacer;

		/// <summary>
		/// The current ability to cast. Set this to a non-null value to automatically start the gathering process.
		/// </summary>
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
		//Helper function that takes in a type rather than AbilityDataItem to cast an ability
		public static void CastAbility<TAbility>() where TAbility : ActiveAbility
		{
			CurrentInterfacer = AbilityDataItem.FindInterfacer<TAbility>();
		}

		public static AbilityDataItem QuickPos;
		public static AbilityDataItem QuickTarget;

		private static bool _isGathering;
		public static bool IsGathering
		{
			get { return _isGathering; }
			private set
			{
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
			//Update the SelectionManager which handles box-selection.
			SelectionManager.Update();
			//Update RTSInterfacing, a useful tool that automatically generates useful data for user-interfacing
			RTSInterfacing.Visualize();

			if (IsGathering)
			{
				//We are currently gathering mouse information. The next click will trigger the command with the mouse position.
				//I.e. Press "T" to use the 'Psionic Storm' ability. Then left click on a position to activate it there.

				//Right click to cancel casting the abiility by setting IsGathering to false
				if (Input.GetMouseButtonDown(1))
				{
					IsGathering = false;
					return;
				}

				//If we left click to release the ability
				//Or if the ability we're activating requires no mouse-based information (i.e. CurrentInterfacer.InformationGather)
				//Trigger the ability
				if (Input.GetMouseButtonDown(0) || CurrentInterfacer.InformationGather == InformationGatherType.None)
				{
					ProcessInterfacer(CurrentInterfacer);
				}
			}
			else
			{
				//We are not gathering information. Instead, allow quickcasted abilities with the mouse. I.e. Right click to move or attack.
				if (Selector.MainSelectedAgent != null)
				{
					if (Input.GetMouseButtonDown(1))
					{
						if (RTSInterfacing.MousedAgent.IsNotNull() &&
							Selector.MainSelectedAgent.GetAbility<Scan>() != null)
						{
							//If the selected agent has Scan (the ability behind attacking) and the mouse is over an agent, send a target command - right clicking on a unit
							ProcessInterfacer((QuickTarget));
						}
						else
						{
							//If there is no agent under the mouse or the selected agent doesn't have Scan, send a Move command - right clicking on terrain
							ProcessInterfacer((QuickPos));
						}
					}
				}
			}
		}


		public static void ProcessInterfacer(AbilityDataItem facer)
		{
			Command com = RTSInterfacing.GetProcessInterfacer(facer);
			Send(com);
		}


		protected virtual void OnGUI()
		{
			if (_boxStyle == null) return;
			this.DrawBox(_boxStyle);
		}

		protected virtual void DrawBox(GUIStyle style)
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