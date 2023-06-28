namespace Projekat_2
{
    public enum LockerState
    {
        Free,
        Reading,
        Writing,
        BothReadingAndWriting
    }

    public class FileRoll
    {
        private string Name {get; set;}
        public LockerState State {get; set;}
        public object ReadingLock{get; set;}
        public object WritingLock{get; set;}

        public FileRoll(string name = "")
        {
            this.Name = name;
            this.State = LockerState.Free;
            this.ReadingLock = new Object();
            this.WritingLock = new Object();
        }

        
    }
}