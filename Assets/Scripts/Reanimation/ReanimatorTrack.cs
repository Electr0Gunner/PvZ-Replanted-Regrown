using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
[System.Serializable]
public class ReanimatorTrack
{
    public string mName
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
            IsAttacher = name.StartsWith("attacher__");
        }
    }

    public ReanimatorTrack(string name)
    {
        mName = name;
        mTransforms = new List<ReanimatorTransform>();
    }

    public override string ToString()
    {
        return name;
    }

    public void ExtractImages()
    {
        for (int i = 0; i < mTransforms.Count; i++)
        {
            mTransforms[i].ExtractImages();
        }
    }

    private string name;

    public List<ReanimatorTransform> mTransforms;

    public bool IsAttacher;
}

public class ReanimatorTrackInstance
{
    public static void PreallocateMemory()
    {
        for (int i = 0; i < 10000; i++)
        {
            new ReanimatorTrackInstance().PrepareForReuse();
        }
    }

    public static ReanimatorTrackInstance GetNewReanimatorTrackInstance()
    {
        if (ReanimatorTrackInstance.unusedObjects.Count > 0)
        {
            return ReanimatorTrackInstance.unusedObjects.Pop();
        }
        return new ReanimatorTrackInstance();
    }

    public void PrepareForReuse()
    {
        Reset();
        ReanimatorTrackInstance.unusedObjects.Push(this);
    }

    public override string ToString()
    {
        return string.Format("Group: {0}", mRenderGroup);
    }

    private ReanimatorTrackInstance()
    {
        Reset();
    }

    private void Reset()
    {
        if (mBlendTransform != null)
        {
            mBlendTransform.PrepareForReuse();
        }
        mBlendTransform = null;
        mBlendCounter = 0;
        mBlendTime = 0;
        mShakeOverride = 0f;
        mShakeX = 0f;
        mShakeY = 0f;
        mAttachmentID = null;
        mRenderGroup = 0;
        mIgnoreClipRect = false;
        mTruncateDisappearingFrames = true;
        mImageOverride = null;
        mTrackColor = new Color(1, 1, 1);
        mIgnoreColorOverride = false;
        mIgnoreExtraAdditiveColor = false;
    }

    public byte mBlendCounter;

    public byte mBlendTime;

    public ReanimatorTransform mBlendTransform;

    public float mShakeOverride;

    public float mShakeX;

    public float mShakeY;

    public Attachment mAttachmentID;

    public Texture2D mImageOverride;

    public int mRenderGroup;

    public Color mTrackColor;

    public bool mIgnoreClipRect;

    public bool mTruncateDisappearingFrames;

    public bool mIgnoreColorOverride;

    public bool mIgnoreExtraAdditiveColor;

    private static Stack<ReanimatorTrackInstance> unusedObjects = new Stack<ReanimatorTrackInstance>();
}