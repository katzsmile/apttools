using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;


namespace APTTools
{
    public partial class MainForm : Form
    {
        public string AptFileName = "";
        public string sver = "0.1";
        public string stitle = "Apt Tools ";
        public const int MAX_LENGTH = 256;
        public int len = 0;
        public XmlDocument xDoc;
        public AptConstData c;
        public int CHAR_SIG = 0x09876543;

        public struct AptConstData {
	        public int aptdataoffset;
            public int itemcount;
            public AptConstItem[] items;
        };

        public struct FrameItem {
	        public int type;
        };

        public struct OutputFrame {
            public int frameitemcount;
            public FrameItem[] frameitems;
        };

        public struct Character {
	        public int type;
            public int signature;
        };

        public struct Import {
	        public string movie;
	        public string name;
	        public int character;
	        public int pointer; //always zero, used at runtime
        };

        public struct Export {
	        public string name;
            public int character;
        };

        public struct OutputMovie  {
            public uint framecount;
            public ArrayList frames; //offset of frame data
            public uint pointer; //always zero, used at runtime
            public uint charactercount;
            public ArrayList characters; //offset of character data
            public uint screensizex;
            public uint screensizey;
            public uint unknown; //always 33 as far as I can see
            public uint importcount;
            public ArrayList imports; //offset of imports data
            public uint exportcount;
            public ArrayList exports; //offset of exports data
            public uint count; //always zero, used at runtime
        };

        public enum AptConstItemType
        {
	        TYPE_UNDEF = 0,
	        TYPE_STRING = 1,
	        TYPE_NUMBER = 4,
        }

        public struct AptConstItem
        {
            public int type;
            public string strvalue;
            public int numvalue;
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public string readStringTillZero(string file, int pos)
        {
            FileStream S = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader readString = new BinaryReader(S);
            readString.BaseStream.Position = pos;
            char[] work = new char[MAX_LENGTH];

            int i = 0;

            char c = readString.ReadChar();
            while (c != Convert.ToChar(0x00))
            {
                work[i++] = c;
                c = readString.ReadChar();
            }
            return new string(work, 0, i);
        }

        private static string IndentXMLString(string xml)
        {
            string outXml = string.Empty;
            MemoryStream ms = new MemoryStream();
            // Create a XMLTextWriter that will send its output to a memory stream (file)
            XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode);
            XmlDocument doc = new XmlDocument();

            try
            {
                // Load the unformatted XML text string into an instance
                // of the XML Document Object Model (DOM)
                doc.LoadXml(xml);

                // Set the formatting property of the XML Text Writer to indented
                // the text writer is where the indenting will be performed
                xtw.Formatting = Formatting.Indented;

                // write dom xml to the xmltextwriter
                doc.WriteContentTo(xtw);
                // Flush the contents of the text writer
                // to the memory stream, which is simply a memory file
                xtw.Flush();

                // set to start of the memory stream (file)
                ms.Seek(0, SeekOrigin.Begin);
                // create a reader to read the contents of
                // the memory stream (file)
                StreamReader sr = new StreamReader(ms);
                // return the formatted string to caller
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return string.Empty;
            }
        }

