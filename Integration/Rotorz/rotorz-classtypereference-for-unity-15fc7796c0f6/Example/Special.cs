// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Example {

	public interface IGreetingLogger {

		void LogGreeting();

	}

	public class DefaultGreetingLogger : IGreetingLogger {

		public void LogGreeting() {
			Debug.Log("Hello, World!");
		}

	}

	public class AnotherGreetingLogger : IGreetingLogger {

		public void LogGreeting() {
			Debug.Log("Greetings!");
		}

	}

}
