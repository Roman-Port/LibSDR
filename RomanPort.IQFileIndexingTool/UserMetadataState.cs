using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool
{
    public class UserMetadataState
    {
        public string stationValue = "";
        public bool stationEdited = false;

        public string artistValue = "";
        public bool artistEdited = false;

        public string titleValue = "";
        public bool titleEdited = false;

        public string radio = "";
        public DateTime time;
        public string prefix = "";
        public string suffix = "";
        public string notes = "";

        public bool IsFilledOut()
        {
            if (stationValue.Length < 4)
                return false;
            if (artistValue == "")
                return false;
            if (titleValue == "")
                return false;
            if (radio == "")
                return false;
            if (prefix == "")
                return false;
            if (suffix == "")
                return false;
            return true;
        }
    }
}
