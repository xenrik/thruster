using System.Data.Common;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;
using System.Text;

public class DefaultScriptProfiler : IScriptProfiler {

    private ProfileData rootEntry;
    private ProfileData currentEntry;

    public DefaultScriptProfiler() {
        rootEntry = new ProfileData();
        rootEntry.Group = "root";

        currentEntry = rootEntry;
    }

    public void StartMethod([CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLineNo = 0) {
        string typeName = GetTypeName(callerFilePath);
        ProfileData data = new ProfileData(typeName, callerMember, callerLineNo);
        data.StopWatch = System.Diagnostics.Stopwatch.StartNew();
        data.ParentEntry = currentEntry;

        currentEntry.Children.Add(data);
        currentEntry = data;
    }

    public void EndMethod() {
        currentEntry.Ticks += currentEntry.StopWatch.ElapsedTicks;
        currentEntry.StopWatch.Stop();
        currentEntry.StopWatch = null;

        currentEntry = currentEntry.ParentEntry;
    }

    public void StartGroup(string groupName, [CallerFilePath]string callerFilePath = null, [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLineNo = 0) {
        string typeName = GetTypeName(callerFilePath);
        ProfileData data = new ProfileData(groupName, typeName, callerMember, callerLineNo);
        data.StopWatch = System.Diagnostics.Stopwatch.StartNew();
        data.ParentEntry = currentEntry;

        currentEntry.Children.Add(data);
        currentEntry = data;    
    }

    public void EndGroup() {
        currentEntry.Ticks += currentEntry.StopWatch.ElapsedTicks;
        currentEntry.StopWatch.Stop();
        currentEntry.StopWatch = null;

        currentEntry = currentEntry.ParentEntry;
    }

    public void Report(ReportMode mode = ReportMode.Tree) {
        switch (mode) {
        case ReportMode.Tree:
            ReportTree(rootEntry);
            break;

        case ReportMode.Method:
            ReportMethods();
            break;
        }
    }

    private void ReportTree(ProfileData entry, int depth = 0, StringBuilder buffer = null) {
        if (buffer == null) {
            buffer = new StringBuilder();
        }

        string indent = new String(' ', depth * 3);
        foreach (ProfileData child in entry.Children) {
            string line = indent;
            if (child.Group.Length > 0) {
                line += child.Group;
            } else {
                line += child.TypeName + "#" + child.MethodName;
            }

            line = line.PadRight(50) + child.DurationMS.ToString().PadLeft(8);
            buffer.AppendLine(line);

            ReportTree(child, depth + 1, buffer);
        }

        if (depth == 0) {
            Debug.Log(buffer.ToString());
        }
    }

    private void ReportMethods() {
        Dictionary<String,ProfileData> groupedData = new Dictionary<String, ProfileData>();
        Stack<ProfileData> entries = new Stack<ProfileData>();
        entries.Push(rootEntry);

        // Group by Method Name
        while (entries.Count > 0) {
            ProfileData data = entries.Pop();
            foreach (ProfileData childData in data.Children) {
                entries.Push(childData);
            }

            if (data.TypeName.Length == 0) {
                continue;
            }

            // Ignore Group for first collation
            String key = data.TypeName + "#" + data.MethodName;
            ProfileData existingData;
            if (!groupedData.TryGetValue(key, out existingData)) {
                existingData = new ProfileData(data.TypeName, data.MethodName, data.Group.Length == 0 ? data.LineNo : 0);
                groupedData[key] = existingData;
            }

            existingData.Calls++;
            existingData.Ticks += data.Ticks;

            // Now include group
            if (data.Group.Length == 0) {
                continue;
            }

            key = data.TypeName + "#" + data.MethodName + "|" + data.Group;
            if (!groupedData.TryGetValue(key, out existingData)) {
                existingData = new ProfileData(data.Group, data.TypeName, data.MethodName, 0);
                groupedData[key] = existingData;
            }

            existingData.Calls++;
            existingData.Ticks += data.Ticks;
        }

        // Sort and Report
        StringBuilder buffer = new StringBuilder();
        var sortedData = groupedData.Values.OrderBy(data => data.Ticks);
        foreach (ProfileData data in groupedData.Values) {
            String line = data.TypeName + "#"+ data.MethodName;
            if (data.Group.Length > 0) {
                line += " (" + data.Group + ")";
            } else {
                line += "@" + data.LineNo;
            }

            line = line.PadRight(50);
            line += "Calls: " + data.Calls.ToString().PadLeft(5);
            line += " Total Duration: " + data.DurationMS.ToString().PadLeft(8) + "ms";
            line += " Avg Duration: " + (data.DurationMS / data.Calls).ToString().PadLeft(8) + "ms/call";

            buffer.AppendLine(line);
        }
        Debug.Log(buffer.ToString());
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
        public int LineNo;

        public long Ticks;
        public long Calls;

        public System.Diagnostics.Stopwatch StopWatch;

        public long DurationNS {
            get {
                return Ticks * 100;
            }
        }
        public long DurationMS {
            get {
                return DurationNS / 1000000;
            }
        }

        public ProfileData() {
            Children = new List<ProfileData>();

            this.Group = "";
            this.TypeName = "";
            this.MethodName = "";
            this.LineNo = 0;

            this.Ticks = 0;
            this.Calls = 0;

            this.StopWatch = null;
        }

        public ProfileData(string group, string typeName, string methodName, int lineNo) : this() {
            this.Group = group;
            this.TypeName = typeName;
            this.MethodName = methodName;
            this.LineNo = lineNo;
        }
        
        public ProfileData(string typeName, string methodName, int lineNo) : this() {
            this.TypeName = typeName;
            this.MethodName = methodName;
            this.LineNo = lineNo;
        }

        public ProfileKey GetKey() {
            return new ProfileKey(Group, TypeName, MethodName);
        }
    }
}