using Un4seen.Bass;
using UnityEngine;

public class BassMusic : MonoBehaviour
{

    void Start()
    {
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_STEREO, System.IntPtr.Zero);

        int stream = Bass.BASS_MusicLoad("assets/resources/music/mainmusic.mo3", 0, 0, BASSFlag.BASS_SAMPLE_LOOP, 0);
        int stream_hihats = Bass.BASS_MusicLoad("assets/resources/music/mainmusic_hihats.mo3", 0, 0, BASSFlag.BASS_SAMPLE_LOOP, 0);
        Bass.BASS_ChannelPlay(stream, false);
        Bass.BASS_ChannelPlay(stream_hihats, false);
    }

    private void OnDestroy()
    {
        Bass.BASS_Free();
    }
}
