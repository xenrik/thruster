using System.Data.Common;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;

public class ScriptProfiler : IScriptProfiler {

    private static bool enabled = Application.isEditor;

    private ProfileData rootEntry;
    private ProfileData currentEntry;

    private static IScriptProfiler profiler;

    private ScriptProfiler() {
        rootEntry = new ProfileData();
        currentEntry = rootEntry;
    }

    public static IScriptProfiler GetInstance() {
        if (profiler == null) {
            if (!enabled) {
                profiler = new DummyScriptProfiler();
            } else {
                profiler = new ScriptProfiler();
            }
        }

        return profiler;
    }

    public void StartMethod([CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "") {
        string typeName = GetTypeName(callerFilePath);
        ProfileData data = new ProfileData(typeName, callerMember);
        data.StopWatch = System.Diagnostics.Stopwatch.StartNew();

        currentEntry.Children.Add(data);
        data.ParentEntry = currentEntry;

        currentEntry = data;
    }

    public void EndMethod() {
        currentEntry.Duration += currentEntry.StopWatch.ElapsedMilliseconds;
        currentEntry.StopWatch.Stop();
        currentEntry.StopWatch = null;

        currentEntry = currentEntry.ParentEntry;
    }

    public void StartGroup(string groupName) {
        ProfileData data = new ProfileData(groupName);
        data.StopWatch = System.Diagnostics.Stopwatch.StartNew();

        currentEntry.Children.Add(data);
        data.ParentEntry = currentEntry;

        currentEntry = data;    
    }

    public void EndGroup() {
        currentEntry.Duration += currentEntry.StopWatch.ElapsedMilliseconds;
        currentEntry.StopWatch.Stop();
        currentEntry.StopWatch = null;

        currentEntry = currentEntry.ParentEntry;
    }

    public void Report() {
        Report(rootEntry, 0);
    }

    private void Report(ProfileData entry, int depth) {
        string indent = new String(' ', depth * 3);
        foreach (ProfileData child in entry.Children) {
            string line = indent;
            if (child.Group.Length > 0) {
                line += child.Group;
            } else {
                line += child.TypeName + "#" + child.MethodName;
            }

            line = line.PadRight(50) + child.Duration.ToString().PadLeft(8);
            Debug.Log(line);

            Report(child, depth + 1);
        }
    }

    public void Reset() {
        rootEntry = new ProfileData();
        currentEntry = rootEntry;
    }

    private static string GetTypeName(string filePath) {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    private struct ProfileKey : IComparable<ProfileKey> {
        public string Group;
        public string TypeName;
        public string MethodName;

        public ProfileKey(string group, string typeName, string methodName) {
            this.Group = group;
            this.TypeName = typeName;
            this.MethodName = methodName;
        }

        public int CompareTo(ProfileKey other) {
            int result = Group.CompareTo(other.Group);
            if (result != 0) {
                return result;
            }

            result = TypeName.CompareTo(other.TypeName);
            if (result != 0) {
                return result;
            }

            return TypeName.CompareTo(other.MethodName);
        }

        public override int GetHashCode() {
            int result = Group.GetHashCode();
            result = result * 31 + TypeName.GetHashCode();
            result = result * 31 + MethodName.GetHashCode();

            return result;
        }

        public override string ToString() {
            return (Group.Length > 0 ? Group + "." : "") +
                TypeName + "#" + MethodName;
        }
    }

    private class ProfileData {
        public ProfileData ParentEntry;
        public List<ProfileData> Children;

        public string Group;
        public string TypeName;
        public string MethodName;

        public long Duration;
        public long Calls;

        public System.Diagnostics.Stopwatch StopWatch;

        public ProfileData() {
            Children = new List<ProfileData>();

            this.Group = "";
            this.TypeName = "";
            this.MethodName = "";

            this.Duration = 0;
            this.Calls = 0;

            this.StopWatch = null;
        }

        public ProfileData(string group) : this() {
            this.Group = group;
        }
        
        public ProfileData(string typeName, string methodName) : this() {
            this.TypeName = typeName;
            this.MethodName = methodName;
        }
    }
}