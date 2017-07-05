using UnityEngine;

/// <summary>
/// Component representing remote audio stream in local scene. Automatically attached to the PUN object which owner's instance has streaming Recorder attached.
/// </summary>
[RequireComponent(typeof (AudioSource))]
[DisallowMultipleComponent]
public class PhotonVoiceSpeaker : Photon.MonoBehaviour
{
    const int maxPlayLagMs = 100;
    private int maxPlayLagSamples;

    // buffering by playing few samples back
    private int playDelaySamples;

    private int frameSize = 0;
    private int frameSamples = 0;
    private int streamSamplePos = 0;

    // non-wrapped play position
    private int playSamplePos
    {
        get { return this.source.clip != null ? this.playLoopCount * this.source.clip.samples + this.source.timeSamples : 0; }
        set
        {
            if (this.source.clip != null)
            {
                this.source.timeSamples = value % this.source.clip.samples;
                this.playLoopCount = value / this.source.clip.samples;
                this.sourceTimeSamplesPrev = this.source.timeSamples;
            }

        }
    }
    private int sourceTimeSamplesPrev = 0;
    private int playLoopCount = 0;
    
    private float lastRecvTime = 0;

    /// <summary>Time when last audio packet was received for the speaker.</summary>
    public float LastRecvTime
    {
        get { return this.lastRecvTime; }
    }

    /// <summary>Is the speaker playing right now.</summary>
    public bool IsPlaying
    {
        get { return this.source.isPlaying; }
    }

    /// <summary>Smoothed difference between (jittering) stream and (clock-driven) player.</summary>
    public int CurrentBufferLag { get; private set; }
    
    // jitter-free stream position
    private int streamSamplePosAvg;

    private AudioSource source;

    void Awake()
    {
        this.source = GetComponent<AudioSource>();
        PhotonVoiceNetwork.LinkSpeakerToRemoteVoice(this);
    }

    // initializes the speaker with remote voice info
    internal void OnVoiceLinked(int frequency, int channels, int encoderDelay, int playDelayMs)
    {
        
        int bufferSamples = frequency; // 1 sec

        this.frameSize = ExitGames.Client.Photon.Voice.VoiceUtil.DelayToFrameSize(encoderDelay, frequency, channels);

        this.frameSamples = ExitGames.Client.Photon.Voice.VoiceUtil.DelayToSamples(encoderDelay, frequency);

        // add 1 frame samples to make sure that we have something to play when delay set to 0
        this.maxPlayLagSamples = maxPlayLagMs * frequency / 1000 + this.frameSamples;
        this.playDelaySamples = playDelayMs * frequency / 1000 + this.frameSamples;

        // init with target value
        this.CurrentBufferLag = this.playDelaySamples;
        this.streamSamplePosAvg = this.playDelaySamples;

        this.source.loop = true;
        // using streaming clip leads to too long delays
        this.source.clip = AudioClip.Create("PhotonVoice", bufferSamples, channels, frequency, false);

        this.streamSamplePos = 0;
        this.playSamplePos = 0;

        this.source.Play();
        this.source.Pause();
    }

    internal void OnVoiceUnlinked()
    {
        if (this.source.clip != null)
        {
            this.source.Stop();
            this.source.clip = null;
        }
    }

    /// <summary>Is the speaker linked to the remote voice (info available and streaming is possible).</summary>
    public bool IsVoiceLinked { get { return this.source != null && this.source.clip != null; } }

    void Update()
    {
        if (this.source != null && this.source.clip != null)
        {
            // loop detection (pcmsetpositioncallback not called when clip loops)
            if (this.source.isPlaying)
            {
                if (this.source.timeSamples < sourceTimeSamplesPrev)
                {
                    playLoopCount++;
                }
                sourceTimeSamplesPrev = this.source.timeSamples;
            }            

            var playPos = this.playSamplePos; // cache calculated value

            // average jittering value
            this.CurrentBufferLag = (this.CurrentBufferLag * 39 + (this.streamSamplePos - playPos)) / 40;

            // calc jitter-free stream position based on clock-driven palyer position and average lag
            this.streamSamplePosAvg = playPos + this.CurrentBufferLag;
            if (this.streamSamplePosAvg > this.streamSamplePos)
            {
                this.streamSamplePosAvg = this.streamSamplePos;
            }

            // start with given delay or when stream position is ok after overrun pause
            if (playPos < this.streamSamplePos - this.playDelaySamples)
            {
                if (!this.source.isPlaying)
                {
                    this.source.UnPause();
                }
            }
            
            if (playPos > this.streamSamplePos - frameSamples)
            {
                if (this.source.isPlaying)
                {
                    if (PhotonVoiceSettings.Instance.DebugInfo)
                    {
                        Debug.LogWarningFormat("PUNVoice: PhotonVoiceSpeaker: player overrun: {0}/{1}({2}) = {3}", playPos, streamSamplePos, streamSamplePosAvg, streamSamplePos - playPos);
                    }

                    // when nothing to play:
                    // pause player  (useful in case if stream is stopped for good) ...
                    this.source.Pause();

                    // ... and rewind to proper position
                    playPos = this.streamSamplePos - this.playDelaySamples;
                    this.playSamplePos = playPos;
                    this.CurrentBufferLag = this.playDelaySamples;
                }
            }
            if (this.source.isPlaying)
            {                
                var lowerBound = this.streamSamplePos - this.playDelaySamples - maxPlayLagSamples;
                if (playPos < lowerBound)
                {
                    if (PhotonVoiceSettings.Instance.DebugInfo)
                    {
                        Debug.LogWarningFormat("PUNVoice: PhotonVoiceSpeaker: player overrun: {0}/{1}({2}) = {3}", playPos, streamSamplePos, streamSamplePosAvg, streamSamplePos - playPos);
                    }

                    // if lag exceeds max allowable, fast forward to proper position                    
                    playPos = this.streamSamplePos - this.playDelaySamples;
                    this.playSamplePos = playPos;
                    this.CurrentBufferLag = this.playDelaySamples;
                }
            }

        }
        
    }

    void OnDestroy() 
    {
        PhotonVoiceNetwork.UnlinkSpeakerFromRemoteVoice(this);
        if (this.source != null)
        {
            this.source.Stop();
        }
    }

    void OnApplicationQuit()
    {
        if (this.source != null)
        {
            this.source.Stop();
        }
    }

    internal void OnAudioFrame(float[] frame)
    {
        if (frame.Length != frameSize)
        {
            Debug.LogErrorFormat("PUNVoice: Audio frames are not of  size: {0} != {1}", frame.Length, frameSize);
            Debug.LogErrorFormat("PUNVoice: {0} {1} {2} {3} {4} {5} {6}", frame[0], frame[1], frame[2], frame[3], frame[4], frame[5], frame[6]);
            return;
        }

        // Store last packet

        // Set last time we got something
        this.lastRecvTime = Time.time;

        this.source.clip.SetData(frame, this.streamSamplePos % this.source.clip.samples);
        this.streamSamplePos += frame.Length / this.source.clip.channels;
    }
}