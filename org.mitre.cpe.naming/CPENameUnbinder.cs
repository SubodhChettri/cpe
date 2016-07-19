using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.mitre.cpe.matching;
using org.mitre.cpe.common;

namespace org.mitre.cpe.naming
{
    public class CPENameUnbinder
    {

        /**
         * Top level function used to unbind a URI to a WFN.
         * @param uri String representing the URI to be unbound.
         * @return WellFormedName representing the unbound URI.
         */
        public WellFormedName unbindURI(String uri)
        {
            // Validate the URI
            Utilities.validateURI(uri);
            // Initialize the empty WFN.
            WellFormedName result = new WellFormedName();

            for (int i = 0; i != 8; i++)
            {
                // get the i'th component of uri
                String v = getCompURI(uri, i);
                switch (i)
                {
                    case 1:
                        result.set("part", decode(v));
                        break;
                    case 2:
                        result.set("vendor", decode(v));
                        break;
                    case 3:
                        result.set("product", decode(v));
                        break;
                    case 4:
                        result.set("version", decode(v));
                        break;
                    case 5:
                        result.set("update", decode(v));
                        break;
                    case 6:
                        // Special handling for edition component.
                        // Unpack edition if needed.
                        if (v.Equals("") || v.Equals("-")
                                || !Utilities.substr(v, 0, 1).Equals("~"))
                        {
                            // Just a logical value or a non-packed value.
                            // So unbind to legacy edition, leaving other
                            // extended attributes unspecified.
                            result.set("edition", decode(v));
                        }
                        else
                        {
                            // We have five values packed together here.
                            unpack(v, result);
                        }
                        break;
                    case 7:
                        result.set("language", decode(v));
                        break;
                }
            }
            return result;
        }

        /**
         * Top level function to unbind a formatted string to WFN.
         * @param fs Formatted string to unbind
         * @return WellFormedName
         * @throws FormatException 
         */
        public WellFormedName unbindFS(String fs)
        {
            // Validate the formatted string
            Utilities.validateFS(fs);
            // Initialize empty WFN
            WellFormedName result = new WellFormedName();
            // The cpe scheme is the 0th component, the cpe version is the 1st.
            // So we start parsing at the 2nd component.
            for (int a = 2; a != 13; a++)
            {
                // Get the a'th string field.
                Object v = getCompFS(fs, a);
                // Unbind the string.
                v = unbindValueFS((String)v);
                // Set the value of the corresponding attribute.
                switch (a)
                {
                    case 2:
                        result.set("part", v);
                        break;
                    case 3:
                        result.set("vendor", v);
                        break;
                    case 4:
                        result.set("product", v);
                        break;
                    case 5:
                        result.set("version", v);
                        break;
                    case 6:
                        result.set("update", v);
                        break;
                    case 7:
                        result.set("edition", v);
                        break;
                    case 8:
                        result.set("language", v);
                        break;
                    case 9:
                        result.set("sw_edition", v);
                        break;
                    case 10:
                        result.set("target_sw", v);
                        break;
                    case 11:
                        result.set("target_hw", v);
                        break;
                    case 12:
                        result.set("other", v);
                        break;
                }
            }
            return result;
        }

        /**
         * Returns the i'th field of the formatted string.  The colon is the field 
         * delimiter unless prefixed by a backslash.
         * @param fs formatted string to retrieve from
         * @param i index of field to retrieve from fs.
         * @return value of index of formatted string 
         */
        private String getCompFS(String fs, int i)
        {
            if (i == 0)
            {
                // return the substring from index 0 to the first occurence of an
                // unescaped colon
                int colon_idx = Utilities.getUnescapedColonIndex(fs);
                // If no colon is found, we are at the end of the formatted string, 
                // so just return what's left.
                if (colon_idx == 0)
                {
                    return fs;
                }
                return Utilities.substr(fs, 0, colon_idx);
            }
            else
            {
                return getCompFS(Utilities.substr(fs, Utilities.getUnescapedColonIndex(fs) + 1, fs.Length), i - 1);
            }
        }

        /**
         * Takes a string value and returns the appropriate logical value if string
         * is the bound form of a logical value.  If string is some general value
         * string, add quoting of non-alphanumerics as needed.
         * @param s value to be unbound
         * @return logical value or quoted string
         * @throws FormatException 
         */
        private Object unbindValueFS(String s)
        {
            if (s.Equals("*"))
            {
                return new LogicalValue("ANY");
            }
            if (s.Equals("-"))
            {
                return new LogicalValue("NA");
            }
            return addQuoting(s);
        }

