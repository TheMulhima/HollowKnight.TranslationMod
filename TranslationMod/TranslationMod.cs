using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Language;
using Modding;

namespace TranslationMod
{
	public class TranslationMod : Mod, ITogglableMod
	{
		//lang, sheetTitle, key, text
		public Dictionary<string, Dictionary<string, Dictionary<string, string>>> translationDict;
		public override string GetVersion() => "1.1.0";

		public TranslationMod()
		{
			InitializeDictionaries();
			ModHooks.LanguageGetHook += LanguageGetHook;
		}

		public override void Initialize()
		{
			Log("Initializing Poorly Translated Mod");
		}

		public void Unload()
		{
			ModHooks.LanguageGetHook -= LanguageGetHook;
		}

		private string LanguageGetHook(string key, string sheettitle, string orig)
		{
			string lang = Language.Language.CurrentLanguage().ToString();
			if (translationDict.ContainsKey(lang))
			{
				if (translationDict[lang].ContainsKey(sheettitle))
				{
					if (translationDict[lang][sheettitle].TryGetValue(key, out var newText))
					{
						return newText;
					}
					else
					{
						LogWarn($"Not Found {lang} {key}:{orig}");
					}
				}
			}

			return orig;
		}

		private void InitializeDictionaries()
		{
			translationDict = new Dictionary<string,Dictionary<string, Dictionary<string, string>>>();
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Dictionary<string, List<string>> LangsAndSheets = new Dictionary<string, List<string>>();
			foreach (string text in executingAssembly.GetManifestResourceNames())
			{
				LogDebug(text);
				if (text.EndsWith("txt"))
				{
					var structure = Path.GetFileNameWithoutExtension(text).Split('.');
					var Lang = structure[2];
					var Sheet = structure[3];
					if (!LangsAndSheets.ContainsKey(Lang))
					{
						LangsAndSheets.Add(Lang, new List<string>{Sheet});
					}
					else
					{
						LangsAndSheets[Lang].Add(Sheet);
					}
				}
			}

			foreach (var (lang, sheetList) in LangsAndSheets)
			{
				Dictionary<string, Dictionary<string, string>> langDict = new();
				foreach (var sheet in sheetList )
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					StreamReader streamReader = new StreamReader(executingAssembly.GetManifestResourceStream(
						$"TranslationMod.Resources.{lang}.{sheet}.txt")!);
					string text2;
					while ((text2 = streamReader.ReadLine()) != null)
					{
						dictionary.Add(text2.Split(new char[]
						{
							':'
						})[0].Trim(), text2.Split(new char[]
						{
							':'
						})[1].Trim());
					}
					streamReader.Close();
					langDict.Add(sheet, dictionary);
				}
				translationDict.Add(lang, langDict);
			}
		}
	}
}
