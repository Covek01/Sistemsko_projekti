namespace Projekat_2
{
    public class FileLocked
    {
        public ReaderWriterLockSlim Locker;
        public string Name{get; set;}

        public FileLocked(string name)
        {
            this.Name = name;
            Locker = new ReaderWriterLockSlim();
        }

        public static bool operator ==(FileLocked file1, FileLocked file2)
        {
            return file1.Name == file2.Name;
        }

        public static bool operator !=(FileLocked file1, FileLocked file2)
        {
            return file1.Name != file2.Name;
        }
    }
}