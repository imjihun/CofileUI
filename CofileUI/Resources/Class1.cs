using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CofileUI
{
	internal class MakeStringResourceFile
	{
		internal string test = "test";
		internal void testdsa()
		{
			ResourceDictionary r = new ResourceDictionary();

			object[] Kind_MainTab = new object[] {"MainTab", "Cofile", "Config", "Log", "Monitor" };
			for(int i = 1; i < Kind_MainTab.Length; i++)
				r.Add(Kind_MainTab[0] + "." + Kind_MainTab[i], Kind_MainTab[i]);

			object[] Type_Dialog = new object[] {"Title", "Message" };
			object[] Kind_Dialog = new object[] {"Dialog", "AllEncrypt", "AllDecrypt", "SelectedEncrypt", "SelectedDecrypt"};
			for(int i = 1; i < Kind_Dialog.Length; i++)
			{
				for(int j = 0; j < Type_Dialog.Length; j++)
				{
					r.Add(Kind_Dialog[0] + "." + Kind_Dialog[i] + "." + Type_Dialog[j], Kind_Dialog[i]);
				}
			}
		}
	}
}
