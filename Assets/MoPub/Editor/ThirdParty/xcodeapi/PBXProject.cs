using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System;
using MoPubInternal.Editor.ThirdParty.xcodeapi.PBX;

namespace MoPubInternal.Editor.ThirdParty.xcodeapi
{
    using PBX;
    using PBXBuildFileSection           = KnownSectionBase<PBXBuildFile>;
    using PBXFileReferenceSection       = KnownSectionBase<PBXFileReference>;
    using PBXGroupSection               = KnownSectionBase<PBXGroup>;
    using PBXContainerItemProxySection  = KnownSectionBase<PBXContainerItemProxy>;
    using PBXReferenceProxySection      = KnownSectionBase<PBXReferenceProxy>;
    using PBXSourcesBuildPhaseSection   = KnownSectionBase<PBXSourcesBuildPhase>;
    using PBXFrameworksBuildPhaseSection= KnownSectionBase<PBXFrameworksBuildPhase>;
    using PBXResourcesBuildPhaseSection = KnownSectionBase<PBXResourcesBuildPhase>;
    using PBXCopyFilesBuildPhaseSection = KnownSectionBase<PBXCopyFilesBuildPhase>;
    using PBXShellScriptBuildPhaseSection = KnownSectionBase<PBXShellScriptBuildPhase>;
    using PBXVariantGroupSection        = KnownSectionBase<PBXVariantGroup>;
    using PBXNativeTargetSection        = KnownSectionBase<PBXNativeTarget>;
    using PBXTargetDependencySection    = KnownSectionBase<PBXTargetDependency>;
    using XCBuildConfigurationSection   = KnownSectionBase<XCBuildConfiguration>;
    using XCConfigurationListSection    = KnownSectionBase<XCConfigurationList>;
    using UnknownSection                = KnownSectionBase<PBXObject>;

    // Determines the tree the given path is relative to
    public enum PBXSourceTree
    {
        Absolute,   // The path is absolute
        Source,     // The path is relative to the source folder
        Group,      // The path is relative to the folder it's in. This enum is used only internally,
        // do not use it as function parameter
        Build,      // The path is relative to the build products folder 
        Developer,  // The path is relative to the developer folder
        Sdk         // The path is relative to the sdk folder
    };

    public class PBXProject
    {
        private Dictionary<string, SectionBase> m_Section = null;
        private PBXElementDict m_RootElements = null;
        private PBXElementDict m_UnknownObjects = null;
        private string m_ObjectVersion = null;
        private List<string> m_SectionOrder = null;
        
        private Dictionary<string, UnknownSection> m_UnknownSections;
        private PBXBuildFileSection buildFiles = null;
        private PBXFileReferenceSection fileRefs = null;
        private PBXGroupSection groups = null;
        private PBXContainerItemProxySection containerItems = null;
        private PBXReferenceProxySection references = null;
        private PBXSourcesBuildPhaseSection sources = null;
        private PBXFrameworksBuildPhaseSection frameworks = null;
        private PBXResourcesBuildPhaseSection resources = null;
        private PBXCopyFilesBuildPhaseSection copyFiles = null;
        private PBXShellScriptBuildPhaseSection shellScripts = null;
        private PBXNativeTargetSection nativeTargets = null;
        private PBXTargetDependencySection targetDependencies = null;
        private PBXVariantGroupSection variantGroups = null;
        private XCBuildConfigurationSection buildConfigs = null;
        private XCConfigurationListSection configs = null;
        private PBXProjectSection project = null;
        
        // FIXME: create a separate PBXObject tree to represent these relationships

        // A build file can be represented only once in all *BuildPhaseSection sections, thus
        // we can simplify the cache by not caring about the file extension
        private Dictionary<string, Dictionary<string, PBXBuildFile>> m_FileGuidToBuildFileMap = null;
        private Dictionary<string, PBXFileReference> m_ProjectPathToFileRefMap = null;
        private Dictionary<string, string> m_FileRefGuidToProjectPathMap = null;
        private Dictionary<PBXSourceTree, Dictionary<string, PBXFileReference>> m_RealPathToFileRefMap = null;
        private Dictionary<string, PBXGroup> m_ProjectPathToGroupMap = null;
        private Dictionary<string, string> m_GroupGuidToProjectPathMap = null;
        private Dictionary<string, PBXGroup> m_GuidToParentGroupMap = null;

        // targetGuid is the guid of the target that contains the section that contains the buildFile
        void BuildFilesAdd(string targetGuid, PBXBuildFile buildFile)
        {
            if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
                m_FileGuidToBuildFileMap[targetGuid] = new Dictionary<string, PBXBuildFile>();
            m_FileGuidToBuildFileMap[targetGuid][buildFile.fileRef] = buildFile;
            buildFiles.AddEntry(buildFile);
        }

        void BuildFilesRemove(string targetGuid, string fileGuid)
        {
            var buildFile = GetBuildFileForFileGuid(targetGuid, fileGuid);
            if (buildFile != null)
            {
                m_FileGuidToBuildFileMap[targetGuid].Remove(buildFile.fileRef);
                buildFiles.RemoveEntry(buildFile.guid);
            }
        }

        PBXBuildFile GetBuildFileForFileGuid(string targetGuid, string fileGuid)
        {
            if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
                return null;
            if (!m_FileGuidToBuildFileMap[targetGuid].ContainsKey(fileGuid))
                return null;
            return m_FileGuidToBuildFileMap[targetGuid][fileGuid];
        }

        void FileRefsAdd(string realPath, string projectPath, PBXGroup parent, PBXFileReference fileRef)
        {
            fileRefs.AddEntry(fileRef);
            m_ProjectPathToFileRefMap.Add(projectPath, fileRef);
            m_FileRefGuidToProjectPathMap.Add(fileRef.guid, projectPath);
            m_RealPathToFileRefMap[fileRef.tree].Add(realPath, fileRef); // FIXME
            m_GuidToParentGroupMap.Add(fileRef.guid, parent);
        }

