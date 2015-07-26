Lockstep Framework
------------------------
The Lockstep Framework (LSF) is a framework designed for games that especially need lockstep simulations. It includes a deterministic 2D physics engine, pathfinding, behavior system, and more. LSF is integrated with Unity but can be abstracted away.

Note: Under development by [John Pan](https://github.com/SnpM).

Quick Setup
-----------
Download the entire Lockstep Framework project and import it into your Unity assets.

Locate the Manager prefab in Core/Example/ and add that into your scene. This prefab comes with 3 components attached: LockstepManager, TestManager, and PlayerManager. LockstepManager contains settings for simulation and non-simulation related things that many other pieces of the LSF use.

TestManager is an example of the script you would write to interact with the LSF. It creates an AgentController, creates 256 agents under that AgentController, and adds the AgentController to PlayerManager for the player to interact with that controller. In FixedUpdate and Update, LockstepManager.Simulate () and LockstepManager.Visualize () are called, respectively. These distribute necessary information to the LSF for when to execute frames.

Click play and enjoy the lockstep simulation of group behaviors and collision responses.

Ability Pattern
----------------
Abilities are moddable behaviors that can be easily attached, detached, and moddified on prefab game objects. They follow the following pattern:
- The overridable Initialize() method is called when the agent the ability belongs to is created and initialized. It provides an argument that is the agent the ability belongs to. Because LSF uses object pooling, the Ability must also be reset in Initialize().
- Simulate() is called every single simulation frame.
- Deactivate() is called when the ability's agent is deactivated (i.e. killed). Note that Simulate() will not be called until after Initialize() is called again.

ActiveAbility Pattern
_____________________________
ActiveAbility inherits from Ability and includes all the patterns described above. In addition ActiveAbilitys can be interacted with by players through Commands.
- Execute () is called when a Command is received and activates the ability. This method provides an argument that is the Command responsible for the ability's activation.
- The ListenInput property is the input that the ability listens to. If a Command with the InputCode of ListenInput is received, Execute () is called on the ability.
 
Essential Abilities
-------------------
Currently, only movement with crowd behaviors is implemented. If you'd like to contribute, please explore Core/Game/Abities/Essential/ and help create more essential behaviors (i.e. Health, Energy, Attack, Stop).

License
--------
The MIT License (MIT)

Copyright (c) 2015 Inkhorn Games and John Pan

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
