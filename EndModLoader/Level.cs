namespace TEiNRandomizer
{
    public class Level
    {
        public string Name;
        public string TSDefault;
        public string TSNeed;
        public string Art;
        public string Folder;
        public bool HasSecret;
        public bool CanReverse;
        public Connections connections;

        public string fileName { get => $"data/tilemaps/{Folder}/{Name}.lvl"; }
        //public LevelFile File;
    }
}