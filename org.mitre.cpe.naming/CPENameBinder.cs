using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.mitre.cpe.matching;
using org.mitre.cpe.common;


namespace org.mitre.cpe.naming
{
    /**
 * The CPENameBinder class is a simple implementation
 * of the CPE Name binding algorithm, as specified in the 
 * CPE Naming Standard version 2.3.  
 * 
 * @see <a href="http://cpe.mitre.org">cpe.mitre.org</a> for more information. 
 * @author Joshua Kraunelis
 * @email jkraunelis@mitre.org
 */
    public class CPENameBinder
    {

        /**
         * Binds a {@link WellFormedName} object to a URI.
         * @param w WellFormedName to be bound to URI
         * @return URI binding of WFN
         */
        public String bindToURI(WellFormedName w)
        {

            // Initialize the output with the CPE v2.2 URI prefix.
            String uri = "cpe:/";

            // Define the attributes that correspond to the seven components in a v2.2. CPE.
            String[] attributes = { "part", "vendor", "product", "version", "update", "edition", "language" };

            // Iterate over the well formed name
            foreach (String a in attributes)
            {
                String v = "";
                if (a.Equals("edition"))
                {
                    // Call the pack() helper function to compute the proper
                    // binding for the edition element.
                    String ed = bindValueForURI(w.get("edition"));
                    String sw_ed = bindValueForURI(w.get("sw_edition"));
                    String t_sw = bindValueForURI(w.get("target_sw"));
                    String t_hw = bindValueForURI(w.get("target_hw"));
                    String oth = bindValueForURI(w.get("other"));
                    v = pack(ed, sw_ed, t_sw, t_hw, oth);
                }
                else
                {
                    // Get the value for a in w, then bind to a string
                    // for inclusion in the URI.
                    v = bindValueForURI(w.get(a));
                }
                // Append v to the URI then add a colon.
                uri = Utilities.strcat(uri, v, ":");
            }
            // Return the URI string, with trailing colons trimmed.
            return trim(uri);
        }

        /**
         * Top-level function used to bind WFN w to formatted string.
         * @param w WellFormedName to bind
         * @return Formatted String
         */
        public String bindToFS(WellFormedName w)
        {
            // Initialize the output with the CPE v2.3 string prefix.
            String fs = "cpe:2.3:";
            String[] parts = {"part", "vendor", "product", "version",
                    "update", "edition", "language", "sw_edition", "target_sw",
                    "target_hw", "other"};
            foreach (String a in parts)
            {
                String v = bindValueForFS(w.get(a));
                fs = Utilities.strcat(fs, v);
                // add a colon except at the very end
                if (!a.Contains("other"))
                {
                    fs = Utilities.strcat(fs, ":");
                }
            }
            return fs;
        }

        /**
         * Convert the value v to its proper string representation for insertion to
         * formatted string.
         * @param v value to convert
         * @return Formatted value
         */
        private String bindValueForFS(Object v)
        {
            if (v.GetType() == typeof(LogicalValue))
            {
                LogicalValue l = (LogicalValue)v;
                // The value NA binds to a blank.
                if (l.isANY())
                {
                    return "*";
                }
                // The value NA binds to a single hyphen.
                if (l.isNA())
                {
                    return "-";
                }
            }
            return processQuotedChars((String)v);
        }

        /**
         * Inspect each character in string s.  Certain nonalpha characters pass 
         * thru without escaping into the result, but most retain escaping.
         * @param s
         * @return 
         */
        private String processQuotedChars(String s)
        {
            String result = "";
            int idx = 0;
            while (idx < Utilities.strlen(s))
            {
                String c = Utilities.substr(s, idx, idx + 1);
                if (!c.Equals("\\"))
                {
                    // unquoted characters pass thru unharmed.
                    result = Utilities.strcat(result, c);
                }
                else
                {
                    // escaped characters are examined.
                    String nextchr = Utilities.substr(s, idx + 1, idx + 2);
                    // the period, hyphen and underscore pass unharmed.
                    if (nextchr.Equals(".") || nextchr.Equals("-") || nextchr.Equals("_"))
                    {
                        result = Utilities.strcat(result, nextchr);
                        idx = idx + 2;
                        continue;
                    }
                    else
                    {
                        // all others retain escaping.
                        result = Utilities.strcat(result, "\\", nextchr);
                        idx = idx + 2;
                        continue;
                    }
                }
                idx = idx + 1;
            }
            return result;
        }

        /**
         * Converts a string to the proper string for including in
         * a CPE v2.2-conformant URI.  The logical value ANY binds 
         * to the blank in the 2.2-conformant URI.
         * @param s string to be converted
         * @return converted string
         */
        private String bindValueForURI(Object s)
        {
            if (s.GetType() == typeof(LogicalValue))
            {
                LogicalValue l = (LogicalValue)s;
                // The value NA binds to a blank.
                if (l.isANY())
                {
                    return "";
                }
                // The value NA binds to a single hyphen.
                if (l.isNA())
                {
                    return "-";
                }
            }

            // If we get here, we're dealing with a string value.
            return transformForURI((String)s);
        }

