
==========================
USING THE RtsCamera SCRIPT
==========================

To use RtsCamera scripts:

1) Add the RtsCamera script to the Main Camera of 
   your scene either manually or through the 
   "Component/Camera-Control/RtsCamera" menu option.

2) Configure the camera script (and associated 
   keyboard and mouse scripts) through the Unity 
   Inspector window.

That's it.  Pretty simple.  

The camera, keyboard, and mouse script options are 
hopefully self-explanatory.

If you have any questions, feel free to email us:

        voxel.frog@gmail.com

==========================
KEYBOARD / MOUSE
==========================

The RtsCamera script requires (and adds automatically) 
the RtsCameraKeys and RtsCameraMouse scripts.

If you do not want either keyboard or mouse control, 
simply disable the unwanted script.

If you really don't want the disabled scripts on your
camera, simple remove the following line(s) from the
RtsCamera.cs file:

   [RequireComponent(typeof(RtsCameraMouse))]
   [RequireComponent(typeof(RtsCameraKeys))]


==========================
MISC
==========================

The RtsEffectsUpdator script is only a sample that 
you probably won't use.   

It's really just there as an example of how to update 
other Unity scripts or game objects that need to be 
"camera position aware".