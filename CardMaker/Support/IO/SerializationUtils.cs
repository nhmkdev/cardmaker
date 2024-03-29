////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
//
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
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace Support.IO
{
#warning The exception catching and ignoring causes all developers unhappy feelings. Make the exception available somehow.

    public static class SerializationUtils
    {
        /// <summary>
        /// Generic byte[] Serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tObject"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T tObject)
        {
            var zStream = new MemoryStream();
            var bFormatter = new BinaryFormatter();
            bFormatter.Serialize(zStream, tObject);
            return zStream.ToArray();
        }

        /// <summary>
        /// Generial byte[] Deserializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrayData"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] arrayData)
        {
            var zStream = new MemoryStream(arrayData);
            var bFormatter = new BinaryFormatter();
            return (T)bFormatter.Deserialize(zStream);
        }

        public static bool DeserializeFromXmlString<T>(string sInput, Encoding zEncoding, ref T obj)
        {
            try
            {
                var zSerializer = new XmlSerializer(typeof (T));
                var zStream = new MemoryStream(zEncoding.GetBytes(sInput));
                TextReader zTextReader = new StreamReader(zStream, zEncoding);

                obj = (T) zSerializer.Deserialize(zTextReader);
                zStream.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SerializeToXmlFile<T>(string sFile, T tObject, Encoding zEncoding)
        {
            try
            {
                var zSerializer = new XmlSerializer(typeof(T));
                if (File.Exists(sFile))
                    File.Delete(sFile);
                var zStream = File.OpenWrite(sFile);

                zSerializer.Serialize(zStream, tObject);
                zStream.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeserializeFromXmlFile<T>(string sFile, Encoding zEncoding, ref T obj)
        {
            FileStream zStream = null;
            try
            {
                var zSerializer = new XmlSerializer(typeof(T));
                zStream = File.OpenRead(sFile);
                TextReader zTextReader = new StreamReader(zStream, zEncoding);

                obj = (T)zSerializer.Deserialize(zTextReader);
                zStream.Close();
                return true;
            }
            catch(Exception){}
            zStream?.Close();
            return false;
        }
    }
}