        void FileRefsRemove(string guid)
        {
            PBXFileReference fileRef = fileRefs[guid];
            fileRefs.RemoveEntry(guid);
            m_ProjectPathToFileRefMap.Remove(m_FileRefGuidToProjectPathMap[guid]);
            m_FileRefGuidToProjectPathMap.Remove(guid);
            foreach (var tree in FileTypeUtils.AllAbsoluteSourceTrees())
                m_RealPathToFileRefMap[tree].Remove(fileRef.path);
            m_GuidToParentGroupMap.Remove(guid);
        }

        void GroupsAdd(string projectPath, PBXGroup parent, PBXGroup gr)
        {
            m_ProjectPathToGroupMap.Add(projectPath, gr);
            m_GroupGuidToProjectPathMap.Add(gr.guid, projectPath);
            m_GuidToParentGroupMap.Add(gr.guid, parent);
            groups.AddEntry(gr);
        }

        void GroupsRemove(string guid)
        {
            m_ProjectPathToGroupMap.Remove(m_GroupGuidToProjectPathMap[guid]);
            m_GroupGuidToProjectPathMap.Remove(guid);
            m_GuidToParentGroupMap.Remove(guid);
            groups.RemoveEntry(guid);
        }

        void RefreshBuildFilesMapForBuildFileGuidList(Dictionary<string, PBXBuildFile> mapForTarget,
                                                      FileGUIDListBase list)
        {
            foreach (string guid in list.files)
            {
                var buildFile = buildFiles[guid];
                mapForTarget[buildFile.fileRef] = buildFile;
            }
        }

        void CombinePaths(string path1, PBXSourceTree tree1, string path2, PBXSourceTree tree2,
                          out string resPath, out PBXSourceTree resTree)
        {
            if (tree2 == PBXSourceTree.Group)
            {
                resPath = Path.Combine(path1, path2);
                resTree = tree1;
                return;
            }
            
            resPath = path2;
            resTree = tree2;
        }

        void RefreshMapsForGroupChildren(string projectPath, string realPath, PBXSourceTree realPathTree, PBXGroup parent)
        {
            var children = new List<string>(parent.children);
            foreach (string guid in children)
            {
                PBXFileReference fileRef = fileRefs[guid];
                string pPath;
                string rPath;
                PBXSourceTree rTree;
 
                if (fileRef != null)
                {
                    pPath = Path.Combine(projectPath, fileRef.name);
                    CombinePaths(realPath, realPathTree, fileRef.path, fileRef.tree, out rPath, out rTree);

                    m_ProjectPathToFileRefMap.Add(pPath, fileRef);
                    m_FileRefGuidToProjectPathMap.Add(fileRef.guid, pPath);
                    m_RealPathToFileRefMap[rTree].Add(rPath, fileRef);
                    m_GuidToParentGroupMap.Add(guid, parent);
                    continue;
                }

                PBXGroup gr = groups[guid];
                if (gr != null)
                {
                    pPath = Path.Combine(projectPath, gr.name);
                    CombinePaths(realPath, realPathTree, gr.path, gr.tree, out rPath, out rTree);
                    
                    m_ProjectPathToGroupMap.Add(pPath, gr);
                    m_GroupGuidToProjectPathMap.Add(gr.guid, pPath);
                    m_GuidToParentGroupMap.Add(guid, parent);
                    RefreshMapsForGroupChildren(pPath, rPath, rTree, gr);
                }
            }
        }

