using System;
using System.IO;
using System.Xml;
namespace Custom_Files
{
    public class XML
    {
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Config.xml");

        XmlDocument document = new XmlDocument();
        
        public void Open()
        {
            try
            {
                document.Load(dbPath);
            }
            catch
            {
                //xml文档的声明部分
                XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", "");//xml文档的声明部分
                document.AppendChild(declaration);//添加至XmlDocument对象中

                XmlElement Root = document.CreateElement("Parameters");//根节点
                    XmlElement Connection = document.CreateElement("Connection");//子节点—连接
                        XmlElement  IP = document.CreateElement("IP");
                        IP.InnerText = "192.168.0.124";
                        XmlElement Port = document.CreateElement("Port");
                        Port.InnerText = "11066";
                    Connection.AppendChild(IP);
                    Connection.AppendChild(Port);
                    XmlElement Calibration = document.CreateElement("Calibration"); //子节点—标定
                        XmlElement V_min = document.CreateElement("V_min");
                        V_min.InnerText = "0";
                        XmlElement Temp_min = document.CreateElement("Temp_min");
                        Temp_min.InnerText = "35.0";
                        XmlElement V_max = document.CreateElement("V_max");
                        V_max.InnerText = "4096";
                        XmlElement Temp_max = document.CreateElement("Temp_max");
                        Temp_max.InnerText = "42.0";
                    Calibration.AppendChild(V_min);
                    Calibration.AppendChild(Temp_min);
                    Calibration.AppendChild(V_max);
                    Calibration.AppendChild(Temp_max);
                Root.AppendChild(Connection);
                Root.AppendChild(Calibration);
                
                document.AppendChild(Root);
                //保存输出路径
                document.Save(dbPath);
            }
        }
        public string Read(string path)
        {
            XmlNode element = document.SelectSingleNode(path);
            return element.InnerText;
        }
        public void Update(string path,string value)
        {
            XmlNode element = document.SelectSingleNode(path);
            element.InnerText = value;
            document.Save(dbPath);
        }
    }
}