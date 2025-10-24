using Un4seen.Bass;
using UnityEngine;
using UnityEngine.Rendering;

public class BassMusic : MonoBehaviour
{
    private int mainMusicHandle = 0;
    private int mainMusicHihatsHandle = 0;
    public bool hordeActive = true;
    void Start()
    {
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_STEREO, System.IntPtr.Zero);

        mainMusicHandle = Bass.BASS_MusicLoad("assets/resources/music/mainmusic.mo3", 0, 0, BASSFlag.BASS_SAMPLE_LOOP, 0);
        mainMusicHihatsHandle = Bass.BASS_MusicLoad("assets/resources/music/mainmusic_hihats.mo3", 0, 0, BASSFlag.BASS_SAMPLE_LOOP, 0);
        Bass.BASS_ChannelPlay(mainMusicHandle, false);
        Bass.BASS_ChannelPlay(mainMusicHihatsHandle, false);
    }

    public void ToggleHorde()
    {
        hordeActive = !hordeActive;
    }


    public void Update()
    {
        float dummy = 0;
        int channels = 0;
        while (Bass.BASS_ChannelGetAttribute(mainMusicHandle, (BASSAttribute)((int)BASSAttribute.BASS_ATTRIB_MUSIC_VOL_CHAN + channels), ref dummy))
        {
            channels++;
            Bass.BASS_ChannelSetAttribute(mainMusicHandle, BASSAttribute.BASS_ATTRIB_MUSIC_VOL_CHAN + channels, hordeActive ? 1 : 0);
        }
        
    }

    private void OnDestroy()
    {
        Bass.BASS_Free();
    }
}
