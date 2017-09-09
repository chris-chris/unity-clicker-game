namespace MoPubInternal.Editor.ThirdParty.xcodeapi.PBX
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using System.IO;

	internal class GUIDToCommentMap
    {
        private Dictionary<string, string> m_Dict = new Dictionary<string, string>();

        public string this[string guid]
        {
            get {
                if (m_Dict.ContainsKey(guid))
                    return m_Dict[guid];
                return null;
            }
        }

        public void Add(string guid, string comment)
        {
            if (m_Dict.ContainsKey(guid))
                return;
            m_Dict.Add(guid, comment);
        }

        public void Remove(string guid)
        {
            m_Dict.Remove(guid);
        }
        
        public string Write(string guid)
        {
            string comment = this[guid];
            if (comment == null)
                return guid;
            return String.Format("{0} /* {1} */", guid, comment);
        }
    }

    internal class PBXGUID
    {
        internal delegate string GuidGenerator();

        // We allow changing Guid generator to make testing of PBXProject possible
        private static GuidGenerator guidGenerator = DefaultGuidGenerator;

        internal static string DefaultGuidGenerator()
        {
            return Guid.NewGuid().ToString("N").Substring(8).ToUpper();
        }

        internal static void SetGuidGenerator(GuidGenerator generator)
        {
            guidGenerator = generator;
        }

        // Generates a GUID.
        public static string Generate()
        {
            return guidGenerator();
        }
    }

    internal class PBXRegex
    {
        public static string GuidRegexString = "[A-Fa-f0-9]{24}";
        public static Regex DontNeedQuotes  = new Regex("^[\\w\\d\\./_*]+$");
    }

    internal class PBXStream
    {
        // Quotes the given string if it contains special characters. Note: if the string already
        // contains quotes, then they are escaped and the entire string quoted again
        public static string QuoteStringIfNeeded(string src)
        {
            if (PBXRegex.DontNeedQuotes.IsMatch(src) && 
                !src.Contains("//") && !src.Contains("/*") && !src.Contains("*/"))
                return src;
            return "\"" + src.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
        }

        // If the given string is quoted, removes the quotes and unescapes any quotes within the string
        public static string UnquoteString(string src)
        {
            if (!src.StartsWith("\"") || !src.EndsWith("\""))
                return src;
            return src.Substring(1, src.Length - 2).Replace("\\\\", "\u569f").Replace("\\\"", "\"")
                                                   .Replace("\\n", "\n").Replace("\u569f", "\\"); // U+569f is a rarely used Chinese character
        }
    }

    internal enum PBXFileType
    {
        NotBuildable,
        Framework,
        Source,
        Resource,
        CopyFile
    }

    internal class FileTypeUtils
    {
        internal class FileTypeDesc
        {
            public FileTypeDesc(string typeName, PBXFileType type)
            {
                this.name = typeName;
                this.type = type;
                this.isExplicit = false;
            }

            public FileTypeDesc(string typeName, PBXFileType type, bool isExplicit)
            {
                this.name = typeName;
                this.type = type;
                this.isExplicit = isExplicit;
            }

            public string name;
            public PBXFileType type;
            public bool isExplicit;
        }

        private static readonly Dictionary<string, FileTypeDesc> types =
            new Dictionary<string, FileTypeDesc>
        {
            { ".a",         new FileTypeDesc("archive.ar",              PBXFileType.Framework) },
            { ".app",       new FileTypeDesc("wrapper.application",     PBXFileType.NotBuildable, true) },
            { ".appex",     new FileTypeDesc("wrapper.app-extension",   PBXFileType.CopyFile) },
            { ".s",         new FileTypeDesc("sourcecode.asm",          PBXFileType.Source) },
            { ".c",         new FileTypeDesc("sourcecode.c.c",          PBXFileType.Source) },
            { ".cc",        new FileTypeDesc("sourcecode.cpp.cpp",      PBXFileType.Source) },
            { ".cpp",       new FileTypeDesc("sourcecode.cpp.cpp",      PBXFileType.Source) },
            { ".swift",     new FileTypeDesc("sourcecode.swift",        PBXFileType.Source) },
            { ".dll",       new FileTypeDesc("file",                    PBXFileType.NotBuildable) },
            { ".framework", new FileTypeDesc("wrapper.framework",       PBXFileType.Framework) },
            { ".h",         new FileTypeDesc("sourcecode.c.h",          PBXFileType.NotBuildable) },
            { ".pch",       new FileTypeDesc("sourcecode.c.h",          PBXFileType.NotBuildable) },
            { ".icns",      new FileTypeDesc("image.icns",              PBXFileType.Resource) },
            { ".xcassets",  new FileTypeDesc("folder.assetcatalog",     PBXFileType.Resource) },
            { ".inc",       new FileTypeDesc("sourcecode.inc",          PBXFileType.NotBuildable) },
            { ".m",         new FileTypeDesc("sourcecode.c.objc",       PBXFileType.Source) },
            { ".mm",        new FileTypeDesc("sourcecode.cpp.objcpp",   PBXFileType.Source ) },
            { ".nib",       new FileTypeDesc("wrapper.nib",             PBXFileType.Resource) },
            { ".plist",     new FileTypeDesc("text.plist.xml",          PBXFileType.Resource) },
            { ".png",       new FileTypeDesc("image.png",               PBXFileType.Resource) },
            { ".rtf",       new FileTypeDesc("text.rtf",                PBXFileType.Resource) },
            { ".tiff",      new FileTypeDesc("image.tiff",              PBXFileType.Resource) },
            { ".txt",       new FileTypeDesc("text",                    PBXFileType.Resource) },
            { ".json",      new FileTypeDesc("text.json",               PBXFileType.Resource) },
            { ".xcodeproj", new FileTypeDesc("wrapper.pb-project",      PBXFileType.NotBuildable) },
            { ".xib",       new FileTypeDesc("file.xib",                PBXFileType.Resource) },
            { ".strings",   new FileTypeDesc("text.plist.strings",      PBXFileType.Resource) },
            { ".storyboard",new FileTypeDesc("file.storyboard",         PBXFileType.Resource) },
            { ".bundle",    new FileTypeDesc("wrapper.plug-in",         PBXFileType.Resource) },
            { ".dylib",     new FileTypeDesc("compiled.mach-o.dylib",   PBXFileType.Framework) }
        };

        public static bool IsKnownExtension(string ext)
        {
            return types.ContainsKey(ext);
        }

        internal static bool IsFileTypeExplicit(string ext)
        {
            if (types.ContainsKey(ext))
                return types[ext].isExplicit;
            return false;
        }

        public static PBXFileType GetFileType(string ext)
        {
            if (types.ContainsKey(ext))
                return types[ext].type;
            return PBXFileType.NotBuildable;
        }

        public static string GetTypeName(string ext)
        {
            if (types.ContainsKey(ext))
                return types[ext].name;
            return "text";
        }

        public static bool IsBuildable(string ext)
        {
            if (types.ContainsKey(ext) && types[ext].type != PBXFileType.NotBuildable)
                return true;
            return false;
        }

        private static readonly Dictionary<PBXSourceTree, string> sourceTree = new Dictionary<PBXSourceTree, string> 
        {
            { PBXSourceTree.Absolute,   "<absolute>" },
            { PBXSourceTree.Group,      "<group>" },
            { PBXSourceTree.Build,      "BUILT_PRODUCTS_DIR" },
            { PBXSourceTree.Developer,  "DEVELOPER_DIR" },
            { PBXSourceTree.Sdk,        "SDKROOT" },
            { PBXSourceTree.Source,     "SOURCE_ROOT" },
        };
        
        private static readonly Dictionary<string, PBXSourceTree> stringToSourceTreeMap = new Dictionary<string, PBXSourceTree> 
        {
            { "<absolute>",         PBXSourceTree.Absolute },
            { "<group>",            PBXSourceTree.Group },
            { "BUILT_PRODUCTS_DIR", PBXSourceTree.Build },
            { "DEVELOPER_DIR",      PBXSourceTree.Developer },
            { "SDKROOT",            PBXSourceTree.Sdk },
            { "SOURCE_ROOT",        PBXSourceTree.Source },
        };

        internal static string SourceTreeDesc(PBXSourceTree tree)
        {
            return sourceTree[tree];
        }
        
        // returns PBXSourceTree.Source on error
        internal static PBXSourceTree ParseSourceTree(string tree)
        {
            if (stringToSourceTreeMap.ContainsKey(tree))
                return stringToSourceTreeMap[tree];
            return PBXSourceTree.Source;
        }

        internal static List<PBXSourceTree> AllAbsoluteSourceTrees()
        {
            return new List<PBXSourceTree>{PBXSourceTree.Absolute, PBXSourceTree.Build,
                                           PBXSourceTree.Developer, PBXSourceTree.Sdk, PBXSourceTree.Source};
        }
    }

} // UnityEditor.iOS.Xcode
