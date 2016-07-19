using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace org.mitre.cpe.common
{
	public class Utilities
	{

		public static String strcat(params String[] strings)
		{
			String s = "";
			for (int i = 0; (i != strings.Length); i++)
			{
				s = string.Concat(s, strings[i]);

			}

			return s;
		}

		public static String substr(String s, int startIndex, int endIndex)
		{
			int length = endIndex - startIndex ;
			return s.Substring(startIndex, length);
		}

		public static int strlen(String s)
		{
			return s.Length;
		}

		public static int strchr(String s, String chr, int off)
		{
			return s.IndexOf(chr, off);
		}

		public static String toLowercase(String s)
		{
			return s.ToLower();
		}

		public static int indexOf(String str1, String str2, int off)
		{
			return str1.IndexOf(str2, off);
		}

		public static bool containsWildcards(String str)
		{
			if ((str.Contains("*") || str.Contains("?")))
			{
				if (!str.Contains("\\\\"))
				{
					return true;
				}

				return false;
			}

			return false;
		}

		public static bool isEvenNumber(int num)
		{
			if (((num % 2)
						== 0))
			{
				return true;
			}
			else
			{
				return false;
			}

		}

		public static int countEscapeCharacters(String str, int start, int end)
		{
			int result = 0;
			bool active = false;
			int i = 0;
			while ((i < end))
			{
				if ((active
							&& (i >= start)))
				{
					result = (result + 1);
				}

				i = (i + 1);
			}

			return result;
		}

		public static int getUnescapedColonIndex(String str)
		{
			bool found = false;
			int colon_idx = 0;
			int start_idx = 0;
			//  Find the first non-escaped colon.
			while (!found)
			{
				colon_idx = str.IndexOf(":", (start_idx + 1));
				//  If no colon is found, return 0.
				if ((colon_idx == -1))
				{
					return 0;
				}

				//  Peek at character before colon.
				if (str.Substring((colon_idx - 1), colon_idx).Equals("\\\\"))
				{
					//  If colon is escaped, keep looking.
					start_idx = colon_idx;
				}
				else
				{
					found = true;
				}

			}

			return colon_idx;
		}

		public static bool isAlphanum(String c)
		{
			if ((c.All(char.IsLetterOrDigit) || c.Equals("_")))
			{
				return true;
			}

			return false;
		}

		public static String reverse(String s)
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		public static void validateURI(String str)
		{
			//  make sure uri starts with cpe:/
			if (!str.ToLower().StartsWith("cpe:/"))
			{
				throw new FormatException("Error: URI must start with \'cpe:/\'.  Given: " + str);
			}

			//  make sure uri doesn't contain more than 7 colons
			int count = 0;
			for (int i = 0; (i != str.Length); i++)
			{
				if ((str.ToCharArray().ElementAt(i) == ':'))
				{
					count++;
				}

			}

			if ((count > 7))
			{
				throw new FormatException("Error parsing URI.  Found "
								+ ((count - 7) + (" extra components in: " + str)));
			}

		}

		public static void validateFS(String str)
		{
			if (!str.ToLower().StartsWith("cpe:2.3:"))
			{
				throw new FormatException("Error: Formatted String must start with \\\"cpe:2.3\\\". Given: " + str);
			}

			//  make sure fs contains exactly 12 unquoted colons
			int count = 0;
			for (int i = 0; (i != str.Length); i++)
			{
				if ((str.ToCharArray().ElementAt(i) == ':'))
				{
					if ((str.ToCharArray().ElementAt((i - 1)) != '\\'))
					{
						count++;
					}

					if ((((i + 1)
								< str.Length)
								&& (str.ToCharArray().ElementAt((i + 1)) == ':')))
					{
						throw new FormatException("Error parsing formatted string.  Found empty component");
					}

				}

			}

			if ((count > 12))
			{
				int extra = (count - 12);
				StringBuilder s = new StringBuilder(("Error parsing formatted string.  Found "
								+ (extra + " extra component")));
				if ((extra > 1))
				{
					s.Append("s");
				}

				s.Append((" in: " + str));
				throw new FormatException(s.ToString());
			}

			if ((count < 12))
			{
				int missing = (12 - count);
				StringBuilder s = new StringBuilder(("Error parsing formatted string. Missing "
								+ (missing + " component")));
				if ((missing > 1))
				{
					s.Append("s");
				}

				throw new FormatException(s.ToString());
			}

		}

		public static bool ContainsControlCharacters(string str)
		{
			bool returnValue = false;
			foreach (char chr in str)
			{
				if (Char.IsControl(chr))
				{
					returnValue = true;
					break;
				}
			}
			return returnValue;
		}
	}

}