        void RefreshAuxMaps()
        {
            foreach (var targetEntry in nativeTargets.entries)
            {
                var map = new Dictionary<string, PBXBuildFile>();
                foreach (string phaseGuid in targetEntry.Value.phases)
                {
                    if (frameworks.entries.ContainsKey(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, frameworks.entries[phaseGuid]);
                    if (resources.entries.ContainsKey(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, resources.entries[phaseGuid]);
                    if (sources.entries.ContainsKey(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, sources.entries[phaseGuid]);
                    if (copyFiles.entries.ContainsKey(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, copyFiles.entries[phaseGuid]);
                }
                m_FileGuidToBuildFileMap[targetEntry.Key] = map;
            }
            RefreshMapsForGroupChildren("", "", PBXSourceTree.Source, groups[project.project.mainGroup]);
        }

        void Clear()
        {
            buildFiles = new PBXBuildFileSection("PBXBuildFile");
            fileRefs = new PBXFileReferenceSection("PBXFileReference");
            groups = new PBXGroupSection("PBXGroup");
            containerItems = new PBXContainerItemProxySection("PBXContainerItemProxy");
            references = new PBXReferenceProxySection("PBXReferenceProxy");
            sources = new PBXSourcesBuildPhaseSection("PBXSourcesBuildPhase");
            frameworks = new PBXFrameworksBuildPhaseSection("PBXFrameworksBuildPhase");
            resources = new PBXResourcesBuildPhaseSection("PBXResourcesBuildPhase");
            copyFiles = new PBXCopyFilesBuildPhaseSection("PBXCopyFilesBuildPhase");
            shellScripts = new PBXShellScriptBuildPhaseSection("PBXShellScriptBuildPhase");
            nativeTargets = new PBXNativeTargetSection("PBXNativeTarget");
            targetDependencies = new PBXTargetDependencySection("PBXTargetDependency");
            variantGroups = new PBXVariantGroupSection("PBXVariantGroup");
            buildConfigs = new XCBuildConfigurationSection("XCBuildConfiguration");
            configs = new XCConfigurationListSection("XCConfigurationList");
            project = new PBXProjectSection();
            m_UnknownSections = new Dictionary<string, UnknownSection>();
            
            m_Section = new Dictionary<string, SectionBase>
            {
                { "PBXBuildFile", buildFiles },
                { "PBXFileReference", fileRefs },
                { "PBXGroup", groups },
                { "PBXContainerItemProxy", containerItems },
                { "PBXReferenceProxy", references },
                { "PBXSourcesBuildPhase", sources },
                { "PBXFrameworksBuildPhase", frameworks },
                { "PBXResourcesBuildPhase", resources },
                { "PBXCopyFilesBuildPhase", copyFiles },
                { "PBXShellScriptBuildPhase", shellScripts },
                { "PBXNativeTarget", nativeTargets },
                { "PBXTargetDependency", targetDependencies },
                { "PBXVariantGroup", variantGroups },
                { "XCBuildConfiguration", buildConfigs },
                { "XCConfigurationList", configs },

                { "PBXProject", project },
            };
            m_RootElements = new PBXElementDict();
            m_UnknownObjects = new PBXElementDict();
            m_ObjectVersion = null;
            m_SectionOrder = new List<string>{
                "PBXBuildFile", "PBXContainerItemProxy", "PBXCopyFilesBuildPhase", "PBXFileReference",
                "PBXFrameworksBuildPhase", "PBXGroup", "PBXNativeTarget", "PBXProject", "PBXReferenceProxy",
                "PBXResourcesBuildPhase", "PBXShellScriptBuildPhase", "PBXSourcesBuildPhase", "PBXTargetDependency",
                "PBXVariantGroup", "XCBuildConfiguration", "XCConfigurationList"
            };
            m_FileGuidToBuildFileMap = new Dictionary<string, Dictionary<string, PBXBuildFile>>();
            m_ProjectPathToFileRefMap = new Dictionary<string, PBXFileReference>();
            m_FileRefGuidToProjectPathMap = new Dictionary<string, string>();
            m_RealPathToFileRefMap = new Dictionary<PBXSourceTree, Dictionary<string, PBXFileReference>>();
            foreach (var tree in FileTypeUtils.AllAbsoluteSourceTrees())
                m_RealPathToFileRefMap.Add(tree, new Dictionary<string, PBXFileReference>());
            m_ProjectPathToGroupMap = new Dictionary<string, PBXGroup>();
            m_GroupGuidToProjectPathMap = new Dictionary<string, string>();
            m_GuidToParentGroupMap = new Dictionary<string, PBXGroup>();
        }

        public static string GetPBXProjectPath(string buildPath)
        {
            return Path.Combine(buildPath, "Unity-iPhone/project.pbxproj");
        }

        public static string GetUnityTargetName()
        {
            return "Unity-iPhone";
        }

        public static string GetUnityTestTargetName()
        {
            return "Unity-iPhone Tests";
        }

        /// Returns a guid identifying native target with name @a name
        public string TargetGuidByName(string name)
        {
            foreach (var entry in nativeTargets.entries)
                if (entry.Value.name == name)
                    return entry.Key;
            return null;
        }

        private FileGUIDListBase BuildSection(PBXNativeTarget target, string path)
        {
            string ext = Path.GetExtension(path);
            var phase = FileTypeUtils.GetFileType(ext);
            switch (phase) {
            case PBXFileType.Framework:
                foreach (var guid in target.phases)
                    if (frameworks.entries.ContainsKey(guid))
                        return frameworks.entries[guid];
                break;
            case PBXFileType.Resource:
                foreach (var guid in target.phases)
                    if (resources.entries.ContainsKey(guid))
                        return resources.entries[guid];
                break;
            case PBXFileType.Source:
                foreach (var guid in target.phases)
                    if (sources.entries.ContainsKey(guid))
                        return sources.entries[guid];
                break;
            case PBXFileType.CopyFile:
                foreach (var guid in target.phases)
                    if (copyFiles.entries.ContainsKey(guid))
                        return copyFiles.entries[guid];
                break;
            }
            return null;
        }

        public static bool IsKnownExtension(string ext)
        {
            return FileTypeUtils.IsKnownExtension(ext);
        }

        public static bool IsBuildable(string ext)
        {
            return FileTypeUtils.IsBuildable(ext);
        }

        // The same file can be referred to by more than one project path.
        private string AddFileImpl(string path, string projectPath, PBXSourceTree tree)
        {
            path = FixSlashesInPath(path);
            projectPath = FixSlashesInPath(projectPath);

            string ext = Path.GetExtension(path);
            if (ext != Path.GetExtension(projectPath))
                throw new Exception("Project and real path extensions do not match");

            string guid = FindFileGuidByProjectPath(projectPath);
            if (guid == null)
                guid = FindFileGuidByRealPath(path);
            if (guid == null)
            {
                PBXFileReference fileRef = PBXFileReference.CreateFromFile(path, GetFilenameFromPath(projectPath), tree);
                PBXGroup parent = CreateSourceGroup(GetDirectoryFromPath(projectPath));
                parent.children.AddGUID(fileRef.guid);
                FileRefsAdd(path, projectPath, parent, fileRef);
                guid = fileRef.guid;
            }
            return guid;
        }

        // The extension of the files identified by path and projectPath must be the same.
        public string AddFile(string path, string projectPath)
        {
            return AddFileImpl(path, projectPath, PBXSourceTree.Source);
        }

        // sourceTree must not be PBXSourceTree.Group
        public string AddFile(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return AddFileImpl(path, projectPath, sourceTree);
        }

        private void AddBuildFileImpl(string targetGuid, string fileGuid, bool weak, string compileFlags)
        {
            PBXNativeTarget target = nativeTargets[targetGuid];
            string ext = Path.GetExtension(fileRefs[fileGuid].path);
            if (FileTypeUtils.IsBuildable(ext) &&
                GetBuildFileForFileGuid(targetGuid, fileGuid) == null)
            {
                PBXBuildFile buildFile = PBXBuildFile.CreateFromFile(fileGuid, weak, compileFlags);
                BuildFilesAdd(targetGuid, buildFile);
                BuildSection(target, ext).files.AddGUID(buildFile.guid);
            }
        }

        public void AddFileToBuild(string targetGuid, string fileGuid)
        {
            AddBuildFileImpl(targetGuid, fileGuid, false, null);
        }

        public void AddFileToBuildWithFlags(string targetGuid, string fileGuid, string compileFlags)
        {
            AddBuildFileImpl(targetGuid, fileGuid, false, compileFlags);
        }
        
        // returns null on error
        // FIXME: at the moment returns all flags as the first element of the array
        public List<string> GetCompileFlagsForFile(string targetGuid, string fileGuid)
        {
            var buildFile = GetBuildFileForFileGuid(targetGuid, fileGuid);
            if (buildFile == null)
                return null;
            if (buildFile.compileFlags == null)
                return new List<string>();
            return new List<string>{buildFile.compileFlags};
        }
        
        public void SetCompileFlagsForFile(string targetGuid, string fileGuid, List<string> compileFlags)
        {
            var buildFile = GetBuildFileForFileGuid(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            buildFile.compileFlags = string.Join(" ", compileFlags.ToArray());
        }

        public bool ContainsFileByRealPath(string path)
        {
            return FindFileGuidByRealPath(path) != null;
        }

        // sourceTree must not be PBXSourceTree.Group
        public bool ContainsFileByRealPath(string path, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return FindFileGuidByRealPath(path, sourceTree) != null;
        }

        public bool ContainsFileByProjectPath(string path)
        {
            return FindFileGuidByProjectPath(path) != null;
        }

        public bool HasFramework(string framework)
        {
            return ContainsFileByRealPath("System/Library/Frameworks/"+framework);
        }

        /// The framework must be specified with the '.framework' extension
        public void AddFrameworkToProject(string targetGuid, string framework, bool weak)
        {
            string fileGuid = AddFile("System/Library/Frameworks/"+framework, "Frameworks/"+framework, PBXSourceTree.Sdk);
            AddBuildFileImpl(targetGuid, fileGuid, weak, null);
        }

        private string GetDirectoryFromPath(string path)
        {
            int pos = path.LastIndexOf('/');
            if (pos == -1)
                return "";
            else
                return path.Substring(0, pos);
        }

        private string GetFilenameFromPath(string path)
        {
            int pos = path.LastIndexOf('/');
            if (pos == -1)
                return path;
            else
                return path.Substring(pos + 1);
        }

        // sourceTree must not be PBXSourceTree.Group
        public string FindFileGuidByRealPath(string path, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            path = FixSlashesInPath(path);
            if (m_RealPathToFileRefMap[sourceTree].ContainsKey(path))
                return m_RealPathToFileRefMap[sourceTree][path].guid;
            return null;
        }

        public string FindFileGuidByRealPath(string path)
        {
            path = FixSlashesInPath(path);

            foreach (var tree in FileTypeUtils.AllAbsoluteSourceTrees())
            {
                string res = FindFileGuidByRealPath(path, tree);
                if (res != null)
                    return res;
            }
            return null;
        }

        public string FindFileGuidByProjectPath(string path)
        {
            path = FixSlashesInPath(path);
            if (m_ProjectPathToFileRefMap.ContainsKey(path))
                return m_ProjectPathToFileRefMap[path].guid;
            return null;
        }

        public void RemoveFileFromBuild(string targetGuid, string fileGuid)
        {
            var buildFile = GetBuildFileForFileGuid(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            BuildFilesRemove(targetGuid, fileGuid);

            string buildGuid = buildFile.guid;
            if (buildGuid != null)
            {
                foreach (var section in sources.entries)
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in resources.entries)
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in copyFiles.entries)
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in frameworks.entries)
                    section.Value.files.RemoveGUID(buildGuid);
            }
        }

        public void RemoveFile(string fileGuid)
        {
            if (fileGuid == null)
                return;

            // remove from parent
            PBXGroup parent = m_GuidToParentGroupMap[fileGuid];
            if (parent != null)
                parent.children.RemoveGUID(fileGuid);
            RemoveGroupIfEmpty(parent);

            // remove actual file
            foreach (var target in nativeTargets.entries)
                RemoveFileFromBuild(target.Value.guid, fileGuid);
            FileRefsRemove(fileGuid);
        }

        void RemoveGroupIfEmpty(PBXGroup gr)
        {
            if (gr.children.Count == 0 && gr.guid != project.project.mainGroup)
            {
                // remove from parent
                PBXGroup parent = m_GuidToParentGroupMap[gr.guid];
                parent.children.RemoveGUID(gr.guid);
                RemoveGroupIfEmpty(parent);

                // remove actual group
                GroupsRemove(gr.guid);
            }
        }

        private void RemoveGroupChildrenRecursive(PBXGroup parent)
        {
            List<string> children = new List<string>(parent.children);
            parent.children.Clear();
            foreach (string guid in children)
            {
                PBXFileReference file = fileRefs[guid];
                if (file != null)
                {
                    foreach (var target in nativeTargets.entries)
                        RemoveFileFromBuild(target.Value.guid, guid);
                    FileRefsRemove(guid);
                    continue;
                }

                PBXGroup gr = groups[guid];
                if (gr != null)
                {
                    RemoveGroupChildrenRecursive(gr);
                    GroupsRemove(parent.guid);
                    continue;
                }
            }
        }

        internal void RemoveFilesByProjectPathRecursive(string projectPath)
        {
            PBXGroup gr = GetSourceGroup(projectPath);
            if (gr == null)
                return;
            RemoveGroupChildrenRecursive(gr);
            RemoveGroupIfEmpty(gr);
        }

        private PBXGroup GetPBXGroupChildByName(PBXGroup group, string name)
        {
            foreach (string guid in group.children)
            {
                var gr = groups[guid];
                if (gr != null && gr.name == name)
                    return gr;
            }
            return null;
        }

        /// Returns the source group identified by sourceGroup. If sourceGroup is empty or null,
        /// root group is returned. If no group is found, null is returned.
        private PBXGroup GetSourceGroup(string sourceGroup)
        {
            sourceGroup = FixSlashesInPath(sourceGroup);

            if (sourceGroup == null || sourceGroup == "")
                return groups[project.project.mainGroup];

            if (m_ProjectPathToGroupMap.ContainsKey(sourceGroup))
                return m_ProjectPathToGroupMap[sourceGroup];
            return null;
        }

        /// Creates source group identified by sourceGroup, if needed, and returns it.
        /// If sourceGroup is empty or null, root group is returned
        private PBXGroup CreateSourceGroup(string sourceGroup)
        {
            sourceGroup = FixSlashesInPath(sourceGroup);

            if (m_ProjectPathToGroupMap.ContainsKey(sourceGroup))
                return m_ProjectPathToGroupMap[sourceGroup];

            PBXGroup gr = groups[project.project.mainGroup];

            if (sourceGroup == null || sourceGroup == "")
                return gr;

            string[] elements = sourceGroup.Trim('/').Split('/');
            string projectPath = null;
            foreach (string pathEl in elements)
            {
                if (projectPath == null)
                    projectPath = pathEl;
                else
                    projectPath += "/" + pathEl;

                PBXGroup child = GetPBXGroupChildByName(gr, pathEl);
                if (child != null)
                    gr = child;
                else
                {
                    PBXGroup newGroup = PBXGroup.Create(pathEl, pathEl, PBXSourceTree.Group);
                    gr.children.AddGUID(newGroup.guid);
                    GroupsAdd(projectPath, gr, newGroup);
                    gr = newGroup;
                }
            }
            return gr;
        }

        // sourceTree must not be PBXSourceTree.Group
        public void AddExternalProjectDependency(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            path = FixSlashesInPath(path);
            projectPath = FixSlashesInPath(projectPath);

            // note: we are duplicating products group for the project reference. Otherwise Xcode crashes.
            PBXGroup productGroup = PBXGroup.CreateRelative("Products");
            groups.AddEntry(productGroup); // don't use GroupsAdd here

            PBXFileReference fileRef = PBXFileReference.CreateFromFile(path, Path.GetFileName(projectPath),
                                                                       sourceTree);
            FileRefsAdd(path, projectPath, null, fileRef);
            CreateSourceGroup(GetDirectoryFromPath(projectPath)).children.AddGUID(fileRef.guid);

            project.project.AddReference(productGroup.guid, fileRef.guid);
        }

        /** This function must be called only after the project the library is in has
            been added as a dependency via AddExternalProjectDependency. projectPath must be
            the same as the 'path' parameter passed to the AddExternalProjectDependency.
            remoteFileGuid must be the guid of the referenced file as specified in
            PBXFileReference section of the external project

            TODO: wtf. is remoteInfo entry in PBXContainerItemProxy? Is in referenced project name or
            referenced library name without extension?
        */
        public void AddExternalLibraryDependency(string targetGuid, string filename, string remoteFileGuid, string projectPath,
                                                 string remoteInfo)
        {
            PBXNativeTarget target = nativeTargets[targetGuid];
            filename = FixSlashesInPath(filename);
            projectPath = FixSlashesInPath(projectPath);

            // find the products group to put the new library in
            string projectGuid = FindFileGuidByRealPath(projectPath);
            if (projectGuid == null)
                throw new Exception("No such project");

            string productsGroupGuid = null;
            foreach (var proj in project.project.projectReferences)
            {
                if (proj.projectRef == projectGuid)
                {
                    productsGroupGuid = proj.group;
                    break;
                }
            }

            if (productsGroupGuid == null)
                throw new Exception("Malformed project: no project in project references");

            PBXGroup productGroup = groups[productsGroupGuid];

            // verify file extension
            string ext = Path.GetExtension(filename);
            if (!FileTypeUtils.IsBuildable(ext))
                throw new Exception("Wrong file extension");

            // create ContainerItemProxy object
            var container = PBXContainerItemProxy.Create(projectGuid, "2", remoteFileGuid, remoteInfo);
            containerItems.AddEntry(container);

            // create a reference and build file for the library
            string typeName = FileTypeUtils.GetTypeName(ext);

            var libRef = PBXReferenceProxy.Create(filename, typeName, container.guid, "BUILT_PRODUCTS_DIR");
            references.AddEntry(libRef);
            PBXBuildFile libBuildFile = PBXBuildFile.CreateFromFile(libRef.guid, false, null);
            BuildFilesAdd(targetGuid, libBuildFile);
            BuildSection(target, ext).files.AddGUID(libBuildFile.guid);

            // add to products folder
            productGroup.children.AddGUID(libRef.guid);
        }

        private void SetDefaultAppExtensionReleaseBuildFlags(XCBuildConfiguration config, string infoPlistPath)
        {
            config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
            config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
            config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
            config.AddProperty("CLANG_ENABLE_MODULES", "YES");
            config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
            config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
            config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
            config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
            config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
            config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
            config.AddProperty("COPY_PHASE_STRIP", "YES");
            config.AddProperty("ENABLE_NS_ASSERTIONS", "NO");
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
            config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
            config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
            config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
            config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
            config.AddProperty("INFOPLIST_FILE", infoPlistPath);
            config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks @executable_path/../../Frameworks");
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "NO");
            config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
            config.AddProperty("SKIP_INSTALL", "YES");
            config.AddProperty("VALIDATE_PRODUCT", "YES");
        }

        private void SetDefaultAppExtensionDebugBuildFlags(XCBuildConfiguration config, string infoPlistPath)
        {
            config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
            config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
            config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
            config.AddProperty("CLANG_ENABLE_MODULES", "YES");
            config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
            config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
            config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
            config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
            config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
            config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
            config.AddProperty("COPY_PHASE_STRIP", "NO");
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_DYNAMIC_NO_PIC", "NO");
            config.AddProperty("GCC_OPTIMIZATION_LEVEL", "0");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "DEBUG=1");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "$(inherited)");
            config.AddProperty("GCC_SYMBOLS_PRIVATE_EXTERN", "NO");
            config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
            config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
            config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
            config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
            config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
            config.AddProperty("INFOPLIST_FILE", infoPlistPath);
            config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "YES");
            config.AddProperty("ONLY_ACTIVE_ARCH", "YES");
            config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
            config.AddProperty("SKIP_INSTALL", "YES");
        }

