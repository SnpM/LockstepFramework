Lockstep Framework
------------------------
The Lockstep Framework (LSF) is a framework designed for games that need lockstep simulations. It includes a deterministic 2D physics engine, pathfinding, behavior system, and more. LSF is integrated with Unity.

Special thanks to Liv Games (http://www.livgames.com), 360 Studio (http://www.360studio.me), and Thoopid (http://www.thoopid.com/) for supporting the development of Lockstep Framework. Also, thank you GladFox (https://github.com/GladFox) and the community for helping with development.

Created by John Pan (https://github.com/SnpM).

Features
__________
- Deterministic math library and simulation
- Custom 2D physics engine on the X-Z plane
- Behaviour system for both individual agents and globally
- Lockstep variables - know when and where desyncs happen
- Size-based pathfinding (big units won't get stuck in narrow gaps)
- Customizable database system
- Support for DarkRift and Photon Networking (Forge Networking support deprecated but let me know if you need it)

Quick Setup
-----------
1. Import the framework into a Unity project and open Lockstep-Framework/Example/ExampleScene
2. Set up the database and settings by navigating to the Lockstep/Database window or pressing Control - Shift - L.
3. In the Settings foldout of the database window, click Load and navigate to Lockstep-Framework/Example/ExampleDatabase/Example_Database.asset to load the preconfigured database for the example.
4. Play!

Note: The example only shows the basic functionality of the framework. Comprehensive examples will be added close to the end of core development.

**Tutorials and more**
---------
To find out more about Lockstep Framework, please explore the wiki (https://github.com/SnpM/LockstepFramework/wiki) and feel free to ask us questions.

Check out tutorial series here: https://github.com/SnpM/LockstepFramework/wiki/Tutorial-Series

Road Map
---------
- DOCUMENTATION - Tutorials for the major features of LS and descriptions of important methods and variables

License
--------
The MIT License (MIT)

Copyright (c) 2015 John Pan

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
