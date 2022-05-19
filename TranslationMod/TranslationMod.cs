using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Language;
using Modding;

namespace TranslationMod
{
	public class TranslationMod : Mod, ITogglableMod
	{
		public Dictionary<string, Dictionary<string, string>> translationDict;
		public override string GetVersion() => "1.0.1";

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
			if (translationDict.ContainsKey(sheettitle))
			{
				if (translationDict[sheettitle].TryGetValue(key, out var newText))
				{
					return newText;
				}
				else
				{
					LogWarn($"Not Found {key}:{orig}");
				}
			}
			return orig;
		}

		private void InitializeDictionaries()
		{
			translationDict = new Dictionary<string, Dictionary<string, string>>();
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			foreach (string text in executingAssembly.GetManifestResourceNames())
			{
				LogDebug(text);
				if (text.EndsWith("txt"))
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					StreamReader streamReader = new StreamReader(executingAssembly.GetManifestResourceStream(text));
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
					translationDict.Add(Path.GetFileNameWithoutExtension(text).Remove(0, 25), dictionary);
				}
			}
		}
	}
}