        // Returns the guid of the new target
        internal string AddAppExtension(string mainTarget, string name, string infoPlistPath)
        {
            string ext = ".appex";
            string fullName = name + ext;
            var productFileRef = PBXFileReference.CreateFromFile("Products/" + fullName, "Products/" + fullName,
                                                                 PBXSourceTree.Group);
            var releaseBuildConfig = XCBuildConfiguration.Create("Release");
            buildConfigs.AddEntry(releaseBuildConfig);
            SetDefaultAppExtensionReleaseBuildFlags(releaseBuildConfig, infoPlistPath);

            var debugBuildConfig = XCBuildConfiguration.Create("Debug");
            buildConfigs.AddEntry(debugBuildConfig);
            SetDefaultAppExtensionDebugBuildFlags(debugBuildConfig, infoPlistPath);

            var buildConfigList = XCConfigurationList.Create();
            configs.AddEntry(buildConfigList);
            buildConfigList.buildConfigs.AddGUID(releaseBuildConfig.guid);
            buildConfigList.buildConfigs.AddGUID(debugBuildConfig.guid);


            var newTarget = PBXNativeTarget.Create(name, productFileRef.guid, "com.apple.product-type.app-extension", buildConfigList.guid);
            nativeTargets.AddEntry(newTarget);
            project.project.targets.Add(newTarget.guid);

            var sourcesBuildPhase = PBXSourcesBuildPhase.Create();
            sources.AddEntry(sourcesBuildPhase);
            newTarget.phases.AddGUID(sourcesBuildPhase.guid);

            var resourcesBuildPhase = PBXResourcesBuildPhase.Create();
            resources.AddEntry(resourcesBuildPhase);
            newTarget.phases.AddGUID(resourcesBuildPhase.guid);

            var frameworksBuildPhase = PBXFrameworksBuildPhase.Create();
            frameworks.AddEntry(frameworksBuildPhase);
            newTarget.phases.AddGUID(frameworksBuildPhase.guid);

            var copyFilesBuildPhase = PBXCopyFilesBuildPhase.Create("Embed App Extensions", "13");
            copyFiles.AddEntry(copyFilesBuildPhase);
            nativeTargets[mainTarget].phases.AddGUID(copyFilesBuildPhase.guid);

            var containerProxy = PBXContainerItemProxy.Create(project.project.guid, "1", newTarget.guid, name);
            containerItems.AddEntry(containerProxy);

            var targetDependency = PBXTargetDependency.Create(newTarget.guid, containerProxy.guid);
            targetDependencies.AddEntry(targetDependency);

            nativeTargets[mainTarget].dependencies.AddGUID(targetDependency.guid);

            AddFile(fullName, "Products/" + fullName, PBXSourceTree.Build);
            var buildAppCopy = PBXBuildFile.CreateFromFile(FindFileGuidByProjectPath("Products/" + fullName), false, "");
            BuildFilesAdd(mainTarget, buildAppCopy);
            copyFilesBuildPhase.files.AddGUID(buildAppCopy.guid);

            AddFile(infoPlistPath, name + "/Supporting Files/Info.plist", PBXSourceTree.Group);

            return newTarget.guid;
        }

