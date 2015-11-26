using UnityEngine;
using System.Collections;
using System;
using Lockstep.Data;
namespace Lockstep
{
	public static class CodeHelper
	{
		public static AgentCode GetAgentCode (string codeName) {
			return (AgentCode) Enum.Parse (typeof (AgentCode), codeName);
		}
	}
}