namespace TEiNRandomizer
{
    public class Level
    {
        public string Name;
        public string TSDefault;
        public string TSNeed;
        public string Art;
        public string Folder;

        public bool HasNPC;

        public Connections MapConnections;
        public Connections Entrances;
        public Connections Exits;
        public Connections Secrets;

        public string InFile { get => $"{Folder}/{Name}.lvl"; }
        public string OutFile;
    }
} 