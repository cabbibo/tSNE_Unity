    // -----------------------------------------------------------------------
// <copyright file="Client.cs" company="Exit Games GmbH">
//   Photon Voice API Framework for Photon - Copyright (C) 2015 Exit Games GmbH
// </copyright>
// <summary>
//   Extends LoadBalancing API with audio streaming functionality.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using POpusCodec;
using POpusCodec.Enums;

namespace ExitGames.Client.Photon.Voice
{
    /// <summary>
    /// Single event code for all events to save codes for user.
    /// Change if conflicts with other code.
    /// </summary>
    enum EventCode
    {
        VoiceEvent = 201
    }

    /// <summary>
    /// Interface to feed LocalVoice with audio data.
    /// Implement it in class wrapping platform-specific autio source.
    /// </summary>
    public interface IAudioStream
    {
        /// <summary>
        /// Read data if it's enough to fill entire buffer.
        /// Return false otherwise.
        /// </summary>
        bool GetData(float[] buffer);

        /// <summary>Sampling rate (frequency).</summary>
        int SamplingRate { get; }
    }

    /// <summary>Default parameters for LocalVoice creation (if not provided in constructor).</summary>
    public struct Default
    {
        /// <summary>Audio sampling rate (frequency).</summary>
        public const int SamplingRate = (int)POpusCodec.Enums.SamplingRate.Sampling24000;
        /// <summary>Number of channels.</summary>
        public const int Channels = (int)POpusCodec.Enums.Channels.Mono;
        /// <summary>Uncompressed frame (audio packet) size in ms.</summary>
        public const int EncoderDelay = (int)POpusCodec.Enums.Delay.Delay20ms;
        /// <summary>Compression quality in terms of bits per second.</summary>
        public const int Bitrate = 30000;
    }

    /// <summary>Describes audio stream properties.</summary>
    public class VoiceInfo
    {
        public VoiceInfo(int samplingRate = Default.SamplingRate, int channels = Default.Channels, int encoderDelay = Default.EncoderDelay, int bitrate = Default.Bitrate, object userdata = null)
        {
            this.SamplingRate = samplingRate;
            this.Channels = channels;
            this.EncoderDelay = encoderDelay;
            this.Bitrate = bitrate;
            this.UserData = userdata;            
        }

        /// <summary>Audio sampling rate (frequency).</summary>
        public int SamplingRate {get; private set;}
        /// <summary>Number of channels.</summary>
        public int Channels { get; private set; }
        /// <summary>Uncompressed frame (audio packet) size in ms.</summary>
        public int EncoderDelay { get; private set; }
        /// <summary>Compression quality in terms of bits per second.</summary>
        public int Bitrate { get; private set; }
        /// <summary>Optional user data. Should be serializable by Photon.</summary>
        public object UserData { get; private set; }
    }

    /// <summary>Helper to provide remote voices infos via Client.RemoteVoiceInfos iterator.</summary>
    public class RemoteVoiceInfo
    {
        internal RemoteVoiceInfo(int playerId, byte voiceId, VoiceInfo info)
        {
            this.PlayerId = playerId;
            this.VoiceId = voiceId;
            this.Info = info;
        }
        /// <summary>Remote voice info.</summary>
        public VoiceInfo Info { get; private set; }
        /// <summary>Player Id of voice owner.</summary>
        public int PlayerId { get; private set; }
        /// <summary>Voice id unique in the room.</summary>
        public byte VoiceId { get; private set; }
    }

    enum EventSubcode : byte
    {
        VoiceInfo = 1,
        VoiceRemove = 2,
        Frame = 3,
        DebugEchoRemoveMyVoices = 10
    }

    enum EventParam : byte
    {
        VoiceId = 1,
        SamplingRate = 2,
        Channels = 3,
        EncoderDelay = 4,
        Bitrate = 5,        
        UserData = 10,
        EventNumber = 11
    }

    /// <summary>
    /// Represents outgoing audio stream. Compresses audio data provided via IAudioStream and broadcasts it to all players in the room.
    /// </summary>
    public class LocalVoice : OpusEncoder, IDisposable
    {
        static public LocalVoice Dummy = new LocalVoice();
        /// <summary>If AudioGroup != 0, voice's data is sent only to clients listening to this group.</summary>
        /// <see cref="Client.ChangeAudioGroups(byte[], byte[])"/>
        public byte AudioGroup { get; set; }

        /// <summary>If true, stream data broadcasted unconditionally.</summary>
        public bool Transmit { set; get; }

        /// <summary>Returns true if stream broadcasts.</summary>
        public bool IsTransmitting
        {
            get { return this.Transmit && (!this.VoiceDetector.On || this.VoiceDetector.Detected); }
        }

        /// <summary>Use to enable or disable voice detector and set its parameters.</summary>
        public VoiceDetector VoiceDetector { get; private set; }

        /// <summary>
        /// Level meter utility.
        /// </summary>
        public LevelMeter LevelMeter { get; private set; }

