using System;
using System.IO;

namespace HigurashiVitaCovnerter {
	class MainClass {
		public const string converterVersionString = "v1.7.4";
		// 3 is v1.2
		// 9 is v1.7
		// 11 is v1.7.2
		// 12 is v1.7.3
		// 13 is v1.7.4
		public const int converterVersionNumber = 13;
		
		public static bool IsRunningOnMono(){
			return Type.GetType ("Mono.Runtime") != null;
		}
		
		public static void Main(string[] args) {

			if (IsRunningOnMono()==false){
				// I don't really want to mess with this on Mono.
				DisableConsoleQuickEdit.Go();
			}else{
				Console.Out.WriteLine("Detected Mono.");
			}
			
			Console.WriteLine("Hello World!");
			Console.Out.WriteLine("Higurashi-Vita Script Converter "+converterVersionString+" ("+converterVersionNumber+")");
			if (args.Length == 2) {
				if (args[0].ToLower() == "ps3") {
					Console.Out.WriteLine("Force PS3 conversion");
					NotStatic.conversionType = NotStatic.type_ps3;
				}else if (args[0].ToLower()=="steam"){
					Console.Out.WriteLine("Force Steam conversion");
					NotStatic.conversionType = NotStatic.type_steam;
				}else{
					Console.Out.WriteLine("Unknown command line argument "+args[0]);
					NotStatic.conversionType = NotStatic.type_undefined;
				}
			}

			if (Directory.Exists("./StreamingAssets") == false){
				Console.Out.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!\nCould not find ./StreamingAssets/\n!!!!!!!!!!!!!!!!!!!!!!!");
				Console.Out.WriteLine("You need to place the StreamingAssets folder in the same directory as this exe file.\nSee the thread for more clear instructions.");
				Console.ReadLine();
				return;
			}
			
			NotStatic athingiethatisntstatic = new NotStatic("./StreamingAssets");
			Console.Out.WriteLine("============= DONE! ==============");
			Console.Out.WriteLine("The conversion is done. You may close this window.");
			Console.ReadLine();
		}
	}
}
