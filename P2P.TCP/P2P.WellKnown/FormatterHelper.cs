using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// FormatterHelper 序列化，反序列化消息的帮助类
    /// </summary>

    public class FormatterHelper
    {
        /// <summary>
        /// 对象序列化为二进制比特数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {

            BinaryFormatter binaryF = new BinaryFormatter();

            MemoryStream ms = new MemoryStream(1024 * 10);

            binaryF.Serialize(ms, obj);

            ms.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[(int)ms.Length];

            ms.Read(buffer, 0, buffer.Length);

            ms.Close();

            return buffer;

        }


        /// <summary>
        /// 反序列化得到一个对象
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static object Deserialize(byte[] buffer)
        {

            BinaryFormatter binaryF = new BinaryFormatter();

            MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length, false);

            object obj = binaryF.Deserialize(ms);

            ms.Close();

            return obj;

        }

    }
}
