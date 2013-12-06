using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HgLib
{
    public static class Hg
    {
        private const int ArgumentsLengthLimit = 20000; // Maximum command length including executable path is 32767

        public static string Version { get; private set; }

        static Hg()
        {
            Version = ProcessLauncher.RunHg("version", "").FirstOrDefault();
        }


        public static string CreateParentRevisionTempFile(string fileName, string root)
        {
            var tempFileName = HgPath.GetRandomTemporaryFileName();

            tempFileName = Path.ChangeExtension(tempFileName, Path.GetExtension(fileName));

            var command = String.Format("cat \"{0}\"  -o \"{1}\"", HgPath.StripRoot(fileName, root), tempFileName);
            Run(command, root);

            Debug.Assert(File.Exists(tempFileName));

            return tempFileName;
        }

        public static string GetParentRevision(string root)
        {
            var summary = Run("summary", root).FirstOrDefault();

            if (summary != null)
            {
                return summary.Split(' ').Skip(1).FirstOrDefault();
            }
            
            return "";
        }
        

        public static HgFileStatus ConvertToStatus(char status)
        {
            switch (status)
            {
                case 'M':
                    return HgFileStatus.Modified;
                case 'A':
                    return HgFileStatus.Added;
                case 'R':
                    return HgFileStatus.Removed;
                case 'C':
                    return HgFileStatus.Clean;
                case '!':
                    return HgFileStatus.Missing;
                case '?':
                    return HgFileStatus.NotTracked;
                case 'I':
                    return HgFileStatus.Ignored;
                case ' ':
                    return HgFileStatus.None;
            }

            throw new ArgumentException("Unexpected status char");
        }


        public static string GetCurrentBranchName(string root)
        {
            return Run("branch", root).FirstOrDefault() ?? "";
        }

        public static string GetRenamedFileOriginalName(string newFileName)
        {
            var originalName = "";

            var fileInfo = GetRawFileInfo(newFileName);
            var renames = GetRenames(fileInfo);

            renames.TryGetValue(newFileName, out originalName);

            return originalName;
        }

        
        public static HgFileInfo[] GetRootStatus(string directory)
        {
            if (String.IsNullOrEmpty(directory))
            {
                return new HgFileInfo[0];
            }

            var output = Run("status -m -a -r -d -c -C", directory);

            return DetectRenames(ParseStatusOutput(directory, output));
        }


        public static HgFileInfo[] AddFiles(string[] fileNames, HgFileStatus filter)
        {
            var filesToAdd = GetFilesToAdd(fileNames, filter);

            if (filesToAdd.Length == 0)
            {
                return new HgFileInfo[0];
            }

            return ProcessFilesAndGetStatus("add", filesToAdd);
        }

        public static HgFileInfo[] RemoveFiles(string[] fileNames)
        {
            return ProcessFilesAndGetStatus("remove", fileNames);
        }

        public static HgFileInfo[] RenameFiles(string[] fileNames, string[] newFileNames)
        {
            for (int i = 0; i < Math.Min(fileNames.Length, newFileNames.Length); i++)
            {
                var root = HgPath.FindRepositoryRoot(fileNames[i]);

                var oldName = HgPath.StripRoot(fileNames[i], root);
                var newName = HgPath.StripRoot(newFileNames[i], root);

                var option = StringComparer.InvariantCultureIgnoreCase.Equals(oldName, newName) ? null : " -A";
                
                Run(String.Format("rename {0} \"{1}\" \"{2}\"", option, oldName, newName), root);
            }

            return GetFileInfo(fileNames.Concat(newFileNames).ToArray());
        }


        public static HgFileInfo[] GetFileInfo(params string[] fileNames)
        {
            var rawFileInfo = GetRawFileInfo(fileNames);

            return DetectRenames(rawFileInfo);
        }

        
        private static HgFileInfo[] GetRawFileInfo(params string[] fileNames)
        {
            var files = new List<HgFileInfo>();

            foreach (var item in ProcessFiles("status -A", fileNames))
            {
                var root = item.Key;
                var output = item.Value;

                files.AddRange(ParseStatusOutput(root, output));
            }

            return files.ToArray();
        }

        private static HgFileInfo[] ProcessFilesAndGetStatus(string command, string[] fileNames)
        {
            ProcessFiles(command, fileNames);
         
            return GetFileInfo(fileNames);
        }
        
        private static Dictionary<string, string[]> ProcessFiles(string command, string[] fileNames)
        {
            var rootOutput = new Dictionary<string, string[]>();

            foreach (var rootGroup in fileNames.GroupBy(x => HgPath.FindRepositoryRoot(x)))
            {
                var root = rootGroup.Key;
                var output = new List<string>();

                foreach (var fileArgs in GetFileArguments(root, rootGroup))
                {
                    var commandOutput = Run(String.Concat(command, fileArgs), root);

                    output.AddRange(commandOutput);
                }

                rootOutput[root] = output.ToArray();
            }

            return rootOutput;
        }

        private static string[] GetFileArguments(string root, IEnumerable<string> fileNames)
        {
            var args = new List<string>();
            var sb = new StringBuilder();

            foreach (var fileName in fileNames.Where(x => !HgPath.IsDirectory(x)).Select(x => HgPath.StripRoot(x, root)))
            {
                if (sb.Length > ArgumentsLengthLimit - fileName.Length - 3)
                {
                    args.Add(sb.ToString());
                    sb.Length = 0;
                }
                
                sb.Append(' ').Append('"').Append(fileName).Append('"');
            }

            args.Add(sb.ToString());

            return args.ToArray();
        }


        private static string[] Run(string args, string workingDirectory)
        {
            return ProcessLauncher.RunHg(args, workingDirectory);
        }

                
        private static HgFileInfo[] ParseStatusOutput(string root, string[] output)
        {
            return output.Select(x => HgFileInfo.FromHgOutput(root, x)).ToArray();
        }

        private static HgFileInfo[] DetectRenames(HgFileInfo[] files)
        {
            var filteredFiles = files.Where(x => x.Status != HgFileStatus.None).ToArray();
            
            foreach (var item in GetRenames(files))
            {
                var fileName = item.Value;
                var newFileName = item.Key;

                var file = filteredFiles.FirstOrDefault(x => x.FullName == newFileName);

                if (file != null)
                {
                    file.OriginalFile = 
                        filteredFiles.FirstOrDefault(x => x.FullName == fileName) ??
                        GetRawFileInfo(fileName).FirstOrDefault();
                }
            }

            return ExcludeCaseSensitiveRenames(filteredFiles);
        }

        private static HgFileInfo[] ExcludeCaseSensitiveRenames(HgFileInfo[] files)
        {
            var dictionary = new Dictionary<string, HgFileInfo>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var file in files)
            {
                if (!dictionary.ContainsKey(file.FullName) || dictionary[file.FullName].Status != HgFileStatus.Renamed || file.Status != HgFileStatus.Removed)
                {
                    dictionary[file.FullName] = file;
                }
            }

            return dictionary.Values.ToArray();
        }

        private static Dictionary<string, string> GetRenames(HgFileInfo[] files)
        {
            var renames = new Dictionary<string, string>();
      
            var newFile = files.FirstOrDefault();

            foreach (var file in files)
            {
                if (newFile.Status == HgFileStatus.Added && file.Status == HgFileStatus.None)
                {
                    renames.Add(newFile.FullName, file.FullName);
                }

                newFile = file;
            }

            return renames;
        }


        private static string[] GetFilesToAdd(string[] fileNames, HgFileStatus filter)
        {
            var files = GetRawFileInfo(fileNames);

            return files
                .Where(x => x.StatusMatches(filter))
                .Select(x => x.FullName)
                .ToArray();
        }
    }
}