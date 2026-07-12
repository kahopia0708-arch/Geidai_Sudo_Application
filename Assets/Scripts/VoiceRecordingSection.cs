using UnityEngine;

public class VoiceRecordingSection : MonoBehaviour
{
    [Header("Recording")]
    [SerializeField] private int maxRecordSeconds = 10;
    [SerializeField] private int frequency = 44100;

    [Header("Playback")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Effects")]
    [SerializeField] private AudioLowPassFilter lowPassFilter;
    [SerializeField] private AudioHighPassFilter highPassFilter;
    [SerializeField] private AudioReverbFilter reverbFilter;
    [SerializeField] private AudioEchoFilter echoFilter;
    [SerializeField] private AudioDistortionFilter distortionFilter;

    [Header("Current Effect Values")]
    [SerializeField] private float pitchCents = 0f;
    [SerializeField] private int tonePresetIndex = 1;
    [SerializeField] private float noiseReductionAmount = 0f;
    [SerializeField] private float lowPassCutoff = 14000f;
    [SerializeField] private float highPassCutoff = 300f;
    [SerializeField] private float reverbLevel = -10000f;
    [SerializeField] private float echoDelay = 300f;
    [SerializeField] private float echoDecayRatio = 0.5f;
    [SerializeField] private float distortionLevel = 0.05f;

    [Header("Bypass")]
    [SerializeField] private bool pitchEnabled = true;
    [SerializeField] private bool toneEnabled = true;
    [SerializeField] private bool noiseReductionEnabled = true;
    [SerializeField] private bool reverbEnabled = true;
    [SerializeField] private bool echoEnabled = false;
    [SerializeField] private bool allEffectsEnabled = true;

    private AudioClip recordedClip;
    private string microphoneDevice;
    private bool hasRecorded;

    private void Awake()
    {
        PrepareAudioComponents();
        ApplyTonePreset(tonePresetIndex);
        ApplyNoiseReduction();
        ApplyAllEffectValues();
    }

    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            Debug.Log("使用するマイク: " + microphoneDevice);
        }
        else
        {
            Debug.LogWarning("マイクが見つかりません。");
        }
    }

    public void ToggleRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogWarning("使用できるマイクがありません。");
            return;
        }

        if (Microphone.IsRecording(microphoneDevice))
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    public void StartRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogWarning("使用できるマイクがありません。");
            return;
        }

        if (Microphone.IsRecording(microphoneDevice))
        {
            Debug.LogWarning("すでに録音中です。");
            return;
        }

        StopPlayback();

        hasRecorded = false;
        recordedClip = Microphone.Start(microphoneDevice, false, maxRecordSeconds, frequency);

        Debug.Log("録音開始");
    }

    public void StopRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogWarning("使用できるマイクがありません。");
            return;
        }

        if (!Microphone.IsRecording(microphoneDevice))
        {
            Debug.LogWarning("録音中ではありません。");
            return;
        }

        Microphone.End(microphoneDevice);

        if (recordedClip == null)
        {
            Debug.LogWarning("録音データがありません。");
            return;
        }

        hasRecorded = true;
        audioSource.clip = recordedClip;

        Debug.Log("録音停止");
    }

    public void PlayRecordedSound()
    {
        if (!hasRecorded || recordedClip == null)
        {
            Debug.LogWarning("再生できる録音データがありません。");
            return;
        }

        audioSource.clip = recordedClip;
        ApplyAllEffectValues();
        audioSource.Play();

        Debug.Log("録音音声を再生");
    }

    public void StopPlayback()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void SaveRecordedSound()
    {
        if (!hasRecorded || recordedClip == null)
        {
            Debug.LogWarning("保存できる録音データがありません。");
            return;
        }

        SoundEffectSettings settings = new SoundEffectSettings
        {
            pitchCents = pitchCents,
            tonePresetIndex = tonePresetIndex,
            noiseReductionAmount = noiseReductionAmount,
            lowPassCutoff = lowPassCutoff,
            highPassCutoff = highPassCutoff,
            reverbLevel = reverbLevel,
            echoDelay = echoDelay,
            echoDecayRatio = echoDecayRatio,
            distortionLevel = distortionLevel
        };

        MySoundCollectionStorage.SaveSoundWithSettings(recordedClip, settings);

        Debug.Log("録音音声と加工設定を保存しました。");
    }

    public void SetPitchCents(float value)
    {
        pitchCents = value;
        ApplyPitch();
    }

    public void SetTonePreset(float value)
    {
        int presetIndex = Mathf.RoundToInt(value);
        ApplyTonePreset(presetIndex);
        ApplyNoiseReduction();
        ApplyToneFilters();
    }

    public void SetNoiseReduction(float value)
    {
        noiseReductionAmount = Mathf.Clamp01(value);
        ApplyTonePreset(tonePresetIndex);
        ApplyNoiseReduction();
        ApplyToneFilters();

        Debug.Log("ノイズリダクション: " + noiseReductionAmount);
    }

    public void SetReverbLevel(float value)
    {
        reverbLevel = value;
        ApplyReverb();
    }

    public void SetEchoDelay(float value)
    {
        echoDelay = value;
        ApplyEcho();
    }

    public void SetEchoDecayRatio(float value)
    {
        echoDecayRatio = value;
        ApplyEcho();
    }

    public void TogglePitchEffect()
    {
        pitchEnabled = !pitchEnabled;
        ApplyPitch();

        Debug.Log("Pitch: " + (pitchEnabled ? "ON" : "OFF"));
    }

    public void ToggleToneEffect()
    {
        toneEnabled = !toneEnabled;
        ApplyToneFilters();

        Debug.Log("Tone: " + (toneEnabled ? "ON" : "OFF"));
    }

    public void ToggleNoiseReductionEffect()
    {
        noiseReductionEnabled = !noiseReductionEnabled;

        ApplyTonePreset(tonePresetIndex);
        ApplyNoiseReduction();
        ApplyToneFilters();

        Debug.Log("Noise Reduction: " + (noiseReductionEnabled ? "ON" : "OFF"));
    }

    public void ToggleReverbEffect()
    {
        reverbEnabled = !reverbEnabled;
        ApplyReverb();

        Debug.Log("Reverb: " + (reverbEnabled ? "ON" : "OFF"));
    }

    public void ToggleEchoEffect()
    {
        echoEnabled = !echoEnabled;
        ApplyEcho();

        Debug.Log("Echo: " + (echoEnabled ? "ON" : "OFF"));
    }

    public void ToggleAllEffects()
    {
        allEffectsEnabled = !allEffectsEnabled;
        ApplyAllEffectValues();

        Debug.Log("All Effects: " + (allEffectsEnabled ? "ON" : "OFF"));
    }

    public void ResetEffects()
    {
        pitchCents = 0f;
        tonePresetIndex = 1;
        noiseReductionAmount = 0f;
        reverbLevel = -10000f;
        echoDelay = 300f;
        echoDecayRatio = 0.5f;

        pitchEnabled = true;
        toneEnabled = true;
        noiseReductionEnabled = true;
        reverbEnabled = true;
        echoEnabled = false;
        allEffectsEnabled = true;

        ApplyTonePreset(tonePresetIndex);
        ApplyNoiseReduction();
        ApplyAllEffectValues();

        Debug.Log("エフェクトをリセットしました。");
    }

    public bool HasRecordedSound()
    {
        return hasRecorded && recordedClip != null;
    }

    public bool IsRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            return false;
        }

        return Microphone.IsRecording(microphoneDevice);
    }

    private void ApplyTonePreset(int presetIndex)
    {
        tonePresetIndex = Mathf.Clamp(presetIndex, 0, 2);

        switch (tonePresetIndex)
        {
            case 0:
                lowPassCutoff = 8000f;
                highPassCutoff = 10f;
                distortionLevel = 0f;
                Debug.Log("音色プリセット: Warm / Muffled");
                break;

            case 1:
                lowPassCutoff = 14000f;
                highPassCutoff = 300f;
                distortionLevel = 0.05f;
                Debug.Log("音色プリセット: Natural / Radio");
                break;

            case 2:
                lowPassCutoff = 22000f;
                highPassCutoff = 800f;
                distortionLevel = 0.35f;
                Debug.Log("音色プリセット: Rough / Distortion");
                break;
        }
    }

    private void ApplyNoiseReduction()
    {
        if (!allEffectsEnabled || !noiseReductionEnabled)
        {
            return;
        }

        float highPassBoost = Mathf.Lerp(0f, 700f, noiseReductionAmount);
        float lowPassReduction = Mathf.Lerp(0f, 5000f, noiseReductionAmount);

        highPassCutoff += highPassBoost;
        lowPassCutoff -= lowPassReduction;

        lowPassCutoff = Mathf.Clamp(lowPassCutoff, 3000f, 22000f);
        highPassCutoff = Mathf.Clamp(highPassCutoff, 10f, 5000f);
    }

    private void ApplyPitch()
    {
        if (audioSource == null)
        {
            return;
        }

        if (!allEffectsEnabled || !pitchEnabled)
        {
            audioSource.pitch = 1f;
            return;
        }

        audioSource.pitch = ConvertCentsToPitch(pitchCents);
    }

    private void ApplyToneFilters()
    {
        if (!allEffectsEnabled || !toneEnabled)
        {
            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = 22000f;
            }

            if (highPassFilter != null)
            {
                highPassFilter.cutoffFrequency = 10f;
            }

            if (distortionFilter != null)
            {
                distortionFilter.distortionLevel = 0f;
            }

            return;
        }

        if (lowPassFilter != null)
        {
            lowPassFilter.cutoffFrequency = lowPassCutoff;
        }

        if (highPassFilter != null)
        {
            highPassFilter.cutoffFrequency = highPassCutoff;
        }

        if (distortionFilter != null)
        {
            distortionFilter.distortionLevel = distortionLevel;
        }
    }

    private void ApplyReverb()
    {
        if (reverbFilter == null)
        {
            return;
        }

        if (!allEffectsEnabled || !reverbEnabled)
        {
            reverbFilter.reverbLevel = -10000f;
            return;
        }

        reverbFilter.reverbLevel = reverbLevel;
    }

    private void ApplyEcho()
    {
        if (echoFilter == null)
        {
            return;
        }

        if (!allEffectsEnabled || !echoEnabled)
        {
            echoFilter.decayRatio = 0f;
            return;
        }

        echoFilter.delay = echoDelay;
        echoFilter.decayRatio = echoDecayRatio;
    }

    private void PrepareAudioComponents()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (lowPassFilter == null)
        {
            lowPassFilter = GetComponent<AudioLowPassFilter>();
        }

        if (lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }

        if (highPassFilter == null)
        {
            highPassFilter = GetComponent<AudioHighPassFilter>();
        }

        if (highPassFilter == null)
        {
            highPassFilter = gameObject.AddComponent<AudioHighPassFilter>();
        }

        if (reverbFilter == null)
        {
            reverbFilter = GetComponent<AudioReverbFilter>();
        }

        if (reverbFilter == null)
        {
            reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
        }

        if (echoFilter == null)
        {
            echoFilter = GetComponent<AudioEchoFilter>();
        }

        if (echoFilter == null)
        {
            echoFilter = gameObject.AddComponent<AudioEchoFilter>();
        }

        if (distortionFilter == null)
        {
            distortionFilter = GetComponent<AudioDistortionFilter>();
        }

        if (distortionFilter == null)
        {
            distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        }
    }

    private void ApplyAllEffectValues()
    {
        ApplyPitch();

        ApplyTonePreset(tonePresetIndex);
        ApplyNoiseReduction();
        ApplyToneFilters();

        ApplyReverb();
        ApplyEcho();
    }

    private float ConvertCentsToPitch(float cents)
    {
        return Mathf.Pow(2f, cents / 1200f);
    }
}