        public string BuildConfigByName(string targetGuid, string name)
        {
            PBXNativeTarget target = nativeTargets[targetGuid];
            foreach (string guid in configs[target.buildConfigList].buildConfigs)
            {
                var buildConfig = buildConfigs[guid];
                if (buildConfig != null && buildConfig.name == name)
                    return buildConfig.guid;
            }
            return null;
        }

        // Adds an item to a build property that contains a value list. Duplicate build properties
        // are ignored. Values for name "LIBRARY_SEARCH_PATHS" are quoted if they contain spaces.
        public void AddBuildProperty(string targetGuid, string name, string value)
        {
            PBXNativeTarget target = nativeTargets[targetGuid];
            foreach (string guid in configs[target.buildConfigList].buildConfigs)
                buildConfigs[guid].AddProperty(name, value);
        }

        public void AddBuildProperty(string[] targetGuids, string name, string value)
        {
            foreach (string t in targetGuids)
                AddBuildProperty(t, name, value);
        }
        public void AddBuildPropertyForConfig(string configGuid, string name, string value)
        {
            buildConfigs[configGuid].AddProperty(name, value);
        }

        public void AddBuildPropertyForConfig(string[] configGuids, string name, string value)
        {
            foreach (string guid in configGuids)
                AddBuildPropertyForConfig(guid, name, value);
        }

