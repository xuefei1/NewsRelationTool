using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsLabel
{
    public class NewsRelationConfig
    {
        private string notRelated = "N";
        private string notSure = "NS";
        private string related = "R";
        private string followUp = "F";
        private string contradict = "C";
        private string overlap = "O";
        private string subsumption = "S";
        private string equivalent = "E";

        public string Equivalent
        {
            get
            {
                return equivalent;
            }
            set
            {
                equivalent = value;
            }
        }
        public string Subsumption
        {
            get
            {
                return subsumption;
            }
            set
            {
                subsumption = value;
            }
        }
        public string NotRelated
        {
            get
            {
                return notRelated;
            }
            set
            {
                notRelated = value;
            }
        }
        public string NotSure
        {
            get
            {
                return notSure;
            }
            set
            {
                notSure = value;
            }
        }
        public string Related
        {
            get
            {
                return related;
            }
            set
            {
                related = value;
            }
        }
        public string FollowUp
        {
            get
            {
                return followUp;
            }
            set
            {
                followUp = value;
            }
        }
        public string Contradict
        {
            get
            {
                return contradict;
            }
            set
            {
                contradict = value;
            }
        }
        public string Overlap
        {
            get
            {
                return overlap;
            }
            set
            {
                overlap = value;
            }
        }
    }
}