        /// <summary>If true, voice detector calibration is in progress.</summary>
        public bool VoiceDetectorCalibrating { get { return voiceDetectorCalibrateCount > 0; } }
        private int voiceDetectorCalibrateCount;
        
        /// <summary>Trigger voice detector calibration process.
        /// While calibrating, keep silence. Voice detector sets threshold basing on measured backgroud noise level.
        /// </summary>
        /// <param name="durationMs">Duration of calibration in milliseconds.</param>
        public void VoiceDetectorCalibrate(int durationMs) 
        {
            voiceDetectorCalibrateCount = this.sourceSamplingRateHz * (int)this.InputChannels * durationMs / 1000;
            LevelMeter.ResetAccumAvgPeakAmp();             
        }
        #region nonpublic

        static private byte idCnt;
        internal byte id;
        internal byte evNumber = 0; // sequence used by receivers to detect loss. will overflow.
        private Client client;
        private IAudioStream audioStream;
        private int sourceSamplingRateHz;
        internal object userData;
        internal int frameSize = 0; // encoder frame size
        private float[] frameBuffer = null;        
        internal int sourceFrameSize = 0;
        private float[] sourceFrameBuffer = null;

//        OpusDecoder _debug_decoder;

        internal LocalVoice() : base(SamplingRate.Sampling08000, Channels.Mono, 1000, OpusApplicationType.Voip, Delay.Delay10ms)
        {
            this.LevelMeter = new LevelMeter(0, 0); 
            this.VoiceDetector = new VoiceDetector(0, 0);
        }

        internal LocalVoice(Client client, byte id, IAudioStream audioStream, object userData, int encoderSamplingRateHz, int numChannels, int bitrate, int delay)
            : base((SamplingRate)encoderSamplingRateHz, (Channels)numChannels, bitrate, OpusApplicationType.Voip, (Delay)delay)
        {
            this.client = client;
            this.id = id;
            this.audioStream = audioStream;
            this.sourceSamplingRateHz = audioStream.SamplingRate;
            this.userData = userData;
            this.frameSize = VoiceUtil.DelayToFrameSize((int)delay, (int)encoderSamplingRateHz, (int)numChannels);
            this.sourceFrameSize = this.frameSize * this.sourceSamplingRateHz / (int)this.InputSamplingRate;
            this.frameBuffer = new float[this.frameSize];
            if (this.sourceFrameSize == this.frameSize)
            {
                this.sourceFrameBuffer = this.frameBuffer;
            }
            else
            {
                this.sourceSamplingRateHz = audioStream.SamplingRate;
                this.sourceFrameBuffer = new float[this.sourceFrameSize];

                this.client.DebugReturn(DebugLevel.WARNING, "[PV] Local voice #" + this.id + " audio source frequency " + this.sourceSamplingRateHz  + " and encoder sampling rate " +  (int)this.InputSamplingRate + " do not match. Resampling will occur before encoding.");
            }

            this.LevelMeter = new LevelMeter(this.sourceSamplingRateHz, numChannels); //1/2 sec
            this.VoiceDetector = new VoiceDetector(this.sourceSamplingRateHz, numChannels);
//            _debug_decoder = new OpusDecoder(this.InputSamplingRate, this.InputChannels);
        }

        internal void service()
        {
            while (processStream());
        }
        
        private bool readStream()
        {
            if (!this.audioStream.GetData(this.sourceFrameBuffer))
            {
                return false;
            }

            this.LevelMeter.process(this.sourceFrameBuffer);

            // process VAD calibration (could be moved to process method of yet another processor)
            if (this.voiceDetectorCalibrateCount != 0)
            {
                this.voiceDetectorCalibrateCount -= this.sourceFrameBuffer.Length;
                if (this.voiceDetectorCalibrateCount <= 0)
                {
                    this.voiceDetectorCalibrateCount = 0;
                    this.VoiceDetector.Threshold = LevelMeter.AccumAvgPeakAmp * 2; 
                }
            }

            if (this.VoiceDetector.On) {
                this.VoiceDetector.process(this.sourceFrameBuffer);
                if (!this.VoiceDetector.Detected)
                {
                    return false;
                }
            }
            if (this.sourceFrameSize != this.frameSize)
            {
                VoiceUtil.Resample(this.sourceFrameBuffer, this.frameBuffer, (int)this.InputChannels);
            }
            return true;
        }

