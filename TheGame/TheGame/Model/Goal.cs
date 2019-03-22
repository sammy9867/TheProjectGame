namespace TheGame.Model
{
    public class Goal
    {
        public int row { get; set; }
        public int column { get; set; }
        public bool isGoal { get; set; }

        //internal bool
        public bool isTaken(int col, int ro)
        {
            if (column == col && row == ro)
                return true;

            return false;
        }

    }
}