        /**
         * Scans an input string and performs the following transformations:
         * - Pass alphanumeric characters thru untouched
         * - Percent-encode quoted non-alphanumerics as needed
         * - Unquoted special characters are mapped to their special forms
         * @param s string to be transformed
         * @return transformed string
         */
        private String transformForURI(String s)
        {
            String result = "";
            int idx = 0;

            while (idx < Utilities.strlen(s))
            {
                // Get the idx'th character of s.
                String thischar = Utilities.substr(s, idx, idx+ 1);
                // Alphanumerics (incl. underscore) pass untouched.
                if (Utilities.isAlphanum(thischar))
                {
                    result = Utilities.strcat(result, thischar);
                    idx = idx + 1;
                    continue;
                }
                // Check for escape character.
                if (thischar.Equals("\\"))
                {
                    idx = idx + 1;
                    String nxtchar = Utilities.substr(s, idx, idx + 1);
                    result = Utilities.strcat(result, pctEncode(nxtchar));
                    idx = idx + 1;
                    continue;
                }
                // Bind the unquoted '?' special character to "%01".
                if (thischar.Equals("?"))
                {
                    result = Utilities.strcat(result, "%01");
                }
                // Bind the unquoted '*' special character to "%02".
                if (thischar.Equals("*"))
                {
                    result = Utilities.strcat(result, "%02");
                }
                idx = idx + 1;
            }
            return result;
        }

        /**
         * Returns the appropriate percent-encoding of character c.
         * Certain characters are returned without encoding.
         * @param c the single character string to be encoded
         * @return the percent encoded string
         */
        private String pctEncode(String c)
        {
            if (c.Equals("!"))
            {
                return "%21";
            }
            if (c.Equals("\""))
            {
                return "%22";
            }
            if (c.Equals("#"))
            {
                return "%23";
            }
            if (c.Equals("$"))
            {
                return "%24";
            }
            if (c.Equals("%"))
            {
                return "%25";
            }
            if (c.Equals("&"))
            {
                return "%26";
            }
            if (c.Equals("'"))
            {
                return "%27";
            }
            if (c.Equals("("))
            {
                return "%28";
            }
            if (c.Equals(")"))
            {
                return "%29";
            }
            if (c.Equals("*"))
            {
                return "%2a";
            }
            if (c.Equals("+"))
            {
                return "%2b";
            }
            if (c.Equals(","))
            {
                return "%2c";
            }
            // bound without encoding.
            if (c.Equals("-"))
            {
                return c;
            }
            // bound without encoding.
            if (c.Equals("."))
            {
                return c;
            }
            if (c.Equals("/"))
            {
                return "%2f";
            }
            if (c.Equals(":"))
            {
                return "%3a";
            }
            if (c.Equals(";"))
            {
                return "%3b";
            }
            if (c.Equals("<"))
            {
                return "%3c";
            }
            if (c.Equals("="))
            {
                return "%3d";
            }
            if (c.Equals(">"))
            {
                return "%3e";
            }
            if (c.Equals("?"))
            {
                return "%3f";
            }
            if (c.Equals("@"))
            {
                return "%40";
            }
            if (c.Equals("["))
            {
                return "%5b";
            }
            if (c.Equals("\\"))
            {
                return "%5c";
            }
            if (c.Equals("]"))
            {
                return "%5d";
            }
            if (c.Equals("^"))
            {
                return "%5e";
            }
            if (c.Equals("`"))
            {
                return "%60";
            }
            if (c.Equals("{"))
            {
                return "%7b";
            }
            if (c.Equals("|"))
            {
                return "%7c";
            }
            if (c.Equals("}"))
            {
                return "%7d";
            }
            if (c.Equals("~"))
            {
                return "%7d";
            }
            // Shouldn't reach here, return original character
            return c;
        }

        /**
         * Packs the values of the five arguments into the single 
         * edition component.  If all the values are blank, the 
         * function returns a blank.
         * @param ed edition string
         * @param sw_ed software edition string
         * @param t_sw target software string
         * @param t_hw target hardware string
         * @param oth other edition information string
         * @return the packed string, or blank
         */
        private String pack(String ed, String sw_ed, String t_sw, String t_hw, String oth)
        {
            if (sw_ed.Equals("") && t_sw.Equals("") && t_hw.Equals("") && oth.Equals(""))
            {
                // All the extended attributes are blank, so don't do
                // any packing, just return ed.
                return ed;
            }
            // Otherwise, pack the five values into a single string
            // prefixed and internally delimited with the tilde.
            return Utilities.strcat("~", ed, "~", sw_ed, "~", t_sw, "~", t_hw, "~", oth);
        }

        /**
         * Removes trailing colons from the URI.
         * @param s the string to be trimmed
         * @return the trimmed string
         */
        private String trim(String s)
        {
            String s1 = Utilities.reverse(s);
            int idx = 0;
            for (int i = 0; i != Utilities.strlen(s1); i++)
            {
                if (Utilities.substr(s1, i, i + 1).Equals(":"))
                {
                    idx = idx + 1;
                }
                else
                {
                    break;
                }
            }
            // Return the substring after all trailing colons,
            // reversed back to its original character order.
            return Utilities.reverse(Utilities.substr(s1, idx, Utilities.strlen(s1)));
        }



    }
}