        public void openConst(string file)
        {            
            AptFileName = Path.GetFileName(file);
            AptFileName = AptFileName.ToLower();
            Text = stitle + sver + " - " + AptFileName;
            //txtOut.AppendText("Processing Const File: " + file +"\n");
            FileStream F = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(F);
            len = (int)reader.BaseStream.Length;
            reader.ReadBytes(20);
            c = new AptConstData();

            c.aptdataoffset = reader.ReadInt32();
            txtOut.AppendText("Offset: " + Convert.ToString(c.aptdataoffset) + "\n");
            c.itemcount = reader.ReadInt32();
            //txtOut.AppendText("Item Counts: " + Convert.ToString(c.itemcount) + "\n");
            int AptConstItem = reader.ReadInt32();
            //txtOut.AppendText("Const Counts: " + Convert.ToString(AptConstItem) + "\n");

            XmlNode xmlconst = xDoc.CreateNode(XmlNodeType.Element, "constdata", "");

            c.items = new AptConstItem[c.itemcount];

            for (int i = 0; i < c.itemcount; i++)
            {
                c.items[i].type = reader.ReadInt32();
                //txtOut.AppendText("Item Type: " + Convert.ToString(c.items[i].type) + "\n");
                XmlNode aitem = xDoc.CreateElement("const");
                if (c.items[i].type == 1)
		        {			        
                    XmlAttribute id = xDoc.CreateAttribute("id");
                    id.InnerText = Convert.ToString(i);
                    aitem.Attributes.Append(id);
                    XmlAttribute typeid = xDoc.CreateAttribute("typeid");
                    typeid.InnerText = Convert.ToString(c.items[i].type);
                    aitem.Attributes.Append(typeid);
                    XmlAttribute type = xDoc.CreateAttribute("type");
                    type.InnerText = "STRING";
                    aitem.Attributes.Append(type);

                    XmlAttribute value = xDoc.CreateAttribute("value");
                    int pos = reader.ReadInt32();
                    c.items[i].strvalue = readStringTillZero(file, pos);
                    value.InnerText = c.items[i].strvalue;
                    aitem.Attributes.Append(value);
                    
		        }
		        else if (c.items[i].type == 4)
		        {
                    XmlAttribute id = xDoc.CreateAttribute("id");
                    id.InnerText = Convert.ToString(i);
                    aitem.Attributes.Append(id);
                    XmlAttribute typeid = xDoc.CreateAttribute("typeid");
                    typeid.InnerText = Convert.ToString(c.items[i].type);
                    aitem.Attributes.Append(typeid);
                    XmlAttribute type = xDoc.CreateAttribute("type");
                    type.InnerText = "NUMBER";
                    aitem.Attributes.Append(type);

                    XmlAttribute value = xDoc.CreateAttribute("value");
                    c.items[i].numvalue = (int)reader.ReadInt32();
                    value.InnerText = Convert.ToString(c.items[i].numvalue);
                    aitem.Attributes.Append(value);
                    
                }
                else if (c.items[i].type == 0)
		        {
                    XmlAttribute id = xDoc.CreateAttribute("id");
                    id.InnerText = Convert.ToString(i);
                    aitem.Attributes.Append(id);
                    XmlAttribute typeid = xDoc.CreateAttribute("typeid");
                    typeid.InnerText = Convert.ToString(c.items[i].type);
                    aitem.Attributes.Append(typeid);
                    XmlAttribute type = xDoc.CreateAttribute("type");
                    type.InnerText = "UNDEF";
                    aitem.Attributes.Append(type);

                    XmlAttribute value = xDoc.CreateAttribute("value");
                    c.items[i].numvalue = (int)reader.ReadInt32();
                    value.InnerText = Convert.ToString(c.items[i].numvalue);
                    aitem.Attributes.Append(value);
 
                }
                xmlconst.AppendChild(aitem);
            }

            XmlDeclaration xmldec = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xDoc.AppendChild(xmldec);
            xDoc.AppendChild(xmlconst);
            //txtOut.AppendText(IndentXMLString(xDoc.InnerXml));

        }

        public void openApt(string file)
        {
            txtOut.AppendText("Processing Const File: " + file + "\n");
            FileStream F = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(F);
            len = (int)reader.BaseStream.Length;
            reader.ReadBytes(c.aptdataoffset);
            OutputMovie m = new OutputMovie();
            txtOut.AppendText("OutputMovie Size: " + Marshal.SizeOf(m) + "\n");
            m.characters = new ArrayList();
            //m.characters.Initialize();

            Character ch = new Character();
            ch.type = reader.ReadInt32();
            ch.signature = reader.ReadInt32();
            m.characters.Add(ch);
            Character che = (Character)m.characters[m.characters.Count - 1];
            txtOut.AppendText("CHAR_TYPE: " + Convert.ToString(che.type) + "\n");
            if (che.signature == CHAR_SIG)
            {
                txtOut.AppendText("CHAR_SIG: " + Convert.ToString(che.signature) + "\n");
            }
            m.exports = new ArrayList();
            //Export ex = new Export();
            //ex.name = 
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            txtOut.Clear();
            xDoc = new XmlDocument();
            using (var ofd = new OpenFileDialog { Filter = "Const File|*.const|All files|*.*" })
                if (DialogResult.OK == ofd.ShowDialog())
                {
                    openConst(ofd.FileName);
                }
            using (var ofd = new OpenFileDialog { Filter = "Apt File|*.apt|All files|*.*" })
                if (DialogResult.OK == ofd.ShowDialog())
                {
                    openApt(ofd.FileName);
                }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog { })
            {
                sfd.Filter = "Unpacked XML|*.xml";
                string SaveFileName = AptFileName;
                if (!AptFileName.Contains(".xml"))
                    SaveFileName = SaveFileName + ".xml";

                sfd.FileName = SaveFileName;
                if (DialogResult.OK == sfd.ShowDialog())
                {
                    xDoc.Save(sfd.FileName);
                }
            }
        }
    }
}
