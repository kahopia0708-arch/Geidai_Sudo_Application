using System;

[Serializable]
public class SoundEffectSettings
{
    public string displayName;
    public string wavFileName;

    public float pitchCents;

    public int tonePresetIndex;
    public float noiseReductionAmount;

    public float lowPassCutoff;
    public float highPassCutoff;
    public float reverbLevel;
    public float echoDelay;
    public float echoDecayRatio;
    public float distortionLevel;
}