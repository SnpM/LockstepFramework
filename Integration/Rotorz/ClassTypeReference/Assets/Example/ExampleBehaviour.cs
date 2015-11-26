// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using TypeReferences;
using UnityEngine;

namespace Example {

	public class ExampleBehaviour : MonoBehaviour {

		[ClassImplements(typeof(IGreetingLogger))]
		public ClassTypeReference greetingLoggerType = typeof(DefaultGreetingLogger);

		private void Start() {
			if (greetingLoggerType.Type == null) {
				Debug.LogWarning("No greeting logger was specified.");
			}
			else {
				var greetingLogger = Activator.CreateInstance(greetingLoggerType) as IGreetingLogger;
				greetingLogger.LogGreeting();
			}
		}

	}

}
