namespace MoPubInternal.Editor.Postbuild
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using MoPubInternal.Editor.ThirdParty.xcodeapi;

	public class PostBuildiOS
	{
		protected static readonly string moPubPluginsPath = "Plugins/iOS/MoPub";

		public static string NormalizePathForPlatform(string path)
		{
			return path.Replace ('/', Path.DirectorySeparatorChar);
		}

		public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string temppath = NormalizePathForPlatform (Path.Combine(destDirName, file.Name));
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs) {
				DirectoryInfo[] dirs = dir.GetDirectories();
				foreach (DirectoryInfo subdir in dirs) {
					string temppath = NormalizePathForPlatform (Path.Combine(destDirName, subdir.Name));
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		// Copy and add a framework (Link Phase) to a PBXProject
		//
		// PBXProject project: the project to modify
		// string target: the target project's GUID
		// string framework: the path to the framework to add
		// string projectPath: the path to add the framework in the project, relative to the project root
		private static void AddFrameworkToProject(PBXProject project, string target,
		                                                    string framework, string buildPath, string projectPath)
		{
			DirectoryCopy(framework, Path.Combine(buildPath, projectPath), true);

			string guid = project.AddFile (projectPath, projectPath);
			project.AddFileToBuild(target, guid);
		}
		
		protected static void AddFrameworksToProject(string[] frameworks, string buildPath, PBXProject project, string target) 
		{
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#else
			// Unity 5 and above should already take care of copying and linking non-platform frameworks
#endif
		}

		protected static void AddPlatformFrameworksToProject(string[] frameworks, PBXProject project, string target)
		{
			foreach (string framework in frameworks) {
				if (!project.HasFramework (framework)) {
					Debug.Log ("Adding " + framework + " to Xcode project");
					project.AddFrameworkToProject (target, framework, false);
				}
			}
		}
		
		protected static void AddPlatformLibsToProject(string[] libs, PBXProject project, string target)
		{
			foreach (string lib in libs) {
				string libGUID = project.AddFile ("usr/lib/" + lib, "Libraries/" + lib, PBXSourceTree.Sdk);
				project.AddFileToBuild (target, libGUID);
			}	
		}

		protected static void AddLibsToProject(string[] libs, PBXProject project, string target, string buildPath)
		{
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#else
			// Unity 5 and above should already take care of copying and linking non-platform frameworks
#endif
		}

		protected static void PreparePlist(string buildpath, string descriptor)
		{
		}

		protected static void AddBuildProperty(PBXProject project, string target, string property, string value)
		{
			project.AddBuildProperty (target, property, value);
		}
	}

}
