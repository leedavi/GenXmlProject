using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Globalization;

namespace NBright.GenXmlDB
{
    public class NBrightData 
    {
        public long ItemId { get; set; }
        public long PortalId { get; set; }
        public long ModuleId { get; set; }
        public string TableCode { get; set; }
        public string KeyData { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string TextData { get; set; }
        public long XrefItemId { get; set; }
        public long ParentItemId { get; set; }
        public string XmlString { get; set; }
        public string Lang { get; set; }
        public long UserId { get; set; }
        public long LegacyItemId { get; set; }

    }

    public class NBrightInfo : NBrightData
    {

        public NBrightInfo()
        {
            ItemId = -1;
            XmlString = "<genxml></genxml>";
        }

        public NBrightInfo(NBrightData nbrightData)
        {
            ItemId = nbrightData.ItemId;
            Lang = nbrightData.Lang;
            ModifiedDate = nbrightData.ModifiedDate;
            ModuleId = nbrightData.ModuleId;
            ParentItemId = nbrightData.ParentItemId;
            PortalId = nbrightData.PortalId;
            KeyData = nbrightData.KeyData;
            TableCode = nbrightData.TableCode;
            TextData = nbrightData.TextData;
            UserId = nbrightData.UserId;
            XmlString = nbrightData.XmlString;
            XrefItemId = nbrightData.XrefItemId;
            LegacyItemId = nbrightData.LegacyItemId;
        }

        public XDocument XMLDoc = null;

        /// <summary>
        /// Returns and sets the XDocument content 
        /// </summary>
        public new string XmlString
        {
            //hides base class property
            get { return XMLDoc.ToString(); }
            set
            {
                try
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        XMLDoc = XDocument.Parse(value);
                    }
                }
                catch (Exception)
                {
                    //trap erorr and don't report. (The XML might be invalid, but we don;t want to stop processing here.)
                    XMLDoc = null;
                }
            }
        }


        #region "Set Properties"

        public void SetXmlPropertyDouble(string xpath, Double value, int precision = 2)
        {
            SetXmlProperty(xpath, Math.Round(value, precision).ToString(""),TypeCode.Double);
        }

        public void SetXmlPropertyDataTime(string xpath, string value, bool securityfilter = true, bool filterlinks = false)
        {
            SetXmlProperty(xpath, value, TypeCode.DateTime, securityfilter, filterlinks);
        }

        public void SetXmlProperty(string xpath, string value, bool securityfilter = true, bool filterlinks = false)
        {
            SetXmlProperty(xpath, value, TypeCode.String, securityfilter, filterlinks);
        }

        public void SetXmlProperty(string xpath, string value, System.TypeCode dataType, bool securityfilter = true, bool filterlinks = false)
        {
            if (!string.IsNullOrEmpty(XmlString))
            {
                try
                {

                    if (dataType == System.TypeCode.Double)
                    {
                        value = Utils.FormatToSave(value, System.TypeCode.Double);
                    }

                    if (dataType == System.TypeCode.DateTime)
                    {
                        if (Utils.IsDate(value, CultureInfo.CurrentCulture)) value = Utils.FormatToSave(value, System.TypeCode.DateTime);
                    }

                    if (securityfilter)
                    {
                        // clear cross scripting if not html field.
                        value = Utils.FormatDisableScripting(value, filterlinks);
                    }

                    if (XMLDoc.XPathSelectElement(xpath) == null)
                    {
                        // node does not exist, try and use simple logic to create simple xpath parse.
                        // this will not work in complex xpath, but that should not be the case here.
                        var xsplit = xpath.Split('/');
                        var xpathpart = "";
                        var xpathpartold = "";
                        var node = XMLDoc.Root;
                        foreach (var x in xsplit)
                        {
                            xpathpart += x;
                            if (XMLDoc.XPathSelectElement(xpathpart) == null)
                            {
                                XMLDoc.XPathSelectElement(xpathpartold).Add(new XElement(x));
                            }
                            else
                            {
                                node = XMLDoc.XPathSelectElement(xpathpart);
                            }
                            xpathpartold = xpathpart;
                            xpathpart += "/";
                        }
                    }
                    XMLDoc.XPathSelectElement(xpath).Value = value;

                    // do the datatype after the node is created
                    var typeCode = "";
                    if (dataType == System.TypeCode.DateTime) typeCode = "date";
                    if (dataType == System.TypeCode.Double) typeCode = "double";

                    if (typeCode != "")
                    {
                        if (XMLDoc.XPathSelectElement(xpath).Attribute("datatype") == null)
                        {
                            XMLDoc.XPathSelectElement(xpath).Add(new XAttribute("datatype", typeCode));
                        }
                        else
                        {
                            XMLDoc.XPathSelectElement(xpath).Attribute("datatype").SetValue(typeCode);
                        }
                    }

                }
                catch (Exception ex)
                {
                    // ignore
                    Console.WriteLine(ex.Message);
                }


            }
        }

