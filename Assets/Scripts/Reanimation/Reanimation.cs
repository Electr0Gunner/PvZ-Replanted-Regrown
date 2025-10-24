using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public enum ReanimFlags //Prefix: PARAMREANIMFLAG
{
    NoAtlas,
    FastDrawInSwMode
}
public enum ReanimLoopType
{
    Loop,
    LoopFullLastFrame,
    PlayOnce,
    PlayOnceAndHold,
    PlayOnceFullLastFrame,
    PlayOnceFullLastFrameAndHold
}
public struct ReanimatorFrameTime
{
    public float mFraction;

    public short mAnimFrameBeforeInt;

    public short mAnimFrameAfterInt;
}

public class Reanimation : MonoBehaviour
{
    public static int RENDER_GROUP_HIDDEN = -1;

    public static int RENDER_GROUP_NORMAL = 0;

    public static float DEFAULT_FIELD_PLACEHOLDER = -99999f;

    public static bool Interpolate = true;

    private static bool DidClipIgnore = false;

    private static Matrix4x4 TempMatrix;

    public Matrix4x4 OverlayMatrix;

    public ReanimatorDefinition Definition;
    public float AnimTime;
    public float AnimRate;
    public int FrameStart = 0;
    public int FrameCount = 0;
    public int CurrentFrame = 0;
    public short FrameBasePose;
    public ReanimLoopType LoopType = ReanimLoopType.Loop;
    public int LoopCount = 0;
    private bool CanGetFrameTime = true; 
    private ReanimatorFrameTime FrameTime;
    public ReanimatorTrackInstance[] mTrackInstances = new ReanimatorTrackInstance[100];
    public float LastFrameTime;
    public bool Clip;
    public bool IsDead;
    public Color ColorOverride;

