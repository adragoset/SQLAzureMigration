using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public class FederationDetails
    {
        public int Federation_id { get; set; }
        public string FederationName { get; set; }
        private List<FederationMemberDistribution> _Members;

        public List<FederationMemberDistribution> Members
        {
            get
            {
                if (_Members == null)
                {
                    _Members = new List<FederationMemberDistribution>();
                }
                return _Members;
            }
        }

        public override string ToString()
        {
            return FederationName;
        }
    }
}
