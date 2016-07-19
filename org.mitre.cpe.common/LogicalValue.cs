using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.mitre.cpe.common
{
    public class LogicalValue
    {

        private Boolean any = false;
        private Boolean na = false;

        // Object must be constructed with the String "ANY" or "NA".  
        public LogicalValue(String type)
        {
            if (type.Equals("ANY"))
            {
                this.any = true;
            }
            else if (type.Equals("NA"))
            {
                this.na = true;
            }
            else
            {
                throw new ArgumentException("LogicalValue must be ANY or NA");
            }
        }

        public Boolean isANY()
        {
            return this.any;
        }

        public Boolean isNA()
        {
            return this.na;
        }

        public String ToString()
        {
            if (this.any)
            {
                return "ANY";
            }
            return "NA";
        }
    }
}
