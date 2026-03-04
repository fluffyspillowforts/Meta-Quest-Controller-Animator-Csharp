using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Add this script with wherever your PhotonVRPlayer script is

public class MetaQuestControllerAnimatorSync : MonoBehaviourPun, IPunObservable
{
    private List<MetaQuestControllerAnimator> _fingerAnimators = new List<MetaQuestControllerAnimator>();

    private void Start()
    {
        GetComponentsInChildren<MetaQuestControllerAnimator>(_fingerAnimators);

        if (_fingerAnimators.Count == 0)
        {
            Debug.LogWarning("No MetaQuestControllerAnimator found in children");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((byte)_fingerAnimators.Count);

            foreach (var animator in _fingerAnimators)
            {
                byte bitMask = 0;
                List<byte> changedValues = new List<byte>();

                for (int i = 0; i < animator.Animations.Count && i < 8; i++)
                {
                    byte currentByte = (byte)Mathf.Round(animator.Animations[i].currentValue * 255f);
                    byte lastByte = animator.Animations[i].lastSentByte;
                    byte delta = (byte)Mathf.Abs(currentByte - lastByte);

                    if (delta >= animator.NetworkDeltaThreshold)
                    {
                        bitMask |= (byte)(1 << i);
                        changedValues.Add(currentByte);
                        animator.Animations[i].lastSentByte = currentByte;
                    }
                }

                stream.SendNext(bitMask);
                foreach (byte val in changedValues)
                    stream.SendNext(val);
            }
        }
        else
        {
            byte animatorCount = (byte)stream.ReceiveNext();

            for (int a = 0; a < animatorCount && a < _fingerAnimators.Count; a++)
            {
                byte bitMask = (byte)stream.ReceiveNext();

                for (int i = 0; i < _fingerAnimators[a].Animations.Count && i < 8; i++)
                {
                    if ((bitMask & (1 << i)) != 0 && stream.AvailableToRead)
                    {
                        byte quantized = (byte)stream.ReceiveNext();
                        _fingerAnimators[a].Animations[i].networkTarget = quantized / 255f;
                    }
                }
            }
        }
    }
}