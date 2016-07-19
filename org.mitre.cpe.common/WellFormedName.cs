using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace org.mitre.cpe.common
{
    public class WellFormedName
    {

        //  Underlying wfn representation.
        String[] attributes = new String[]{"part", "vendor", "product", "version",
        "update", "edition", "language", "sw_edition", "target_sw",
        "target_hw", "other"};

        Hashtable wfn = null;

        public string part
        {
            get
            {
                object o = wfn["part"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string vendor
        {
            get
            {
                object o = wfn["vendor"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string product
        {
            get
            {
                object o = wfn["product"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string version
        {
            get
            {
                object o = wfn["version"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string update
        {
            get
            {
                object o = wfn["update"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string edition
        {
            get
            {
                object o = wfn["edition"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string language
        {
            get
            {
                object o = wfn["language"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string sw_edition
        {
            get
            {
                object o = wfn["sw_edition"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string target_sw
        {
            get
            {
                object o = wfn["target_sw"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string target_hw
        {
            get
            {
                object o = wfn["target_hw"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public string other
        {
            get
            {
                object o = wfn["other"];
                String returnValue = String.Empty;
                if (o.GetType() == typeof(LogicalValue))
                {
                    returnValue = ((LogicalValue)o).ToString();
                }
                else
                {
                    returnValue = (string)o;
                }
                return returnValue;

            }
        }

        public WellFormedName()
        {
            this.wfn = new Hashtable();

            foreach (String a in attributes)
            {
                // don't set part to ANY
                if (!a.Equals("part"))
                {
                    this.set(a, new LogicalValue("ANY"));
                }
            }
        }

        public WellFormedName(Object part, Object vendor, Object product, Object version, Object update, Object edition, Object language, Object sw_edition, Object target_sw, Object target_hw, Object other)
        {

            this.wfn = new Hashtable();
            this.set("part", part);
            this.set("vendor", vendor);
            this.set("product", product);
            this.set("version", version);
            this.set("update", update);
            this.set("edition", edition);
            this.set("language", language);
            this.set("sw_edition", sw_edition);
            this.set("target_sw", target_sw);
            this.set("target_hw", target_hw);
            this.set("other", other);

        }
        public Object get(String attribute)
        {
            if (this.wfn.ContainsKey(attribute))
            {
                return this.wfn[attribute];
            }
            else
            {
                return new LogicalValue("ANY");
            }

        }

        public void set(String attribute, Object value)
        {
            //  Iterate over permissible attributes.
            foreach (String a in attributes)
            {
                //  If the argument is a valid attribute, set that attribute's value.
                if (attribute.Equals(a))
                {
                    //  check to see if we're setting a LogicalValue ANY or NA
                    if ((value is LogicalValue))
                    {
                        //  don't allow logical values in part component
                        if (attribute.Equals("part"))
                        {
                            throw new Exception("Error! part component cannot be a logical value");
                        }

                        //  put the Object in the ht and break
                        this.wfn[attribute] = value;
                        break;
                    }

                    if (((value == null)
                                || ((String)(value)).Equals("")))
                    {
                        //  if value is null or blank, set attribute to default logical ANY
                        this.wfn[attribute] = new LogicalValue("ANY");
                        break;
                    }

                    String svalue = ((String)(value));
                    Regex reg1, reg2;
                    //  Reg exs
                    //  check for printable characters - no control characters
                    if (Utilities.ContainsControlCharacters(svalue))
                    {
                        throw new FormatException(("Error! encountered non printable character in: " + svalue));
                    }

                    //  svalue has whitespace
                    reg1 = new Regex(".*\\s+.*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                    //if (svalue.matches(".*\\\\s+.*"))
                    if (reg1.IsMatch(svalue))
                    {
                        throw new FormatException(("Error! component cannot contain whitespace: " + svalue));
                    }

                    //  svalue has more than one unquoted star
                    reg1 = new Regex(".*\\*{2,}.*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    if (reg1.IsMatch(svalue))
                    {
                        throw new FormatException(("Error! component cannot contain more than one * in sequence: " + svalue));
                    }

                    //  svalue has unquoted punctuation embedded
                    reg1 = new Regex(
                              ".*(?<!\\\\)[\\!\\\"\\#\\$\\%\\&\\\'\\(\\)\\+\\,\\.\\/\\:\\;\\<\\=\\>\\@\\[\\]\\^\\`\\{\\|\\}\\~\\-].*",
                            RegexOptions.IgnoreCase
                            | RegexOptions.IgnorePatternWhitespace
                            );
                    if (reg1.IsMatch(svalue))
                    {
                        throw new FormatException(("Error! component cannot contain unquoted punctuation: " + svalue));
                    }

                    //  svalue has an unquoted *
                    reg1 = new Regex(".+(?<!\\\\)[\\*].+",
                            RegexOptions.IgnoreCase
                            | RegexOptions.IgnorePatternWhitespace
                            );
                    //if (svalue.matches(".+(?<!\\\\\\\\)[\\\\*].+"))
                    if (reg1.IsMatch(svalue))
                    {
                        throw new FormatException(("Error! component cannot contain embedded *: " + svalue));
                    }

                    //  svalue has embedded unquoted ?
                    //  this will catch a single unquoted ?, so make sure we deal with that
                    if (svalue.Contains("?"))
                    {
                        if (svalue.Equals("?"))
                        {
                            //  single ? is valid
                            this.wfn[attribute] = svalue;
                            break;
                        }

                        //  remove leading and trailing ?s
                        string v = svalue;
                        while ((v.IndexOf("?") == 0))
                        {
                            //  remove all leading ?'s
                            v = v.Remove(0, 1);
                        }

                        v = Utilities.reverse(v);
                        while ((v.IndexOf("?") == 0))
                        {
                            //  remove all trailing ?'s (string has been reversed)
                            v = v.Remove(0, 1);
                        }

                        //  back to normal
                        v = Utilities.reverse(v);
                        //  after leading and trailing ?s are removed, check if value
                        //  contains unquoted ?s
                        reg1 = new Regex(
      ".+(?<!\\\\)[\\?].+",
    RegexOptions.IgnoreCase
    | RegexOptions.IgnorePatternWhitespace
    );
                        if (reg1.IsMatch(v))
                        {
                            throw new FormatException(("Error! component cannot contain embedded ?: " + svalue));
                        }

                    }

                    //  single asterisk is not allowed
                    if (svalue.Equals("*"))
                    {
                        throw new FormatException(("Error! component cannot be a single *: " + svalue));
                    }

                    //  quoted hyphen not allowed by itself
                    if (svalue.Equals("-"))
                    {
                        throw new FormatException(("Error! component cannot be quoted hyphen: " + svalue));
                    }

                    //  part must be a, o, or h
                    if (attribute.Equals("part"))
                    {
                        if ((!svalue.Equals("a")
                                    && (!svalue.Equals("o")
                                    && !svalue.Equals("h"))))
                        {
                            throw new FormatException(("Error! part component must be one of the following: \'a\', \'o\', \'h\': " + svalue));
                        }

                    }

                    //  should be good to go
                    this.wfn[attribute] = svalue;
                    break;
                }

            }

        }


        public String ToString()
        {
            StringBuilder sb = new StringBuilder("wfn:[");
            foreach (String attr in attributes)
            {
                sb.Append(attr);
                sb.Append("=");
                Object o = wfn[attr];
                if (o.GetType() == typeof(LogicalValue))
                {
                    sb.Append(((LogicalValue)o).ToString());
                    sb.Append(", ");
                }
                else
                {
                    sb.Append("\"");
                    sb.Append(o);
                    sb.Append("\", ");
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            return sb.ToString();
        }
    }
}
