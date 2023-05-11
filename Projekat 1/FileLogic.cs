namespace Projekat_1
{
    public class FileLogic
    {
        public List<string> LockList {get; set;}
        public FileLogic()
        {
            this.LockList = new List<string>();
        }

        public void add(string fileName)
        {
            if (!LockList.Contains(fileName))
            {
                LockList.Add(fileName);
            }
        }

        public void remove(string fileName)
        {
            LockList.Remove(fileName);
        }

        public string getElement(string fileName)
        {
            if (LockList.Contains(fileName))
            {
                return LockList[LockList.IndexOf(fileName)];
            }
            else{
                return null;
            }
        }

        public bool contains(string fileName)
        {
            return LockList.Contains(fileName);
        }
    }
}