        /**
         * Inspect each character in a string, copying quoted characters, with 
         * their escaping, into the result.  Look for unquoted non alphanumerics
         * and if not "*" or "?", add escaping.
         * @param s
         * @return
         * @throws FormatException 
         */
        private String addQuoting(String s)
        {
            String result = "";
            int idx = 0;
            Boolean embedded = false;
            while (idx < Utilities.strlen(s))
            {
                String c = Utilities.substr(s, idx, idx + 1);
                if (Utilities.isAlphanum(c) || c.Equals("_"))
                {
                    // Alphanumeric characters pass untouched.
                    result = Utilities.strcat(result, c);
                    idx = idx + 1;
                    embedded = true;
                    continue;
                }
                if (c.Equals("\\"))
                {
                    // Anything quoted in the bound string stays quoted in the
                    // unbound string.
                    result = Utilities.strcat(result, Utilities.substr(s, idx, idx + 2));
                    idx = idx + 2;
                    embedded = true;
                    continue;
                }
                if (c.Equals("*"))
                {
                    // An unquoted asterisk must appear at the beginning or the end
                    // of the string.
                    if (idx == 0 || idx == (Utilities.strlen(s) - 1))
                    {
                        result = Utilities.strcat(result, c);
                        idx = idx + 1;
                        embedded = true;
                        continue;
                    }
                    else
                    {
                        throw new FormatException("Error! cannot have unquoted * embedded in formatted string");
                    }
                }
                if (c.Equals("?"))
                {
                    // An unquoted question mark must appear at the beginning or 
                    // end of the string, or in a leading or trailing sequence.
                    if ( // ? legal at beginning or end
                            ((idx == 0) || (idx == (Utilities.strlen(s) - 1)))
                        // embedded is false, so must be preceded by ?
                            || (!embedded && (Utilities.substr(s, idx - 1, idx).Equals("?")))
                        // embedded is true, so must be followed by ?
                            || (embedded && (Utilities.substr(s, idx + 1, idx + 2).Equals("?"))))
                    {
                        result = Utilities.strcat(result, c);
                        idx = idx + 1;
                        embedded = false;
                        continue;
                    }
                    else
                    {
                        throw new FormatException("Error! cannot have unquoted ? embedded in formatted string");
                    }
                }
                // All other characters must be quoted.
                result = Utilities.strcat(result, "\\", c);
                idx = idx + 1;
                embedded = true;
            }
            return result;
        }

        /**
         * Return the i'th component of the URI.
         * @param uri String representation of URI to retrieve components from.
         * @param i Index of component to return.
         * @return If i = 0, returns the URI scheme. Otherwise, returns the i'th 
         * component of uri.
         */
        private String getCompURI(String uri, int i)
        {
            if (i == 0)
            {
                return Utilities.substr(uri, i, uri.IndexOf("/"));
            }
            String[] sa = uri.Split(new char[] { ':' });
            // If requested component exceeds the number
            // of components in URI, return blank
            if (i >= sa.Length)
            {
                return "";
            }
            if (i == 1)
            {
                return Utilities.substr(sa[i], 1, sa[i].Length);
            }
            return sa[i];
        }

