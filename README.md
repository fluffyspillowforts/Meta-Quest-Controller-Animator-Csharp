# Meta-Quest-Controller-Animator-Csharp

A C# script for unity XR PUN 2 photon VR games where you can setup animations for Meta Quest (Quest 2, 3, and 3s) controllers (Best for Gtag fangames, where players have only three fingers.)

Intended use: Make finger animations on Gtag fangames.

Animation setup:

Add this script anywhere on the model you want to animate
Add a Photon View component
(If its the left hand then enable is left, if not then keep it off)
====================================================================
Setup:
In the Inspector, click Add Element under "Finger Animations"
Bone: Drag in the actual bone Transform (the bone/model for the finger/asset you want to animate)
Target: Create the target pose:
Duplicate the bone in the Hierarchy
Rotate/move the duplicate to a curled position (I'd recomend rotating the actual finger bone then duping it if the thing you want to animate is just a bone for a higher model)
Drag this duplicate into the Target field
In the Inspector, disable the renderer or mark it as hidden
Inputs: Add the inputs that curl this finger (e.g., Grip + Trigger)
Smooth Speed: higher = snappier (I'd recomend 30)
^ [To do this on another hand] dupelicate this setup for the other hand but you have to remake all of the duped bones and re-apply everything, also toggle the "is left" (Left hand had "is left" on and right hand has it off)

Photon syncing setup:

Local (Owner) Update
When you own the hand (photonView.IsMine == true):
Read input from your XR controller (Grip, Trigger, Touch, etc.)
Smooth the input value using SmoothSpeed
Apply the smoothed value to your bone rotations/positions
Send over network via OnPhotonSerializeView() ↓
Remote Update
When viewing another player's hand (photonView.IsMine == false):
Receive the current animation value from the network
Interpolate using RemoteSmoothSpeed (should be higher than SmoothSpeed to smooth latency)
Apply the interpolated value to the remote hand

(^ If you can't code or get it working, then provide claude ( https://claude.ai ) or chatgpt this guide and the script and provide it your photon scripts and it'll add them properly.)
