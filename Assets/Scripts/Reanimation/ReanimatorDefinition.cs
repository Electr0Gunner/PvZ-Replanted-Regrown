using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReanimatorDefinition
{
    public ReanimatorDefinition()
    {
        mFPS = 12;
        mTrackCount = 0;
        mReanimAtlas = null;
        mTracks = new List<ReanimatorTrack>();
    }

    public void ExtractImages()
    {
        for (int i = 0; i < mTracks.Count; i++)
        {
            mTracks[i].ExtractImages();
        }
    }

    public List<ReanimatorTrack> mTracks;

    public short mTrackCount;

    public int mFPS;

    public ReanimAtlas mReanimAtlas;
}
