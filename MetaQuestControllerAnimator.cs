using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

// Add this script anywhere on the model you want to animate
// Add a Photon View component
// (If its the left hand then enable is left, if not then keep it off)
//====================================================================
// Setup:
// In the Inspector, click Add Element under "Finger Animations"
// Bone: Drag in the actual bone Transform (the bone/model for the finger/asset you want to animate)
// Target: Create the target pose:
// Duplicate the bone in the Hierarchy
// Rotate/move the duplicate to a curled position (I'd recomend rotating the actual finger bone then duping it if the thing you want to animate is just a bone for a higher model)
// Drag this duplicate into the Target field
// In the Inspector, disable the renderer or mark it as hidden
// Inputs: Add the inputs that curl this finger (e.g., Grip + Trigger)
// Smooth Speed: higher = snappier (I'd recomend 30)
// ^ [To do this on another hand] dupelicate this setup for the other hand but you have to remake all of the duped bones and re-apply everything, also toggle the "is left" (Left hand had "is left" on and right hand has it off)

public class MetaQuestControllerAnimator : MonoBehaviourPun, IPunObservable
{
    public enum FingerInput
    {
        Grip, Trigger, PrimaryButton, SecondaryButton, ThumbstickClick,
        ThumbstickTouch, TriggerTouch, PrimaryTouch, SecondaryTouch, MenuButton,
    }

    [System.Serializable]
    public class FingerAnimation
    {
        public Transform Bone;
        public Transform Target;
        public List<FingerInput> Inputs = new List<FingerInput> { FingerInput.Grip };
        public float SmoothSpeed = 10f;

        [HideInInspector] public Quaternion restRotation;
        [HideInInspector] public Vector3 restPosition;
        [HideInInspector] public float currentValue;
        [HideInInspector] public float networkTarget;
        [HideInInspector] public byte lastSentByte;
    }

    public bool IsLeftHand = false;
    public List<FingerAnimation> Animations = new List<FingerAnimation>();
    public float RemoteSmoothSpeed = 18f;
    public bool PreviewMode = false;
    public byte NetworkDeltaThreshold = 5;
    public int SendRateFrames = 3;

    [System.Serializable]
    public class InputPreview
    {
        public FingerInput Input;
        [Range(0f, 1f)] public float Value = 0f;
    }

    public List<InputPreview> InputPreviews = new List<InputPreview>();

    private InputDevice _device;
    private bool _deviceFound;
    private int _frameCounter;

    private void Start()
    {
        foreach (var anim in Animations)
        {
            if (anim.Bone == null) continue;
            anim.restRotation = anim.Bone.localRotation;
            anim.restPosition = anim.Bone.localPosition;
        }

        if (photonView.IsMine)
            TryFindDevice();
    }

    private void Update()
    {
        if (photonView.IsMine)
            OwnerUpdate();
        else
            RemoteUpdate();
    }

    private void OwnerUpdate()
    {
        if (!PreviewMode && !_deviceFound)
            TryFindDevice();

        foreach (var anim in Animations)
        {
            if (anim.Bone == null || anim.Target == null) continue;

            float rawInput = PreviewMode ? ReadPreviewMax(anim.Inputs) : ReadInputMax(anim.Inputs);
            anim.currentValue = Mathf.Lerp(anim.currentValue, rawInput, anim.SmoothSpeed * Time.deltaTime);
            ApplyBone(anim, anim.currentValue);
        }
    }

    private void RemoteUpdate()
    {
        foreach (var anim in Animations)
        {
            if (anim.Bone == null || anim.Target == null) continue;

            anim.currentValue = Mathf.Lerp(anim.currentValue, anim.networkTarget, RemoteSmoothSpeed * Time.deltaTime);
            ApplyBone(anim, anim.currentValue);
        }
    }

    private void ApplyBone(FingerAnimation anim, float t)
    {
        anim.Bone.localRotation = Quaternion.Slerp(anim.restRotation, anim.Target.localRotation, t);
        anim.Bone.localPosition = Vector3.Lerp(anim.restPosition, anim.Target.localPosition, t);
    }

