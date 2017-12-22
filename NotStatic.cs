using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.IO.Compression;
using System.Threading;

namespace HigurashiVitaCovnerter {
	public class NotStatic {
		// .... This isn't really a big program. It's okay if I make everything public and static, right?
		
		public const int type_undefined = 0;
		public const int type_ps3 = 1;
		public const int type_steam = 3;
		public static int conversionType = type_undefined;
		
		public static int normalBustBackgroundWidth;
		public static int normalBustBackgroundHeight;
		
		public static int ps3BustWidth;
		public static int ps3BustHeight;
		public static int ps3BackgroundWidth;
		public static int ps3BackgroundHeight;
		
		public static double ratioNormalBustBackground;
		public static double ratioPs3Background;
		public static double ratioPs3Bust;
		
		public static int screenWidth=0;
		public static int screenHeight=0;
		
		public static bool isADVMode=false;
		
		const int PLATFORMCHOICE_VITA = 0;
		const int PLATFORMCHOICE_3DS = 1;
		const int PLATFORMCHOICE_ANDROID = 2;
		
		// Dividing by a smaller number gives a bigger result than dividing by a bigger a number
		// Whichever one needs to stretch less is the one we stretch to
		static bool FitToWidth(int _imgWidth, int _imgHeight){
			double _tempWidthResult = (double)_imgWidth/screenWidth;
			double _tempHeightResult = (double)_imgHeight/screenHeight;
			if (_tempWidthResult>_tempHeightResult){
				return true;
			}
			return false;
		}
		
		static double ImageToScreenRatio(int _imgSize, int _screenSize){
			return _imgSize/(double)_screenSize;
		}
		
		static int SizeScaledOutput(int _original, double _scaleFactor){
			return (int)Math.Floor(_original/(double)_scaleFactor);
		}
		
		string GetProbablePresetFilename(string _scriptsFolderWithSlash, string _presetsFolderWithSlash){
			string[] _presetFolderEntries = Directory.GetFiles(_presetsFolderWithSlash, "*", SearchOption.AllDirectories);
			string[] _scriptFolderEntries = Directory.GetFiles(_scriptsFolderWithSlash, "*", SearchOption.AllDirectories);
			string[] _presetFirstFilenames = new string[_presetFolderEntries.Length];
			int i;
			// Get the first script filename from each preset file.
			for (i=0;i<_presetFolderEntries.Length;i++){
				StreamReader _myStreamReader = new StreamReader(new FileStream(_presetFolderEntries[i],FileMode.Open));
				if (int.Parse(_myStreamReader.ReadLine())==0){// First line is the number of script files, we don't need it as long as it's not 0.
					continue;
				}
				_presetFirstFilenames[i] = _myStreamReader.ReadLine();
				_myStreamReader.Dispose();
			}
			for (i=0;i<_scriptFolderEntries.Length;i++){
				for (int j=0;j<_presetFolderEntries.Length;j++){
					if (Path.GetFileNameWithoutExtension(_scriptFolderEntries[i])==_presetFirstFilenames[j]){
						if (i!=_scriptFolderEntries.Length){ // If we actually found something.
							Console.Out.WriteLine("Detected that this is "+_presetFolderEntries[j]);
							return _presetFolderEntries[j];
						}
					}
				}
			}
			Console.Out.WriteLine("Could not find proper preset.");
			return null;
		}
		
		public string platformChoiceToString(int _userChoice){
			if (_userChoice == PLATFORMCHOICE_VITA){
				return "VITA";
			}else if (_userChoice == PLATFORMCHOICE_3DS){
				return "3DS";
			}else if (_userChoice == PLATFORMCHOICE_ANDROID){
				return "ANDROID";
			}
			return "UNKNOWN";
		}
		