        private bool processStream()
        {
            if (this.client.State == LoadBalancing.ClientState.Joined && this.Transmit)
            {
                if (readStream())
                {
                    this.client.sendFrame(this.id, this.evNumber++, this.compress(this.frameBuffer), this.AudioGroup);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private byte[] compress(float[] buffer)
        {
            var res = this.Encode(buffer);
//            var tmp = _debug_decoder.DecodePacketFloat(res);
//            this.client.DebugReturn(DebugLevel.INFO, "[PV] Encode: " + res.Length + "/" + buffer.Length /* *4 */ + " " + VoiceUtil.tostr(res) + " " + VoiceUtil.tostr(buffer));
            //            this.client.DebugReturn(DebugLevel.INFO, "[PV] Decode === : " +  this.FrameSizePerChannel + " "+ tmp.Length + " " + VoiceUtil.tostr(tmp));
            return res;
        }

        #endregion
    }

    #region nonpublic

    internal class RemoteVoice : OpusDecoder
    {
        // Client.RemoteVoiceInfos support
        internal VoiceInfo Info { get; private set; }

        internal RemoteVoice(Client client, VoiceInfo info, SamplingRate outputSamplingRateHz, Channels numChannels, byte lastEventNumber)
            : base(outputSamplingRateHz, numChannels)
        {
            this.client = client;
            this.Info = info;
            this.lastEvNumber = lastEventNumber;
        }        

        internal byte lastEvNumber = 0;
        private Client client;

        internal float[] decompress(byte[] buffer)
        {
            float[] res;
            if (buffer == null && this.client.UseLossCompensation)
            {
                res = this.DecodePacketLostFloat();
                this.client.DebugReturn(DebugLevel.ALL, "[PV] lost packet decoded length: " + res.Length);
            }
            else
            {
                res = this.DecodePacketFloat(buffer);
            }
//            this.client.DebugReturn(DebugLevel.INFO, "[PV]Decode: " + res.Length /* *4 */ + "/" + buffer.Length + " " + Util.tostr(res) + " " + Util.tostr(buffer));
            return res;
        }       
    }
    
    #endregion

    /// <summary>
    /// This class extends LoadBalancingClient with audio streaming functionality.
    /// </summary>
    /// <remarks>
    /// Use LoadBalancing workflow to join Voice room. All standard LoadBalancing features available.
    /// To work with audio:
    /// Create outgoing audio streams with Client.CreateLocalVoice method.
    /// Handle new incoming audio streams info with Client.OnRemoteVoiceInfoAction.
    /// Handle incoming audio streams data with Client.OnAudioFrameAction.
    /// </remarks>
    public class Client : LoadBalancing.LoadBalancingClient
    {
        /// <summary>
        /// If true, outgoing stream routed back to client via server same way as for remote client's streams.
        /// Can be swithed any time. OnRemoteVoiceInfoAction and OnRemoteVoiceRemoveAction are triggered if required.
        /// </summary>
        /// <remarks>
        /// For debug purposes only. 
        /// Room consistency is not guranteed if the property set to true at least once during join session.
        /// </remarks>
        public bool DebugEchoMode { 
            get {return debugEchoMode;}
            set
            {
                this.debugEchoMode = value;
                // need to update my voices in remote voice list if switched while joined
                if (this.State == LoadBalancing.ClientState.Joined)
                {
                    if (this.debugEchoMode)
                    {
                        // send to self - easiest way to setup speakers
                        this.sendVoicesInfo(this.LocalPlayer.ID, this.localVoices);
                    }
                    else
                    {
                        object[] content = new object[] { (byte)0, EventSubcode.DebugEchoRemoveMyVoices};
                        var opt = new LoadBalancing.RaiseEventOptions();
                        opt.TargetActors = new int[] { this.LocalPlayer.ID };
                        this.OpRaiseEvent((byte)EventCode.VoiceEvent, content, true, opt);
                    }
                }
            }
        }

        /// <summary>Lost frames simulation ratio.</summary>
        public int DebugLostPercent { get; set; }

        /// <summary>Loss compensation for dropped frames. True by default.</summary>
        public bool UseLossCompensation { get; set; } // set to true in constructor

        /// <summary>Lost frames counter.</summary>
        public int FramesLost { get; private set; }

        /// <summary>Received frames counter.</summary>
        public int FramesReceived { get; private set; }

        /// <summary>
        /// Register a method to be called when remote voice info arrived (after join or new new remote voice creation).
        /// Metod parameters: (int playerId, byte voiceId, VoiceInfo voiceInfo);
        /// </summary>
        public Action<int, byte, VoiceInfo> OnRemoteVoiceInfoAction { get; set; }
        /// <summary>
        /// Register a method to be called when remote voice removed.
        /// Metod parameters: (int playerId, byte voiceId)
        /// </summary>
        public Action<int, byte> OnRemoteVoiceRemoveAction { get; set; }
        /// <summary>
        /// Register a method to be called when new audio frame received. 
        /// Metod parameters: (int playerId, byte voiceId, float[] frame)
        /// </summary>
        public Action<int, byte, float[]> OnAudioFrameAction { get; set; }

        // let user code set actions which we occupy; call them in our actions
        /// <summary>Register a method to be called when an event got dispatched. Gets called at the end of OnEvent().</summary>
        /// <see cref="ExitGames.Client.Photon.LoadBalancing.LoadBalancingClient.OnEventAction"/>
        new public Action<EventData> OnEventAction { get; set; } // called by choice client action, so user still can use action

        /// <summary>Iterates through all remote voices infos.</summary>
        public IEnumerable<RemoteVoiceInfo> RemoteVoiceInfos
        { 
            get {
                foreach (var playerVoices in remoteVoices)
                {
                    foreach (var voice in playerVoices.Value)
                    {
                        yield return new RemoteVoiceInfo(playerVoices.Key, voice.Key, voice.Value.Info);
                    }
                }
            } 
        }

        /// <summary>Creates Client instance</summary>
        public Client()
        {
            this.UseLossCompensation = true;
            base.OnEventAction = onEventActionVoiceClient;
        }

        /// <summary>
        /// This method dispatches all available incoming commands and then sends this client's outgoing commands.
        /// Call this method regularly (2..20 times a second).
        /// </summary>
        new public void Service()
        {
            base.Service();
            foreach (var v in localVoices)
            {
                v.service();
            }
        }

        /// <summary>
        /// Creates new local voice (outgoing audio stream).
        /// </summary>
        /// <param name="audioStream">Object providing audio data for the outgoing stream.</param>
        /// <param name="voiceInfo">Outgoing audio stream parameters (should be set according to Opus encoder restrictions).</param>
        /// <returns>Outgoing stream handler.</returns>
        /// <remarks>
        /// audioStream.SamplingRate and voiceInfo.SamplingRate may do not match. Automatic resampling will occur in this case.
        /// </remarks>
        public LocalVoice CreateLocalVoice(IAudioStream audioStream, VoiceInfo voiceInfo)
        {
            // id assigned starting from 1 and up to 255

            byte newId = 0; // non-zero if successfully assigned
            if (voiceIdCnt == 255)
            {
                // try to reuse id
                var ids = new bool[256];
                foreach (var v in localVoices) 
                {
                    ids[v.id] = true;
                }
                // ids[0] is not used
                for (byte id = 1; id != 0 /* < 256 */ ; id++)
                {
                    if (!ids[id])
                    {
                        newId = id;
                        break;
                    }
                }
            }
            else
            {
                voiceIdCnt++;
                newId = voiceIdCnt;
            }

            if (newId != 0)
            {
                var v = new LocalVoice(this, newId, audioStream, voiceInfo.UserData, voiceInfo.SamplingRate, voiceInfo.Channels, voiceInfo.Bitrate, voiceInfo.EncoderDelay);
                localVoices.Add(v);

                this.DebugReturn(DebugLevel.INFO, "[PV] Local voice #" + v.id + " added: src_f=" + audioStream.SamplingRate + " enc_f=" + v.InputSamplingRate + " ch=" + v.InputChannels + " d=" + v.EncoderDelay + " s=" + v.frameSize + " b=" + v.Bitrate + " ud=" + voiceInfo.UserData);
                if (this.State == LoadBalancing.ClientState.Joined)
                {
                    this.sendVoicesInfo(0, new List<LocalVoice>() { v }); // broadcast if joined
                }
                v.AudioGroup = this.globalAudioGroup;
                return v;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes local voice (outgoing audio stream).
        /// <param name="voice">Handler of outgoing stream to be removed.</param>
        /// </summary>
        public void RemoveLocalVoice(LocalVoice v)
        {
            localVoices.Remove(v);
            if (this.State == LoadBalancing.ClientState.Joined)
            {
                this.sendVoiceRemove(new List<LocalVoice>() { v });
            }
            this.DebugReturn(DebugLevel.INFO, "[PV] Local voice #" + v.id + " removed");
        }

        /// <summary>
        /// Change audio groups listended by client. Works only while joined to a voice room.
        /// </summary>
        /// <see cref="LocalVoice.AudioGroup"/>
        /// <see cref="SetGlobalAudioGroup(byte)"/>
        /// <remarks>
        /// Note the difference between passing null and byte[0]:
        ///   null won't add/remove any groups.
        ///   byte[0] will add/remove all (existing) groups.
        /// First, removing groups is executed. This way, you could leave all groups and join only the ones provided.
        /// </remarks>
        /// <param name="groupsToRemove">Groups to remove from listened. Null will not leave any. A byte[0] will remove all.</param>
        /// <param name="groupsToAdd">Groups to add to listened. Null will not add any. A byte[0] will add all current.</param>
        /// <returns>If request could be enqueued for sending</returns>
        public virtual bool ChangeAudioGroups(byte[] groupsToRemove, byte[] groupsToAdd)
        {
            return this.loadBalancingPeer.OpChangeGroups(groupsToRemove, groupsToAdd);
        }

        /// <summary>
        /// Set global audio group for this client. This call sets AudioGroup for existing local voices and for created later to given value.
        /// Client set as listening to this group only until ChangeAudioGroups called. This method can be called any time.
        /// </summary>
        /// <see cref="LocalVoice.AudioGroup"/>
        /// <see cref="ChangeAudioGroups(byte[], byte[])"/>
        public byte GlobalAudioGroup
        {
            get { return this.globalAudioGroup; }
            set
            {
                this.loadBalancingPeer.DebugOut = DebugLevel.ALL;
                this.globalAudioGroup = value;
                if (this.State == LoadBalancing.ClientState.Joined)
                {
                    if (globalAudioGroup != 0)
                    {
                        this.loadBalancingPeer.OpChangeGroups(new byte[0], new byte[] { this.globalAudioGroup });
                    }
                    else
                    {
                        this.loadBalancingPeer.OpChangeGroups(new byte[0], null);
                    }
                }
                foreach (var v in this.localVoices)
                {
                    v.AudioGroup = this.globalAudioGroup;
                }
            }
        }


        #region nonpublic

        private byte globalAudioGroup;
        private bool debugEchoMode;
        private byte voiceIdCnt = 0;

        private List<LocalVoice> localVoices = new List<LocalVoice>();
        // player id -> voice id -> voice
        private Dictionary<int, Dictionary<byte, RemoteVoice>> remoteVoices = new Dictionary<int, Dictionary<byte, RemoteVoice>>();

        // send to others if playerId == 0 or to playerId only
        private void sendVoicesInfo(int playerId, List<LocalVoice> voicesToSend)
        {
            object[] infos = new object[voicesToSend.Count];
            object[] content = new object[] { (byte)0, EventSubcode.VoiceInfo, infos };
            int i = 0;
            foreach (var v in voicesToSend)
            {
                infos[i] = new Hashtable() { 
                    { (byte)EventParam.VoiceId, v.id },
                    { (byte)EventParam.SamplingRate, v.InputSamplingRate },
                    { (byte)EventParam.Channels, v.InputChannels },
                    { (byte)EventParam.EncoderDelay, v.EncoderDelay },
                    { (byte)EventParam.Bitrate, v.Bitrate },                    
                    { (byte)EventParam.UserData, v.userData },
                    { (byte)EventParam.EventNumber, v.evNumber }
                };
                i++;
                this.DebugReturn(DebugLevel.INFO, "[PV] Voice #" + v.id + " info sent: f=" + v.InputSamplingRate + ", ch=" + v.InputChannels + " d=" + v.EncoderDelay + " s=" + v.frameSize + " b=" + v.Bitrate + " ev=" + v.evNumber);
            }

            var opt = new LoadBalancing.RaiseEventOptions();
            if (playerId != 0)
            {
                opt.TargetActors = new int[] { playerId };
            }
            else
            { // bradcast to others
                if (this.DebugEchoMode) // and to self as well if debugging
                {
                    opt.Receivers = LoadBalancing.ReceiverGroup.All;
                }
            }
            this.OpRaiseEvent((byte)EventCode.VoiceEvent, content, true, opt);
        }

        private void sendVoiceRemove(List<LocalVoice> voicesToSend)
        {
            
            byte[] ids = new byte[voicesToSend.Count];
            object[] content = new object[] { (byte)0, EventSubcode.VoiceRemove, ids };

            int i = 0;
            foreach (var v in voicesToSend)
            {
                ids[i] = v.id;
                i++;
                this.DebugReturn(DebugLevel.INFO, "[PV] Voice #" + v.id + " remove sent");                
            }
            var opt = new LoadBalancing.RaiseEventOptions();
            if (this.DebugEchoMode)
            {
                opt.Receivers = LoadBalancing.ReceiverGroup.All;
            }
            this.OpRaiseEvent((byte)EventCode.VoiceEvent, content, true, opt);
        }

        internal void sendFrame(byte voiceId, byte evNumber, byte[] frame, byte audioGroup)
        {            
            object[] content = new object[] { voiceId, evNumber, frame };

            var opt = new LoadBalancing.RaiseEventOptions();
            if (this.DebugEchoMode)
            {
                opt.Receivers = LoadBalancing.ReceiverGroup.All;
            }
            opt.InterestGroup = audioGroup;
            this.OpRaiseEvent((byte)EventCode.VoiceEvent, content, false, opt);
            this.loadBalancingPeer.SendOutgoingCommands();
        }

        private void onEventActionVoiceClient(EventData ev)
        {
            int playerId;
            switch (ev.Code)
            {
                case (byte)LoadBalancing.EventCode.Join:
                    playerId = (int)ev[LoadBalancing.ParameterCode.ActorNr];
                    if (playerId == this.LocalPlayer.ID) 
                    {
                        clearRemoteVoices();
                        this.sendVoicesInfo(0, this.localVoices);// my join, broadcast
                        if (this.globalAudioGroup != 0)
                        {
                            this.loadBalancingPeer.OpChangeGroups(new byte[0], new byte[] { this.globalAudioGroup });
                        }
                    }
                    else 
                    {
                        this.sendVoicesInfo(playerId, this.localVoices);// send to new joined only
                    }
                    break;
                case (byte)LoadBalancing.EventCode.Leave:
                    {
                        playerId = (int)ev[LoadBalancing.ParameterCode.ActorNr];
                        if (playerId == this.LocalPlayer.ID)
                        {
                            clearRemoteVoices();                            
                        }
                        else
                        {
                            onPlayerLeave(playerId);
                        }
                    }
                    break;                
                case (byte)EventCode.VoiceEvent:                    
                    // Single event code for all events to save codes for user.
                    // Payloads are arrays. If first array element is 0 than next is event subcode. Otherwise, the event is data frame with voiceId in 1st element.
                    object[] content = (object[])ev[(byte)LoadBalancing.ParameterCode.CustomEventContent];
                    if ((byte)content[0] == (byte)0)
                    {
                        switch ((byte)content[1])
                        {
                            case (byte)EventSubcode.VoiceInfo:
                                onVoiceInfo(ev, content[2]);
                                break;
                            case (byte)EventSubcode.VoiceRemove:
                                onVoiceRemove(ev, content[2]);
                                break;
                            case (byte)EventSubcode.DebugEchoRemoveMyVoices:
                                Dictionary<byte, RemoteVoice> playerVoices = null;
                                if (this.remoteVoices.TryGetValue(this.LocalPlayer.ID, out playerVoices))
                                {
                                    foreach (var v in playerVoices)
                                    {
                                        if (this.OnRemoteVoiceRemoveAction != null)
                                        {
                                            this.OnRemoteVoiceRemoveAction(this.LocalPlayer.ID, v.Key);
                                        }
                                    }
                                    this.remoteVoices.Remove(this.LocalPlayer.ID);
                                }
                                break;
                            default:
                                this.DebugReturn(DebugLevel.ERROR, "[PV] Unknown sevent subcode " + content[1]);
                                break;
                        }
                    }
                    else 
                    {
                        onFrame(ev, content);
                    }                    
                    break;
            }

            if (this.OnEventAction != null) this.OnEventAction(ev);
        }

        private void clearRemoteVoices()
        {
            if (this.OnRemoteVoiceRemoveAction != null)
            {
                foreach (var playerVoices in remoteVoices)
                {
                    foreach (var voice in playerVoices.Value)
                    {
                        this.OnRemoteVoiceRemoveAction(playerVoices.Key, voice.Key);
                    }
                }
            }
            remoteVoices.Clear();
            this.DebugReturn(DebugLevel.INFO, "[PV] Remote voices cleared");
        }

        private void onPlayerLeave(int playerId)
        {
            Dictionary<byte, RemoteVoice> playerVoices = null;
            if (this.remoteVoices.TryGetValue(playerId, out playerVoices))
            {
                this.remoteVoices.Remove(playerId);
                this.DebugReturn(DebugLevel.INFO, "[PV] Player " + playerId + " voices removed on leave");
                if (this.OnRemoteVoiceRemoveAction != null)
                {
                    foreach (var v in playerVoices)
                    {
                        this.OnRemoteVoiceRemoveAction(playerId, v.Key);
                    }
                }
            }
            else
            {
                this.DebugReturn(DebugLevel.WARNING, "[PV] Voices of player " + playerId + " not found when trying to remove on player leave");
            }
        }

        private void onVoiceInfo(EventData ev, object payload)
        {
            var playerId = (int)ev[LoadBalancing.ParameterCode.ActorNr];
            Dictionary<byte, RemoteVoice> playerVoices = null;
            if (!this.remoteVoices.TryGetValue(playerId, out playerVoices))
            {
                playerVoices = new Dictionary<byte, RemoteVoice>();
                this.remoteVoices[playerId] = playerVoices;
            }
            playerVoices = this.remoteVoices[playerId];
            foreach (var el in (object[])payload)
            {
                var h = (Hashtable)el;
                var voiceId = (byte)h[(byte)EventParam.VoiceId];
                if (!playerVoices.ContainsKey(voiceId))
                {
                    var samplingRate = (SamplingRate)h[(byte)EventParam.SamplingRate];
                    var channels = (Channels)h[(byte)EventParam.Channels];
                    var encoderDelay = (int)h[(byte)EventParam.EncoderDelay];
                    var bitrate = (int)h[(byte)EventParam.Bitrate];
                    var userData = h[(byte)EventParam.UserData];

                    var eventNumber = (byte)h[(byte)EventParam.EventNumber];

                    this.DebugReturn(DebugLevel.INFO, "[PV] Player " + playerId + " voice #" + voiceId + " info received: f=" + samplingRate + ", ch=" + channels + " d=" + encoderDelay + " b=" + bitrate + " ud=" + userData + " ev=" + eventNumber);

                    var info = new VoiceInfo((int)samplingRate, (int)channels, encoderDelay, bitrate, userData);
                    playerVoices[voiceId] = new RemoteVoice(this, info, samplingRate, channels, eventNumber);
                    if (this.OnRemoteVoiceInfoAction != null) this.OnRemoteVoiceInfoAction(playerId, voiceId, info);
                }
                else
                {
                    this.DebugReturn(DebugLevel.WARNING, "[PV] Info duplicate for voice #" + voiceId + " of player " + playerId);
                }
            }
        }

        private void onVoiceRemove(EventData ev, object payload)
        {
            var playerId = (int)ev[LoadBalancing.ParameterCode.ActorNr];
            var voiceIds = (byte[])payload;
            Dictionary<byte, RemoteVoice> playerVoices = null;

            if (this.remoteVoices.TryGetValue(playerId, out playerVoices))
            {
                foreach (var voiceId in voiceIds)
                {
                    if (playerVoices.Remove(voiceId))
                    {
                        this.DebugReturn(DebugLevel.INFO, "[PV] Voice #" + voiceId + " of player " + playerId + " removed");
                        if (this.OnRemoteVoiceRemoveAction != null)
                        {
                            this.OnRemoteVoiceRemoveAction(playerId, voiceId);
                        }
                    }
                    else
                    {
                        this.DebugReturn(DebugLevel.WARNING, "[PV] Voice #" + voiceId + " of player " + playerId + " not found when trying to remove");
                    }
                }
            }
            else
            {
                this.DebugReturn(DebugLevel.WARNING, "[PV] Voice list of player " + playerId + " not found when trying to remove voice(s)");
            }
        }

        Random rnd = new Random();
        private void onFrame(EventData ev, object[] content)
        {
            var playerId = (int)ev[LoadBalancing.ParameterCode.ActorNr];
            Dictionary<byte, RemoteVoice> playerVoices = null;

            byte voiceId = (byte)content[0];
            byte evNumber = (byte)content[1];
            byte[] receivedBytes = (byte[])content[2];

            if (this.DebugLostPercent > 0 && rnd.Next(100) < this.DebugLostPercent)
            {
                this.DebugReturn(DebugLevel.WARNING, "[PV] Debug Lost Sim: 1 packet dropped");
                return;
            }

            FramesReceived++;

            if (this.remoteVoices.TryGetValue(playerId, out playerVoices))
            {
                RemoteVoice voice = null;
                if (playerVoices.TryGetValue(voiceId, out voice))
                {
                    // receive-gap detection and compensation
                    if (evNumber != voice.lastEvNumber)
                    {
                        int missing = byteDiff(evNumber, voice.lastEvNumber);                        
                        if (missing != 0)
                        {
                            this.DebugReturn(DebugLevel.ALL, "[PV] evNumer: " + evNumber + " playerVoice.lastEvNumber: " + voice.lastEvNumber + " missing: " + missing);
                        }

                        voice.lastEvNumber = evNumber;

                        if (this.UseLossCompensation)
                        {
                            for (int i = 0; i < missing; i++)
                            {
                                receiveFrame(null, voice, playerId, voiceId);
                            }
                        }

                        FramesLost += missing;
                    }
                    receiveFrame(receivedBytes, voice, playerId, voiceId);
                }
                else
                {
                    this.DebugReturn(DebugLevel.WARNING, "[PV] Frame event for not inited voice #" + voiceId + " of player " + playerId);
                }
            }
            else
            {
                this.DebugReturn(DebugLevel.WARNING, "[PV] Frame event for voice #" + voiceId + " of not inited player " + playerId);
            }
        }

        private byte byteDiff(byte latest, byte last)
        {
            return (byte)(latest - (last + 1));
        }

        private void receiveFrame(byte[] frame, RemoteVoice remoteVoice, int playerId, byte voiceId)
        {
            float[] decodedSamples = remoteVoice.decompress(frame);
            if (this.OnAudioFrameAction != null) this.OnAudioFrameAction(playerId, voiceId, decodedSamples);
        }

        //public string ToStringFull()
        //{
        //    return string.Format("Photon.Voice.Client, local: {0}, remote: {1}",  localVoices.Count, remoteVoices.Count);
        //}

        #endregion

    }

    /// <summary>
    /// Audio parameters and data conversion utilities.
    /// </summary>
    public static class VoiceUtil
    {
        internal static void Resample(float[] src, float[] dst, int channels)
        {
            //TODO: Low-pass filter
            for (int i = 0; i < dst.Length; i += channels)
            {
                var interp = (i * src.Length / dst.Length);
                for (int ch = 0; ch < channels; ch++)
                {
                    dst[i + ch] = src[interp + ch];
                }
            }
        }

        /// <summary>Converts frame size (samples*channels) to delay</summary>
        public static int FrameSizeToDelay(int frameSize, int samplingRate, int numChannels)
        {
            return SamplesToDelay(frameSize / numChannels, samplingRate);
        }

        /// <summary>Converts sample conunt to delay</summary>
        public static int SamplesToDelay(int samples, int samplingRate)
        {
            return 2 * samples * 1000 / samplingRate;
        }

        /// <summary>Converts delay to frame size (samples*channels)</summary>
        public static int DelayToFrameSize(int _encoderDelay, int _inputSamplingRate, int numChannels)
        {
            return VoiceUtil.DelayToSamples(_encoderDelay,_inputSamplingRate) * numChannels;
        }

        /// <summary>Converts delay to samples count</summary>
        public static int DelayToSamples(int _encoderDelay, int _inputSamplingRate)
        {
            // as implemented in OpusEncoder.EncoderDelay
            return (int)((((int)_inputSamplingRate) / 1000) * ((decimal)_encoderDelay) / 2);
        }

        internal static string tostr<T>(T[] x, int lim = 10)
        {
            System.Text.StringBuilder b = new System.Text.StringBuilder();
            for (var i = 0; i < (x.Length < lim ? x.Length : lim); i++)
            {
                b.Append("-");
                b.Append(x[i]);
            }
            return b.ToString();
        }

        internal static int bestEncoderSampleRate(int f)
        {
            int diff = int.MaxValue;
            int res = (int)SamplingRate.Sampling48000;
            foreach (var x in Enum.GetValues(typeof(SamplingRate)))
            {
                var d = Math.Abs((int)x - f);
                if (d < diff)
                {
                    diff = d;
                    res = (int)x;
                }
            }
            return res;
        }
    }

    /// <summary>
    /// Utility for measurement audio signal parameters.
    /// </summary>
    public class LevelMeter
    {
        // sum of all values in buffer
        float ampSum;
        // max of values from start buffer to current pos
        float ampPeak;
        int bufferSize;
        float[] buffer;
        int prevValuesPtr;

        float accumAvgPeakAmpSum;
        int accumAvgPeakAmpCount;

        internal LevelMeter(int samplingRate, int numChannels)
        {
            this.bufferSize = samplingRate * numChannels / 2; // 1/2 sec
            this.buffer = new float[this.bufferSize];
        }

        /// <summary>
        /// Average of last values in current 1/2 sec. buffer.
        /// </summary>
        public float CurrentAvgAmp { get { return ampSum / this.bufferSize; } }

        /// <summary>
        /// Max of last values in 1/2 sec. buffer as it was at last buffer wrap.
        /// </summary>
        public float CurrentPeakAmp
        {
            get;
            private set;
        }

        /// <summary>
        /// Average of CurrentPeakAmp's since last reset.
        /// </summary>
        public float AccumAvgPeakAmp { get { return this.accumAvgPeakAmpCount == 0 ? 0 : accumAvgPeakAmpSum / this.accumAvgPeakAmpCount; } }

        /// <summary>
        /// Reset LevelMeter.AccumAvgPeakAmp.
        /// </summary>
        public void ResetAccumAvgPeakAmp() { this.accumAvgPeakAmpSum = 0; this.accumAvgPeakAmpCount = 0; }

        internal void process(float[] buf)
        {
            foreach (var v in buf)
            {
                var a = v;
                if (a < 0)
                {
                    a = -a;
                }
                ampSum = ampSum + a - this.buffer[this.prevValuesPtr];
                this.buffer[this.prevValuesPtr] = a;

                if (ampPeak < a)
                {
                    ampPeak = a;
                }
                if (this.prevValuesPtr == 0)
                {
                    CurrentPeakAmp = ampPeak;
                    ampPeak = 0;
                    accumAvgPeakAmpSum += CurrentPeakAmp;
                    accumAvgPeakAmpCount++;
                }

                this.prevValuesPtr = (this.prevValuesPtr + 1) % this.bufferSize;
            }            
        }
    }

    /// <summary>
    /// Simple voice activity detector triggered by signal level.
    /// </summary>
    public class VoiceDetector
    {
        /// <summary>If true, voice detection enabled.</summary>
        public bool On { get; set; }
        /// <summary>Voice detected as soon as signal level exceeds threshold.</summary>
        public float Threshold { get; set; }

        /// <summary>If true, voice detected.</summary>
        public bool Detected { get; private set; }

        /// <summary>Keep detected state during this time after signal level dropped below threshold.</summary>
        public int ActivityDelayMs {
            get { return this.activityDelay; }
            set {
                this.activityDelay = value;
                this.activityDelayValuesCount = value * valuesCountPerSec / 1000;
            } 
        }

        int activityDelay;
        int autoSilenceCounter = 0;
        int valuesCountPerSec;
        int activityDelayValuesCount;

        internal VoiceDetector(int samplingRate, int numChannels)
        {
            this.valuesCountPerSec = samplingRate * numChannels;
            this.Threshold = 0.01f;
            this.ActivityDelayMs = 500;
        }

        internal void process(float[] buffer)
        {
            if (this.On)
            {
                foreach (var s in buffer)
                {
                    if (s > this.Threshold)
                    {
                        this.Detected = true;
                        this.autoSilenceCounter = 0;
                    }
                    else
                    {
                        this.autoSilenceCounter++;
                    }
                }
                if (this.autoSilenceCounter > this.activityDelayValuesCount)
                {
                    this.Detected = false;
                }
            }
            else
            {
                this.Detected = false;
            }
        }
    }
}