        public void SetBuildProperty(string targetGuid, string name, string value)
        {
            PBXNativeTarget target = nativeTargets[targetGuid];
            foreach (string guid in configs[target.buildConfigList].buildConfigs)
                buildConfigs[guid].SetProperty(name, value);
        }
        public void SetBuildProperty(string[] targetGuids, string name, string value)
        {
            foreach (string t in targetGuids)
                SetBuildProperty(t, name, value);
        }
        public void SetBuildPropertyForConfig(string configGuid, string name, string value)
        {
            buildConfigs[configGuid].SetProperty(name, value);
        }
        public void SetBuildPropertyForConfig(string[] configGuids, string name, string value)
        {
            foreach (string guid in configGuids)
                SetBuildPropertyForConfig(guid, name, value);
        }

        /// Interprets the value of the given property as a set of space-delimited strings, then
        /// removes strings equal to items to removeValues and adds strings in addValues.
        public void UpdateBuildProperty(string targetGuid, string name, string[] addValues, string[] removeValues)
        {
            PBXNativeTarget target = nativeTargets[targetGuid];
            foreach (string guid in configs[target.buildConfigList].buildConfigs)
                buildConfigs[guid].UpdateProperties(name, addValues, removeValues);
        }
        public void UpdateBuildProperty(string[] targetGuids, string name, string[] addValues, string[] removeValues)
        {
            foreach (string t in targetGuids)
                UpdateBuildProperty(t, name, addValues, removeValues);
        }
        public void UpdateBuildPropertyForConfig(string configGuid, string name, string[] addValues, string[] removeValues)
        {
            buildConfigs[configGuid].UpdateProperties(name, addValues, removeValues);
        }
        public void UpdateBuildPropertyForConfig(string[] configGuids, string name, string[] addValues, string[] removeValues)
        {
            foreach (string guid in configGuids)
                UpdateBuildProperty(guid, name, addValues, removeValues);
        }

