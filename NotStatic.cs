using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace HigurashiVitaCovnerter {
	public class NotStatic {
		const int type_undefined = 0;
		const int type_ps3 = 1;
		const int type_updated = 2;
		const int type_old = 3;
		int type = type_ps3;

		void FixScriptFolders(string StreamingAssetsNoEndSlash){
			if (Directory.Exists(StreamingAssetsNoEndSlash+"\\Update\\")==true){
				Console.Out.WriteLine("Transfer Update to Scripts");
				foreach(string file in Directory.GetFiles(StreamingAssetsNoEndSlash+"\\Update\\")){
					File.Copy(file, Path.Combine(StreamingAssetsNoEndSlash+"\\Scripts\\", Path.GetFileName(file)),true);
				}
			}else{
				Console.Out.WriteLine("...? There's no Update folder...");
			}
		}
		
		public NotStatic(string StreamingAssetsNoEndSlash) {
			string[] folderEntries = Directory.GetDirectories(StreamingAssetsNoEndSlash);

			FixScriptFolders(StreamingAssetsNoEndSlash);
				
			//return;
			Console.Out.WriteLine("========= SCRIPTS START ==========");
			FixScripts(StreamingAssetsNoEndSlash+"\\Scripts\\");
			Console.Out.WriteLine("========= SCRIPTS DONE ==========");
			Console.Out.WriteLine("========= IMAGES, START =========");
			Console.Out.WriteLine("This may take a while, please wait warmly.");
			//Console.ReadLine();
			return;
			for (int i = 0; i < folderEntries.Length; i++) {
				//if (type == type_ps3) {
					if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "CG") {
						FixImages(folderEntries[i]);
					}
				//} else if (type == type_updated) {
					if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "CGAlt") {
						FixImages(folderEntries[i]);
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
				Console.WriteLine("Fixed funkycall");
			}
			return tofix;
		}

		void FixSpecificScript(string filename) {
			Console.Out.WriteLine("(All) Script: {0}",filename);
			//string[] lines;
			string[] lines = File.ReadAllLines(filename);
			string line;
			for (int i = 0; i < lines.Length; i++) {
				line = lines[i].TrimStart((char)09);

				if (line.Length >= 4){
					if (line.Substring(0,4)=="void"){
						line = "function" + line.Substring(4,line.Length-4);
					}
				}
				if (line.Length >= 11) {
					if (line.Substring(0, 11) == "ShakeScreen") {
						line = AddLastArg(line);
					} else if (line.Substring(0, 11) == "void main()") {
						line = "//MyLegGuyisanoob";
						Console.WriteLine("Fixed void main()");
						lines[i] = line;
						// Do continue as to not check for the mylegnoob line
						continue;
					}
				}
				if (line.Length >=17){
					if (line.Substring(0, 17) == "SetSpeedOfMessage") {
						line = AddLastArg(line);
					}
					if (line.Substring(0, 17) == "//MyLegGuyisanoob"){
						Console.Out.WriteLine("(Already done)", filename);
						return;
					}
				} else {
					// Single char tests
					if (line == "{") {
						if (lines[i-1].Substring(0,2)=="if"){
							line="then";
						}else{
							line="";
						}
						Console.WriteLine("Fixed left bracket");
					}
					if (line=="}"){
						line="end";
						Console.WriteLine("Fixed right bracket");
					}
				}

				lines[i] = line;
			}

			File.WriteAllLines(filename, lines);

			Console.Out.WriteLine("(Done)");
		}

		void FixImages(string folderpath) {
			
			//string[] fileEntries = Directory.GetFiles(folderpath);
			string[] fileEntries = Directory.GetFiles(folderpath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < fileEntries.Length; i++) {
				bool doneSomething=false;
				using (Bitmap currentFile = new Bitmap(fileEntries[i])) {
					// image processing
					if (currentFile != null) {
						//if (type == type_ps3) {
							// Is a background
							if ((currentFile.Width == 1280 && currentFile.Height == 720) || (currentFile.Width==1920 && currentFile.Height == 1080)) {
								Console.Out.WriteLine("(PS3) Background: {0}", fileEntries[i]);
								Bitmap happy = new Bitmap(currentFile, new Size(960, 540));
								currentFile.Dispose();
								happy.Save(fileEntries[i]);
								happy.Dispose();
								doneSomething=true;
							} else if ((currentFile.Width == 960 && currentFile.Height == 720) || (currentFile.Width==1280 && currentFile.Height == 960)) { // Is a ps3 bust
								Console.Out.WriteLine("(PS3) Bust: {0}", fileEntries[i]);
								Bitmap happy = new Bitmap(currentFile, new Size(725, 544));
								currentFile.Dispose();
								happy.Save(fileEntries[i]);
								happy.Dispose();
								doneSomething=true;
							}
						//} else if (type == type_updated) {
							if (currentFile.Width == 1280 && currentFile.Height == 960) {
								Console.Out.WriteLine("(Steam) Bust: {0}", fileEntries[i]);
								Bitmap happy = new Bitmap(currentFile, new Size(640, 480));
								currentFile.Dispose();
								happy.Save(fileEntries[i]);
								happy.Dispose();
								doneSomething=true;
							}
						//}

					}
					if (doneSomething==false){
						Console.Out.WriteLine("(Already Converted) Ignored: {0}", fileEntries[i]);
					}
				};
			}
		}

	}
}
