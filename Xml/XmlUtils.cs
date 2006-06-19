using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace BlueprintIT.Xml
{
  public class XmlUtils
  {
    public static XmlDocument LoadXml(FileInfo file)
    {
      return LoadXml(file.FullName);
    }

    public static XmlDocument LoadXml(string path)
    {
      TextReader reader = new StreamReader(path, Encoding.UTF8);
      XmlDocument result = LoadXml(reader);
      reader.Close();
      return result;
    }

    public static XmlDocument LoadXml(Stream stream)
    {
      XmlReaderSettings settings = new XmlReaderSettings();
      XmlReader reader = XmlReader.Create(stream, settings);
      XmlDocument document = new XmlDocument();
      document.Load(reader);
      reader.Close();

      return document;
    }

    public static XmlDocument LoadXml(TextReader input)
    {
      XmlReaderSettings settings = new XmlReaderSettings();
      XmlReader reader = XmlReader.Create(input, settings);
      XmlDocument document = new XmlDocument();
      document.Load(reader);
      reader.Close();

      return document;
    }

    public static void SaveXml(XmlDocument document, FileInfo file)
    {
      SaveXml(document, file.FullName);
    }

    public static void SaveXml(XmlDocument document, string path)
    {
      TextWriter writer = new StreamWriter(path, false, Encoding.UTF8);
      SaveXml(document, writer);
      writer.Flush();
      writer.Close();
    }

    public static void SaveXml(XmlDocument document, StringBuilder result)
    {
      TextWriter writer = new StringWriter(result);
      SaveXml(document, writer);
      writer.Flush();
      writer.Close();
    }

    public static void SaveXml(XmlDocument document, Stream stream)
    {
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.IndentChars = "  ";
      settings.Encoding = Encoding.UTF8;
      settings.CloseOutput = false;

      XmlWriter writer = XmlWriter.Create(stream, settings);
      document.WriteTo(writer);
      writer.Flush();
      writer.Close();
    }

    public static void SaveXml(XmlDocument document, TextWriter output)
    {
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.IndentChars = "  ";
      settings.Encoding = Encoding.UTF8;
      settings.CloseOutput = false;

      XmlWriter writer = XmlWriter.Create(output, settings);
      document.WriteTo(writer);
      writer.Flush();
      writer.Close();
    }

  }
}