		public NotStatic(string StreamingAssetsNoEndSlash) {
			string[] folderEntries = Directory.GetDirectories(StreamingAssetsNoEndSlash);
			int _tempAnswer=-1;
			int _userDeviceTarget;
			while (_tempAnswer<0 || _tempAnswer>2 ){
				if (_tempAnswer>2){
					DrawDivider();
					Console.Out.WriteLine("That number is too high. Please enter the number in parentheses before the system name.");
				}
				DrawDivider();
				Console.Out.WriteLine("What device are you converting files for?");
				Console.Out.WriteLine("("+ PLATFORMCHOICE_VITA +") PS Vita");
				Console.Out.WriteLine("("+ PLATFORMCHOICE_3DS +") 3ds");
				Console.Out.WriteLine("("+ PLATFORMCHOICE_ANDROID +") Android Device");
				Console.Out.Write("Answer: ");
				_tempAnswer = InputNumber();
			}
			_userDeviceTarget=_tempAnswer;
			// Set the resolution for the user if they're on the Vita.
			if (_tempAnswer==PLATFORMCHOICE_VITA){
				screenWidth=960;
				screenHeight=544;
			}
			if (_tempAnswer==PLATFORMCHOICE_3DS){
				screenWidth=400;
				screenHeight=240;
			}
			
			// Ask the user for their screen resolution if it's not already set
			if (screenWidth==0 || screenHeight ==0){
				_tempAnswer=-1;
				do{
					Console.Out.WriteLine("Input the WIDTH of your device's screen in pixels");
					_tempAnswer = InputNumber();
				}while(_tempAnswer<=0);
				screenWidth = _tempAnswer;
				_tempAnswer=-1;
				do{
					Console.Out.WriteLine("Input the HEIGHT of your device's screen in pixels");
					_tempAnswer = InputNumber();
				}while(_tempAnswer<=0);
				screenHeight = _tempAnswer;
			}
			
			if (screenHeight>screenWidth){
				Console.Out.WriteLine("I meant the resolution in LANDSCAPE mode. Whatever, I'll swap the values for you.");
				int _tempInt;
				_tempInt = screenWidth;
				screenWidth = screenHeight;
				screenHeight = _tempInt;
			}
			
			Console.Out.WriteLine("Screen resolution {0}x{1} pixels.",screenWidth,screenHeight);
			
			// This block of code sets the value we multiply image sizes by depending on the image type
			// If we're going to fit to width for normal backgrounds, busts, and updated busts
			// The goal is to fit to the screen's width or height while keeping aspect ratio and not cutting anything off
			if (FitToWidth(640,480)==true){
				ratioNormalBustBackground = ImageToScreenRatio(640,screenWidth);
			}else{
				ratioNormalBustBackground = ImageToScreenRatio(480,screenHeight);
			}
			// If we're going to fit to width for ps3 backgrounds
			if (FitToWidth(1920,1080)==true){
				ratioPs3Background = ImageToScreenRatio(1920,screenWidth);
			}else{
				ratioPs3Background = ImageToScreenRatio(1080,screenHeight);
			}
			// We always need the ps3 busts to be as tall as the backgrounds
			
			ps3BackgroundWidth = SizeScaledOutput(1920,ratioPs3Background);
			ps3BackgroundHeight = SizeScaledOutput(1080,ratioPs3Background);
			
			ratioPs3Bust = ImageToScreenRatio(960,ps3BackgroundHeight);
			
			normalBustBackgroundWidth = SizeScaledOutput(640,ratioNormalBustBackground);
			normalBustBackgroundHeight = SizeScaledOutput(480,ratioNormalBustBackground);
			
			ps3BustWidth = SizeScaledOutput(1280,ratioPs3Bust);
			ps3BustHeight = SizeScaledOutput(960,ratioPs3Bust);

			Console.Out.WriteLine("Old background & normal busts: {0}x{1}",normalBustBackgroundWidth,normalBustBackgroundHeight);
			Console.Out.WriteLine("PS3 background: {0}x{1}",ps3BackgroundWidth,ps3BackgroundHeight);
			Console.Out.WriteLine("PS3 busts: {0}x{1}",ps3BustWidth,ps3BustHeight);

			// Detect if PS3 patch or not
			if (conversionType==type_undefined){
				if (File.Exists(StreamingAssetsNoEndSlash+"/CG/re_se_de_a1.png")==true){
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
					Console.Out.WriteLine("Can't find sample image.");
				}
			}else{
				Console.Out.WriteLine("=== CONVERSION TYPE OVERRIDE ====");
			}
			
			if (conversionType==type_undefined){
				int answer = -1;
				while (answer==-1){
					DrawDivider();
					Console.Out.WriteLine("I'm not sure if you have the PS3 patch or not. Are you using the PS3 Voices & Graphics patch by 07th Modding? (y/n)");
					answer = YesOrNo();
				}
				
				if (answer==1){
					conversionType = type_ps3;
					Console.Out.WriteLine("Set to PS3 patch Higurashi.");
				}else if (answer==0){
					conversionType = type_steam;
					Console.Out.WriteLine("Set to normal Higurashi");
				}else{
					throw(new Exception("Invalid answer variable. Needs to be 1 or 0. It is "+answer));
				}
				
			}
			
			if (conversionType == type_ps3){
				if (Options.promptADVMode){
					int answer = -1;
					while (answer==-1){
						DrawDivider();
						Console.Out.WriteLine("ADV mode presents text like a regular visual novel would, in a textbox at the bottom of the screen. The disadvantage is that you can see less text at once.");
						Console.Out.WriteLine("Would you like to enable ADV mode? (y/n)");
						answer = YesOrNo();
					}
					if (answer==1){
						isADVMode=true;
					}
				}
				if (File.Exists("./GameSpecificAdvBoxDEFAULT.png") && File.Exists("./GameSpecificAdvBox3DS.png")){
					Console.Out.WriteLine("Copy ADV box.");
					File.Copy("./GameSpecificAdvBoxDEFAULT.png",(StreamingAssetsNoEndSlash+"/GameSpecificAdvBoxDEFAULT.png"),true);
					File.Copy("./GameSpecificAdvBox3DS.png",(StreamingAssetsNoEndSlash+"/GameSpecificAdvBox3DS.png"),true);
				}
				if (Options.downloadLatestScripts==true){
					if (conversionType==type_ps3){
						DownloadUpdateScripts(StreamingAssetsNoEndSlash);
					}
				}
			}
			
			
			if (isADVMode==true){
				FileStream fp = File.Open(StreamingAssetsNoEndSlash+"/Scripts/_GameSpecific.lua",FileMode.Create);
				StreamWriter sw =new StreamWriter(fp);
				sw.WriteLine("OptionsSetTextMode(TEXTMODE_AVD);");
				sw.WriteLine("OptionsLoadADVBox();");
				sw.Close();
				sw.Dispose();
			}
			
			// Copies update to scripts
			FixScriptFolders(StreamingAssetsNoEndSlash);
			
			string probablePresetFilename = GetProbablePresetFilename(StreamingAssetsNoEndSlash+"/Scripts/",Options.includedPresetsFolderName);
			if (probablePresetFilename==null && _userDeviceTarget==PLATFORMCHOICE_3DS){
				DrawDivider();
				Console.Out.WriteLine("The \"preset file\" for this StreamingAssets folder was not found. You'll need to use manual script selection if you don't fix this problem. Make sure your StreamingAssets/Scripts directorty has scripts. If you can't fix the problem, ask for help.");
				Console.Out.WriteLine("=== Press any key to continue ===");
				Console.ReadKey();
				DrawDivider();
			}else{ // We know which game this is.
				// Put the correct preset in the StreamingAssets folder.
				File.Copy(probablePresetFilename,StreamingAssetsNoEndSlash+"/"+Path.GetFileName(probablePresetFilename),true);
				StreamWriter _embeddedPresetFilenameFile = new StreamWriter(new FileStream(StreamingAssetsNoEndSlash+"/includedPreset.txt",FileMode.Create));
				_embeddedPresetFilenameFile.WriteLine(Path.GetFileName(probablePresetFilename));
				_embeddedPresetFilenameFile.Dispose();
				// Apply the patch, if it exists.
				if (Directory.Exists(Path.Combine(Options.includedPatchesFolderName,Path.GetFileName(probablePresetFilename)+(conversionType == type_ps3 ? "PS3" : "NORMAL")+platformChoiceToString(_userDeviceTarget)))){
					CopyDirToDir(Path.Combine(Options.includedPatchesFolderName,Path.GetFileName(probablePresetFilename)+(conversionType == type_ps3 ? "PS3" : "NORMAL")+platformChoiceToString(_userDeviceTarget)),StreamingAssetsNoEndSlash);
				}else if (Directory.Exists(Path.Combine(Options.includedPatchesFolderName,Path.GetFileName(probablePresetFilename)+(conversionType == type_ps3 ? "PS3" : "NORMAL")+"ALL"))){
					CopyDirToDir(Path.Combine(Options.includedPatchesFolderName,Path.GetFileName(probablePresetFilename)+(conversionType == type_ps3 ? "PS3" : "NORMAL")+"ALL"),StreamingAssetsNoEndSlash);
				}
			}
			
			Console.Out.WriteLine("========= SCRIPTS START ==========");
			FixScripts(StreamingAssetsNoEndSlash+"/Scripts/");
			
			Console.Out.WriteLine("========= SCRIPTS DONE ==========");
			Console.Out.WriteLine("========= PRESETS START =========");
			if (Directory.Exists(Options.includedPresetsFolderName) == true) {
				// Only copy presets if it's PS Vita
				if (_userDeviceTarget==0){
					Directory.CreateDirectory(StreamingAssetsNoEndSlash + "/Presets");
					CopyPresets(StreamingAssetsNoEndSlash);
				}
			} else {
				Console.WriteLine("!!!!!!!!!! WARNINING !!!!!!!!!!!!!");
				Console.Out.WriteLine("The folder \"Presets\" was not found in the same directory as this exe.\nIf you have misplaced the folder, please redownload the program.\nIf you ignore this warning, your StreamingAssets folder will have no presets in it by default.\nYou'll need to put them all in yourself.");
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
				if (!Directory.Exists(StreamingAssetsNoEndSlash+"/SE")){
					Directory.CreateDirectory(StreamingAssetsNoEndSlash+"/SE");
				}
				File.Copy("./wa_038.ogg",StreamingAssetsNoEndSlash+"/SE/wa_038.ogg");
			}else{
				Console.Out.WriteLine("Oh, the menu sound effect isn't here. Oh well.");
			}
			
			/*
			if (File.Exists("./happy.lua")==true){
				//File.Copy("./happy.lua",StreamingAssetsNoEndSlash+"/happybackup.lua",true);
				File.Copy("./happy.lua",StreamingAssetsNoEndSlash+"/happy.lua",true);
			}else{
				DrawDivider();
				DrawDivider();
				Console.Out.WriteLine("Oh, the happy.lua file isn't here. Oh well. YOU BETTER REMEMBER TO PUT IT THERE YOURSELF!");
				Console.Out.WriteLine("THERE SHOULD BE A FILE CALLED happy.lua IN THE SAME DIRECTORY AS THIS EXE FILE. IF IT IS MISSING, REDOWNLOAD THE CONVERTER PROGRAM. IF IT'S STILL GONE, REPORT TO MYLEGGUY!");
				DrawDivider();
				DrawDivider();
			}*/
			
			Console.Out.WriteLine("========= IMAGES, START =========");
			Console.Out.WriteLine("This may take a while, please wait.");
			
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
					DeleteDirectoryRetry(folderEntries[i], true);
				}
				if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "CompiledUpdateScripts") {
					Console.Out.WriteLine("(All) Directory CompiledUpdateScripts not needed.");
					DeleteDirectoryRetry(folderEntries[i], true);
				}
				if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "temp") {
					Console.Out.WriteLine("(All) Directory temp not needed.");
					DeleteDirectoryRetry(folderEntries[i], true);
				}
				if (Path.GetFileNameWithoutExtension(folderEntries[i]) == "Update") {
					Console.Out.WriteLine("(All) Directory Update no longer needed.");
					DeleteDirectoryRetry(folderEntries[i], true);
				}
			}
			Console.Out.WriteLine("Dating conversion");
			DeleteIfExist(StreamingAssetsNoEndSlash+"/date.xxm0ronslayerxx");
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
			if (probablePresetFilename!=null && _userDeviceTarget == PLATFORMCHOICE_3DS){
				bool _actuallyDidRename=false;
				Console.Out.WriteLine("====== Rename folder ======");
				for (int i=0;i!=20;i++){
					try{
						Directory.Move(StreamingAssetsNoEndSlash,StreamingAssetsNoEndSlash+"_"+Path.GetFileName(probablePresetFilename));
						i=19;
						_actuallyDidRename=true;
					}catch(Exception e){
						Console.Out.WriteLine("Failed to rename StreamingAssets directory, retrying "+i+"/20");
						Thread.Sleep(500);
					}
				}
				if (_actuallyDidRename==false){
					DrawDivider();
					DrawDivider();
					DrawDivider();
					Console.Out.WriteLine("Okay, so I failed to rename the StreamingAssets folder. You're probably using it, or something. What you need to do is....\n");
					
					Console.Out.WriteLine("rename StreamingAssets to StreamingAssets_"+Path.GetFileName(probablePresetFilename));
					
					Console.Out.WriteLine("\nMake sure you do it.");
					Console.Out.WriteLine("=== Press enter twice to continue ===");
					Console.ReadLine();
					Console.ReadLine();
				}
				
			}
		}
		
		// Returns the number the user put in. -1 otherwise
		int InputNumber(){
			string answer = Console.ReadLine();
			int _tempResult;
			if (int.TryParse(answer, out _tempResult)==false){
				Console.Out.WriteLine(answer + " is not a number. Try again.");
				return -1;
			}
			return _tempResult;
		}
		
		public static void DeleteDirectoryRetry(string destinationDir, bool stuffinsidediryesorno) {
			const int maxRetry = 10;
			for (var retries = 1; retries < maxRetry; retries++) {
				try{
					Directory.Delete(destinationDir, stuffinsidediryesorno);
				}catch (DirectoryNotFoundException) {
					return;	// The directory was deleted.
				}catch (IOException){ // System.IO.IOException: The directory is not empty
					Console.Out.WriteLine("Failed to delete "+destinationDir+". Retrying in 500 ms. Try "+retries+"/"+maxRetry);
					Thread.Sleep(500);
					continue;
				}
				return;
			}
			// depending on your use case, consider throwing an exception here
		}
				
		// Returns 1 if user inputs "y" or "yes"
		// Returns 0 if user inputs "n" or "no"
		// Returns -1 otherwise
		int YesOrNo(){
			string answer = Console.ReadLine();
			
			if (answer=="yes" || answer=="y"){
				return 1;
			}else if (answer=="no" || answer=="n"){
				return 0;
			}else{
				Console.Out.WriteLine("Invalid answer "+answer+" please enter \"y\" or \"n\" without quotations.");
				return -1;
			}
		}
		
		public static void DrawDivider(){
			Console.Out.WriteLine("==================================");
		}
		
		void CopyDirToDir(string srcDirNoEndSlash, string destDirNoEndSlash){
			Console.Out.WriteLine("Copy "+srcDirNoEndSlash+"/ to "+destDirNoEndSlash+"/");
			
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(srcDirNoEndSlash, "*",  SearchOption.AllDirectories)){
				Directory.CreateDirectory(dirPath.Replace(srcDirNoEndSlash, destDirNoEndSlash));
			}
			
			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(srcDirNoEndSlash, "*.*",  SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(srcDirNoEndSlash, destDirNoEndSlash), true);
		}
		
		void DownloadUpdateScripts(string StreamingAssetsNoEndSlash){
			int answer = -1;
			while (answer==-1){
				DrawDivider();
				Console.Out.WriteLine("Because you are using the PS3 patches, you need to download the latest scripts from the Github repo.");
				Console.Out.WriteLine("This is requiered for Onikakushi (ch1) and Watanagashi (ch2) to run correctly.");
				Console.Out.WriteLine("It is optional for Tatarigoroshi (ch3) and Himatsubushi (ch4).");
				Console.Out.WriteLine("Would you like me to download them for you? (y/n)");
				answer = YesOrNo();
			}
			
			if (answer==1){
				Console.Out.WriteLine("Good answer!");
			}else if (answer==0){
				Console.Out.WriteLine("Okay. If you're converting chapters 1 or 2, you better have already done it yourself.");
				return;
			}else{
				throw(new Exception("Invalid answer when it should have to be 1 or 0. It is "+answer.ToString()));
			}
			
			// You can only get to this code if you choose yes
			Console.Out.WriteLine("Loading url database...");
			
			List<string> databaseNameList = new List<string>();
			List<string> databaseURLList = new List<string>();
			
			int counter = 0;
			string line;

			if (File.Exists("./RepoDatabase.txt")==false){
				Console.Out.WriteLine("./RepoDatabase.txt not found!");
			}
			
			// Read the file and display it line by line.
			StreamReader file = new StreamReader("./RepoDatabase.txt");
			while((line = file.ReadLine()) != null){
				if (counter%2==0){
					Console.WriteLine ("> Name: "+line);
					databaseNameList.Add(line);
				}else{
					Console.WriteLine ("> URL: "+line);
					databaseURLList.Add(line);
				}
				counter++;
			}
			file.Close();
			
			answer = -1;
			while (answer<=0 || answer>(databaseNameList.Count+1) ){
				if (answer>(databaseNameList.Count+1)){
					DrawDivider();
					Console.Out.WriteLine("That number is too high. Please enter the number in parentheses before the name.");
				}
				DrawDivider();
				Console.Out.WriteLine("Select the game you're converting.");
				for (int i=0;i<databaseNameList.Count;i++){
					Console.Out.WriteLine("("+(i+1)+") "+databaseNameList[i]);
				}
				Console.Out.WriteLine("("+(databaseNameList.Count+1)+") My game isn't listed.");
				answer = InputNumber();
			}
			
			if (answer==databaseNameList.Count+1){
				Console.Out.WriteLine("Don't worry about it then.");
				Console.ReadLine();
				return;
			}
			Console.Out.WriteLine("Good, you chose "+databaseNameList[answer-1]);
			Console.Out.WriteLine("Trying to download the Github repo at "+databaseURLList[answer-1]+" to ./githubRepo.zip");
			using (WebClient client = new WebClient()){
				try{
					client.DownloadFile(databaseURLList[answer-1], "./githubRepo.zip");
				}catch(Exception e){
					DrawDivider();
					Console.Out.WriteLine("Failed to download the file. An error occurred. Here's the error:");
					Console.Out.WriteLine(e.ToString());
					DrawDivider();
					Console.Out.WriteLine("Press enter to crash the program.");
					Console.ReadLine();
					throw(e);
				}
			}
			if (!File.Exists("./githubRepo.zip")){
				DrawDivider();
				Console.Out.WriteLine("There were no errors, but the downloaded file just isn't there.");
				DrawDivider();
				Console.Out.WriteLine("Press enter to crash the program.");
				Console.ReadLine();
				throw(new Exception("The file just isn't there for some reason."));
			}
			DrawDivider();
			Console.Out.WriteLine("Download complete. Extracting zip file...");
			if (Directory.Exists("./githubRepoExtracted")){
				Console.Out.WriteLine("Removing ./githubRepoExtracted");
				DeleteDirectoryRetry("./githubRepoExtracted",true);
			}
			try{
				ZipFile.ExtractToDirectory("./githubRepo.zip","./githubRepoExtracted");
			}catch(Exception e){
				DrawDivider();
				DrawDivider();
				DrawDivider();
				Console.Out.WriteLine("There was a problem extracting ./githubRepo.zip. Here's the error:");
				Console.Out.WriteLine(e.ToString());
				DrawDivider();
				if (MainClass.IsRunningOnMono()==true){
					Console.Out.WriteLine("!!!!!!!!!!!!!!!!!!!");
					Console.Out.WriteLine("Make sure Mono is updated!");
					Console.Out.WriteLine("!!!!!!!!!!!!!!!!!!!");
				}
				Console.Out.WriteLine("You can extract the file yourself if you want. Press enter to crash the program or continue if you've extracted it yourself.");
				Console.Out.WriteLine("If you want to extract the zip file yourself, extract ./githubRepo.zip to ./githubRepoExtracted. There should be a folder inside of ./githubRepoExtracted called chapterName-master");
				DrawDivider();
				Console.ReadLine();
				if (!Directory.Exists("./githubRepoExtracted")){
					throw(e);
				}
			}
			DrawDivider();
			Console.Out.WriteLine("Done extracting!");
			Console.Out.WriteLine("Looking inside...");
			string[] insideZipFolders = Directory.GetDirectories("./githubRepoExtracted");
			if (insideZipFolders.Length==0){
				Console.Out.WriteLine("For some reason, there are no directories in ./githubRepoExtracted!");
				Console.Out.WriteLine("There should be at least one fodler which contains a script folder.");
				Console.Out.WriteLine("Cannot continue. Press enter to crash!");
				Console.ReadLine();
				throw(new Exception("Nothing found in extracted zip directory."));
			}
			
			string zipRootNoSlash = insideZipFolders[0];
			DrawDivider();
			Console.Out.WriteLine("Found "+zipRootNoSlash);
			
			Console.Out.WriteLine("Copying the ZIP's stuff to "+StreamingAssetsNoEndSlash+"/");
			string[] insideZipFolderFolder = Directory.GetDirectories(zipRootNoSlash);
			for (int i=0;i<insideZipFolderFolder.Length;i++){
				Directory.CreateDirectory(StreamingAssetsNoEndSlash+"/"+Path.GetFileName(insideZipFolderFolder[i]));
				CopyDirToDir(insideZipFolderFolder[i], StreamingAssetsNoEndSlash+"/"+Path.GetFileName((insideZipFolderFolder[i])));
			}
			DrawDivider();
			Console.Out.WriteLine("Remove extracted ZIP...");
			DeleteDirectoryRetry("./githubRepoExtracted",true);
			
			File.Delete("./githubRepo.zip");
			
			for (int i=0;i<4;i++){
				DrawDivider();
			}
			
			Console.Out.WriteLine("Done download updated stuff!");
			
		}

		
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
			foreach(string file in Directory.GetFiles(Options.includedPresetsFolderName)){
				File.Copy(file, Path.Combine(StreamingAssetsNoEndSlash+"/Presets/", Path.GetFileName(file)),true);
			}
		}
		
		void DeleteIfExist(string filepath){
			if (File.Exists(filepath)==true){
				File.Delete(filepath);
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
				Console.WriteLine("Fixed empty arg");
			}
			return tofix;
		}
		
		short GetNumberOfTabsAtStart(string line){
			for (short i=0;i<line.Length;i++){
				if (line.Substring(i,1)!=(""+(char)09)){
					return i;
				}
			}
			return (short)line.Length;
		}
		
		string ChangeIfHere(string tomod, string evil_stuff_we_dont_want, string fresh_stuff_we_do_want){
			if (tomod.TrimStart((char)09)==evil_stuff_we_dont_want){
				return (char)09+fresh_stuff_we_do_want;
			}else{
				return tomod;
			}
		}

		void FixSpecificScript(string filename) {
			Console.Out.WriteLine("(All) Script: {0}",filename);
			//string[] lines;
			string[] lines = File.ReadAllLines(filename);
			short[] tabsOnLines = new short[lines.Length];
			string line;
			string lastLine="";
			bool marked = false;
			List<bool> explicitThens = new List<bool>();
			for (int i = 0; i < lines.Length; i++) {
				//Console.Out.WriteLine("Line " + i.ToString());
				tabsOnLines[i] = (short)GetNumberOfTabsAtStart(lines[i]);
				line = lines[i].TrimStart((char)09);
				
				if (isADVMode==false){
					// This WILL change in the future. 
					line = ChangeIfHere(line,"AdvMode = 1;", "AdvMode = 0;");
					line = ChangeIfHere(line,"InitAdvMode = 1;", "InitAdvMode = 0;");
				}
				
				if (marked == false && line.Length == 0 ) {
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
					}else if (line.Substring(0,3)=="int"){
						line = "local "+line.Substring(3);
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
				// Fix things that should be string args.
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
						Console.WriteLine("Fixed left bracket (Explicit)");
						explicitThens.Add(true);
					}else  if (line.Substring(0,1)=="}"){
						if (line.IndexOf("else")!=-1) {
						    	line = "else";
						}else{
							if ((i + 1 < lines.Length) && lines[i + 1].Length>=4) {
								lines[i + 1] = lines[i + 1].TrimStart((char)09);
								if (lines[i + 1].Substring(0, 4) == "else") {
									line = "";
									Console.WriteLine("Fixed right bracket (ELSE)");
								} else {
									explicitThens.RemoveAt(explicitThens.Count-1);
									line = "end";
									Console.WriteLine("Fixed right bracket (END)");
								}
							} else {
								explicitThens.RemoveAt(explicitThens.Count-1);
								line = "end";
								Console.WriteLine("Fixed right bracket (END)");
							}
						}

					}
				}
				
				// If statement fixing and array subscript fixing
				if (line.Length>=2){
					if (line.Substring(0,2)=="if"){
						int _leftskwigilybraket = 0;

						LeftSquigilyCheck:
						//Console.Out.WriteLine("We start the search at {0}",_leftskwigilybraket);
						_leftskwigilybraket = line.IndexOf("{",_leftskwigilybraket+1);
						if (_leftskwigilybraket!=-1){
							//Console.Out.WriteLine("Found left index at {0}",_leftskwigilybraket);
							int _elseIndex = line.IndexOf("else");
							if (( _elseIndex != -1 && _leftskwigilybraket<_elseIndex) || (_elseIndex==-1)){ // If there's an "else" don't draw if we're more to the left
								line = SingleStringReplacePosition(line,"{"," then",_leftskwigilybraket);
								explicitThens.Add(true);
								goto LeftSquigilyCheck;
							}else{ // if this after an "else", just remove it
								line = SingleStringReplacePosition(line,"{","",_leftskwigilybraket);
								goto LeftSquigilyCheck;
							}
						}
						
						_leftskwigilybraket=0;
						RightSquigilyCheck:
						
						_leftskwigilybraket = line.IndexOf("}",_leftskwigilybraket+1);
						if (_leftskwigilybraket!=-1){
							int _elseIndex = line.IndexOf("else");
							if (( _elseIndex != -1 && _leftskwigilybraket>_elseIndex) || (_elseIndex==-1)){ // If there's an "else" don't draw if we're more to the left
								line = SingleStringReplacePosition(line,"}"," end",_leftskwigilybraket+1);
								explicitThens.RemoveAt(explicitThens.Count-1);
								goto RightSquigilyCheck;
							}else{
								line = SingleStringReplacePosition(line,"}","",_leftskwigilybraket+1);
								goto RightSquigilyCheck;
							}
						}
						
						// then fix for single line if statements
						if (line.IndexOf("then")==-1){
							// If the next line is }, don't add "then"
							if ( !( (i!=lines.Length-1) && (lines[i+1].TrimStart((char)09).Length>=1) && (lines[i+1].TrimStart((char)09)).Substring(0,1)=="{") ){
								line = line + " then";
								explicitThens.Add(false);
							}
						}


						// In c and stuff
						// if (noob)
						// checks if noob is 1
						// in Lua, that will check if the variable noob isn't nil
						// we need to change this to if (noob==1)
						
						// Check for comparison operator in 'if' statement
						int additionalNeededRightBrackets=0;
						bool foundOperator=false;
						for (int j=line.IndexOf("(")+1;j<line.Length;j++){
							if (line.Substring(j,1)=="("){
								additionalNeededRightBrackets+=1;
							}else if (line.Substring(j,1)==")"){
								additionalNeededRightBrackets-=1;
								if (additionalNeededRightBrackets==-1){
									foundOperator=false;
									// end of 'if' statement
									break;
								}
							}else if (line.Substring(j,1)=="=" || line.Substring(j,1)==">" || line.Substring(j,1)=="<"){
								foundOperator=true;
								break;
							}
						}
						// If no operator, add ==1
						if (foundOperator==false){
							// Does not include the end one. 
							additionalNeededRightBrackets=0;
							int foundEndIfBracketPosition=0;
							for (int j=line.IndexOf("(")+1;j<line.Length;j++){
								if (line.Substring(j,1)=="("){
									additionalNeededRightBrackets+=1;
								}else if (line.Substring(j,1)==")"){
									additionalNeededRightBrackets-=1;
									if (additionalNeededRightBrackets==-1){
										foundEndIfBracketPosition=j;
										break;
									}
								}
							}
							line = line.Insert(foundEndIfBracketPosition,"==1");
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
				// Try to fix if statements that don't have any brackets
				if (i>=2){
					if (tabsOnLines[i]<tabsOnLines[i-1] && !(line.Length>=4 && line.Substring(0,4)=="else")){ // If this has less tabs than the last one and this isn't an "else" line
						if ((lines[i-2].Length>=2) && lines[i-2].Substring(0,2)=="if" && explicitThens[explicitThens.Count-1]==false){
							if (line!="end"){
								line = "end "+line;
							}
						}
					}
				}

				lines[i] = line;
				lastLine = lines[i];
			}

			File.WriteAllLines(filename, lines);

			Console.Out.WriteLine("(Done)");
		}

		string SingleStringReplacePosition(string original, string tofind,string newthing, int minoffset){
			int cpos = original.IndexOf(tofind);
			original = original.Substring(0,cpos)+newthing+original.Substring(cpos+1,original.Length-(cpos+1));
			return original;
		}
		
		
		public static Bitmap goodResizeImage(Bitmap _sourceImage, Size _newSize){
			Bitmap _resultBitmap = new Bitmap(_newSize.Width,_newSize.Height);
			using (Graphics goodGraphics = Graphics.FromImage(_resultBitmap)){
                goodGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                goodGraphics.DrawImage(_sourceImage,0,0,_newSize.Width,_newSize.Height);
			}
			return _resultBitmap;
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
						
						// FOR ANY IMAGE THAT WE'RE UNSURE ABOUT, MAKE IT 640xSOMETHING!
						
						if ((currentFile.Width == 1280 && currentFile.Height == 720) || (currentFile.Width == 1920 && currentFile.Height == 1080)) { // 720p or 1080p is PS3 background
							Console.Out.WriteLine("(PS3) Background: {0}", fileEntries[i]);
							Bitmap happy =  goodResizeImage(currentFile, new Size(ps3BackgroundWidth, ps3BackgroundHeight));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						}else if (currentFile.Width == 1280 && currentFile.Height == 960) { // 960p is a character bust
							Bitmap happy=null;
							if (conversionType==type_steam){
								Console.Out.WriteLine("(Steam) Bust: {0}", fileEntries[i]);
								happy =  goodResizeImage(currentFile, new Size(normalBustBackgroundWidth, normalBustBackgroundHeight));
							}else if (conversionType==type_ps3){
								Console.Out.WriteLine("(PS3) Bust: {0}", fileEntries[i]);
								happy =  goodResizeImage(currentFile, new Size(ps3BustWidth, ps3BustHeight));
							}
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						} else if (currentFile.Width == 1024 && currentFile.Height == 768) { // Idk what this is, make it 640x480 just to be safe
							Console.Out.WriteLine("(Wierd) Thingie: {0}", fileEntries[i]);
							Bitmap happy =  goodResizeImage(currentFile, new Size(normalBustBackgroundWidth, normalBustBackgroundHeight));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						} else if (currentFile.Width==1920){ // Unknown. 
							
							Console.Out.WriteLine("(Unknown 1920) ???: {0}", fileEntries[i]);
							//Bitmap happy = new Bitmap(currentFile, new Size(960, (int)Math.Floor((double)currentFile.Height/2)));
							// We're actually going to save this as a 640x480 because we don't know if this is a sprite or not
							Bitmap happy =  goodResizeImage(currentFile, new Size(640, (int)Math.Floor((double)currentFile.Height/3)));
							
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						} else if (currentFile.Width==1024){ // There is a sprite in Tatarigoroshi that scrolls up so it has a huge height. 
							Console.Out.WriteLine("(Unknown 1024) ???: {0}", fileEntries[i]);
							Bitmap happy =  goodResizeImage(currentFile, new Size(640, (int)Math.Floor((double)currentFile.Height/1.6)));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						}else if (currentFile.Width==640 && currentFile.Height==480){
							Console.Out.WriteLine("(Old) Background/Bust: {0}", fileEntries[i]);
							Bitmap happy =  goodResizeImage(currentFile, new Size(normalBustBackgroundWidth, normalBustBackgroundHeight));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						}else if (Options.isSecretFeature==true && currentFile.Width==800 && currentFile.Height==600){
							Console.Out.WriteLine("(Test Game) Image: {0}", fileEntries[i]);
							double _tempRatio;
							if (FitToWidth(currentFile.Width,currentFile.Height)==true){
								_tempRatio = ImageToScreenRatio(currentFile.Width,screenWidth);
							}else{
								_tempRatio = ImageToScreenRatio(currentFile.Height,screenHeight);
							}
							Bitmap happy =  goodResizeImage(currentFile, new Size(SizeScaledOutput(currentFile.Width,_tempRatio), SizeScaledOutput(currentFile.Height,_tempRatio)));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						}else if (resaveanyway == true) {
							// Some images don't fade correctly for some reason. I don't know why, but a resave fixes it.
							Console.Out.WriteLine("(No Reason) Resave: {0}", fileEntries[i]);
							Bitmap happy =  goodResizeImage(currentFile, new Size(currentFile.Width, currentFile.Height));
							currentFile.Dispose();
							happy.Save(fileEntries[i]);
							happy.Dispose();
							doneSomething = true;
						}
			

					}
					if (doneSomething==false){
						Console.Out.WriteLine("(No Need) Ignored: {0}", fileEntries[i]);
					}
				}
			}
		}

	}
}
