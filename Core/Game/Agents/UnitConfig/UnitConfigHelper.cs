using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep.Data;
using System.Collections.Generic;
using System;
using System.Reflection;
namespace Lockstep
{
	public class UnitConfigHelper : BehaviourHelper
	{
		static bool setted = false;
		protected override void OnInitialize ()
		{
			if (!setted) {
			SetupConfigs ();
				setted = true;
			}
		}
		UnitConfigElementDataItem [] ConfigElementData;
		IUnitConfigDataItem [] ConfigData;

		Dictionary<string, UnitConfigElementDataItem> ConfigElementMap;
		void SetupConfigs ()
		{
			IUnitConfigDataProvider database;

			//todo guard
			if (LSDatabaseManager.TryGetDatabase (out database)) {
				ConfigElementData = database.UnitConfigElementData;
				ConfigElementMap = new Dictionary<string, UnitConfigElementDataItem> ();
				for (int i = 0; i < ConfigElementData.Length; i++) {
					var item = ConfigElementData [i];
					ConfigElementMap.Add (item.Name, item);
				}
				ConfigData = database.UnitConfigData;
				for (int i = 0; i < ConfigData.Length; i++) {
					IUnitConfigDataItem item = ConfigData [i];
					LSAgent agent = AgentController.GetAgentTemplate (item.Target);
					for (int j = 0; j < item.Stats.Length; j++) {
						Stat stat = item.Stats [j];
						//todo guard
						var element = ConfigElementMap [stat.ConfigElement];
						Component component = agent.GetComponent(element.ComponentType);
						SetField (component, element.Field, stat.Value);
					}
				}
			}
		}

		void SetField (object obj, string fieldName, long value)
		{
			Type objType = obj.GetType ();
			
			FieldInfo fieldInfo = objType.GetField (fieldName, (System.Reflection.BindingFlags)~0);

			if (fieldInfo.FieldType == typeof (long)) {
				fieldInfo.SetValue (obj, value);
			} else if (fieldInfo.FieldType == typeof (int)) {
				fieldInfo.SetValue (obj, FixedMath.RoundToInt (value));
			} else {
				Debug.Log (string.Format ("Field '{0}' of type '{1}' is not valid", fieldName, objType));
			}
		}
	}
}