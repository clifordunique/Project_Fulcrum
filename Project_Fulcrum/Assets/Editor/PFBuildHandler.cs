// C# example.
using UnityEditor;
using System.Diagnostics;

public class ScriptBatch 
{
	[MenuItem("MyTools/Windows Build With Uncompressed Assetbundles")]
	public static void BuildGame ()
	{
		// Get filename.
		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
		string[] levels = new string[] {"Assets/Scenes/MainMenu.unity", "Assets/Scenes/MultiplayerTest.unity"};

		// Build player.
		BuildPipeline.BuildAssetBundles(  path + "/AssetBundles/", BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
		BuildPipeline.BuildPlayer(levels, path + "/BuiltGame.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

		// Copy a file from the project folder to the build folder, alongside the built game.
		//FileUtil.CopyFileOrDirectory("Assets/Templates/Readme.txt", path + "Readme.txt");

		// Run the game (Process class from System.Diagnostics).
		Process proc = new Process();
		proc.StartInfo.FileName = path + "/BuiltGame.exe";
		proc.Start();
	}
}
