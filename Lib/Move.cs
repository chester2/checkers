namespace Lib
{
    public struct Move
    {
        public ulong From { get; }
        public ulong To { get; }
        public ulong Capture { get; }

        public Move(ulong from, ulong to, ulong capture)
        {
            this.From = from;
            this.To = to;
            this.Capture = capture;
        }

        public Move(ulong from, ulong to) : this(from, to, 0) { }
    }
}