    private float ReadInputMax(List<FingerInput> inputs)
    {
        float max = 0f;
        foreach (var input in inputs)
            max = Mathf.Max(max, ReadInput(input));
        return max;
    }

    private float ReadPreviewMax(List<FingerInput> inputs)
    {
        float max = 0f;
        foreach (var input in inputs)
            max = Mathf.Max(max, ReadPreview(input));
        return max;
    }

    private float ReadPreview(FingerInput input)
    {
        foreach (var preview in InputPreviews)
            if (preview.Input == input) return preview.Value;
        return 0f;
    }

    private float ReadInput(FingerInput input)
    {
        switch (input)
        {
            case FingerInput.Grip:            return Axis(CommonUsages.grip);
            case FingerInput.Trigger:         return Axis(CommonUsages.trigger);

            case FingerInput.PrimaryButton:   return Button(CommonUsages.primaryButton) ? 1f : 0f;
            case FingerInput.SecondaryButton: return Button(CommonUsages.secondaryButton) ? 1f : 0f;
            case FingerInput.ThumbstickClick: return Button(CommonUsages.primary2DAxisClick) ? 1f : 0f;
            case FingerInput.PrimaryTouch:    return Button(CommonUsages.primaryTouch) ? 1f : 0f;
            case FingerInput.SecondaryTouch:  return Button(CommonUsages.secondaryTouch) ? 1f : 0f;
            case FingerInput.MenuButton:      return Button(CommonUsages.menuButton) ? 1f : 0f;

            case FingerInput.ThumbstickTouch: return Button(CommonUsages.primary2DAxisTouch) ? 0.6f : 0f;
            case FingerInput.TriggerTouch:    return Button(CommonUsages.triggerButton) ? 0.5f : 0f;

            default: return 0f;
        }
    }

    private float Axis(InputFeatureUsage<float> usage)
    {
        float v;
        return _device.TryGetFeatureValue(usage, out v) ? v : 0f;
    }

    private bool Button(InputFeatureUsage<bool> usage)
    {
        bool v;
        return _device.TryGetFeatureValue(usage, out v) && v;
    }

    private void TryFindDevice()
    {
        var characteristics = IsLeftHand
            ? InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller
            : InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        _deviceFound = (devices.Count > 0);
        if (_deviceFound) _device = devices[0];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            _frameCounter++;
            
            if (_frameCounter < SendRateFrames)
            {
                stream.SendNext((byte)0);
                return;
            }
            
            _frameCounter = 0;

            byte bitMask = 0;
            List<byte> changedValues = new List<byte>();

            for (int i = 0; i < Animations.Count && i < 8; i++)
            {
                byte currentByte = (byte)Mathf.Round(Animations[i].currentValue * 255f);
                byte delta = (byte)Mathf.Abs(currentByte - Animations[i].lastSentByte);

                if (delta >= NetworkDeltaThreshold)
                {
                    bitMask |= (byte)(1 << i);
                    changedValues.Add(currentByte);
                    Animations[i].lastSentByte = currentByte;
                }
            }

            stream.SendNext(bitMask);
            foreach (byte val in changedValues)
                stream.SendNext(val);
        }
        else
        {
            byte bitMask = (byte)stream.ReceiveNext();

            for (int i = 0; i < Animations.Count && i < 8; i++)
            {
                if ((bitMask & (1 << i)) != 0 && stream.AvailableToRead)
                {
                    byte quantized = (byte)stream.ReceiveNext();
                    Animations[i].networkTarget = quantized / 255f;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        foreach (var anim in Animations)
        {
            if (anim.Bone == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(anim.Bone.position, 0.004f);

            if (anim.Target == null) continue;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(anim.Target.position, 0.004f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(anim.Bone.position, anim.Target.position);
        }
    }
#endif
}