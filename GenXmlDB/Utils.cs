using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NBright.GenXmlDB
{
    public class Utils
    {
        public static string FormatToSave(string inpData)
        {
            return FormatToSave(inpData, TypeCode.String);
        }

        public static string FormatToSave(string inpData, TypeCode dataTyp)
        {
            if (String.IsNullOrEmpty(inpData))
                return inpData;
            switch (dataTyp)
            {
                case TypeCode.Double:
                    //always save  new CultureInfo("en-US") format to the XML
                    if (IsNumeric(inpData, CultureInfo.CurrentCulture))
                    {
                        var num = Convert.ToDouble(inpData, CultureInfo.CurrentCulture);
                        return num.ToString(new CultureInfo("en-US"));
                    }
                    if (IsNumeric(inpData)) // just check if we have a Invariant double
                    {
                        var num = Convert.ToDouble(inpData, CultureInfo.InvariantCulture);
                        return num.ToString(new CultureInfo("en-US"));
                    }
                    return "0";
                case TypeCode.DateTime:
                    if (IsDate(inpData))
                    {
                        var dte = Convert.ToDateTime(inpData);
                        return dte.ToString("s");
                    }
                    return "";
                default:
                    return FormatDisableScripting(inpData);
            }
        }


        public static string FormatToDisplay(string inpData, TypeCode dataTyp, string formatCode = "")
        {
            return FormatToDisplay(inpData, "", dataTyp, formatCode);
        }

        public static string FormatToDisplay(string inpData, string cultureCode, TypeCode dataTyp, string formatCode = "")
        {
            if (String.IsNullOrEmpty(inpData))
            {
                if (dataTyp == TypeCode.Double)
                {
                    return "0";
                }
                return inpData;
            }
            var outCulture = CultureInfo.CurrentCulture;
            if (cultureCode != "") outCulture = new CultureInfo(cultureCode);
            switch (dataTyp)
            {
                case TypeCode.Double:
                    if (IsNumeric(inpData))
                    {
                        return Double.Parse(inpData, CultureInfo.InvariantCulture).ToString(formatCode, outCulture);
                    }
                    return "0";
                case TypeCode.DateTime:
                    if (IsDate(inpData))
                    {
                        if (formatCode == "") formatCode = "d";
                        return DateTime.Parse(inpData).ToString(formatCode, outCulture);
                    }
                    return inpData;
                default:
                    return inpData;
            }
        }

        /// <summary>
        ///  IsNumeric function check if a given value is numeric, based on the culture code passed.  If no culture code is passed then a test on InvariantCulture is done.
        /// </summary>
        public static bool IsNumeric(object expression, CultureInfo cultureInfo = null)
        {
            if (expression == null) return false;

            double retNum;
            bool isNum = false;
            if (cultureInfo != null)
            {
                isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Number, cultureInfo.NumberFormat, out retNum);
            }
            else
            {
                isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Number, CultureInfo.InvariantCulture, out retNum);
            }

            return isNum;
        }

        public static bool IsNumeric(object expression, string cultureCode)
        {
            return IsNumeric(expression, new CultureInfo(cultureCode));
        }

        // IsDate culture Function
        public static bool IsDate(object expression)
        {
            DateTime rtnD;
            return DateTime.TryParse(Convert.ToString(expression), CultureInfo.CurrentCulture,DateTimeStyles.None, out rtnD);
        }

        public static bool IsDate(object expression, CultureInfo cultureInfo)
        {
            DateTime rtnD;
            return DateTime.TryParse(Convert.ToString(expression), cultureInfo, DateTimeStyles.None, out rtnD);
        }

        public static bool IsDate(object expression, string cultureCode)
        {
            return IsDate(expression, new CultureInfo(cultureCode));
        }




        /// -----------------------------------------------------------------------------
        ///  <summary>
        ///  This function uses Regex search strings to remove HTML tags which are
        ///  targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        ///  to provide more robust checking as additional holes are found.
        ///  </summary>
        ///  <param name="strInput">This is the string to be filtered</param>
        /// <param name="filterlinks">Remove href link elements</param>
        /// <returns>Filtered UserInput</returns>
        ///  <remarks>
        ///  This is a private function that is used internally by the InputFilter function
        ///  </remarks>
        /// -----------------------------------------------------------------------------
        public static string FormatDisableScripting(string strInput, bool filterlinks = true)
        {
            var tempInput = strInput;
            if (strInput == " " || String.IsNullOrEmpty(strInput))
            {
                return tempInput;
            }
            tempInput = FilterStrings(tempInput, filterlinks);
            return tempInput;
        }


        /// -----------------------------------------------------------------------------
        ///  <summary>
        ///  This function uses Regex search strings to remove HTML tags which are
        ///  targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        ///  to provide more robust checking as additional holes are found.
        ///  </summary>
        ///  <param name="strInput">This is the string to be filtered</param>
        /// <param name="filterlinks">remove href elements</param>
        /// <returns>Filtered UserInput</returns>
        ///  <remarks>
        ///  This is a private function that is used internally by the FormatDisableScripting function
        ///  </remarks>
        ///  <history>
        ///      [cathal]        3/06/2007   Created
        ///  </history>
        /// -----------------------------------------------------------------------------
        private static string FilterStrings(string strInput, bool filterlinks)
        {
            //setup up list of search terms as items may be used twice
            var tempInput = strInput;
            var listStrings = new List<string>
            {
                "<script[^>]*>.*?</script[^><]*>",
                "<script",
                "<input[^>]*>.*?</input[^><]*>",
                "<object[^>]*>.*?</object[^><]*>",
                "<embed[^>]*>.*?</embed[^><]*>",
                "<applet[^>]*>.*?</applet[^><]*>",
                "<form[^>]*>.*?</form[^><]*>",
                "<option[^>]*>.*?</option[^><]*>",
                "<select[^>]*>.*?</select[^><]*>",
                "<iframe[^>]*>.*?</iframe[^><]*>",
                "<iframe.*?<",
                "<iframe.*?",
                "<ilayer[^>]*>.*?</ilayer[^><]*>",
                "<form[^>]*>",
                "</form[^><]*>",
                "onerror",
                "onmouseover",
                "javascript:",
                "vbscript:",
                "unescape",
                "alert[\\s(&nbsp;)]*\\([\\s(&nbsp;)]*'?[\\s(&nbsp;)]*[\"(&quot;)]?",
                @"eval*.\(",
                "onload"
            };

            if (filterlinks)
            {
                listStrings.Add("<a[^>]*>.*?</a[^><]*>");
                listStrings.Add("<a");
            }

            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            const string replacement = " ";

            //check if text contains encoded angle brackets, if it does it we decode it to check the plain text
            if (tempInput.Contains("&gt;") && tempInput.Contains("&lt;"))
            {
                //text is encoded, so decode and try again
                tempInput = WebUtility.HtmlDecode(tempInput);
                tempInput = listStrings.Aggregate(tempInput,
                    (current, s) => Regex.Replace(current, s, replacement, options));

                //Re-encode
                tempInput = WebUtility.HtmlEncode(tempInput);
            }
            else
            {
                tempInput = listStrings.Aggregate(tempInput,
                    (current, s) => Regex.Replace(current, s, replacement, options));
            }
            return tempInput;
        }



        #region "NBrightInfo Import Export Merge"


        public static string ConvertToXmlItem(NBrightInfo nbrightInfo, bool withTextData = true)
        {
            // don't use serlization, becuase depending what is in the TextData field could make it fail.
            var xmlOut = new StringBuilder("<item><itemid>" + nbrightInfo.ItemId.ToString("") + "</itemid><portalid>" + nbrightInfo.PortalId.ToString("") + "</portalid><moduleid>" + nbrightInfo.ModuleId.ToString("") + "</moduleid><xrefitemid>" + nbrightInfo.XrefItemId.ToString("") + "</xrefitemid><parentitemid>" + nbrightInfo.ParentItemId.ToString("") + "</parentitemid><tablecode>" + nbrightInfo.TableCode + "</tablecode><ref>" + nbrightInfo.KeyData + "</ref><lang>" + nbrightInfo.Lang + "</lang><userid>" + nbrightInfo.UserId.ToString("") + "</userid>" + nbrightInfo.XmlString);
            if (withTextData && nbrightInfo.TextData != null)
            {
                xmlOut.Append("<textdata><![CDATA[" + nbrightInfo.TextData.Replace("<![CDATA[", "***CDATASTART***").Replace("]]>", "***CDATAEND***") + "]]></textdata>");
            }
            if (withTextData && nbrightInfo.FreeTextIndexData != null)
            {
                xmlOut.Append("<freetextdata><![CDATA[" + nbrightInfo.FreeTextIndexData.Replace("<![CDATA[", "***CDATASTART***").Replace("]]>", "***CDATAEND***") + "]]></freetextdata>");
            }
            xmlOut.Append("</item>");

            return xmlOut.ToString();
        }

        public static NBrightInfo ConvertFromXmlItem(string xmlItem)
        {
            var nbrightInfo = new NBrightInfo();
            if (!String.IsNullOrEmpty(xmlItem))
            {
                try
                {

                    nbrightInfo.XmlString = xmlItem;
                    //itemid
                    var selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/itemid");
                    if (selectSingleNode != null) nbrightInfo.ItemId = Convert.ToInt32(selectSingleNode.Value);

                    //portalid
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/portalid");
                    if (selectSingleNode != null) nbrightInfo.PortalId = Convert.ToInt32(selectSingleNode.Value);

                    // moduleid
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/moduleid");
                    if (selectSingleNode != null) nbrightInfo.ModuleId = Convert.ToInt32(selectSingleNode.Value);

                    //xrefitemid
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/xrefitemid");
                    if (selectSingleNode != null) nbrightInfo.XrefItemId = Convert.ToInt32(selectSingleNode.Value);

                    //parentitemid
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/parentitemid");
                    if (selectSingleNode != null) nbrightInfo.ParentItemId = Convert.ToInt32(selectSingleNode.Value);

                    //typecode
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/tablecode");
                    if (selectSingleNode != null) nbrightInfo.TableCode = selectSingleNode.Value;

                    //guidkey
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/ref");
                    if (selectSingleNode != null) nbrightInfo.KeyData = selectSingleNode.Value;

                    //XmlData
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/genxml");
                    if (selectSingleNode != null) nbrightInfo.XmlString = selectSingleNode.ToString();

                    //TextData
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/textdata");
                    if (selectSingleNode != null)
                        nbrightInfo.TextData = selectSingleNode.ToString().Replace("***CDATASTART***", "<![CDATA[").Replace("***CDATAEND***", "]]>");

                    //FreeTextIndexData
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/freetextdata");
                    if (selectSingleNode != null)
                        nbrightInfo.FreeTextIndexData = selectSingleNode.ToString().Replace("***CDATASTART***", "<![CDATA[").Replace("***CDATAEND***", "]]>");

                    //lang
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/lang");
                    if (selectSingleNode != null) nbrightInfo.Lang = selectSingleNode.Value;

                    //userid
                    selectSingleNode = nbrightInfo.XMLDoc.XPathSelectElement("item/userid");
                    if ((selectSingleNode != null) && (Utils.IsNumeric(selectSingleNode.Value)))
                        nbrightInfo.UserId = Convert.ToInt32(selectSingleNode.Value);
                }
                catch (Exception ex)
                {
                    nbrightInfo.TextData = ex.ToString();
                }
            }
            return nbrightInfo;
        }

        public static NBrightData ConvertToNBrightData(NBrightInfo nbrightInfo)
        {
            var nbd = new NBrightData();
            nbd.ItemId = nbrightInfo.ItemId;
            nbd.Lang = nbrightInfo.Lang;
            nbd.ModifiedDate = nbrightInfo.ModifiedDate;
            nbd.ModuleId = nbrightInfo.ModuleId;
            nbd.ParentItemId = nbrightInfo.ParentItemId;
            nbd.PortalId = nbrightInfo.PortalId;
            nbd.KeyData = nbrightInfo.KeyData;
            nbd.TableCode = nbrightInfo.TableCode;
            nbd.TextData = nbrightInfo.TextData;
            nbd.UserId = nbrightInfo.UserId;
            nbd.XmlString = nbrightInfo.XmlString;
            nbd.XrefItemId = nbrightInfo.XrefItemId;
            nbd.FreeTextIndexData = nbrightInfo.FreeTextIndexData;
            return nbd;
        }

        public static NBrightData ConvertToNBrightDataBase(NBrightInfo nbrightInfo)
        {
            // remove and language dependant data
            nbrightInfo.XMLDoc.XPathSelectElement("genxml/lang").Remove();
            var nbd = ConvertToNBrightData(nbrightInfo);
            nbd.Lang = "";
            nbd.ParentItemId = 0;
            return nbd;
        }

        public static NBrightData ConvertToNBrightDataLang(NBrightInfo nbrightInfo)
        {
            // keep only language dependant data
            var nodLang = nbrightInfo.XMLDoc.XPathSelectElement("genxml/lang/genxml");
            nbrightInfo.XmlString = nodLang.ToString();
            var nbd = ConvertToNBrightData(nbrightInfo);
            nbd.ParentItemId = 0; // this should be set by the update function.
            return nbd;
        }

        public static NBrightInfo ConvertToNBrightInfo(NBrightData nbrightData)
        {
            var nbrightInfo = new NBrightInfo();
            nbrightInfo.ItemId = nbrightData.ItemId;
            nbrightInfo.Lang = nbrightData.Lang;
            nbrightInfo.ModifiedDate = nbrightData.ModifiedDate;
            nbrightInfo.ModuleId = nbrightData.ModuleId;
            nbrightInfo.ParentItemId = nbrightData.ParentItemId;
            nbrightInfo.PortalId = nbrightData.PortalId;
            nbrightInfo.KeyData = nbrightData.KeyData;
            nbrightInfo.TableCode = nbrightData.TableCode;
            nbrightInfo.TextData = nbrightData.TextData;
            nbrightInfo.UserId = nbrightData.UserId;
            nbrightInfo.XmlString = nbrightData.XmlString;
            nbrightInfo.XrefItemId = nbrightData.XrefItemId;
            nbrightInfo.FreeTextIndexData = nbrightData.FreeTextIndexData;
            return nbrightInfo;
        }



        #endregion




    }

}
