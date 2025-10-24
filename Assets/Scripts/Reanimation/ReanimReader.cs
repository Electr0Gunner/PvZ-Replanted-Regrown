using System;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

public class ReanimReader
{
    private static ReanimatorTransform previous;

    public static float DEG_TO_RAD = 0.017453292f;

    internal enum ReanimOptimisationType
    {
        New,
        CopyPrevious,
        Placeholder
    }

    internal enum ReanimScaleType
    {
        NoScale,
        InvertAndScale,
        ScaleFromPC = 0xFF
    }
    public static ReanimatorDefinition Read(string data)
    {
        var fileText = data;
        if (!fileText.StartsWith("<reanim>"))
            fileText = "<reanim>" + fileText;
        if (!fileText.EndsWith("/<reanim>"))
            fileText = fileText + "</reanim>";


        XmlDocument reanimFile = new XmlDocument();
        reanimFile.LoadXml(fileText);

        XmlNode root = reanimFile.SelectSingleNode("reanim");

        ReanimatorDefinition reanimatorDefinition = new ReanimatorDefinition();
        reanimatorDefinition.mFPS = int.Parse(root["fps"].InnerText);
        foreach (XmlNode track in root.SelectNodes("track"))
        {
            reanimatorDefinition.mTrackCount++;
            ReanimatorTrack reanimatorTrack;
            ReadReanimTrack(track, ReanimScaleType.ScaleFromPC, out reanimatorTrack);
            reanimatorDefinition.mTracks.Add(reanimatorTrack);
        }    
        
        /*
        
        CustomContentReader customContentReader = new CustomContentReader(input);
        ReanimScaleType doScale = (ReanimScaleType)customContentReader.ReadByte();
        reanimatorDefinition.mFPS = customContentReader.ReadSingle();
        reanimatorDefinition.mTrackCount = (short)customContentReader.ReadInt32();
        reanimatorDefinition.mTracks = new ReanimatorTrack[reanimatorDefinition.mTrackCount];
        for (int i = 0; i < reanimatorDefinition.mTrackCount; i++)
        {
            ReanimatorTrack reanimatorTrack;
            ReadReanimTrack(customContentReader, doScale, out reanimatorTrack);
            reanimatorDefinition.mTracks[i] = reanimatorTrack;
        }*/
        return reanimatorDefinition;
    }
    
    private static void ReadReanimTrack(XmlNode trackNode, ReanimScaleType doScale, out ReanimatorTrack track)
    {
        XmlNode nameNode = trackNode["name"];
        string name = nameNode.InnerText;
        track = new ReanimatorTrack(name);
        bool isGround = false;//ReanimatorXnaHelpers.ReanimatorTrackNameToId(name) == Reanimation.ReanimTrackId__ground;
        int index = 0;
        foreach (XmlNode transformNode in trackNode.SelectNodes("t"))
        {
            ReanimatorTransform reanimatorTransform;
            ReadReanimTransform(transformNode, doScale, isGround, out reanimatorTransform);
            track.mTransforms.Add(reanimatorTransform);
            index++;
        }
        previous = null; //Make sure to not copy values from the previous track
    }

    private static ReanimatorTransform GetDefault()
    {
        ReanimatorTransform newReanimatorTransform = ReanimatorTransform.GetNewReanimatorTransform();
        newReanimatorTransform.mFontName = string.Empty;
        newReanimatorTransform.mImageName = string.Empty;
        newReanimatorTransform.mText = string.Empty;
        newReanimatorTransform.mAlpha = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mFrame = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mScaleX = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mScaleY = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mSkewX = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mSkewY = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mSkewXCos = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mSkewXSin = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mSkewYCos = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mSkewYSin = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mTransX = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        newReanimatorTransform.mTransY = Reanimation.DEFAULT_FIELD_PLACEHOLDER;
        return newReanimatorTransform;
    }

    private static void ReadReanimTransform(XmlNode transformNode, ReanimScaleType doScale, bool isGround, out ReanimatorTransform transform)
    {
        ReanimOptimisationType reanimOptimisationType = ReanimOptimisationType.New;
        if (reanimOptimisationType == ReanimOptimisationType.Placeholder)
        {
            transform = GetDefault();
        }
        else if (reanimOptimisationType == ReanimOptimisationType.CopyPrevious)
        {
            transform = previous;
        }
        else
        {
            transform = ReanimatorTransform.GetReanimatorTransformForLoadingThread();
            transform.mFontName = transformNode["font"]?.InnerText ?? (previous != null ? previous.mFontName : string.Empty);
            transform.mImageName = transformNode["i"]?.InnerText ?? (previous != null ? previous.mImageName : string.Empty);
            transform.mText = transformNode["text"]?.InnerText ?? (previous != null ? previous.mText : string.Empty);
            transform.mAlpha = ParseOrDefault(transformNode["a"], previous != null ? previous.mAlpha : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
            transform.mFrame = ParseOrDefault(transformNode["f"], previous != null ? previous.mFrame : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
            Debug.Log(transform.mFrame);
            transform.mScaleX = ParseOrDefault(transformNode["sx"], previous != null ? previous.mScaleX : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
            transform.mScaleY = ParseOrDefault(transformNode["sy"], previous != null ? previous.mScaleY : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
            transform.mSkewX = ParseOrDefault(transformNode["kx"], previous != null ? previous.mSkewX : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
            transform.mSkewY = ParseOrDefault(transformNode["ky"], previous != null ? previous.mSkewY : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
            transform.mTransX = ParseOrDefault(transformNode["x"], previous != null ? previous.mTransX : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
            transform.mTransY = ParseOrDefault(transformNode["y"], previous != null ? previous.mTransY : Reanimation.DEFAULT_FIELD_PLACEHOLDER);
        }
        previous = transform;
    }
    static float ParseOrDefault(XmlNode node, float fallback)
    {
        return node != null ? float.Parse(node.InnerText) : fallback;
    }
}
