using System;
using System.IO;

namespace Foster.Framework
{
    public static class Wave
    {
        public enum Format : short
        {
            None = 0,
            PCM = 1,
            MSADPCM = 2,
            IEEE = 3,
            IMA4 = 17
        }

        private static short ReadSample8(byte[] buffer, int index)
        {
            return (short)(((buffer[index] - sbyte.MaxValue) / (float)sbyte.MaxValue) * short.MaxValue);
        }

        private static short ReadSample16(byte[] buffer, int i)
        {
            return (short)(buffer[i] | (buffer[i + 1] << 8));
        }

        private static short ReadSample24(byte[] buffer, int index)
        {
            var value = buffer[index] | (buffer[index + 1] << 8) | (buffer[index + 2] << 16);

            var normalized = value / (float)(1 << 23);
            return (short)(normalized * short.MaxValue);
        }

        private static short ReadSample32(byte[] buffer, int index)
        {
            var value = buffer[index] |
                        ((uint)buffer[index + 1] << 8) |
                        ((uint)buffer[index + 2] << 16) |
                        ((uint)buffer[index + 3] << 24);

            var normalized = value / (double)int.MaxValue;
            return (short)(normalized * short.MaxValue);
        }

        private static short ReadSample64(byte[] buffer, int index)
        {
            var value = buffer[index] |
                        ((long)buffer[index + 1] << 8) |
                        ((long)buffer[index + 2] << 16) |
                        ((long)buffer[index + 3] << 24) |
                        ((long)buffer[index + 4] << 32) |
                        ((long)buffer[index + 5] << 40) |
                        ((long)buffer[index + 6] << 48) |
                        ((long)buffer[index + 7] << 56);

            var normalized = value / (double)long.MaxValue;
            return (short)(normalized * short.MaxValue);
        }

        public static int GetSampleAlignment(Format format, AudioChannel channels, int blockAlignment)
        {
            return format switch
            {
                Format.IMA4 => ((((blockAlignment - (4 / (int)channels)) / 4) * 8) + 1),
                Format.MSADPCM => ((((blockAlignment / (int)channels) - 7) * 2) + 2),
                _ => 0
            };
        }

        public static bool TryLoad(
            Stream stream,
            out byte[] buffer,
            out Format format,
            out int frequency,
            out AudioChannel channels,
            out int blockAlignment,
            out int bitsPerSample,
            out int samplesPerBlock,
            out int sampleCount)
        {
            buffer = Array.Empty<byte>();
            format = Format.None;
            frequency = 0;
            channels = AudioChannel.None;
            blockAlignment = 0;
            bitsPerSample = 0;
            samplesPerBlock = 0;
            sampleCount = 0;

            using var reader = new BinaryReader(stream);

            var signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
            {
                Log.Error("Stream is not a wave file");
                return false;
            }

            reader.BaseStream.Position += 4; // Skip RIFF chunksize

            var waveFormat = new string(reader.ReadChars(4));
            if (waveFormat != "WAVE")
            {
                Log.Error("Stream is not a wave file");
                buffer = Array.Empty<byte>();
                return false;
            }

            var streamLength = reader.BaseStream.Length;
            var bufferFilled = false;
            while (!bufferFilled)
            {
                var chunkId = new string(reader.ReadChars(4));
                var chunkSize = reader.ReadInt32();

                if ((reader.BaseStream.Position + chunkSize) > streamLength)
                {
                    Log.Error("Wave format header is invalid");
                    return false;
                }

                switch (chunkId)
                {
                    case "fmt ":
                        {
                            format = (Format)reader.ReadInt16();
                            chunkSize -= 2;

                            if (!Enum.IsDefined(typeof(Format), format))
                            {
                                Log.Error($"Wave format is not recognized: {format}");
                                return false;
                            }

                            channels = (AudioChannel)reader.ReadInt16();
                            chunkSize -= 2;

                            if ((channels != AudioChannel.Mono) && (channels != AudioChannel.Stereo))
                            {
                                Log.Error($"Wave format does not support {channels} channels");
                                return false;
                            }

                            frequency = reader.ReadInt32();
                            chunkSize -= 4;

                            reader.BaseStream.Position += 4; // Skip fmt ByteRate
                            chunkSize -= 4;

                            blockAlignment = reader.ReadInt16();
                            chunkSize -= 2;

                            bitsPerSample = reader.ReadInt16();
                            chunkSize -= 2;

                            if (chunkSize > 0)
                            {
                                if (format != Format.PCM)
                                {
                                    var extraDataSize = reader.ReadInt16();
                                    if (format == Format.IMA4)
                                    {
                                        samplesPerBlock = reader.ReadInt16();
                                        extraDataSize -= 2;
                                    }

                                    reader.BaseStream.Position += extraDataSize;
                                }
                                else
                                {
                                    reader.BaseStream.Position += chunkSize;
                                }
                            }

                            break;
                        }
                    case "fact":
                        {
                            if (format == Format.IMA4)
                            {
                                sampleCount = reader.ReadInt32() * (int)channels;
                                chunkSize -= 4;
                            }

                            reader.BaseStream.Position += chunkSize;
                            break;
                        }
                    case "data":
                        {
                            buffer = reader.ReadBytes(chunkSize);
                            bufferFilled = true;
                            break;
                        }
                    default:
                        {
                            reader.BaseStream.Position += chunkSize;
                            break;
                        }
                }
            }

            if (samplesPerBlock == 0)
            {
                samplesPerBlock = GetSampleAlignment(format, channels, blockAlignment);
            }

            if (sampleCount == 0)
            {
                switch (format)
                {
                    case Format.IMA4:
                    case Format.MSADPCM:
                        {
                            sampleCount = ((buffer.Length / blockAlignment) * samplesPerBlock) +
                                          GetSampleAlignment(format, channels, buffer.Length % blockAlignment);

                            break;
                        }
                    case Format.PCM:
                    case Format.IEEE:
                        {
                            sampleCount = buffer.Length / (((int)channels * bitsPerSample) / 8);
                            break;
                        }
                    default:
                        {
                            Log.Error($"Wave format is not recognized: {format}");
                            return false;
                        }
                }
            }

            return true;
        }

        public static short[] ConvertPCM(byte[] buffer, int bitsPerSample)
        {
            if (bitsPerSample < 8 || bitsPerSample > 64 || (bitsPerSample & 7) > 0)
            {
                Log.Error($"Bitrate of {bitsPerSample.ToString()} is not supported by PCM");
                return Array.Empty<short>();
            }

            var bytesPerSample = bitsPerSample / 8;
            var outBuffer = new short[buffer.Length / bytesPerSample];
            for (var i = 0; i < outBuffer.Length; ++i)
            {
                outBuffer[i] = bitsPerSample switch
                {
                    8 => ReadSample8(buffer, i * bytesPerSample),
                    16 => ReadSample16(buffer, i * bytesPerSample),
                    24 => ReadSample24(buffer, i * bytesPerSample),
                    32 => ReadSample32(buffer, i * bytesPerSample),
                    64 => ReadSample64(buffer, i * bytesPerSample),
                    _ => 0
                };
            }

            return outBuffer;
        }
    }
}