        /// Replaces '\' with '/'. We need to apply this function to all paths that come from the user
        /// of the API because we store paths to pbxproj and on windows we may get path with '\' slashes
        /// instead of '/' slashes
        private static string FixSlashesInPath(string path)
        {
            if (path == null)
                return null;
            return path.Replace('\\', '/');
        }

        private void BuildCommentMapForBuildFiles(GUIDToCommentMap comments, List<string> guids, string sectName)
        {
            foreach (var guid in guids)
            {
                var buildFile = buildFiles[guid];
                if (buildFile != null)
                {
                    var fileRef = fileRefs[buildFile.fileRef];
                    if (fileRef != null)
                        comments.Add(guid, String.Format("{0} in {1}", fileRef.name, sectName));
                    else
                    {
                        var reference = references[buildFile.fileRef];
                        if (reference != null)
                            comments.Add(guid, String.Format("{0} in {1}", reference.path, sectName));
                    }
                }
            }
        }

        private GUIDToCommentMap BuildCommentMap()
        {
            GUIDToCommentMap comments = new GUIDToCommentMap();

            // buildFiles are handled below
            // filerefs are handled below
            foreach (var e in groups.entries.Values)
                comments.Add(e.guid, e.name);
            foreach (var e in containerItems.entries.Values)
                comments.Add(e.guid, "PBXContainerItemProxy");
            foreach (var e in references.entries.Values)
                comments.Add(e.guid, e.path);
            foreach (var e in sources.entries.Values)
            {
                comments.Add(e.guid, "Sources");
                BuildCommentMapForBuildFiles(comments, e.files, "Sources");
            }
            foreach (var e in resources.entries.Values)
            {
                comments.Add(e.guid, "Resources");
                BuildCommentMapForBuildFiles(comments, e.files, "Resources");
            }
            foreach (var e in frameworks.entries.Values)
            {
                comments.Add(e.guid, "Frameworks");
                BuildCommentMapForBuildFiles(comments, e.files, "Frameworks");
            }
            foreach (var e in copyFiles.entries.Values)
            {
                string sectName = e.name;
                if (sectName == null)
                    sectName = "CopyFiles";
                comments.Add(e.guid, sectName);
                BuildCommentMapForBuildFiles(comments, e.files, sectName);
            }
            foreach (var e in shellScripts.entries.Values)
                comments.Add(e.guid, "ShellScript");
            foreach (var e in targetDependencies.entries.Values)
                comments.Add(e.guid, "PBXTargetDependency");
            foreach (var e in nativeTargets.entries.Values)
            {
                comments.Add(e.guid, e.name);
                comments.Add(e.buildConfigList, String.Format("Build configuration list for PBXNativeTarget \"{0}\"", e.name));
            }
            foreach (var e in variantGroups.entries.Values)
                comments.Add(e.guid, e.name);
            foreach (var e in buildConfigs.entries.Values)
                comments.Add(e.guid, e.name);
            foreach (var e in project.entries.Values)
            {
                comments.Add(e.guid, "Project object");
                comments.Add(e.buildConfigList, "Build configuration list for PBXProject \"Unity-iPhone\""); // FIXME: project name is hardcoded
            }
            foreach (var e in fileRefs.entries.Values)
                comments.Add(e.guid, e.name);
            if (m_RootElements.Contains("rootObject") && m_RootElements["rootObject"] is PBXElementString)
                comments.Add(m_RootElements["rootObject"].AsString(), "Project object");

            return comments;
        }

        public void ReadFromFile(string path)
        {
            ReadFromString(File.ReadAllText(path));
        }

        public void ReadFromString(string src)
        {
            TextReader sr = new StringReader(src);
            ReadFromStream(sr);
        }

        private static PBXElementDict ParseContent(string content)
        {
            TokenList tokens = Lexer.Tokenize(content);
            var parser = new Parser(tokens);
            TreeAST ast = parser.ParseTree();
            return Serializer.ParseTreeAST(ast, tokens, content);
        }

        public void ReadFromStream(TextReader sr)
        {
            Clear();
            m_RootElements = ParseContent(sr.ReadToEnd());
            
            if (!m_RootElements.Contains("objects"))
                throw new Exception("Invalid PBX project file: no objects element");
            
            var objects = m_RootElements["objects"].AsDict();
            m_RootElements.Remove("objects");
            m_RootElements.SetString("objects", "OBJMARKER");
            
            if (m_RootElements.Contains("objectVersion"))
            {
                m_ObjectVersion = m_RootElements["objectVersion"].AsString();
                m_RootElements.Remove("objectVersion");
            }

            var allGuids = new List<string>();
            string prevSectionName = null;
            foreach (var kv in objects.values)
            {
                allGuids.Add(kv.Key);
                var el = kv.Value;

                if (!(el is PBXElementDict) || !el.AsDict().Contains("isa"))
                {
                    m_UnknownObjects.values.Add(kv.Key, el);
                    continue;
                }
                var dict = el.AsDict();
                var sectionName = dict["isa"].AsString();
                
                if (m_Section.ContainsKey(sectionName))
                {
                    var section = m_Section[sectionName];
                    section.AddObject(kv.Key, dict);
                }
                else
                {
                    UnknownSection section;
                    if (m_UnknownSections.ContainsKey(sectionName))
                        section = m_UnknownSections[sectionName];
                    else
                    {
                        section = new UnknownSection(sectionName);
                        m_UnknownSections.Add(sectionName, section);
                    }
                    section.AddObject(kv.Key, dict);
                    
                    // update section order
                    if (!m_SectionOrder.Contains(sectionName))
                    {
                        int pos = 0;
                        if (prevSectionName != null)
                        {
                            // this never fails, because we already added any previous unknown sections
                            // to m_SectionOrder
                            pos = m_SectionOrder.FindIndex(x => x == prevSectionName);  
                            pos += 1;
                        }
                        m_SectionOrder.Insert(pos, sectionName);
                    }
                }
                prevSectionName = sectionName;
            }
            RepairStructure(allGuids);
            RefreshAuxMaps();
        }

