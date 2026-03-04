# Meta-Quest-Controller-Animator-Csharp

A C# script for unity XR PUN 2 photon VR games where you can setup animations for Meta Quest (Quest 2, 3, and 3s) controllers (Best for Gtag fangames, where players have only three fingers.)<br>

Intended use: Make finger animations on Gtag fangames.<br>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br>
**Animation setup**<br>
*Add the MetaQuestControllerAnimator.cs script component anywhere on the model you want to animate Add a Photon View component (If its the left hand then enable is left, if not then keep it off)*<br>
Setup:<br>
In the Inspector, click Add Element under "Finger Animations"<br>
Bone: Drag in the actual bone Transform (the bone/model for the finger/asset you want to animate)<br>
Target: Create the target pose:<br>
Duplicate the bone in the Hierarchy<br>
Rotate/move the duplicate to a curled position (I'd recomend rotating the actual finger bone then duping it if the thing you want to animate is just a bone for a higher model)<br>
Drag this duplicate into the Target field<br>
In the Inspector, disable the renderer or mark it as hidden<br>
Inputs: Add the inputs that curl this finger (e.g., Grip + Trigger)<br>
Smooth Speed: higher = snappier (I'd recomend 30)<br>
^ [To do this on another hand] dupelicate this setup for the other hand but you have to remake all of the duped bones and re-apply everything, also toggle the "is left" (Left hand had "is left" on and right hand has it off)<br>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br>
<br>
<br>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br>
**Photon syncing setup:**<br>
Find your PhotonVRPlayer script on a game object (usally on your local player object)<br>
Add the MetaQuestControllerAnimatorSync.cs component on the game object that has the PhotonVRPlayer.cs component<br>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br>

(*If you have any issues or errors message me on discord, my username is fluffyspillowforts*)<br>

****Please credit me in your project if you use this!****
