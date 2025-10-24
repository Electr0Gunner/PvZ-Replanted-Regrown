using UnityEngine;

public class ReanimTester : MonoBehaviour
{
    public Reanimation reanim;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAnim(string name)
    {
        reanim.PlayReanim(name, ReanimLoopType.Loop, 0.3f, 12);
    }
}
