using System;
using System.IO;

namespace HigurashiVitaCovnerter {
	class MainClass {
		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			if (args.Length == 2) {
				if (args[0] == "pc") {
					NotStatic.FixImages(args[1],true);
					Console.Out.WriteLine("=== No reason resave complete ===");
					Console.ReadLine();
					return;
				}
			}

			if (Directory.Exists("./StreamingAssets") == false){
				Console.Out.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!\nCould not find ./StreamingAssets/\n!!!!!!!!!!!!!!!!!!!!!!!");
				Console.Out.WriteLine("You need to place the StreamingAssets folder in the same directory as this exe file.\nSee the thread for more clear instructions.");
				Console.ReadLine();
				return;
			}

			NotStatic athingiethatisntstatic = new NotStatic("./StreamingAssets");
			Console.Out.WriteLine("========= DONE! =========");
			Console.Out.WriteLine("The conversion is done. You may close this window.");
			Console.ReadLine();
		}
	}
}