        public void WriteToFile(string path)
        {
            File.WriteAllText(path, WriteToString());
        }

        public void WriteToStream(TextWriter sw)
        {
            sw.Write(WriteToString());
        }
 
        public string WriteToString()
        {
            var commentMap = BuildCommentMap();            
            var emptyChecker = new PropertyCommentChecker();
            var emptyCommentMap = new GUIDToCommentMap();
            
            // since we need to add custom comments, the serialization is much more complex
            StringBuilder objectsSb = new StringBuilder();
            if (m_ObjectVersion != null) // objectVersion comes right before objects
                objectsSb.AppendFormat("objectVersion = {0};\n\t", m_ObjectVersion);
            objectsSb.Append("objects = {");
            foreach (string sectionName in m_SectionOrder)
            {
                if (m_Section.ContainsKey(sectionName))
                    m_Section[sectionName].WriteSection(objectsSb, commentMap);
                else if (m_UnknownSections.ContainsKey(sectionName))
                    m_UnknownSections[sectionName].WriteSection(objectsSb, commentMap); 
            }
            foreach (var kv in m_UnknownObjects.values)
                Serializer.WriteDictKeyValue(objectsSb, kv.Key, kv.Value, 2, false, emptyChecker, emptyCommentMap);
            objectsSb.Append("\n\t};");
            
            StringBuilder contentSb = new StringBuilder();
            contentSb.AppendLine("// !$*UTF8*$!");
            Serializer.WriteDict(contentSb, m_RootElements, 0, false, 
                                 new PropertyCommentChecker(new string[]{"rootObject/*"}), commentMap);
            contentSb.AppendLine();
            string content = contentSb.ToString();
            
            content = content.Replace("objects = OBJMARKER;", objectsSb.ToString());
            return content;
        }
        
        // This method walks the project structure and removes invalid entries.
        void RepairStructure(List<string> allGuids)
        {
            var guidSet = new Dictionary<string, bool>(); // emulate HashSet on .Net 2.0
            foreach (var guid in allGuids)
                guidSet.Add(guid, false);
            
            while (RepairStructureImpl(guidSet) == false)
                ;
        }

        static void RepairStructureRemoveMissingGuids(PBX.GUIDList guidList, Dictionary<string, bool> allGuids)
        {
            List<string> guidsToRemove = null;
            foreach (var guid in guidList)
            {
                if (!allGuids.ContainsKey(guid))
                {
                    if (guidsToRemove == null)
                        guidsToRemove = new List<string>();
                    guidsToRemove.Add(guid);
                }
            }
            if (guidsToRemove != null)
            {
                foreach (var guid in guidsToRemove)
                    guidList.RemoveGUID(guid);
            }
        }
        
        static void RepairStructureAnyType<T>(KnownSectionBase<T> section, 
                                              Func<T, bool> checker,
                                              Dictionary<string, bool> allGuids, ref bool ok) where T : PBXObject, new()
        {
            List<string> guidsToRemove = null;
            foreach (var kv in section.entries)
            {
                if (!checker(kv.Value))
                {
                    if (guidsToRemove == null)
                        guidsToRemove = new List<string>();
                    guidsToRemove.Add(kv.Key);
                }
            }
            if (guidsToRemove != null)
            {
                ok = false;
                foreach (var guid in guidsToRemove)
                {
                    section.RemoveEntry(guid);
                    allGuids.Remove(guid);
                }
            }
        }
        
        static void RepairStructureGuidList<T>(KnownSectionBase<T> section, 
                                               Func<T, PBX.GUIDList> listRetrieveFunc,
                                               Dictionary<string, bool> allGuids, ref bool ok) where T : PBXObject, new()
        {
            Func<T, bool> checker = (T obj) =>
            {
                var list = listRetrieveFunc(obj);
                if (list == null)
                    return false;
                RepairStructureRemoveMissingGuids(list, allGuids);
                return true;
            };
            RepairStructureAnyType(section, checker, allGuids, ref ok);
        }

        // Returns true if repair was successful
        bool RepairStructureImpl(Dictionary<string, bool> allGuids)
        {
            bool ok = true;
            
            // PBXBuildFile
            Func<PBXBuildFile, bool> buildFilesChecker = (PBXBuildFile obj) =>
            {
                if (obj.fileRef == null || !allGuids.ContainsKey(obj.fileRef))
                    return false;
                return true;
            };
            RepairStructureAnyType(buildFiles, buildFilesChecker, allGuids, ref ok);
            // PBXFileReference / fileRefs not cleaned
            
            // PBXGroup
            RepairStructureGuidList(groups, o => o.children, allGuids, ref ok);
            
            // PBXContainerItem / containerItems not cleaned
            // PBXReferenceProxy / references not cleaned
            
            // PBXSourcesBuildPhase
            RepairStructureGuidList(sources, o => o.files, allGuids, ref ok); 
            // PBXFrameworksBuildPhase
            RepairStructureGuidList(frameworks, o => o.files, allGuids, ref ok); 
            // PBXResourcesBuildPhase
            RepairStructureGuidList(resources, o => o.files, allGuids, ref ok); 
            // PBXCopyFilesBuildPhase
            RepairStructureGuidList(copyFiles, o => o.files, allGuids, ref ok); 
            // PBXShellScriptsBuildPhase
            RepairStructureGuidList(shellScripts, o => o.files, allGuids, ref ok); 
 
            // PBXNativeTarget
            RepairStructureGuidList(nativeTargets, o => o.phases, allGuids, ref ok);

            // PBXTargetDependency / targetDependencies not cleaned
            
            // PBXVariantGroup
            RepairStructureGuidList(variantGroups, o => o.children, allGuids, ref ok);
            
            // XCBuildConfiguration / buildConfigs not cleaned

            // XCConfigurationList
            RepairStructureGuidList(configs, o => o.buildConfigs, allGuids, ref ok);
            
            // PBXProject project not cleaned
            return ok;
        }
    }

} // namespace UnityEditor.iOS.Xcode
