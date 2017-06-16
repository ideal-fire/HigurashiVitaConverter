using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace HigurashiVitaCovnerter {
	public class NotStatic {
		public const int type_undefined = 0;
		public const int type_ps3 = 1;
		public const int type_steam = 3;
		public static int conversionType = type_undefined;

		
		
		void FixScriptFolders(string StreamingAssetsNoEndSlash){
			if (Directory.Exists(StreamingAssetsNoEndSlash+"/Update/")==true){
				Console.Out.WriteLine("Transfer Update to Scripts");
				foreach(string file in Directory.GetFiles(StreamingAssetsNoEndSlash+"/Update/")){
					File.Copy(file, Path.Combine(StreamingAssetsNoEndSlash+"/Scripts/", Path.GetFileName(file)),true);
				}
			}else{
				Console.Out.WriteLine("...? There's no Update folder...");
			}
		}

		void CopyPresets(string StreamingAssetsNoEndSlash) {
			foreach(string file in Directory.GetFiles("./PackagedPresets/")){
				File.Copy(file, Path.Combine(StreamingAssetsNoEndSlash+"/Presets/", Path.GetFileName(file)),true);
			}
		}
		
		void DeleteIfExist(string filepath){
			if (File.Exists(filepath)==true){
				File.Delete(filepath);
			}
		}
		
		public NotStatic(string StreamingAssetsNoEndSlash) {
			string[] folderEntries = Directory.GetDirectories(StreamingAssetsNoEndSlash);

			FixScriptFolders(StreamingAssetsNoEndSlash);
			
			//return;
			
			Console.Out.WriteLine("========= SCRIPTS START ==========");
			FixScripts(StreamingAssetsNoEndSlash+"/Scripts/");

			//return;

			Console.Out.WriteLine("========= SCRIPTS DONE ==========");
			Console.Out.WriteLine("========= PRESETS START =========");
			if (Directory.Exists("./PackagedPresets/") == true) {
				Directory.CreateDirectory(StreamingAssetsNoEndSlash + "/Presets");
				CopyPresets(StreamingAssetsNoEndSlash);
			} else {
				Console.WriteLine("!!!!!!!!!! WARNINING !!!!!!!!!!!!!");
				Console.Out.WriteLine("The folder PackagedPresets was not found in the same directory as this exe.\nIf you have misplaced the folder, please redownload the program.\nIf you ignore this warning, your StreamingAssets folder will have no presets in it by default.\nYou'll need to put them all in yourself.");
				Console.WriteLine("!!!!!!!!!! WARNINING !!!!!!!!!!!!!");
				Console.Out.WriteLine("==Press enter to continue==");
				Console.ReadLine();
			}
			Console.Out.WriteLine("========= PRESETS END ===========");
			Console.Out.WriteLine("=========== MENU SFX ============");
			if (File.Exists("./wa_038.ogg")==true){
				if (File.Exists(StreamingAssetsNoEndSlash+"/SE/wa_038.ogg")){
					File.Delete(StreamingAssetsNoEndSlash+"/SE/wa_038.ogg");
				}
				File.Copy("./wa_038.ogg",StreamingAssetsNoEndSlash+"/SE/wa_038.ogg");
			}else{
				Console.Out.WriteLine("Oh, the menu sound effect isn't here. Oh well.");
			}
			Console.Out.WriteLine("========= IMAGES, START =========");
			Console.Out.WriteLine("This may take a while, please wait warmly.");
			
			
			// Detect if PS3 patch or not
			if (File.Exists(StreamingAssetsNoEndSlash+"/CG/re_se_de_a1.png")==true && conversionType==type_undefined){
				using (Bitmap _tempRena = new Bitmap(StreamingAssetsNoEndSlash+"/CG/re_se_de_a1.png")){
					if (_tempRena.Width==640 && _tempRena.Height==480){
						Console.Out.WriteLine("Detected normal Higurashi");
						conversionType = type_steam;
					}else if (_tempRena.Width==1280 && _tempRena.Height == 960){
						Console.Out.WriteLine("Detected PS3 Higurashi (Safe)");
						conversionType = type_ps3;
					}else if (_tempRena.Width==720 && _tempRena.Height==540){
						conversionType = type_ps3;
					}else{
						//Console.Out.WriteLine("Detected PS3 Higurashi (Unsure)");
						conversionType=type_undefined;
					}
					_tempRena.Dispose();
				}
			}else{
				Console.Out.WriteLine("=== CONVERSION TYPE OVERRIDE ====");
			}
			
			if (conversionType==type_undefined){
				Console.Out.WriteLine("I'm not sure if you have the PS3 patch or not. Are you using the PS3 Voices & Graphics patch by 07th Modding? (y/n)");
				string answer = Console.ReadLine();
				do{
					if (answer=="yes" || answer=="y"){
						conversionType = type_ps3;
						Console.Out.WriteLine("Set to PS3 patch Higurashi.");
						answer="temp";
					}else if (answer=="no" || answer=="n"){
						conversionType = type_steam;
						Console.Out.WriteLine("Set to normal Higurashi");
						answer="temp";
					}else{
						Console.Out.WriteLine("Invalid answer "+answer+" please enter y or n");
						Console.Out.WriteLine("I'm not sure if you have the PS3 patch or not. Are you using the PS3 Voices & Graphics patch by 07th Modding? (y/n)");
						answer = Console.ReadLine();
					}
				}while (answer!="temp");
			}
			
			//Console.ReadLine();
			//return;
			for (int i = 0; i < folderEntries.Length; i++) {
				//if (type == type_ps3) {
					if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "CG") {
						FixImages(folderEntries[i], false);
					}
				//} else if (type == type_updated) {
					if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "CGAlt") {
						FixImages(folderEntries[i], false);
					}
				//}

				if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "CompiledScripts") {
					Console.Out.WriteLine("(All) Directory CompiledScripts not needed.");
					Directory.Delete(folderEntries[i], true);
				}
				if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "CompiledUpdateScripts") {
					Console.Out.WriteLine("(All) Directory CompiledUpdateScripts not needed.");
					Directory.Delete(folderEntries[i], true);
				}
				if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "temp") {
					Console.Out.WriteLine("(All) Directory temp not needed.");
					Directory.Delete(folderEntries[i], true);
				}
				if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "Update") {
					Console.Out.WriteLine("(All) Directory Update no longer needed.");
					Directory.Delete(folderEntries[i], true);
				}
			}
			Console.Out.WriteLine("Dating conversion");
			using (StreamWriter sw = new StreamWriter(StreamingAssetsNoEndSlash+"/date.xxm0ronslayerxx")){
				sw.WriteLine("Converted.");
				sw.WriteLine(MainClass.converterVersionString);
				sw.WriteLine(MainClass.converterVersionNumber);
				sw.WriteLine("//MyLegGuyisanoob");
				sw.WriteLine(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt"));
            }
			Console.Out.WriteLine("====== DELETE USELESS STUFF ======");
			DeleteIfExist(StreamingAssetsNoEndSlash+"/assetsreadme.txt");
			DeleteIfExist(StreamingAssetsNoEndSlash+"/update.txt");

		}

		void FixScripts(string folderpath) {
			//string[] fileEntries = Directory.GetFiles(folderpath);
			string[] fileEntries = Directory.GetFiles(folderpath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < fileEntries.Length; i++) {
				FixSpecificScript(fileEntries[i]);
			}
		}

		string AddLastArg(string tofix) {
			if (tofix.Substring(tofix.Length-4,4)==", );"){
				tofix = tofix.Substring(0, tofix.Length - 2);
				tofix += "0 );";
				Console.WriteLine("Fixed empty arg");
			}
			return tofix;
		}

		void FixSpecificScript(string filename) {
			Console.Out.WriteLine("(All) Script: {0}",filename);
			//string[] lines;
			string[] lines = File.ReadAllLines(filename);
			string line;
			bool marked = false;
			for (int i = 0; i < lines.Length; i++) {
				
				line = lines[i].TrimStart((char)09);
				if (line.Length == 0 && marked == false) {
					line = "//MyLegGuyisanoob";
					marked = true;
					lines[i] = line;
					Console.Out.WriteLine("Marked script as done.");
					continue;
				}
				if (line.Length >= 4){
                    line = AddLastArg(line);
					if (line.Substring(0,4)=="void"){
						line = "function" + line.Substring(4,line.Length-4);
					}
					// Convert char arrays to regular array
					if (line.Substring(0, 4) == "char") {
						if (line.IndexOf('[') != -1) {
							Console.Out.WriteLine("Fix char array "+line.Substring(5, line.IndexOf('[') - 5));
							line = line.Substring(5, line.IndexOf('[') - 5) + " = {}";
						}

					}
				}
				if (line.Length >= 11) {
					if (line.Substring(0, 11) == "ShakeScreen" || line.Substring(0,11)=="ChangeScene") {
						line = AddLastArg(line);
					}
				}
				if (line.Length >=17){
					if (line.Substring(0, 17) == "SetSpeedOfMessage") {
						line = AddLastArg(line);
					}
					if (line.Substring(0, 17) == "//MyLegGuyisanoob"){
						Console.Out.WriteLine("(Already done)");
						return;
					}
				}
				if (line.Length >= 13) {
					if ((line.Substring(0, 13) == "SetGlobalFlag" || line.IndexOf("GetGlobalFlag")!=-1) || (line.Length>=22 && line.IndexOf("LoadValueFromLocalWork")!=-1)) {
						int start = 3;
						if (line.Substring(0, 13) == "SetGlobalFlag") {
							start = 0;
						} else if (line.IndexOf("GetGlobalFlag") != -1) {
							start = line.IndexOf("GetGlobalFlag");
						} else if (line.IndexOf("LoadValueFromLocalWork") != -1) {
							start = line.IndexOf("LoadValueFromLocalWork");
						}
						line = line.Replace(" ", "");
						int startleft = line.IndexOf('(');
						if (startleft < start) {
							startleft = line.IndexOf('(', startleft+1);
						}
						int firstargend = line.IndexOf(',');
						if (firstargend == -1) {
							firstargend = line.IndexOf(')', startleft + 1);
						}
						line = line.Substring(0, startleft+1) + '"' + line.Substring(startleft+1, firstargend-startleft-1) + '"' + line.Substring(firstargend, line.Length - firstargend);
					}
				}
				if (line.Length>=1){
					// Single char tests
					if (line.Substring(0,1) == "{") {
						if (lines[i-1].Length>=2 && lines[i-1].Substring(0,2)=="if"){
							line="then";
						}else{
							line="";
						}
						Console.WriteLine("Fixed left bracket");
					}else  if (line.Substring(0,1)=="}"){


						if ((i + 1 < lines.Length) && lines[i + 1].Length>=4) {
							lines[i + 1] = lines[i + 1].TrimStart((char)09);
							if (lines[i + 1].Substring(0, 4) == "else") {
								line = "";
								Console.WriteLine("Fixed right bracket (ELSE)");
							} else {
								line = "end";
								Console.WriteLine("Fixed right bracket (END)");
							}
						} else {
							line = "end";
							Console.WriteLine("Fixed right bracket (END)");
						}

					}
				}

				if (line.IndexOf('[') != -1 && line.IndexOf(']') != -1) {
					if (line.Length >= 3) {
						int result;
						if (int.TryParse(line.Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1), out result) == true) {
							//line = result + "noob";
							line = line.Substring(0, line.IndexOf('[')+1) + (result + 1) + line.Substring(line.IndexOf(']'),line.Length-line.IndexOf(']'));
							Console.Out.WriteLine("Fixed array subscript.");
						} else {
							Console.Out.WriteLine("Array subscript int conversion failed. "+line.Substring(line.IndexOf('[')+1, line.IndexOf(']')-line.IndexOf('[')-1));
						}

					}
				}
				

				lines[i] = line;
			}

			File.WriteAllLines(filename, lines);

			Console.Out.WriteLine("(Done)");
		}

		public static void FixImages(string folderpath, bool resaveanyway) {
			resaveanyway = true;

			//string[] fileEntries = Directory.GetFiles(folderpath);
			string[] fileEntries = Directory.GetFiles(folderpath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < fileEntries.Length; i++) {
				bool doneSomething=false;
				
				if (Path.GetExtension(fileEntries[i])!=".png"){
					Console.Out.WriteLine("(Ignored) Not PNG: {0}",fileEntries[i]);
					continue;
				}
				
				using (Bitmap currentFile = new Bitmap(fileEntries[i])) {
					// image processing
					if (currentFile != null) {
						//if (type == type_ps3) {
						// Is a background
						if ((currentFile.Width == 1280 && currentFile.Height == 720) || (currentFile.Width == 1920 && currentFile.Height == 1080)) {
							Console.Out.WriteLine("(PS3) Background: {0}", fileEntries[i]);
							Bitmap happy = new Bitmap(currentFile, new Size(960, 540));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						}else if (currentFile.Width == 1280 && currentFile.Height == 960) {
							Bitmap happy=null;
							if (conversionType==type_steam){
								Console.Out.WriteLine("(Steam) Bust: {0}", fileEntries[i]);
								happy = new Bitmap(currentFile, new Size(640, 480));
							}else if (conversionType==type_ps3){
								Console.Out.WriteLine("(PS3) Bust: {0}", fileEntries[i]);
								happy = new Bitmap(currentFile, new Size(720, 540));
							}
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						} else if (currentFile.Width == 1024 && currentFile.Height == 768) {
							Console.Out.WriteLine("(Wierd) Thingie: {0}", fileEntries[i]);
							Bitmap happy = new Bitmap(currentFile, new Size(640, 480));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						} else if (currentFile.Width==1920){
							Console.Out.WriteLine("(Unknown 1920) ???: {0}", fileEntries[i]);
							//Bitmap happy = new Bitmap(currentFile, new Size(960, (int)Math.Floor((double)currentFile.Height/2)));
							// We're actually going to save this as a 640x480 because we don't know if this is a sprite or not
							Bitmap happy = new Bitmap(currentFile, new Size(640, (int)Math.Floor((double)currentFile.Height/3)));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						} else if (currentFile.Width==1024){
							Console.Out.WriteLine("(Unknown 1024) ???: {0}", fileEntries[i]);
							Bitmap happy = new Bitmap(currentFile, new Size(640, (int)Math.Floor((double)currentFile.Height/1.6)));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						} else if (resaveanyway == true) {
							// Some images don't fade correctly for some reason. I don't know why, but a resave fixes it.
							Console.Out.WriteLine("(No Reason) Resave: {0}", fileEntries[i]);
							Bitmap happy = new Bitmap(currentFile, new Size(currentFile.Width, currentFile.Height));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						}
			

					}
					if (doneSomething==false){
						Console.Out.WriteLine("(No Need) Ignored: {0}", fileEntries[i]);
					}
				};
			}
		}

	}
}
