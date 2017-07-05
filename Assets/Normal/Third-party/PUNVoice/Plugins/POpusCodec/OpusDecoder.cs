using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POpusCodec.Enums;
using System.Runtime.InteropServices;

namespace POpusCodec
{
    public class OpusDecoder : IDisposable
    {
        private IntPtr _handle = IntPtr.Zero;
        private string _version = string.Empty;
        private const int MaxFrameSize = 5760;
#pragma warning disable 414 // "not used" warning
        private bool _previousPacketInvalid = false; // TODO: _previousPacketInvalid breaks lost decoding currently
#pragma warning restore 414
        private int _channelCount = 2;

        public string Version
        {
            get
            {
                return _version;
            }
        }

        private Bandwidth? _previousPacketBandwidth = null;

        public Bandwidth? PreviousPacketBandwidth
        {
            get
            {
                return _previousPacketBandwidth;
            }
        }

        public OpusDecoder(SamplingRate outputSamplingRateHz, Channels numChannels)
        {
            if ((outputSamplingRateHz != SamplingRate.Sampling08000)
                && (outputSamplingRateHz != SamplingRate.Sampling12000)
                && (outputSamplingRateHz != SamplingRate.Sampling16000)
                && (outputSamplingRateHz != SamplingRate.Sampling24000)
                && (outputSamplingRateHz != SamplingRate.Sampling48000))
            {
                throw new ArgumentOutOfRangeException("outputSamplingRateHz", "Must use one of the pre-defined sampling rates (" + outputSamplingRateHz + ")");
            }
            if ((numChannels != Channels.Mono)
                && (numChannels != Channels.Stereo))
            {
                throw new ArgumentOutOfRangeException("numChannels", "Must be Mono or Stereo");
            }

            _channelCount = (int)numChannels;
            _handle = Wrapper.opus_decoder_create(outputSamplingRateHz, numChannels);
            _version = Marshal.PtrToStringAnsi( Wrapper.opus_get_version_string());

            if (_handle == IntPtr.Zero)
            {
                throw new OpusException(OpusStatusCode.AllocFail, "Memory was not allocated for the encoder");
            }
        }

        public short[] DecodePacketLost()
        {
            _previousPacketInvalid = true;

            var lastPacketDur = Wrapper.get_opus_decoder_ctl(_handle, OpusCtlGetRequest.LastPacketDurationRequest);

            short[] tempData = new short[lastPacketDur /*MaxFrameSize*/ * _channelCount];

            int numSamplesDecoded = Wrapper.opus_decode(_handle, null, tempData, 0, _channelCount);

            if (numSamplesDecoded == 0)
                return new short[] { };

            short[] pcm = new short[numSamplesDecoded * _channelCount];
            Buffer.BlockCopy(tempData, 0, pcm, 0, pcm.Length * sizeof(short));

            return pcm;
        }


        private float[] lostDataFloats;     // this is just a empty float[] of the size of the missing package. it can be re-used
        public float[] DecodePacketLostFloat()
        {
            _previousPacketInvalid = true;

            var lastPacketDur = Wrapper.get_opus_decoder_ctl(_handle, OpusCtlGetRequest.LastPacketDurationRequest);

            if (lostDataFloats == null || lostDataFloats.Length != (lastPacketDur /*MaxFrameSize*/*_channelCount))
            {
                lostDataFloats = new float[lastPacketDur /*MaxFrameSize*/ * _channelCount];
            }

            int numSamplesDecoded = Wrapper.opus_decode(_handle, null, lostDataFloats, 0, _channelCount);

            if (numSamplesDecoded == 0)
                return new float[] { };


            if (pcm == null || pcm.Length != (numSamplesDecoded * _channelCount))
            {
                pcm = new float[numSamplesDecoded * _channelCount];
            }
            Buffer.BlockCopy(lostDataFloats, 0, pcm, 0, pcm.Length * sizeof(float));

            return pcm;
        }


        public short[] DecodePacket(byte[] packetData)
        {
            short[] tempData = new short[MaxFrameSize * _channelCount];

            int bandwidth = Wrapper.opus_packet_get_bandwidth(packetData);

            int numSamplesDecoded = 0;

            if (bandwidth == (int)OpusStatusCode.InvalidPacket)
            {
                numSamplesDecoded = Wrapper.opus_decode(_handle, null, tempData, 0, _channelCount);
                _previousPacketInvalid = true;
            }
            else
            {
                _previousPacketBandwidth = (Bandwidth)bandwidth;
                // TODO: _previousPacketInvalid breaks lost decoding currently
                numSamplesDecoded = Wrapper.opus_decode(_handle, packetData, tempData, /* _previousPacketInvalid ? 1 :*/ 0, _channelCount);

                _previousPacketInvalid = false;
            }

            if (numSamplesDecoded == 0)
                return new short[] { };

            short[] pcm = new short[numSamplesDecoded * _channelCount];
            Buffer.BlockCopy(tempData, 0, pcm, 0, pcm.Length * sizeof(short));

            return pcm;
        }

        float[] tempData;
        public float[] DecodePacketFloat(byte[] packetData)
        {
            if (tempData == null || tempData.Length != MaxFrameSize * _channelCount)
            {
                tempData = new float[MaxFrameSize * _channelCount];
            }

            int bandwidth = Wrapper.opus_packet_get_bandwidth(packetData);

            int numSamplesDecoded = 0;

            if (bandwidth == (int)OpusStatusCode.InvalidPacket)
            {
                numSamplesDecoded = Wrapper.opus_decode(_handle, null, tempData, 0, _channelCount);
                _previousPacketInvalid = true;
            }
            else
            {
                _previousPacketBandwidth = (Bandwidth)bandwidth;
                // TODO: _previousPacketInvalid breaks lost decoding currently
                numSamplesDecoded = Wrapper.opus_decode(_handle, packetData, tempData, /* _previousPacketInvalid ? 1 : */ 0, _channelCount);

                _previousPacketInvalid = false;
            }

            if (numSamplesDecoded == 0)
                return new float[] { };

            if (pcm == null || pcm.Length != (numSamplesDecoded*_channelCount))
            {
                pcm = new float[numSamplesDecoded * _channelCount];
            }
            Buffer.BlockCopy(tempData, 0, pcm, 0, pcm.Length * sizeof(float));

            return pcm;
        }

        private float[] pcm;    // we can re-use this array, as the clip is copying the data internally (and we add this to the clip immediately)

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                Wrapper.opus_decoder_destroy(_handle);
                _handle = IntPtr.Zero;
            }
        }
    }
}
