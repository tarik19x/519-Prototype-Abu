using UnityEngine;

public class SfxHandler : MonoBehaviour
{
    [SerializeField] private VoiceManager VM;
    [SerializeField] private AudioClip GR, RB;
    int counter = 0;

    public void Play()
    {
        counter = VM.counter;
        if (counter == 0 ) SoundManager.Instance.PlaySound(GR);
        else { SoundManager.Instance.PlaySound(RB); }
    }
}