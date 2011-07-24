using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace JavascriptResxGenerator
{
	class Program
	{
		private static string[] _ReservedWords = new string[] { "abstract", "as", "boolean", "break", "byte", "case", "catch", "char", "class", "continue", "const", "debugger", "default", "delete", "do", "double", "else", "enum", "export", "extends", "false", "final", "finally", "float", "for", "function", "goto", "if", "implements", "import", "in", "instanceof", "int", "interface", "is", "long", "namespace", "native", "new", "null", "package", "private", "protected", "public", "return", "short", "static", "super", "switch", "synchronized", "this", "throw", "throws", "transient", "true", "try", "typeof", "use", "var", "void", "volatile", "while", "with" };
		private static string _JavascriptFileTemplate = "(function () {" + Environment.NewLine + Environment.NewLine + "#REGISTERNAMESPACE#" + Environment.NewLine + Environment.NewLine + "\tvar resources = {#KEYS#};" + Environment.NewLine + Environment.NewLine + "\t#NAMESPACE# = resources;" + Environment.NewLine + Environment.NewLine + "})();";
		private static string _VsDocFileTemplate = "(function () {" + Environment.NewLine + Environment.NewLine + "#REGISTERNAMESPACE#" + Environment.NewLine + Environment.NewLine + "\t#NAMESPACE# = { #KEYS# };" + Environment.NewLine + Environment.NewLine + "})();";

		static void Main(string[] args)
		{
			string resxFilePath = args[0];
			string outDir = args[1];
			string vsdocOutDir = args[2];
			string registerNamespace = args[3];

			if (Path.HasExtension(outDir))
			{
				outDir = Path.GetDirectoryName(outDir);
			}

			if (Path.HasExtension(vsdocOutDir))
			{
				vsdocOutDir = Path.GetDirectoryName(vsdocOutDir);
			}

			string[] resxFiles = null;

			if (!Path.HasExtension(resxFilePath)) // The specified path is a directory. Generate JS Resx files for all .resx files in that directory.
			{
				resxFiles = Directory.GetFiles(resxFilePath, "*.resx");
			}
			else // The specified path points to a single .resx file. Generate JS Resx files for that, and it's siblings (other .resx files with the same name, except the region specific (.en, .da) token).
			{
				string baseResxFileName = GetBaseFileName(resxFilePath);

				resxFiles = Directory.GetFiles(Path.GetDirectoryName(resxFilePath), baseResxFileName + "*.resx");
			}

			foreach (var item in resxFiles)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
				string baseJsFileName = GetBaseFileName(item);
				string resourceNamespace = String.Join(".", registerNamespace, baseJsFileName);

				string javascriptOutDir = String.Format("{0}\\{1}.en.js", outDir, resourceNamespace);
				string javascriptVsdocOutDir = String.Format("{0}\\{1}-vsdoc.js", vsdocOutDir, resourceNamespace);
				bool generateVsDoc = true;

				if (fileNameWithoutExtension.Contains("."))
				{
					generateVsDoc = false;

					string regionToken = fileNameWithoutExtension.Substring(fileNameWithoutExtension.IndexOf('.') + 1);

					javascriptOutDir = String.Format("{0}\\{1}.{2}.js", outDir, resourceNamespace, regionToken);
				}

				GenerateJavaScriptResxFile(item, javascriptOutDir, javascriptVsdocOutDir, resourceNamespace, registerNamespace, generateVsDoc);
			}
		}

		private static void GenerateJavaScriptResxFile(string resxFilePath, string outDir, string vsdocOutDir, string resourceNamespace, string registerNamespace, bool generateVsDoc)
		{
			StringBuilder resourceSb = new StringBuilder();
			StringBuilder vsdocSb = new StringBuilder();

			using (ResXResourceReader rr = new ResXResourceReader(resxFilePath))
			{
				IDictionaryEnumerator di = rr.GetEnumerator();

				foreach (DictionaryEntry de in rr)
				{
					string key = de.Key as string;
					string value = de.Value as string;

					if (_ReservedWords.Contains(key))
					{
						key = "_" + key;
					}

					resourceSb.AppendFormat("'{0}': '{1}',", key, value.Replace("'", "\\'").Replace(Environment.NewLine, String.Empty));

					if (generateVsDoc)
					{
						vsdocSb.AppendFormat("'{0}': '',", key);
					}
				}
			}

			if (!String.IsNullOrWhiteSpace(registerNamespace))
			{
				registerNamespace = "\t" + registerNamespace + " = {};";
			}

			var resourceFileContents = _JavascriptFileTemplate.Replace("#KEYS#", resourceSb.ToString().TrimEnd(',')).Replace("#REGISTERNAMESPACE#", registerNamespace).Replace("#NAMESPACE#", resourceNamespace);
			File.WriteAllText(outDir, resourceFileContents);

			if (generateVsDoc)
			{
				var vsdocFileContents = _VsDocFileTemplate.Replace("#KEYS#", vsdocSb.ToString().TrimEnd(',')).Replace("#REGISTERNAMESPACE#", registerNamespace).Replace("#NAMESPACE#", resourceNamespace);
				File.WriteAllText(vsdocOutDir, vsdocFileContents);
			}
		}

		private static string GetBaseFileName(string filePath)
		{
			string baseFileName = Path.GetFileNameWithoutExtension(filePath);

			if (baseFileName.Contains("."))
			{
				baseFileName = baseFileName.Substring(0, baseFileName.LastIndexOf('.'));
			}

			return baseFileName;
		}
	}
}