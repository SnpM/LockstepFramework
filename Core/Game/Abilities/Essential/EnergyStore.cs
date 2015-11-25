using UnityEngine;
using System.Collections;
using Lockstep;

namespace Lockstep
{
	public class EnergyStore : Ability
	{
		[SerializeField, FixedNumber(true)]
		private long
			_genRate;
		[SerializeField, FixedNumber]
		private long
			_maxEnergy;

		public long MaxEnergy { get { return _maxEnergy; } }

		private long _energyAmount;

		public long EnergyAmount {
			get { return _energyAmount;}
			private set {
				_energyAmount = value;
				setStatBuffer = 1;
			}
		}
		private int setStatBuffer;

		public bool IsCapped {
			get { return EnergyAmount == MaxEnergy;}
		}

		protected override void OnInitialize ()
		{
			EnergyAmount = MaxEnergy;
		}

		protected override void OnSimulate ()
		{
			Increase (_genRate);
		}

		public bool Use (long amount)
		{
			if (EnergyAmount >= amount) {
				EnergyAmount -= amount;
				return true;
			}
			return false;
		}
		protected override void OnVisualize ()
		{
			if (setStatBuffer > 0) setStatBuffer--;
			if (setStatBuffer == 0) {
				Agent.StatsBarer.SetFill (Lockstep.UI.StatType.Energy, (float)(EnergyAmount / (double)MaxEnergy));
			}

		}

		public void Increase (long amount)
		{
			if (EnergyAmount < MaxEnergy) {
				EnergyAmount += amount;
				if (EnergyAmount > MaxEnergy) EnergyAmount = MaxEnergy;
			}
		}
	}
}
