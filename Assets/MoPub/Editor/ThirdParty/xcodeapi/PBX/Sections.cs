// Base classes for section handling

namespace MoPubInternal.Editor.ThirdParty.xcodeapi.PBX
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.IO;

    // common base
    internal abstract class SectionBase
    {
        public abstract void AddObject(string key, PBXElementDict value);
        public abstract void WriteSection(StringBuilder sb, GUIDToCommentMap comments);
    }

    // known section: contains objects that we care about
    internal class KnownSectionBase<T> : SectionBase where T : PBXObject, new()
    {
        public SortedDictionary<string, T> entries = new SortedDictionary<string, T>();

        private string m_Name;

        public KnownSectionBase(string sectionName)
        {
            m_Name = sectionName;
        }

        public override void AddObject(string key, PBXElementDict value)
        {
            T obj = new T();
            obj.guid = key;
            obj.SetPropertiesWhenSerializing(value);
            obj.UpdateVars();
            entries[obj.guid] = obj;
        }

        public override void WriteSection(StringBuilder sb, GUIDToCommentMap comments)
        {
            if (entries.Count == 0)
                return;            // do not write empty sections

            sb.AppendFormat("\n\n/* Begin {0} section */", m_Name);
            foreach (T obj in entries.Values)
            {
                obj.UpdateProps();
                sb.AppendFormat("\n\t\t{0} = ", comments.Write(obj.guid));
                Serializer.WriteDict(sb, obj.GetPropertiesWhenSerializing(), 2, 
                                     obj.shouldCompact, obj.checker, comments);
                sb.Append(";");
            }
            sb.AppendFormat("\n/* End {0} section */", m_Name);
        }

        public T this[string guid]
        {
            get {
                if (entries.ContainsKey(guid))
                    return entries[guid];
                return null;
            }
        }

        public void AddEntry(T obj)
        {
            entries[obj.guid] = obj;
        }

        public void RemoveEntry(string guid)
        {
            if (entries.ContainsKey(guid))
                entries.Remove(guid);
        }
    }

    // we assume there is only one PBXProject entry
    internal class PBXProjectSection : KnownSectionBase<PBXProjectObject>
    {
        public PBXProjectSection() : base("PBXProject")
        {
        }

        public PBXProjectObject project
        {
            get {
                foreach (var kv in entries)
                    return kv.Value;
                return null;
            }
        }
    }

} // UnityEditor.iOS.Xcode
