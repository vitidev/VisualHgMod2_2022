namespace VisualHg
{
    public class DiffToolPreset
    {
        public string Name { get; private set; }

        public string Arguments { get; private set; }


        public DiffToolPreset(string name, string args)
        {
            Name = name;
            Arguments = args;
        }


        public static DiffToolPreset[] Presets { get; private set; }

        static DiffToolPreset()
        {
            Presets = new[]
            {
                new DiffToolPreset("Araxis Merge", "/wait /2 /title1:%NameA% /title2:%NameB%  %PathA% %PathB%"),
                new DiffToolPreset("Beyond Compare 3", "%PathA% %PathB%  /ro1 /title1=%NameA% /title2=%NameB%"),
                new DiffToolPreset("Compare It!", "%PathA% /=%NameA% /R1  %PathB% /=%NameB%"),
                new DiffToolPreset("Devart CodeCompare", "/t1=%NameA% /t2=%NameB%  %PathA% %PathB%"),
                new DiffToolPreset("DiffMerge", "%PathA% %PathB% /t1=%NameA% /t2=%NameB%"),
                new DiffToolPreset("Diffuse", "%PathA% -L %NameA%  %PathB% -L %NameB%"),
                new DiffToolPreset("Ellié Computing Merge", "%PathA% %PathB% --mode=diff2 --title1=%NameA% --title2=%NameB%"),
                new DiffToolPreset("ExamDiff", "%PathA% %PathB% /dn1:%NameA% /dn2:%NameB%"),
                new DiffToolPreset("Perforce P4Merge", "%PathA% %PathB%"),
                new DiffToolPreset("SlickEdit VSDiff", "%PathA% %PathB%"),
                new DiffToolPreset("TortoiseHg KDiff3", "%PathA% --fname %NameA%  %PathB% --fname %NameB%"),
                new DiffToolPreset("TortoiseSVN Merge", "/base:%PathA% /mine:%PathB%  /basename:%NameA% /minename:%NameB%"),
                new DiffToolPreset("WinMerge", "-e -x -u -wl -dl %NameA% -dr %NameB% %PathA% %PathB%"),
            };
        }
    }
}
