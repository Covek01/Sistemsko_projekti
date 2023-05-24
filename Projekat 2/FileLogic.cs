namespace Projekat_2
{
    public class FileLogic
    {
        public List<FileLocked> LockList {get; set;}
        public FileLogic()
        {
            this.LockList = new List<FileLocked>();
        }

        public static bool LockListContainsFileName(List<FileLocked> list, string fileName)
        {

            foreach(FileLocked file in list)
            {
                if (file.Name == fileName)
                {
                    return true;
                }
            }

            return false;
        }

        public static FileLocked FindFirstOccurence(List<FileLocked> list, string fileName)
        {
            foreach(FileLocked file in list)
            {
                if (file.Name == fileName)
                {
                    return file;
                }
            }
            return null;
        }


        public void add(string fileName)
        {
           

            if (!FileLogic.LockListContainsFileName(this.LockList, fileName))
            {
                LockList.Add(new FileLocked(fileName));
            }
        }

        public void remove(string fileName)
        {
            LockList.Remove(FileLogic.FindFirstOccurence(this.LockList, fileName));
        }

        public FileLocked getElement(string fileName)
        {
            return FileLogic.FindFirstOccurence(this.LockList, fileName);
        }

        
    }
}