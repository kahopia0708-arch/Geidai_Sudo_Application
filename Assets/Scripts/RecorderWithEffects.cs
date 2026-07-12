using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class RecorderWithEffects : MonoBehaviour
{
    [Header("Recording")]
    public int sampleRate = 44100;
    public int maxRecordSeconds = 300;
    public Dropdown microphoneDropdown;

    [Header("Audio")]
    public AudioSource audioSource;
    private string micDevice;
    private AudioClip recordedClip;
    private bool isRecording = false;

    [Header("UI - Controls")]
    public Button recordButton;
    public Button stopButton;
    public Button playButton;
    public Button saveButton;
    public InputField saveFileNameInput;

    [Header("Pitch (playback)")]
    public Toggle pitchToggle;
    public Slider pitchSlider; // 0..1 -> map to 0.5..2.0

    [Header("Noise Reduction (simple)")]
    public Toggle noiseToggle;
    public Slider noiseCutoffSlider; // 0..1 -> 200..12000 Hz
    public Slider noiseGateSlider;   // 0..1 -> 0..0.05

    [Header("Timbre (preset)")]
    public Toggle timbreToggle;
    public Dropdown timbrePresetDropdown; // 0:Original 1:Robot 2:Bitcrush
    public Slider timbreIntensitySlider;  // 0..1

    [Header("Reverb (simple)")]
    public Toggle reverbToggle;
    public Slider reverbLevelSlider; // 0..1

    // Audio DSP volatile copies (safe for audio thread)
    volatile float v_pitch = 1f;
    volatile bool v_pitchOn = true;

    volatile bool v_noiseOn = false;
    volatile float v_noiseCutoff = 12000f;
    volatile float v_noiseGate = 0f;

    volatile bool v_timbreOn = false;
    volatile int v_timbrePreset = 0;
    volatile float v_timbreIntensity = 0f;

    volatile bool v_reverbOn = false;
    volatile float v_reverbLevel = 0f;

    // DSP internal states for audio thread
    private float[] lpState;        // per-channel low-pass state
    private double[] robotPhase;    // per-channel ring-mod phase
    private float[] reverbBuffer;   // interleaved simple reverb buffer
    private int[] reverbPos;        // per-channel write positions

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        PopulateMicrophoneDropdown();
        HookupUI();
        InitDSPBuffers(Mathf.Max(1, audioSource.clip != null ? audioSource.clip.channels : 2));
        ApplyUIToVolatile();
    }

    void InitDSPBuffers(int presumedChannels)
    {
        int channels = Mathf.Max(1, presumedChannels);
        lpState = new float[channels];
        robotPhase = new double[channels];

        int maxReverbSamplesPerChannel = Mathf.CeilToInt(sampleRate * 0.5f);
        reverbBuffer = new float[maxReverbSamplesPerChannel * channels];
        reverbPos = new int[channels];
        for (int i = 0; i < channels; i++) reverbPos[i] = 0;
    }

    void PopulateMicrophoneDropdown()
    {
        if (microphoneDropdown == null) return;
        microphoneDropdown.ClearOptions();
        var devices = Microphone.devices;
        if (devices.Length == 0)
        {
            microphoneDropdown.options.Add(new Dropdown.OptionData("No Microphone"));
            microphoneDropdown.interactable = false;
            return;
        }
        foreach (var d in devices) microphoneDropdown.options.Add(new Dropdown.OptionData(d));
        microphoneDropdown.value = 0;
        micDevice = devices[0];
        microphoneDropdown.onValueChanged.AddListener(i =>
        {
            if (i >= 0 && i < devices.Length) micDevice = devices[i];
        });
    }

    void HookupUI()
    {
        if (recordButton) recordButton.onClick.AddListener(StartRecording);
        if (stopButton) stopButton.onClick.AddListener(StopRecording);
        if (playButton) playButton.onClick.AddListener(PlayRecorded);
        if (saveButton) saveButton.onClick.AddListener(SaveProcessedToFile);

        if (pitchSlider) pitchSlider.onValueChanged.AddListener(v => { v_pitch = Mathf.Lerp(0.5f, 2.0f, v); });
        if (pitchToggle) pitchToggle.onValueChanged.AddListener(val => v_pitchOn = val);

        if (noiseCutoffSlider) noiseCutoffSlider.onValueChanged.AddListener(v => v_noiseCutoff = Mathf.Lerp(200f, 12000f, v));
        if (noiseGateSlider) noiseGateSlider.onValueChanged.AddListener(v => v_noiseGate = Mathf.Lerp(0f, 0.05f, v));
        if (noiseToggle) noiseToggle.onValueChanged.AddListener(val => v_noiseOn = val);

        if (timbrePresetDropdown) timbrePresetDropdown.onValueChanged.AddListener(i => v_timbrePreset = i);
        if (timbreIntensitySlider) timbreIntensitySlider.onValueChanged.AddListener(v => v_timbreIntensity = v);
        if (timbreToggle) timbreToggle.onValueChanged.AddListener(val => v_timbreOn = val);

        if (reverbToggle) reverbToggle.onValueChanged.AddListener(val => v_reverbOn = val);
        if (reverbLevelSlider) reverbLevelSlider.onValueChanged.AddListener(v => v_reverbLevel = v);
    }

    void Update()
    {
        ApplyUIToVolatile();
    }

    void ApplyUIToVolatile()
    {
        v_pitchOn = (pitchToggle == null) ? true : pitchToggle.isOn;
        if (pitchSlider != null) v_pitch = Mathf.Lerp(0.5f, 2.0f, pitchSlider.value);
        if (!v_pitchOn) v_pitch = 1f;
        if (audioSource != null) audioSource.pitch = v_pitch;

        if (noiseCutoffSlider != null) v_noiseCutoff = Mathf.Lerp(200f, 12000f, noiseCutoffSlider.value);
        if (noiseGateSlider != null) v_noiseGate = Mathf.Lerp(0f, 0.05f, noiseGateSlider.value);
        if (noiseToggle != null) v_noiseOn = noiseToggle.isOn;

        if (timbrePresetDropdown != null) v_timbrePreset = timbrePresetDropdown.value;
        if (timbreIntensitySlider != null) v_timbreIntensity = timbreIntensitySlider.value;
        if (timbreToggle != null) v_timbreOn = timbreToggle.isOn;

        if (reverbToggle != null) v_reverbOn = reverbToggle.isOn;
        if (reverbLevelSlider != null) v_reverbLevel = reverbLevelSlider.value;
    }

    // RECORDING
    public void StartRecording()
    {
        if (isRecording) return;
        if (string.IsNullOrEmpty(micDevice))
        {
            var devs = Microphone.devices;
            if (devs.Length == 0)
            {
                Debug.LogWarning("No microphone devices found.");
                return;
            }
            micDevice = devs[0];
        }
        recordedClip = Microphone.Start(micDevice, false, maxRecordSeconds, sampleRate);
        isRecording = true;
        audioSource.Stop();
        Debug.Log("Recording started: " + micDevice);
    }

    public void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(micDevice);
        isRecording = false;

        if (recordedClip != null)
        {
            int pos = Microphone.GetPosition(micDevice);
            if (pos > 0)
            {
                int channels = recordedClip.channels;
                float[] all = new float[recordedClip.samples * channels];
                recordedClip.GetData(all, 0);

                int trimmedSamplesPerChannel = pos;
                float[] trimmed = new float[trimmedSamplesPerChannel * channels];
                Array.Copy(all, 0, trimmed, 0, trimmed.Length);

                AudioClip trimmedClip = AudioClip.Create(recordedClip.name + "_trim", trimmedSamplesPerChannel, channels, recordedClip.frequency, false);
                trimmedClip.SetData(trimmed, 0);
                recordedClip = trimmedClip;
            }
        }

        audioSource.clip = recordedClip;
        Debug.Log("Recording stopped. Length: " + (recordedClip != null ? recordedClip.length.ToString("F2") + "s" : "null"));
        if (recordedClip != null) InitDSPBuffers(recordedClip.channels);
    }

    // PLAYBACK
    public void PlayRecorded()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("No recorded clip.");
            return;
        }
        audioSource.clip = recordedClip;
        audioSource.Play();
    }

    // SAVE: render processed audio offline and save via WavUtility.Save
    public void SaveProcessedToFile()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("No recorded clip to save.");
            return;
        }

        string fileName = (saveFileNameInput != null && !string.IsNullOrEmpty(saveFileNameInput.text)) ? saveFileNameInput.text : "processed_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string path = Path.Combine(Application.persistentDataPath, fileName + ".wav");

        int channels = recordedClip.channels;
        int samplesPerChannel = recordedClip.samples;
        float[] interleaved = new float[samplesPerChannel * channels];
        recordedClip.GetData(interleaved, 0);

        float[] processed = ProcessSamplesOffline(interleaved, channels, recordedClip.frequency);

        AudioClip outClip = AudioClip.Create(fileName + "_proc", processed.Length / channels, channels, recordedClip.frequency, false);
        outClip.SetData(processed, 0);

        try
        {
            // ← ここを WavUtility.Save に合わせています
            WavUtility.Save(path, outClip);
            Debug.Log("Saved processed WAV to: " + path);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save WAV: " + ex.Message);
        }
    }

    float[] ProcessSamplesOffline(float[] interleaved, int channels, int frequency)
    {
        int totalSamples = interleaved.Length;
        float[] outBuf = new float[totalSamples];

        bool noiseOn = v_noiseOn;
        float noiseCutoff = v_noiseCutoff;
        float noiseGate = v_noiseGate;
        bool timbreOn = v_timbreOn;
        int timbrePreset = v_timbrePreset;
        float timbreIntensity = v_timbreIntensity;
        bool reverbOn = v_reverbOn;
        float reverbLevel = v_reverbLevel;

        float[] lp = new float[channels];
        double[] phase = new double[channels];

        int downFactor = 1;
        if (timbrePreset == 2) downFactor = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(1, 20, timbreIntensity)), 1, 64);

        int maxDelaySamples = Mathf.CeilToInt(frequency * 0.5f);
        int delaySamples = Mathf.CeilToInt(Mathf.Lerp(0.02f, 0.15f, reverbLevel) * frequency);
        if (delaySamples < 1) delaySamples = 1;
        float reverbDecay = Mathf.Lerp(0f, 0.85f, reverbLevel);

        float[][] reverbBufs = new float[channels][];
        int[] reverbIdx = new int[channels];
        for (int c = 0; c < channels; c++)
        {
            reverbBufs[c] = new float[delaySamples + 1];
            reverbIdx[c] = 0;
        }

        int dsCounter = 0;
        float[] dsLast = new float[channels];

        float dt = 1.0f / frequency;
        float rc = 1.0f / (2f * Mathf.PI * Mathf.Max(1f, noiseCutoff));
        float alpha = dt / (rc + dt);

        for (int i = 0; i < totalSamples; i += channels)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                int idx = i + ch;
                float x = interleaved[idx];
                float y = x;

                if (noiseOn)
                {
                    if (Mathf.Abs(y) < noiseGate) y = 0f;
                    float prev = lp[ch];
                    prev = prev + alpha * (y - prev);
                    lp[ch] = prev;
                    y = prev;
                }

                if (timbreOn)
                {
                    if (timbrePreset == 1)
                    {
                        float carrierFreq = Mathf.Lerp(30f, 400f, timbreIntensity);
                        double inc = 2.0 * Math.PI * carrierFreq / frequency;
                        double ph = phase[ch];
                        float carrier = (float)Math.Sin(ph);
                        phase[ch] = (ph + inc) % (2.0 * Math.PI);
                        float ring = y * carrier;
                        y = Mathf.Lerp(y, ring, timbreIntensity);
                    }
                    else if (timbrePreset == 2)
                    {
                        if (dsCounter == 0) dsLast[ch] = y;
                        y = dsLast[ch];
                    }
                }

                if (reverbOn && reverbLevel > 0f)
                {
                    float delayed = reverbBufs[ch][reverbIdx[ch]];
                    float newSample = y + delayed * reverbDecay;
                    reverbBufs[ch][reverbIdx[ch]] = newSample;
                    reverbIdx[ch]++;
                    if (reverbIdx[ch] >= reverbBufs[ch].Length) reverbIdx[ch] = 0;
                    y = Mathf.Lerp(y, delayed, Mathf.Clamp01(reverbLevel));
                }

                outBuf[idx] = y;
            }

            if (timbrePreset == 2)
            {
                dsCounter = (dsCounter + 1) % downFactor;
            }
        }

        return outBuf;
    }

    void OnDisable()
    {
        if (isRecording) StopRecording();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        bool noiseOn = v_noiseOn;
        float noiseCutoff = v_noiseCutoff;
        float noiseGate = v_noiseGate;

        bool timbreOn = v_timbreOn;
        int timbrePreset = v_timbrePreset;
        float timbreIntensity = v_timbreIntensity;

        bool reverbOn = v_reverbOn;
        float reverbLevel = v_reverbLevel;

        if (lpState == null || lpState.Length < channels) lpState = new float[channels];
        if (robotPhase == null || robotPhase.Length < channels) robotPhase = new double[channels];

        float dt = 1.0f / sampleRate;
        float rc = 1.0f / (2f * Mathf.PI * Mathf.Max(1f, noiseCutoff));
        float alpha = dt / (rc + dt);

        int downFactor = 1;
        if (timbrePreset == 2) downFactor = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(1, 20, timbreIntensity)), 1, 64);

        int dsCounter = 0;
        float[] dsLast = new float[channels];

        int delaySamples = Mathf.CeilToInt(Mathf.Lerp(0.02f, 0.15f, reverbLevel) * sampleRate);
        if (delaySamples < 1) delaySamples = 1;
        float reverbDecay = Mathf.Lerp(0f, 0.85f, reverbLevel);
        if (reverbBuffer == null || reverbBuffer.Length < (delaySamples + 1) * channels)
        {
            reverbBuffer = new float[(delaySamples + 1) * channels];
            reverbPos = new int[channels];
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                int idx = i + ch;
                float x = data[idx];
                float y = x;

                if (noiseOn)
                {
                    if (Mathf.Abs(y) < noiseGate) y = 0f;
                    float prev = lpState[ch];
                    prev = prev + alpha * (y - prev);
                    lpState[ch] = prev;
                    y = prev;
                }

                if (timbreOn)
                {
                    if (timbrePreset == 1)
                    {
                        float carrierFreq = Mathf.Lerp(30f, 400f, timbreIntensity);
                        double inc = 2.0 * Math.PI * carrierFreq / sampleRate;
                        double ph = robotPhase[ch];
                        float carrier = (float)Math.Sin(ph);
                        robotPhase[ch] = (ph + inc) % (2.0 * Math.PI);
                        float ring = y * carrier;
                        y = Mathf.Lerp(y, ring, timbreIntensity);
                    }
                    else if (timbrePreset == 2)
                    {
                        if (dsCounter == 0) dsLast[ch] = y;
                        y = dsLast[ch];
                    }
                }

                if (reverbOn && reverbLevel > 0f)
                {
                    int pos = reverbPos[ch] % (delaySamples + 1);
                    int bufIndex = pos * channels + ch;
                    float delayed = reverbBuffer[bufIndex];
                    float newSample = y + delayed * reverbDecay;
                    reverbBuffer[bufIndex] = newSample;
                    reverbPos[ch] = (reverbPos[ch] + 1) % (delaySamples + 1);
                    y = Mathf.Lerp(y, delayed, Mathf.Clamp01(reverbLevel));
                }

                data[idx] = y;
            }

            if (timbrePreset == 2)
            {
                dsCounter = (dsCounter + 1) % downFactor;
            }
        }
    }
}