using System;
using System.Collections.Generic;

namespace SQLAzureMWUtils
{
    public class FederationMemberDistribution
    {
        public string DistrubutionName { get; set; }
        public string DatabaseName { get; set; }
        public string Low { get; set; }
        public string High { get; set; }
        public string FedType { get; set; }
        public List<FederationTableInfo> Tables = new List<FederationTableInfo>();
        public int Member_ID { get; set; }

        public override string ToString()
        {
            if (FedType.Equals("root")) return "Root";

            string tmpLow = Low;
            string tmpHigh = High;

            if (High.Length == 0)
            {
                tmpHigh = "Max";
            }

            switch (FedType)
            {
                case "int":
                    if (Low == "-2147483648")
                    {
                        tmpLow = "Min";
                    }
                    break;

                case "uniqueidentifier":
                    if (Low == "00000000-0000-0000-0000-000000000000")
                    {
                        tmpLow = "Min";
                    }
                    break;

                case "varbinary":
                    if (Low == "0x")
                    {
                        tmpLow = "Min";
                    }
                    break;

                default:
                    if (Low == "-9223372036854775808")
                    {
                        tmpLow = "Min";
                    }
                    break;
            }

            return DistrubutionName + " (" + tmpLow + " to " + tmpHigh + ")";
        }
    }
}