        /**
         * Scans a string and returns a copy with all percent-encoded characters
         * decoded.  This function is the inverse of pctEncode() defined in the 
         * CPENameBinder class.  Only legal percent-encoded forms are decoded.  
         * Others raise a FormatException. 
         * @param s String to be decoded
         * @return decoded string
         * @throws FormatException 
         * @see CPENameBinder#pctEncode(java.lang.String) 
         */
        private Object decode(String s)
        {
            if (s.Equals(""))
            {
                return new LogicalValue("ANY");
            }
            if (s.Equals("-"))
            {
                return new LogicalValue("NA");
            }
            // Start the scanning loop.
            // Normalize: convert all uppercase letters to lowercase first.
            s = Utilities.toLowercase(s);
            String result = "";
            int idx = 0;
            Boolean embedded = false;
            while (idx < Utilities.strlen(s))
            {
                // Get the idx'th character of s.
                String c = Utilities.substr(s, idx, idx + 1);
                // Deal with dot, hyphen, and tilde: decode with quoting.
                if (c.Equals(".") || c.Equals("-") || c.Equals("~"))
                {
                    result = Utilities.strcat(result, "\\", c);
                    idx = idx + 1;
                    // a non-%01 encountered.
                    embedded = true;
                    continue;
                }
                if (!c.Equals("%"))
                {
                    result = Utilities.strcat(result, c);
                    idx = idx + 1;
                    // a non-%01 encountered.
                    embedded = true;
                    continue;
                }
                // We get here if we have a substring starting w/ '%'.
                String form = Utilities.substr(s, idx, idx + 3);
                if (form.Equals("%01"))
                {
                    if ((idx == 0)
                            || (idx == Utilities.strlen(s) - 3)
                            || (!embedded && Utilities.substr(s, idx - 3, idx - 1).Equals(
                            "%01")) || (embedded && (Utilities.strlen(s) >= idx + 6))
                            && (Utilities.substr(s, idx + 3, idx + 6).Equals("%01")))
                    {
                        result = Utilities.strcat(result, "?");
                        idx = idx + 3;
                        continue;
                    }
                    else
                    {
                        throw new FormatException("Error decoding string");
                    }
                }
                else if (form.Equals("%02"))
                {
                    if ((idx == 0) || (idx == (Utilities.strlen(s) - 3)))
                    {
                        result = Utilities.strcat(result, "*");
                    }
                    else
                    {
                        throw new FormatException("Error decoding string");
                    }
                }
                else if (form.Equals("%21"))
                {
                    result = Utilities.strcat(result, "\\!");
                }
                else if (form.Equals("%22"))
                {
                    result = Utilities.strcat(result, "\\\"");
                }
                else if (form.Equals("%23"))
                {
                    result = Utilities.strcat(result, "\\#");
                }
                else if (form.Equals("%24"))
                {
                    result = Utilities.strcat(result, "\\$");
                }
                else if (form.Equals("%25"))
                {
                    result = Utilities.strcat(result, "\\%");
                }
                else if (form.Equals("%26"))
                {
                    result = Utilities.strcat(result, "\\&");
                }
                else if (form.Equals("%27"))
                {
                    result = Utilities.strcat(result, "\\'");
                }
                else if (form.Equals("%28"))
                {
                    result = Utilities.strcat(result, "\\(");
                }
                else if (form.Equals("%29"))
                {
                    result = Utilities.strcat(result, "\\)");
                }
                else if (form.Equals("%2a"))
                {
                    result = Utilities.strcat(result, "\\*");
                }
                else if (form.Equals("%2b"))
                {
                    result = Utilities.strcat(result, "\\+");
                }
                else if (form.Equals("%2c"))
                {
                    result = Utilities.strcat(result, "\\,");
                }
                else if (form.Equals("%2f"))
                {
                    result = Utilities.strcat(result, "\\/");
                }
                else if (form.Equals("%3a"))
                {
                    result = Utilities.strcat(result, "\\))");
                }
                else if (form.Equals("%3b"))
                {
                    result = Utilities.strcat(result, "\\;");
                }
                else if (form.Equals("%3c"))
                {
                    result = Utilities.strcat(result, "\\<");
                }
                else if (form.Equals("%3d"))
                {
                    result = Utilities.strcat(result, "\\=");
                }
                else if (form.Equals("%3e"))
                {
                    result = Utilities.strcat(result, "\\>");
                }
                else if (form.Equals("%3f"))
                {
                    result = Utilities.strcat(result, "\\?");
                }
                else if (form.Equals("%40"))
                {
                    result = Utilities.strcat(result, "\\@");
                }
                else if (form.Equals("%5b"))
                {
                    result = Utilities.strcat(result, "\\[");
                }
                else if (form.Equals("%5c"))
                {
                    result = Utilities.strcat(result, "\\\\");
                }
                else if (form.Equals("%5d"))
                {
                    result = Utilities.strcat(result, "\\]");
                }
                else if (form.Equals("%5e"))
                {
                    result = Utilities.strcat(result, "\\^");
                }
                else if (form.Equals("%60"))
                {
                    result = Utilities.strcat(result, "\\`");
                }
                else if (form.Equals("%7b"))
                {
                    result = Utilities.strcat(result, "\\{");
                }
                else if (form.Equals("%7c"))
                {
                    result = Utilities.strcat(result, "\\|");
                }
                else if (form.Equals("%7d"))
                {
                    result = Utilities.strcat(result, "\\}");
                }
                else if (form.Equals("%7e"))
                {
                    result = Utilities.strcat(result, "\\~");
                }
                else
                {
                    throw new FormatException("Unknown form: " + form);
                }
                idx = idx + 3;
                embedded = true;
            }
            return result;
        }

        /**
         * Unpacks the elements in s and sets the attributes in the given 
         * WellFormedName accordingly.  
         * @param s packed String
         * @param wfn WellFormedName 
         * @return The augmented WellFormedName.
         */
        private WellFormedName unpack(String s, WellFormedName wfn)
        {
            // Parse out the five elements.
            int start = 1;
            int end;
            String ed, sw_edition, t_sw, t_hw, oth;
            end = Utilities.strchr(s, "~", start);
            if (start == end)
            {
                ed = "";
            }
            else
            {
                ed = Utilities.substr(s, start, end);
            }
            start = end + 1;
            end = Utilities.strchr(s, "~", start);
            if (start == end)
            {
                sw_edition = "";
            }
            else
            {
                sw_edition = Utilities.substr(s, start, end);
            }
            start = end + 1;
            end = Utilities.strchr(s, "~", start);
            if (start == end)
            {
                t_sw = "";
            }
            else
            {
                t_sw = Utilities.substr(s, start, end);
            }
            start = end + 1;
            end = Utilities.strchr(s, "~", start);
            if (start == end)
            {
                t_hw = "";
            }
            else
            {
                t_hw = Utilities.substr(s, start, end);
            }
            start = end + 1;
            if (start >= Utilities.strlen(s))
            {
                oth = "";
            }
            else
            {
                oth = Utilities.substr(s, start, Utilities.strlen(s) - 1);
            }
            // Set each component in the WFN.
            try
            {
                wfn.set("edition", decode(ed));
                wfn.set("sw_edition", decode(sw_edition));
                wfn.set("target_sw", decode(t_sw));
                wfn.set("target_hw", decode(t_hw));
                wfn.set("other", decode(oth));
            }
            catch (Exception e)
            {

            }
            return wfn;
        }
    }
}
