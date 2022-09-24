using System;
using System.Xml.Serialization;

namespace RG351MP.Generators.BmFont.Models
{
    [Serializable]
    public class FontPage
    {
        [XmlAttribute("id")]
        public Int32 ID
        {
            get;
            set;
        }

        [XmlAttribute("file")]
        public string File
        {
            get;
            set;
        }
    }
}