using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DotNEToolkit
{
    public class XmlHelper
    {
        private static XmlSerializerNamespaces emptyNameSpace;
        private static XmlWriterSettings defaultWriterSettings;

        static XmlHelper()
        {
            defaultWriterSettings = new XmlWriterSettings();
            defaultWriterSettings.OmitXmlDeclaration = true;
            defaultWriterSettings.Encoding = Encoding.UTF8;

            emptyNameSpace = new XmlSerializerNamespaces();
            emptyNameSpace.Add("", "");

            SerializerNamespaces = emptyNameSpace;
            WritterSerializerSettings = defaultWriterSettings;
        }

        public static XmlWriterSettings WritterSerializerSettings
        {
            get;
            set;
        }

        public static XmlSerializerNamespaces SerializerNamespaces
        {
            get;
            set;
        }


        public static XmlNode ToXmlNode<ITEM>(ITEM item)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(memStream, WritterSerializerSettings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ITEM));
                    serializer.Serialize(writer, item, SerializerNamespaces);
                    writer.Flush();
                }

                memStream.Position = 0;
                XmlSerializer nodeSerializer = new XmlSerializer(typeof(XmlNode));
                return (XmlNode)nodeSerializer.Deserialize(memStream);
            }

            
        }

        public static ITEM Parse<ITEM>(XmlNode node)
        {
            return Parse<ITEM>(node.OuterXml);
        }

        public static ITEM Parse<ITEM>(string xml)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ITEM));
                return (ITEM)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Parse xml from file path
        /// </summary>
        /// <typeparam name="ITEM"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks>It is caller's responsibility to catch exception</remarks>
        public static ITEM ParseFromFile<ITEM>(string filePath)
        {
            using (Stream fstream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return ParseFromStream<ITEM>(fstream);
            }
        }

        public static string ToXmlString<ITEM>(ITEM item)
        {
            return ToXmlString<ITEM>(item, WritterSerializerSettings, SerializerNamespaces);
        }

        public static string ToXmlString<ITEM>(ITEM item, Encoding encoding)
        {
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.Encoding = encoding;
            setting.OmitXmlDeclaration = true;
            return ToXmlString<ITEM>(item, setting, SerializerNamespaces);
        }


        /// <summary>
        /// Serialize object to xml string
        /// </summary>
        /// <typeparam name="ITEM"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks>It is caller's responsibility to catch exception</remarks>
        public static string ToXmlString<ITEM>(ITEM item, XmlWriterSettings settings, XmlSerializerNamespaces xmlNamespaces)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ITEM));
            using (MemoryStream memStream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                {
                    serializer.Serialize(writer, item, xmlNamespaces);
                    writer.Flush();
                    writer.Close();
                    memStream.Position = 0;
                    using (StreamReader reader = new StreamReader(memStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Serialize an object to xml file
        /// </summary>
        /// <typeparam name="ITEM"></typeparam>
        /// <param name="item"></param>
        /// <param name="filePath"></param>
        /// <remarks>It is caller's responsibility to catch exception</remarks>
        public static void ToXmlFile<ITEM>(ITEM item, string filePath)
        {
            using (FileStream fstream = new FileStream(filePath, FileMode.Create))
            {
                ToXmlStream<ITEM>(item, fstream);
            }
        }


        public static void ToXmlStream<ITEM>(ITEM item, Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ITEM));
            serializer.Serialize(stream, item);
        }

        public static ITEM ParseFromStream<ITEM>(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ITEM));
            return (ITEM)serializer.Deserialize(stream);
        }
    }
}
