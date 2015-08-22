/////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006, Frank Blumenberg
// 
// See License.txt for complete licensing and attribution information.
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
// 
/////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////
//
// This code contains code from the Endogine sprite engine by Jonas Beckeman.
// http://www.endogine.com/CS/
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PhotoshopFile
{
    public class PsdFile
    {
        public enum ColorModes
        {
            Bitmap = 0,
            Grayscale = 1,
            Indexed = 2,
            RGB = 3,
            CMYK = 4,
            Multichannel = 7,
            Duotone = 8,
            Lab = 9
        };

        ///////////////////////////////////////////////////////////////////////////

        public void Load(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                Load(stream);
            }
        }

        public void Load(Stream stream)
        {
            var reader = new BinaryReverseReader(stream);

            LoadHeader(reader);
            LoadColorModeData(reader);
            LoadImageResources(reader);
            LoadLayerAndMaskInfo(reader);

            LoadImage(reader);
        }

        ///////////////////////////////////////////////////////////////////////////
        // Modification to skip image reading when only seeking information

        public void Load(string fileName, bool bReadImageData)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                Load(stream, bReadImageData);
            }
        }

        public void Load(Stream stream, bool bReadImageData)
        {
            var reader = new BinaryReverseReader(stream);

            LoadHeader(reader);
            LoadColorModeData(reader);
            LoadImageResources(reader);
            LoadLayerAndMaskInfo(reader);

            if (bReadImageData)
            {
                LoadImage(reader);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        public void Save(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                Save(stream);
            }
        }

        public void Save(Stream stream)
        {
            var writer = new BinaryReverseWriter(stream)
            {
                AutoFlush = true
            };

            SaveHeader(writer);
            SaveColorModeData(writer);
            SaveImageResources(writer);
            SaveLayerAndMaskInfo(writer);
            SaveImage(writer);
        }

        ///////////////////////////////////////////////////////////////////////////

        #region Header

        /// <summary>
        /// Always equal to 1.
        /// </summary>
        private short m_version = 1;

        public short Version
        {
            get { return m_version; }
        }

        private short m_channels;

        /// <summary>
        /// The number of channels in the image, including any alpha channels.
        /// Supported range is 1 to 24.
        /// </summary>
        public short Channels
        {
            get { return m_channels; }
            set
            {
                if (value < 1 || value > 24)
                    throw new ArgumentException("Supported range is 1 to 24");
                m_channels = value;
            }
        }


        private int m_rows;

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Rows
        {
            get { return m_rows; }
            set
            {
                if (value < 0 || value > 30000)
                    throw new ArgumentException("Supported range is 1 to 30000.");
                m_rows = value;
            }
        }


        private int m_columns;

        /// <summary>
        /// The width of the image in pixels. 
        /// </summary>
        public int Columns
        {
            get { return m_columns; }
            set
            {
                if (value < 0 || value > 30000)
                    throw new ArgumentException("Supported range is 1 to 30000.");
                m_columns = value;
            }
        }


        private int m_depth;

        /// <summary>
        /// The number of bits per channel. Supported values are 1, 8, and 16.
        /// </summary>
        public int Depth
        {
            get { return m_depth; }
            set
            {
                if (value == 1 || value == 8 || value == 16)
                    m_depth = value;
                else
                    throw new ArgumentException("Supported values are 1, 8, and 16.");
            }
        }

        private ColorModes m_colorMode;

        /// <summary>
        /// The color mode of the file.
        /// </summary>
        public ColorModes ColorMode
        {
            get { return m_colorMode; }
            set { m_colorMode = value; }
        }


        ///////////////////////////////////////////////////////////////////////////

        private void LoadHeader(BinaryReverseReader reader)
        {
            Debug.WriteLine("LoadHeader started at " + reader.BaseStream.Position);

            var signature = new string(reader.ReadChars(4));
            if (signature != "8BPS")
                throw new IOException("The given stream is not a valid PSD file");

            m_version = reader.ReadInt16();
            if (m_version != 1)
                throw new IOException("The PSD file has an unkown version");

            //6 bytes reserved
            reader.BaseStream.Position += 6;

            m_channels = reader.ReadInt16();
            m_rows = reader.ReadInt32();
            m_columns = reader.ReadInt32();
            m_depth = reader.ReadInt16();
            m_colorMode = (ColorModes) reader.ReadInt16();
        }

        ///////////////////////////////////////////////////////////////////////////

        private const string EightBPS = "8BPS";
        private static readonly char[] ArrayEightBPS = EightBPS.ToCharArray();

        private void SaveHeader(BinaryReverseWriter writer)
        {
            Debug.WriteLine("SaveHeader started at " + writer.BaseStream.Position);

            writer.Write(ArrayEightBPS);
            writer.Write(Version);
            writer.Write(new byte[] {0x0, 0x0, 0x0, 0x0, 0x0, 0x0});
            writer.Write(m_channels);
            writer.Write(m_rows);
            writer.Write(m_columns);
            writer.Write((short) m_depth);
            writer.Write((short) m_colorMode);
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region ColorModeData

        /// <summary>
        /// If ColorMode is ColorModes.Indexed, the following 768 bytes will contain 
        /// a 256-color palette. If the ColorMode is ColorModes.Duotone, the data 
        /// following presumably consists of screen parameters and other related information. 
        /// Unfortunately, it is intentionally not documented by Adobe, and non-Photoshop 
        /// readers are advised to treat duotone images as gray-scale images.
        /// </summary>
        public byte[] ColorModeData = new byte[0];

        private void LoadColorModeData(BinaryReverseReader reader)
        {
            Debug.WriteLine("LoadColorModeData started at " + reader.BaseStream.Position);

            uint paletteLength = reader.ReadUInt32();
            if (paletteLength > 0)
            {
                ColorModeData = reader.ReadBytes((int) paletteLength);
            }
        }

        private void SaveColorModeData(BinaryReverseWriter writer)
        {
            Debug.WriteLine("SaveColorModeData started at " + writer.BaseStream.Position);

            writer.Write((uint) ColorModeData.Length);
            writer.Write(ColorModeData);
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region ImageResources

        private readonly List<ImageResource> m_imageResources = new List<ImageResource>();

        /// <summary>
        /// The Image resource blocks for the file
        /// </summary>
        public List<ImageResource> ImageResources
        {
            get { return m_imageResources; }
        }


        // This method implements the test condition for 
        // finding the ResolutionInfo.
        private static bool IsResolutionInfo(ImageResource res)
        {
            return res.ID == (int) ResourceIDs.ResolutionInfo;
        }

        public ResolutionInfo Resolution
        {
            get { return (ResolutionInfo) m_imageResources.Find(IsResolutionInfo); }

            set
            {
                ImageResource oldValue = m_imageResources.Find(IsResolutionInfo);
                if (oldValue != null)
                    m_imageResources.Remove(oldValue);

                m_imageResources.Add(value);
            }
        }


        ///////////////////////////////////////////////////////////////////////////

        private void LoadImageResources(BinaryReverseReader reader)
        {
            Debug.WriteLine("LoadImageResources started at " + reader.BaseStream.Position);

            m_imageResources.Clear();

            uint imgResLength = reader.ReadUInt32();
            if (imgResLength <= 0)
                return;

            long startPosition = reader.BaseStream.Position;

            while ((reader.BaseStream.Position - startPosition) < imgResLength)
            {
                var imgRes = new ImageResource(reader);

                var resID = (ResourceIDs)imgRes.ID;
                switch (resID)
                {
                    case ResourceIDs.ResolutionInfo:
                        imgRes = new ResolutionInfo(imgRes);
                        break;
                    case ResourceIDs.Thumbnail1:
                    case ResourceIDs.Thumbnail2:
                        imgRes = new Thumbnail(imgRes);
                        break;
                    case ResourceIDs.AlphaChannelNames:
                        imgRes = new AlphaChannels(imgRes);
                        break;
                }

                m_imageResources.Add(imgRes);
            }

            //-----------------------------------------------------------------------
            // make shure we are not on a wrong offset, so set the stream position 
            // manually
            reader.BaseStream.Position = startPosition + imgResLength;
        }

        ///////////////////////////////////////////////////////////////////////////

        private void SaveImageResources(BinaryReverseWriter writer)
        {
            Debug.WriteLine("SaveImageResources started at " + writer.BaseStream.Position);

            using (new LengthWriter(writer))
            {
                foreach (ImageResource imgRes in m_imageResources)
                    imgRes.Save(writer);
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region LayerAndMaskInfo

        private readonly List<Layer> m_layers = new List<Layer>();

        public List<Layer> Layers
        {
            get { return m_layers; }
        }

        public bool AbsoluteAlpha { get; set; }

        ///////////////////////////////////////////////////////////////////////////

        private void LoadLayerAndMaskInfo(BinaryReverseReader reader)
        {
            Debug.WriteLine("LoadLayerAndMaskInfo started at " + reader.BaseStream.Position);

            uint layersAndMaskLength = reader.ReadUInt32();

            if (layersAndMaskLength <= 0)
                return;

            long startPosition = reader.BaseStream.Position;

            LoadLayers(reader);
            LoadGlobalLayerMask(reader);

            //-----------------------------------------------------------------------

            //Debug.Assert(reader.BaseStream.Position == startPosition + layersAndMaskLength, "LoadLayerAndMaskInfo");

            //-----------------------------------------------------------------------
            // make shure we are not on a wrong offset, so set the stream position 
            // manually
            reader.BaseStream.Position = startPosition + layersAndMaskLength;
        }

        ///////////////////////////////////////////////////////////////////////////

        private void SaveLayerAndMaskInfo(BinaryReverseWriter writer)
        {
            Debug.WriteLine("SaveLayerAndMaskInfo started at " + writer.BaseStream.Position);

            using (new LengthWriter(writer))
            {
                SaveLayers(writer);
                SaveGlobalLayerMask(writer);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        private void LoadLayers(BinaryReverseReader reader)
        {
            Debug.WriteLine("LoadLayers started at " + reader.BaseStream.Position);

            uint layersInfoSectionLength = reader.ReadUInt32();

            if (layersInfoSectionLength <= 0)
                return;

            long startPosition = reader.BaseStream.Position;

            short numberOfLayers = reader.ReadInt16();

            // If <0, then number of layers is absolute value,
            // and the first alpha channel contains the transparency data for
            // the merged result.
            if (numberOfLayers < 0)
            {
                AbsoluteAlpha = true;
                numberOfLayers = Math.Abs(numberOfLayers);
            }

            m_layers.Clear();

            if (numberOfLayers == 0)
                return;

            for (int i = 0; i < numberOfLayers; i++)
            {
                m_layers.Add(new Layer(reader, this));
            }

            foreach (Layer layer in m_layers)
            {
                foreach (Layer.Channel channel in layer.Channels)
                {
                    if (channel.ID != -2)
                        channel.LoadPixelData(reader);
                }

                layer.MaskData.LoadPixelData(reader);
            }

            //-----------------------------------------------------------------------

            if (reader.BaseStream.Position%2 == 1)
                reader.ReadByte();

            //-----------------------------------------------------------------------
            // make shure we are not on a wrong offset, so set the stream position 
            // manually
            reader.BaseStream.Position = startPosition + layersInfoSectionLength;
        }

        ///////////////////////////////////////////////////////////////////////////

        private void SaveLayers(BinaryReverseWriter writer)
        {
            Debug.WriteLine("SaveLayers started at " + writer.BaseStream.Position);

            using (new LengthWriter(writer))
            {
                var numberOfLayers = (short)m_layers.Count;
                if (AbsoluteAlpha)
                    numberOfLayers = (short) -numberOfLayers;

                writer.Write(numberOfLayers);

                foreach (var layer in m_layers)
                {
                    layer.Save(writer);
                }

                foreach (var layer in m_layers)
                {
                    foreach (Layer.Channel channel in layer.Channels)
                    {
                        channel.SavePixelData(writer);
                    }
                }

                if (writer.BaseStream.Position%2 == 1)
                    writer.Write((byte) 0);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        private byte[] GlobalLayerMaskData = new byte[0];

        private void LoadGlobalLayerMask(BinaryReverseReader reader)
        {
            Debug.WriteLine("LoadGlobalLayerMask started at " + reader.BaseStream.Position);

            uint maskLength = reader.ReadUInt32();

            if (maskLength <= 0)
                return;

            GlobalLayerMaskData = reader.ReadBytes((int) maskLength);
        }

        ///////////////////////////////////////////////////////////////////////////

        private void SaveGlobalLayerMask(BinaryReverseWriter writer)
        {
            Debug.WriteLine("SaveGlobalLayerMask started at " + writer.BaseStream.Position);

            writer.Write((uint) GlobalLayerMaskData.Length);
            writer.Write(GlobalLayerMaskData);
        }

        ///////////////////////////////////////////////////////////////////////////

        #endregion

        ///////////////////////////////////////////////////////////////////////////

        #region ImageData

        /// <summary>
        /// The raw image data from the file, seperated by the channels.
        /// </summary>
        public byte[][] m_imageData;

        public byte[][] ImageData
        {
            get { return m_imageData; }
            set { m_imageData = value; }
        }


        private ImageCompression m_imageCompression;

        public ImageCompression ImageCompression
        {
            get { return m_imageCompression; }
            set { m_imageCompression = value; }
        }


        ///////////////////////////////////////////////////////////////////////////

        private void LoadImage(BinaryReverseReader reader)
        {
            Debug.WriteLine("LoadImage started at " + reader.BaseStream.Position);

            m_imageCompression = (ImageCompression) reader.ReadInt16();

            m_imageData = new byte[m_channels][];

            //---------------------------------------------------------------

            if (m_imageCompression == ImageCompression.Rle)
            {
                // The RLE-compressed data is proceeded by a 2-byte data count for each row in the data,
                // which we're going to just skip.
                reader.BaseStream.Position += m_rows*m_channels*2;
            }

            //---------------------------------------------------------------

            int bytesPerRow = 0;

            switch (m_depth)
            {
                case 1:
                    bytesPerRow = m_columns; //NOT Shure
                    break;
                case 8:
                    bytesPerRow = m_columns;
                    break;
                case 16:
                    bytesPerRow = m_columns*2;
                    break;
            }

            //---------------------------------------------------------------

            for (int ch = 0; ch < m_channels; ch++)
            {
                m_imageData[ch] = new byte[m_rows*bytesPerRow];

                switch (m_imageCompression)
                {
                    case ImageCompression.Raw:
                        reader.Read(m_imageData[ch], 0, m_imageData[ch].Length);
                        break;
                    case ImageCompression.Rle:
                    {
                        for (int i = 0; i < m_rows; i++)
                        {
                            int rowIndex = i*m_columns;
                            RleHelper.DecodedRow(reader.BaseStream, m_imageData[ch], rowIndex, bytesPerRow);
                        }
                    }
                        break;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        private void SaveImage(BinaryReverseWriter writer)
        {
            Debug.WriteLine("SaveImage started at " + writer.BaseStream.Position);

            writer.Write((short) m_imageCompression);

            if (m_imageCompression == ImageCompression.Rle)
            {
                SaveImageRLE(writer);
            }
            else
            {
                for (int ch = 0; ch < m_channels; ch++)
                {
                    writer.Write(m_imageData[ch]);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        private void SaveImageRLE(BinaryReverseWriter writer)
        {
            // we will write the correct lengths later, so remember 
            // the position
            var lengthPosition = writer.BaseStream.Position;

            var rleRowLenghs = new int[m_rows * m_channels];

            if (m_imageCompression == ImageCompression.Rle)
            {
                for (int i = 0; i < rleRowLenghs.Length; i++)
                {
                    writer.Write((short) 0x1234);
                }
            }

            //---------------------------------------------------------------

            for (int ch = 0; ch < m_channels; ch++)
            {
                var startIdx = ch * m_rows;

                for (int row = 0; row < m_rows; row++)
                    rleRowLenghs[row + startIdx] = RleHelper.EncodedRow(writer.BaseStream, m_imageData[ch],
                        row*m_columns, m_columns);
            }

            //---------------------------------------------------------------

            long endPosition = writer.BaseStream.Position;

            writer.BaseStream.Position = lengthPosition;

            for (var i = 0; i < rleRowLenghs.Length; i++)
            {
                writer.Write((short) rleRowLenghs[i]);
            }

            writer.BaseStream.Position = endPosition;

            //---------------------------------------------------------------
        }

        ///////////////////////////////////////////////////////////////////////////

        #endregion
    }

    /// <summary>
    /// The possible Compression methods.
    /// </summary>
    public enum ImageCompression
    {
        /// <summary>
        /// Raw data
        /// </summary>
        Raw = 0,

        /// <summary>
        /// RLE compressed
        /// </summary>
        Rle = 1,

        /// <summary>
        /// ZIP without prediction.
        /// <remarks>
        /// This is currently not supported since it is ot documented.
        /// Loading will result in an image where all channels are set to zero.
        /// </remarks>
        /// </summary>
        Zip = 2,

        /// <summary>
        /// ZIP with prediction.
        /// <remarks>
        /// This is currently not supported since it is ot documented. 
        /// Loading will result in an image where all channels are set to zero.
        /// </remarks>
        /// </summary>
        ZipPrediction = 3
    }


    internal class RleHelper
    {
        ////////////////////////////////////////////////////////////////////////

        private class RlePacketStateMachine
        {
            private bool m_rlePacket;
            private readonly byte[] m_packetValues = new byte[128];
            private int packetLength;
            private readonly Stream m_stream;

            internal void Flush()
            {
                byte header;
                if (m_rlePacket)
                {
                    header = (byte) (-(packetLength - 1));
                }
                else
                {
                    header = (byte) (packetLength - 1);
                }

                m_stream.WriteByte(header);

                int length = (m_rlePacket ? 1 : packetLength);

                m_stream.Write(m_packetValues, 0, length);

                packetLength = 0;
            }

            internal void Push(byte color)
            {
                if (packetLength == 0)
                {
                    // Starting a fresh packet.
                    m_rlePacket = false;
                    m_packetValues[0] = color;
                    packetLength = 1;
                }
                else if (packetLength == 1)
                {
                    // 2nd byte of this packet... decide RLE or non-RLE.
                    m_rlePacket = (color == m_packetValues[0]);
                    m_packetValues[1] = color;
                    packetLength = 2;
                }
                else if (packetLength == m_packetValues.Length)
                {
                    // Packet is full. Start a new one.
                    Flush();
                    Push(color);
                }
                else if (packetLength >= 2 && m_rlePacket && color != m_packetValues[packetLength - 1])
                {
                    // We were filling in an RLE packet, and we got a non-repeated color.
                    // Emit the current packet and start a new one.
                    Flush();
                    Push(color);
                }
                else if (packetLength >= 2 && m_rlePacket && color == m_packetValues[packetLength - 1])
                {
                    // We are filling in an RLE packet, and we got another repeated color.
                    // Add the new color to the current packet.
                    ++packetLength;
                    m_packetValues[packetLength - 1] = color;
                }
                else if (packetLength >= 2 && !m_rlePacket && color != m_packetValues[packetLength - 1])
                {
                    // We are filling in a raw packet, and we got another random color.
                    // Add the new color to the current packet.
                    ++packetLength;
                    m_packetValues[packetLength - 1] = color;
                }
                else if (packetLength >= 2 && !m_rlePacket && color == m_packetValues[packetLength - 1])
                {
                    // We were filling in a raw packet, but we got a repeated color.
                    // Emit the current packet without its last color, and start a
                    // new RLE packet that starts with a length of 2.
                    --packetLength;
                    Flush();
                    Push(color);
                    Push(color);
                }
            }

            internal RlePacketStateMachine(Stream stream)
            {
                m_stream = stream;
            }
        }

        ////////////////////////////////////////////////////////////////////////

        public static int EncodedRow(Stream stream, byte[] imgData, int startIdx, int columns)
        {
            long startPosition = stream.Position;

            var machine = new RlePacketStateMachine(stream);

            for (int x = 0; x < columns; ++x)
                machine.Push(imgData[x + startIdx]);

            machine.Flush();

            return (int) (stream.Position - startPosition);
        }

        ////////////////////////////////////////////////////////////////////////

        public static void DecodedRow(Stream stream, byte[] imgData, int startIdx, int columns)
        {
            int count = 0;
            while (count < columns)
            {
                var byteValue = (byte)stream.ReadByte();

                var len = (int)byteValue;
                if (len < 128)
                {
                    len++;
                    while (len != 0 && (startIdx + count) < imgData.Length)
                    {
                        byteValue = (byte) stream.ReadByte();

                        imgData[startIdx + count] = byteValue;
                        count++;
                        len--;
                    }
                }
                else if (len > 128)
                {
                    // Next -len+1 bytes in the dest are replicated from next source byte.
                    // (Interpret len as a negative 8-bit int.)
                    len ^= 0x0FF;
                    len += 2;
                    byteValue = (byte) stream.ReadByte();

                    while (len != 0 && (startIdx + count) < imgData.Length)
                    {
                        imgData[startIdx + count] = byteValue;
                        count++;
                        len--;
                    }
                }
                else if (128 == len)
                {
                    // Do nothing
                }
            }
        }
    }
}