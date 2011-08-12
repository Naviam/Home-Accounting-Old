using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Naviam.WebUI.Helpers.Parsers
{
    public class ParsersFactory
    {
        public static ParserStatement ParseFile(string fileName)
        {
            var assem = Assembly.GetExecutingAssembly();
            foreach (Type t in assem.GetExportedTypes())
            {
                if (!t.IsClass || !t.IsPublic) continue;
                if (t.BaseType == typeof(ParserBase))
                {
                    ParserBase parser = (ParserBase)Activator.CreateInstance(t);
                    if (parser != null)
                    {
                        if (parser.IsMyFile(fileName))
                        {
                            var stat = parser.ParseFile(fileName);
                            return stat;
                        }
                    }
                }
            }
            return null;
        }
    }
}