    public ReanimationCompiledAsset CompiledReanimation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Reset();
        Definition = CompiledReanimation.definition;
        IsDead = false;
        AnimRate = Definition.mFPS;
        LastFrameTime = -1f;
        if (Definition.mTrackCount != 0)
        {
            FrameCount = Definition.mTracks[0].mTransforms.Count;
            for (int i = 0; i < Definition.mTrackCount; i++)
            {
                ReanimatorTrackInstance newReanimatorTrackInstance = ReanimatorTrackInstance.GetNewReanimatorTrackInstance();
                mTrackInstances[i] = newReanimatorTrackInstance;
            }
            return;
        }
        FrameCount = 0;
    }

    public void DestoryReanim()
    {
        if (IsDead)
        {
            return;
        }
        //Active = false;
        IsDead = true;
        for (int i = 0; i < Definition.mTrackCount; i++)
        {
            ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
         //   GlobalMembersAttachment.AttachmentDie(ref reanimatorTrackInstance.mAttachmentID);
        }
    }

    public void OnDestroy()
    {
        DestoryReanim();
    }
    private void Reset()
    {
        for (int i = 0; i < mTrackInstances.Length; i++)
        {
            if (mTrackInstances[i] != null)
            {
                mTrackInstances[i].PrepareForReuse();
            }
            mTrackInstances[i] = null;
        }
        Clip = false;
        AnimTime = 0f;
        AnimRate = 12f;
        LastFrameTime = -1f;
        Definition = null;
        IsDead = false;
        FrameStart = 0;
        FrameCount = 0;
        FrameBasePose = -1;
        LoopCount = 0;
        OverlayMatrix = Matrix4x4.identity;
        //mRenderOrder = 0;
        // mReanimationHolder = null;
        // mFilterEffect = FilterEffectType.None;
        CanGetFrameTime = true;
    }
    void Update()
    {
        CanGetFrameTime = true;
        if (FrameCount == 0)
        {
            return;
        }
        if (IsDead)
        {
            return;
        }
        LastFrameTime = AnimTime;
        AnimTime += Time.deltaTime * AnimRate / FrameCount;
        if (AnimRate > 0f)
        {
            if (LoopType != ReanimLoopType.Loop)
            {
                if (LoopType != ReanimLoopType.LoopFullLastFrame)
                {
                    if (LoopType == ReanimLoopType.PlayOnce || LoopType == ReanimLoopType.PlayOnceFullLastFrame)
                    {
                        if (AnimTime >= 1f)
                        {
                            AnimTime = 1f;
                            LoopCount = 1;
                            IsDead = true;
                            goto IL_1C4;
                        }
                        goto IL_1C4;
                    }
                    else
                    {
                        if ((LoopType == ReanimLoopType.PlayOnceAndHold || LoopType == ReanimLoopType.PlayOnceFullLastFrameAndHold) && AnimTime >= 1f)
                        {
                            LoopCount = 1;
                            AnimTime = 1f;
                            goto IL_1C4;
                        }
                        goto IL_1C4;
                    }
                }
            }
            while (AnimTime >= 1f)
            {
                LoopCount++;
                AnimTime -= 1f;
            }
        }
        else
        {
            if (LoopType != ReanimLoopType.Loop)
            {
                if (LoopType != ReanimLoopType.LoopFullLastFrame)
                {
                    if (LoopType == ReanimLoopType.PlayOnce || LoopType == ReanimLoopType.PlayOnceFullLastFrame)
                    {
                        if (AnimTime < 0f)
                        {
                            AnimTime = 0f;
                            LoopCount = 1;
                            IsDead = true;
                            goto IL_1C4;
                        }
                        goto IL_1C4;
                    }
                    else
                    {
                        if ((LoopType == ReanimLoopType.PlayOnceAndHold || LoopType == ReanimLoopType.PlayOnceFullLastFrameAndHold) && AnimTime < 0f)
                        {
                            LoopCount = 1;
                            AnimTime = 0f;
                            goto IL_1C4;
                        }
                        goto IL_1C4;
                    }
                }
            }
            while (AnimTime < 0f)
            {
                LoopCount++;
                AnimTime += 1f;
            }
        }
        IL_1C4:
        int trackCount = Definition.mTrackCount;
        for (int i = 0; i < trackCount; i++)
        {
            ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
            if (reanimatorTrackInstance.mRenderGroup != RENDER_GROUP_HIDDEN)
            {
                if (reanimatorTrackInstance.mBlendCounter > 0)
                {
                    ReanimatorTrackInstance reanimatorTrackInstance2 = reanimatorTrackInstance;
                    reanimatorTrackInstance2.mBlendCounter -= 1;
                }
                if (reanimatorTrackInstance.mShakeOverride != 0f)
                {
                    
                    reanimatorTrackInstance.mShakeX = Random.Range(-reanimatorTrackInstance.mShakeOverride, reanimatorTrackInstance.mShakeOverride); ;
                    reanimatorTrackInstance.mShakeY = Random.Range(-reanimatorTrackInstance.mShakeOverride, reanimatorTrackInstance.mShakeOverride); ;
                }
                ReanimatorTrack reanimatorTrack = Definition.mTracks[i]; 
                // No attachments yet
                /*
                if (reanimatorTrack.IsAttacher)
                {
                    UpdateAttacherTrack(i);
                }
                if (reanimatorTrackInstance.mAttachmentID != null)
                {
                    GetAttachmentOverlayMatrix(i, out aOverlayMatrix);
                    GlobalMembersAttachment.AttachmentUpdateAndSetMatrix(ref reanimatorTrackInstance.mAttachmentID, ref aOverlayMatrix);
                }
                */
            }
        }

    }

    public void OnRenderObject()
    {
        CanGetFrameTime = true;
        DrawRenderGroup(RENDER_GROUP_NORMAL);
    }
    public void DrawRenderGroup(int theRenderGroup)
    {
        if (IsDead)
        {
            return;
        }
        for (int i = 0; i < Definition.mTrackCount; i++)
        {
            ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
            if (reanimatorTrackInstance.mRenderGroup == theRenderGroup)
            {
                bool flag = DrawTrack(i, theRenderGroup);/*
                if (reanimatorTrackInstance.mAttachmentID != null)
                {
                    Attachment attachmentID = reanimatorTrackInstance.mAttachmentID;
                    for (int j = 0; j < attachmentID.mNumEffects; j++)
                    {
                        AttachEffect attachEffect = attachmentID.mEffectArray[j];
                        if (attachEffect.mEffectType == EffectType.Reanim)
                        {
                            Reanimation reanimation = (Reanimation)attachEffect.mEffectID;
                            reanimation.mColorOverride = mColorOverride;
                            reanimation.mExtraAdditiveColor = mExtraAdditiveColor;
                            reanimation.mExtraOverlayColor = mExtraOverlayColor;
                        }
                    }
                    GlobalMembersAttachment.AttachmentDraw(reanimatorTrackInstance.mAttachmentID, g, !flag, false);
                }*/
            }
        }
    }

    private bool DrawTrack(int theTrackIndex, int theRenderGroup)
    {
        ReanimatorTransform reanimatorTransform;
        GetCurrentTransform(theTrackIndex, out reanimatorTransform, true);
        if (reanimatorTransform == null)
        {
            return false;
        }
        if (reanimatorTransform.mFrame < 0f)
        {
            reanimatorTransform.PrepareForReuse();
            return false;
        }
        int i = (int)(reanimatorTransform.mFrame + 0.5f);
        ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
        Color trackColor = reanimatorTrackInstance.mTrackColor;
        if (!reanimatorTrackInstance.mIgnoreColorOverride)
        {
            trackColor.r = ColorOverride.r * trackColor.r;
            trackColor.g = ColorOverride.g * trackColor.g;
            trackColor.b = ColorOverride.b * trackColor.b;
            trackColor.a = ColorOverride.a * trackColor.a;
        }
        
        int num = Mathf.Clamp((int)(reanimatorTransform.mAlpha * trackColor.a + 0.5f), 0, 1);
        if (num <= 0)
        {
            reanimatorTransform.PrepareForReuse();
       //     return false;
        }
        trackColor.a = num;
        Color theColor;

        
      /*  if (mEnableExtraAdditiveDraw)
        {
            theColor = new SexyColor(mExtraAdditiveColor.mRed, mExtraAdditiveColor.mGreen, mExtraAdditiveColor.mBlue, TodCommon.ColorComponentMultiply(mExtraAdditiveColor.mAlpha, num));
        }
        else*/
        {
            theColor = default; //I Cant do much past this rn
        }
        string imageName = reanimatorTransform.mImageName;
        Texture2D image = null;
        foreach (AtlasEntry entry in Definition.mReanimAtlas.entries)
        {
            if (entry.name.ToLower().Trim() == imageName.ToLower().Trim())
            {
                image = entry.texture;
                break;
            }
            image = entry.texture;
        }
        bool flag = false;
        float num2 = 0f;
        float num3 = 0f;
        if (image != null)
        {
            float num4 = image.width;
            float num5 = image.height;
            num2 = num4 * 0.5f;
            num3 = num5 * 0.5f;
        }
        else if (reanimatorTransform.mFont != null && !string.IsNullOrEmpty(reanimatorTransform.mText))
        {
            /*
            float num6 = reanimatorTransform.mFont.StringWidth(reanimatorTransform.mText);
            num2 = -num6 * 0.5f;
            num3 = reanimatorTransform.mFont.mAscent;
            */
        }
        else
        {
            if (!(Definition.mTracks[theTrackIndex].mName == "fullscreen"))
            {
                reanimatorTransform.PrepareForReuse();
                return false;
            }
            flag = true;
        }
        Rect trect = new Rect(0, 0, Screen.width, Screen.height); //TODO: ADD CLIPRECT
        Reanimation.DidClipIgnore = false;
        if (reanimatorTrackInstance.mIgnoreClipRect)
        {
            trect = new Rect(0, 0, Screen.width, Screen.height);
            Reanimation.DidClipIgnore = true;
        }
        float num7 = reanimatorTransform.mSkewXCos * reanimatorTransform.mScaleX;
        float num8 = -reanimatorTransform.mSkewXSin * reanimatorTransform.mScaleX;
        float num9 = reanimatorTransform.mSkewYSin * reanimatorTransform.mScaleY;
        float num10 = reanimatorTransform.mSkewYCos * reanimatorTransform.mScaleY;
        float num11 = num7 * num2 + num9 * num3 + reanimatorTransform.mTransX;
        float num12 = num8 * num2 + num10 * num3 + reanimatorTransform.mTransY;
        Reanimation.TempMatrix = new Matrix4x4
        {
            m00 = num7 * OverlayMatrix.m01 + num8 * OverlayMatrix.m11,
            m01 = num7 * OverlayMatrix.m02 + num8 * OverlayMatrix.m12,
            m02 = 0f,
            m03 = 0f,
            m10 = num9 * OverlayMatrix.m01 + num10 * OverlayMatrix.m11,
            m11 = num9 * OverlayMatrix.m02 + num10 * OverlayMatrix.m12,
            m12 = 0f,
            m13 = 0f,
            m20 = 0f,
            m21 = 0f,
            m22 = 1f,
            m23 = 0f,
            m30 = num11 * OverlayMatrix.m01 + num12 * OverlayMatrix.m11 + OverlayMatrix.m31+ reanimatorTrackInstance.mShakeX - 0.5f,
            m31 = num11 * OverlayMatrix.m02 + num12 * OverlayMatrix.m12 + OverlayMatrix.m32 + reanimatorTrackInstance.mShakeY - 0.5f,
            m32 = 0f,
            m33 = 1f
        };
        if (theTrackIndex == 9)
        {
            int num13 = 0;
            num13++;
        }
        if (image != null)
        {
            if (reanimatorTrackInstance.mImageOverride != null)
            {
                image = reanimatorTrackInstance.mImageOverride;
            }
            /*  if (mFilterEffect != FilterEffectType.None)
              {
                  image = FilterEffect.FilterEffectGetImage(image, mFilterEffect);
              }*/
            /*  while (i >= image.mNumCols)
              {
                  i -= image.mNumCols;
              }*/
            int num14 = 0;
            int celWidth = image.width;
            int celHeight = image.height;
            Rect theSrcRect = new Rect(celWidth * i, celHeight * num14, celWidth, celHeight);
            //    ReanimBltMatrix(g, image, ref Reanimation.tempMatrix, ref trect, trackColor, Graphics.DrawMode.DRAWMODE_NORMAL, theSrcRect, isHardwareClipRequired);
            //  if (mEnableExtraAdditiveDraw)
            //  {
            //      ReanimBltMatrix(g, image, ref Reanimation.tempMatrix, ref trect, theColor, Graphics.DrawMode.DRAWMODE_ADDITIVE, theSrcRect, isHardwareClipRequired);
            // }
            // TodCommon.OffsetForGraphicsTranslation = true;

            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = image;
            mat.color = trackColor;

            Mesh quad = GenerateQuad(theSrcRect, image);
            RenderParams rp = new RenderParams(mat);
            Graphics.RenderMesh(rp, quad, 0, Reanimation.TempMatrix);
        }
        else if (reanimatorTransform.mFont != null && !string.IsNullOrEmpty(reanimatorTransform.mText))
        {
            /*
            TodCommon.TodDrawStringMatrix(g, reanimatorTransform.mFont, Reanimation.tempMatrix, reanimatorTransform.mText, trackColor);
            if (mEnableExtraAdditiveDraw)
            {
                Graphics.DrawMode drawMode = g.mDrawMode;
                g.SetDrawMode(Graphics.DrawMode.DRAWMODE_ADDITIVE);
                TodCommon.TodDrawStringMatrix(g, reanimatorTransform.mFont, Reanimation.tempMatrix, reanimatorTransform.mText, theColor);
                g.SetDrawMode(drawMode);
            }
            */
        }
        else if (flag)
        {

            //  Color color = g.GetColor();
            //  g.SetColor(trackColor);
            // g.FillRect(-g.mTransX, -g.mTransY, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
            //  g.SetColor(color);
        }
        reanimatorTransform.PrepareForReuse();
        return true;
    }

    static Mesh GenerateQuad(Rect srcRect, Texture2D tex)
    {
        Mesh mesh = new Mesh();

        float width = srcRect.width;
        float height = srcRect.height;

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0)
        };

        int[] triangles = new int[6] { 0, 2, 1, 2, 3, 1 };

        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(srcRect.x / tex.width, srcRect.y / tex.height),
            new Vector2((srcRect.x + srcRect.width) / tex.width, srcRect.y / tex.height),
            new Vector2(srcRect.x / tex.width, (srcRect.y + srcRect.height) / tex.height),
            new Vector2((srcRect.x + srcRect.width) / tex.width, (srcRect.y + srcRect.height) / tex.height)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
    public void PlayReanim(string trackName, ReanimLoopType loopType, float blendTime, float animRate)
    {
        if (blendTime > 0)
        {
            StartBlend(blendTime);
        }
        if (animRate != 0f)
        {
            AnimRate = animRate;
        }
        LoopType = loopType;
        LoopCount = 0;
        SetFramesForLayer(trackName);
    }
    public void SetFramesForLayer(string theTrackName)
    {
        if (AnimRate >= 0f)
        {
            AnimTime = 0f;
        }
        else
        {
            AnimTime = 0.9999999f;
        }
        LastFrameTime = -1f;
        GetFramesForLayer(theTrackName, out FrameStart, out FrameCount);
    }


    public void GetFramesForLayer(string theTrackName, out int theFrameStart, out int theFrameCount)
    {
        if (Definition.mTrackCount == 0)
        {
            theFrameStart = 0;
            theFrameCount = 0;
            return;
        }
        int num = FindTrackIndex(theTrackName);
        ReanimatorTrack reanimatorTrack = Definition.mTracks[num];
        theFrameStart = 0;
        theFrameCount = 1;
        short num2;
        for (num2 = 0; num2 < reanimatorTrack.mTransforms.Count; num2 += 1)
        {
            ReanimatorTransform reanimatorTransform = reanimatorTrack.mTransforms[num2];
            if (reanimatorTransform.mFrame >= 0f)
            {
                theFrameStart = num2;
                break;
            }
        }
        for (int i = reanimatorTrack.mTransforms.Count - 1; i >= num2; i--)
        {
            ReanimatorTransform reanimatorTransform2 = reanimatorTrack.mTransforms[i];
            if (reanimatorTransform2.mFrame >= 0f)
            {
                theFrameCount = (short)(i - theFrameStart + 1);
                return;
            }
        }
    }
    public int FindTrackIndex(string theTrackName)
    {
        for (int i = 0; i < Definition.mTrackCount; i++)
        {
            string trackName = Definition.mTracks[i].mName;
            string text = theTrackName.ToLower();
            if (trackName == text)
            {
                return i;
            }
        }
        return 0;
    }

    public bool TrackExists(string theTrackName)
    {
        string text = theTrackName.ToLower();
        for (int i = 0; i < Definition.mTrackCount; i++)
        {
            string text2 = Definition.mTracks[i].mName.ToLower();
            if (text == text2)
            {
                return true;
            }
        }
        return false;
    }

    public void StartBlend(float theBlendTime)
    {
        CanGetFrameTime = true;
        for (int i = 0; i < Definition.mTrackCount; i++)
        {
            ReanimatorTransform reanimatorTransform;
            GetCurrentTransform(i, out reanimatorTransform, true);
            if (reanimatorTransform != null)
            {
                int num = Mathf.RoundToInt(reanimatorTransform.mFrame);
                if (num < 0)
                {
                    reanimatorTransform.PrepareForReuse();
                }
                else
                {
                    ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
                    if (reanimatorTrackInstance.mBlendTransform != null)
                    {
                        reanimatorTrackInstance.mBlendTransform.PrepareForReuse();
                    }
                    reanimatorTrackInstance.mBlendTransform = reanimatorTransform;
                    reanimatorTrackInstance.mBlendCounter = (byte)(theBlendTime / 3f);
                    reanimatorTrackInstance.mBlendTime = (byte)(theBlendTime / 3f);
                    reanimatorTrackInstance.mBlendTransform.mFont = null;
                    reanimatorTrackInstance.mBlendTransform.mText = string.Empty;
                    reanimatorTrackInstance.mBlendTransform.mImage = null;
                }
            }
        }
    }


    public void GetCurrentTransform(int theTrackIndex, out ReanimatorTransform aTransformCurrent, bool nullIfInvalidFrame)
    {
        ReanimatorFrameTime theFrameTime;
        GetFrameTime(out theFrameTime);
        GetTransformAtTime(theTrackIndex, out aTransformCurrent, theFrameTime, nullIfInvalidFrame);
        if (aTransformCurrent == null)
        {
            return;
        }
        ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
        int num = (int)(aTransformCurrent.mFrame + 0.5f);
        if (num >= 0 && reanimatorTrackInstance.mBlendCounter > 0)
        {
            float theBlendFactor = reanimatorTrackInstance.mBlendCounter / (float)reanimatorTrackInstance.mBlendTime;
            ReanimatorTransform reanimatorTransform;
            BlendTransform(out reanimatorTransform, ref aTransformCurrent, ref reanimatorTrackInstance.mBlendTransform, theBlendFactor);
            if (aTransformCurrent != null)
            {
                aTransformCurrent.PrepareForReuse();
            }
            aTransformCurrent = reanimatorTransform;
        }
    }

    public void GetTransformAtTime(int theTrackIndex, out ReanimatorTransform aTransform, ReanimatorFrameTime theFrameTime, bool nullIfInvalidFrame)
    {
        ReanimatorTrack reanimatorTrack = Definition.mTracks[theTrackIndex];
        ReanimatorTransform reanimatorTransform = reanimatorTrack.mTransforms[theFrameTime.mAnimFrameBeforeInt];
        ReanimatorTransform reanimatorTransform2 = reanimatorTrack.mTransforms[theFrameTime.mAnimFrameAfterInt];
        if (nullIfInvalidFrame && (reanimatorTransform.mFrame == -1f || (reanimatorTransform.mFrame != -1f && reanimatorTransform2.mFrame == -1f && theFrameTime.mFraction > 0f && mTrackInstances[theTrackIndex].mTruncateDisappearingFrames)))
        {
            aTransform = null;
            return;
        }
        float fraction = theFrameTime.mFraction;
        aTransform = ReanimatorTransform.GetNewReanimatorTransform();
        if (Interpolate)
        {
            aTransform.mTransX = reanimatorTransform.mTransX + fraction * (reanimatorTransform2.mTransX - reanimatorTransform.mTransX);
            aTransform.mTransY = reanimatorTransform.mTransY + fraction * (reanimatorTransform2.mTransY - reanimatorTransform.mTransY);
            aTransform.mSkewX = reanimatorTransform.mSkewX + fraction * (reanimatorTransform2.mSkewX - reanimatorTransform.mSkewX);
            aTransform.mSkewY = reanimatorTransform.mSkewY + fraction * (reanimatorTransform2.mSkewY - reanimatorTransform.mSkewY);
            aTransform.mScaleX = reanimatorTransform.mScaleX + fraction * (reanimatorTransform2.mScaleX - reanimatorTransform.mScaleX);
            aTransform.mScaleY = reanimatorTransform.mScaleY + fraction * (reanimatorTransform2.mScaleY - reanimatorTransform.mScaleY);
            aTransform.mAlpha = reanimatorTransform.mAlpha + fraction * (reanimatorTransform2.mAlpha - reanimatorTransform.mAlpha);
            aTransform.mSkewXCos = reanimatorTransform.mSkewXCos + fraction * (reanimatorTransform2.mSkewXCos - reanimatorTransform.mSkewXCos);
            aTransform.mSkewXSin = reanimatorTransform.mSkewXSin + fraction * (reanimatorTransform2.mSkewXSin - reanimatorTransform.mSkewXSin);
            aTransform.mSkewYCos = reanimatorTransform.mSkewYCos + fraction * (reanimatorTransform2.mSkewYCos - reanimatorTransform.mSkewYCos);
            aTransform.mSkewYSin = reanimatorTransform.mSkewYSin + fraction * (reanimatorTransform2.mSkewYSin - reanimatorTransform.mSkewYSin);
        }
        else
        {
            aTransform.mTransX = reanimatorTransform.mTransX;
            aTransform.mTransY = reanimatorTransform.mTransY;
            aTransform.mSkewX = reanimatorTransform.mSkewX;
            aTransform.mSkewY = reanimatorTransform.mSkewY;
            aTransform.mScaleX = reanimatorTransform.mScaleX;
            aTransform.mScaleY = reanimatorTransform.mScaleY;
            aTransform.mAlpha = reanimatorTransform.mAlpha;
            aTransform.mSkewXCos = reanimatorTransform.mSkewXCos;
            aTransform.mSkewXSin = reanimatorTransform.mSkewXSin;
            aTransform.mSkewYCos = reanimatorTransform.mSkewYCos;
            aTransform.mSkewYSin = reanimatorTransform.mSkewYSin;
        }
        aTransform.mImage = reanimatorTransform.mImage;
        aTransform.mFont = reanimatorTransform.mFont;
        aTransform.mText = reanimatorTransform.mText;
        if (reanimatorTransform.mFrame != -1f && reanimatorTransform2.mFrame == -1f && theFrameTime.mFraction > 0f && mTrackInstances[theTrackIndex].mTruncateDisappearingFrames)
        {
            aTransform.mFrame = -1f;
            return;
        }
        aTransform.mFrame = reanimatorTransform.mFrame;
    }
    public void GetFrameTime(out ReanimatorFrameTime theFrameTime)
    {
        if (!CanGetFrameTime)
        {
            theFrameTime = FrameTime;
            return;
        }
        CanGetFrameTime = false;
        theFrameTime = default(ReanimatorFrameTime);
        int num;
        if (LoopType == ReanimLoopType.PlayOnceFullLastFrame || LoopType == ReanimLoopType.LoopFullLastFrame || LoopType == ReanimLoopType.PlayOnceFullLastFrameAndHold)
        {
            num = FrameCount;
        }
        else
        {
            num = FrameCount - 1;
        }
        float num2 = FrameStart + num * AnimTime;
        float num3 = (int)num2;
        theFrameTime.mFraction = num2 - num3;
        theFrameTime.mAnimFrameBeforeInt = (short)(num3 + 0.5f);
        if (theFrameTime.mAnimFrameBeforeInt >= FrameStart + FrameCount - 1)
        {
            theFrameTime.mAnimFrameBeforeInt = (short)(FrameStart + FrameCount - 1);
            theFrameTime.mAnimFrameAfterInt = theFrameTime.mAnimFrameBeforeInt;
        }
        else
        {
            theFrameTime.mAnimFrameAfterInt = (short)(theFrameTime.mAnimFrameBeforeInt + 1);
        }
        FrameTime = theFrameTime;
    }


    public static void BlendTransform(out ReanimatorTransform theResult, ref ReanimatorTransform theTransform1, ref ReanimatorTransform theTransform2, float theBlendFactor)
    {
        theResult = ReanimatorTransform.GetNewReanimatorTransform();
        theResult.mTransX = Mathf.Lerp(theTransform1.mTransX, theTransform2.mTransX, theBlendFactor);
        theResult.mTransY = Mathf.Lerp(theTransform1.mTransY, theTransform2.mTransY, theBlendFactor);
        theResult.mScaleX = Mathf.Lerp(theTransform1.mScaleX, theTransform2.mScaleX, theBlendFactor);
        theResult.mScaleY = Mathf.Lerp(theTransform1.mScaleY, theTransform2.mScaleY, theBlendFactor);
        theResult.mAlpha = Mathf.Lerp(theTransform1.mAlpha, theTransform2.mAlpha, theBlendFactor);
        float num = theTransform2.mSkewX;
        float num2 = theTransform2.mSkewY;
        while (num > theTransform1.mSkewX + 180f)
        {
            num -= 360f;
            num = theTransform1.mSkewX;
        }
        while (num < theTransform1.mSkewX - 180f)
        {
            num += 360f;
            num = theTransform1.mSkewX;
        }
        while (num2 > theTransform1.mSkewY + 180f)
        {
            num2 -= 360f;
            num2 = theTransform1.mSkewY;
        }
        while (num2 < theTransform1.mSkewY - 180f)
        {
            num2 += 360f;
            num2 = theTransform1.mSkewY;
        }
        theResult.mSkewX = Mathf.Lerp(theTransform1.mSkewX, num, theBlendFactor);
        theResult.mSkewY = Mathf.Lerp(theTransform1.mSkewY, num2, theBlendFactor);
        theResult.mSkewXCos = Mathf.Cos(theResult.mSkewX * -Mathf.Deg2Rad);
        theResult.mSkewXSin = Mathf.Sin(theResult.mSkewX * -Mathf.Deg2Rad);
        theResult.mSkewYCos = Mathf.Cos(theResult.mSkewY * -Mathf.Deg2Rad);
        theResult.mSkewYSin = Mathf.Sin(theResult.mSkewY * -Mathf.Deg2Rad);
        theResult.mFrame = theTransform1.mFrame;
        theResult.mFont = theTransform1.mFont;
        theResult.mText = theTransform1.mText;
        theResult.mImage = theTransform1.mImage;
    }
}
