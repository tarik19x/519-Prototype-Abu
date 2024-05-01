using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    private bool isBPressed;
    public int counter = 0;
    [SerializeField] private AudioSource GR, RM;

    void Update()
    {
        if(isBPressed)
        {
            isBPressed = OVRInput.Get(OVRInput.Button.Two);
            if(!isBPressed)
            {
                counter++;
                counter %= 2;

                if(counter == 0) { GR.Play(); }
                else { RM.Play(); }
            }
        } else
        {
            isBPressed = OVRInput.Get(OVRInput.Button.Two);
        }
    }
}