        #endregion

        #region "Get Properties"

        /// <summary>
        /// Get Raw XML value using XPath
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns>string</returns>
        public string GetXmlProperty(string xpath)
        {
            if (XMLDoc != null)
            {
                try
                {
                    return XMLDoc.XPathSelectElement(xpath).Value; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "XML READ ERROR";
                }
            }
            return "";
        }

        /// <summary>
        /// Get Formatted XML value using XPath
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns>string</returns>
        public string GetXmlPropertyFormat(string xpath,string formatCode = "")
        {
            if (XMLDoc != null)
            {
                try
                {
                    var value = XMLDoc.XPathSelectElement(xpath).Value;
                    var datatype = XMLDoc.XPathSelectElement(xpath).Attribute("datatype") .Value;
                    var typecode = TypeCode.String;
                    if (datatype != null)
                    {
                        if (datatype == "double") typecode = TypeCode.Double;
                        if (datatype == "date") typecode = TypeCode.DateTime;
                    }
                    return Utils.FormatToDisplay(value, typecode, formatCode);   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "XML READ ERROR";
                }
            }
            return "";
        }

        /// <summary>
        /// return int data type from XML
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public int GetXmlPropertyInt(string xpath)
        {
            if (XMLDoc != null)
            {
                try
                {
                    var x = XMLDoc.XPathSelectElement(xpath).Value;
                    if (Utils.IsNumeric(x)) return Convert.ToInt32(x);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// return double data type from XML
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public double GetXmlPropertyDouble(string xpath)
        {
            if (XMLDoc != null)
            {
                try
                {
                    var x = XMLDoc.XPathSelectElement(xpath).Value;
                    if (Utils.IsNumeric(x)) return Convert.ToDouble(x);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return 0;
                }
            }
            return 0;
        }

        #endregion

        #region "Merge"


        /// <summary>
        /// Simple method to add/replace XML section
        /// </summary>
        /// <param name="xmlstring">XML to add</param>
        /// <param name="xpathparentnode">XML node to be replace by the "xmlstring", if blank the XML is added to the root node</param>
        public void ReplaceXmlSection(string xmlstring, string xpathparentnode = "")
        {
            try
            {
                var mergwDoc = XDocument.Parse(xmlstring);
                if (xpathparentnode == "")
                {
                    XMLDoc.Root.Add(mergwDoc.Elements());
                }
                else
                {
                    if (XMLDoc.XPathSelectElement(xpathparentnode) != null)
                    {
                        XMLDoc.XPathSelectElement(xpathparentnode).Remove();
                    }
                    SetXmlProperty(xpathparentnode, "");
                    XMLDoc.XPathSelectElement(xpathparentnode).Add(mergwDoc.Elements());
                }
            }
            catch (Exception ex)
            {
                // ignore
                Console.WriteLine(ex.Message);
            }

        }

        public void AddXmlLang(string xmlstring)
        {
            ReplaceXmlSection(xmlstring, "genxml/lang");
        }



        #endregion


    }

}
