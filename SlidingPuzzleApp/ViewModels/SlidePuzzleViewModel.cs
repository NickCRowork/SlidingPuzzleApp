using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SlidingPuzzleApp.ViewModels
{
    class SlidePuzzleViewModel : BaseViewModel
    {
        private string imageA1;
        public string ImageA1 { get => imageA1; set => SetProperty(ref imageA1, value); }
        private string imageA2;
        public string ImageA2 { get => imageA2; set => SetProperty(ref imageA2, value); }
        private string imageA3;
        public string ImageA3 { get => imageA3; set => SetProperty(ref imageA3, value); }
        private string imageB1;
        public string ImageB1 { get => imageB1; set => SetProperty(ref imageB1, value); }
        private string imageB2;
        public string ImageB2 { get => imageB2; set => SetProperty(ref imageB2, value); }
        private string imageB3;
        public string ImageB3 { get => imageB3; set => SetProperty(ref imageB3, value); }
        private string imageC1;
        public string ImageC1 { get => imageC1; set => SetProperty(ref imageC1, value); }
        private string imageC2;
        public string ImageC2 { get => imageC2; set => SetProperty(ref imageC2, value); }
        private string imageC3;
        public string ImageC3 { get => imageC3; set => SetProperty(ref imageC3, value); }
        private string moveText;
        public string MoveText { get => moveText; set => SetProperty(ref moveText, value); }
        private string minimumSteps;
        public string MinimumSteps { get => minimumSteps; set => SetProperty(ref minimumSteps, value); }
        private string timeTaken;
        public string TimeTaken { get => timeTaken; set => SetProperty(ref timeTaken, value); }
        public Command SendGoBack { get; }
        public Command SwapImage { get; }
        public Command SendSolveRecursive { get; }

        private string currentEmpty;
        private List<string> swappable;
        private string[] answer;
        private int movesTaken = 0;
        private List<(int, long)> stepsToTime; 

        public SlidePuzzleViewModel()
        {
            SendGoBack = new Command(GoBack);
            SwapImage = new Command<string>(PerformImageSwap);
            SendSolveRecursive = new Command(CountBFSStepsToSolve);
            swappable = new List<string>();
            answer = new string[] { "blue.png", "brown.png", "pink.png", "green.png", "orange.png", "gray.png", "purple.png", "red.png", "yellow.png" };
            currentEmpty = "ImageA3";
            ImageA1 = "blue.png";
            ImageA2 = "brown.png";
            ImageA3 = "gray.png";
            ImageB1 = "green.png";
            ImageB2 = "orange.png";
            ImageB3 = "pink.png";
            ImageC1 = "purple.png";
            ImageC2 = "red.png";
            ImageC3 = "yellow.png";
            stepsToTime = new List<(int, long)>();

            SetSwappable();
            SetMoveText();
        }

        private void PerformImageSwap(string swapper)
        {
            Console.WriteLine(swapper);
            if (swappable.Contains(swapper))
            {
                swapper = "Image" + swapper;
                string oldValue = GetType().GetProperty(swapper).GetValue(this).ToString();
                GetType().GetProperty(swapper).SetValue(this, GetType().GetProperty(currentEmpty).GetValue(this));
                GetType().GetProperty(currentEmpty).SetValue(this, oldValue);
                currentEmpty = swapper;
                SetSwappable();
                movesTaken++;
                SetMoveText();
                CheckAnswer();
            }
        }

        private bool CheckAnswer()
        {
            if (Enumerable.SequenceEqual(new string[] { ImageA1, ImageA2, ImageA3, ImageB1, ImageB2, ImageB3, ImageC1, ImageC2, ImageC3 }, answer))
            {
                MoveText = "Solved!";
                swappable.Clear();
                return true;
            }
            return false;
        }

        private bool CheckAnswer(string[] board, string[] answer)
        {
            if (Enumerable.SequenceEqual(board, answer))
            {
                return true;
            }
            return false;
        }

        internal static bool CompareArray(int[] a, int[] b)
        {
            if(a.Length == b.Length)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if(a[i] != b[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private void SetSwappable()
        {
            swappable.Clear();
            char row = currentEmpty[5];
            char column = currentEmpty[6];
            if (row == 'A')
            {
                swappable.Add(((char)(row + 1)).ToString() + column.ToString());
            }
            else
            {
                swappable.Add(((char)(row + 1)).ToString() + column.ToString());
                swappable.Add(((char)(row - 1)).ToString() + column.ToString());
            }
            if (column == '1')
            {
                swappable.Add(row.ToString() + ((char)(column + 1)).ToString());
            }
            else
            {
                swappable.Add(row.ToString() + ((char)(column + 1)).ToString());
                swappable.Add(row.ToString() + ((char)(column - 1)).ToString());
            }
        }

        private void SetMoveText()
        {
            MoveText = "Moves taken: " + movesTaken;
        }

        private async void GoBack()
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        /*
         * In order to determine how to rank scores
         * Need to recursively solve the problem and count the minimum number of moves
         * Probably give a little leeway after that too
         * 
         * In order to recursively solve, no visual element is needed,
         * but instead a computational array that makes sense to the computer
         * Also needs to be delivered a list of moveable indices each round
         * 
         * Example array:   [0,  1,  2,
         *                   3,  4,  5,
         *                   6,  7,  8]
         *
         * Each round, the indices that can be moved around the blank index are:
         * +1 if not at max value
         * -1 if not at min value
         * +width if result not greater than max value
         * -width if result not less than max value
        */

        private void CountBFSStepsToSolve()
        {
            //Builds the board to work with
            string[] currentBoard = new string[] { ImageA1, ImageA2, ImageA3, ImageB1, ImageB2, ImageB3, ImageC1, ImageC2, ImageC3 };
            //Tells how wide the board is for vertical movement options
            int width = 3;
            //Indicates the value of the "blank" square that can be moved into
            string blank = "gray.png";
            int blankIndex = Array.FindIndex(currentBoard, s => s == blank);
            int[] intBoard;
            int[] intAnswer;
            ChangeBoardsToInt(currentBoard, answer, blank, out intBoard, out intAnswer);

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            StateNode solution = BreadthFirstSearch(intBoard, intAnswer, blankIndex, width);
            watch.Stop();
            if(solution != null)
            {
                MinimumSteps = "Minimum steps: " + solution.depth;
                TimeTaken = "Steps: " + solution.depth + " Time: " + watch.ElapsedMilliseconds + "ms";
                stepsToTime.Add((solution.depth, watch.ElapsedMilliseconds));
            }
            else
            {
                MinimumSteps = "Unsolvable";
            }
        }

        private void ChangeBoardsToInt(string[] currentBoard, string[] answer, string blank, out int[] intBoard, out int[] intanswer)
        {
            intBoard = new int[currentBoard.Length];
            intanswer = new int[answer.Length];
            for(int i = 0; i < answer.Length; i++)
            {
                int blankUsed = 0;
                string current = answer[i];
                int boardIndex = Array.FindIndex(currentBoard, s => s == current);
                if (current.Equals(blank))
                {
                    intanswer[i] = 0;
                    blankUsed -= 1;
                    intBoard[boardIndex] = 0;
                    continue;
                }
                intanswer[i] = i + 1 + blankUsed;
                intBoard[boardIndex] = i + 1 + blankUsed;
            }
        }

        /// <summary>
        /// Counts smallest number of steps that the sliding puzzle will take.
        /// Sends it to the property for minimum steps.
        /// </summary>
        private void CountRecursiveStepsToSolve()
        {
            //Builds the board to work with
            string[] currentBoard = new string[] { ImageA1, ImageA2, ImageA3, ImageB1, ImageB2, ImageB3, ImageC1, ImageC2, ImageC3 };
            //Tells how wide the board is for vertical movement options
            int width = 3;
            //Indicates the value of the "blank" square that can be moved into
            string blank = "gray.png";
            //Index of the "blank" square
            int currentIndex = Array.FindIndex(currentBoard, s => s == blank);
            //Indices that can be moved
            int[] swappableIndices = GetSwappableIndices(currentIndex, width, currentBoard.Length - 1);
            //Score of the board (how far from completion it is, with 0 == completed)
            int previousScore = ScoreAnswer(currentBoard, answer, width, blank);
            MinimumSteps = "Minimum steps: " + SwapRecursive(0, currentIndex, currentIndex, swappableIndices, width, currentBoard, answer, previousScore, blank);
        }

        /// <summary>
        /// Recursively solves the sliding puzzle for minimum number of moves.
        /// </summary>
        /// <param name="steps">Current steps taken to solve.</param>
        /// <param name="currentIndex">Index of the "blank" square that can be moved into.</param>
        /// <param name="previousIndex">Previous index of the "blank" square. Used to prevent moving back and forth.</param>
        /// <param name="swappableIndices">Indices that the "blank" square can be swapped with. Adjacent to the "blank" square.</param>
        /// <param name="width">Width of the sliding puzzle.</param>
        /// <param name="currentBoard">Current sliding puzzle to work with.</param>
        /// <param name="answer">The sliding puzzle in its finished form.</param>
        /// <param name="previousScore">Score previously earned by the board. Used to abandon paths that don't lead closer to completion.</param>
        /// <param name="blank">Value of the "blank" square that can be moved into.</param>
        /// <returns>Minimum number of steps needed to finish the sliding puzzle.</returns>
        private int SwapRecursive(int steps, int currentIndex, int previousIndex, int[] swappableIndices, int width, string[] currentBoard, string[] answer, int previousScore, string blank)
        {
            if (ScoreAnswer(currentBoard, answer, width, blank) == 0)
            {//Puzzle finished, so return number of steps taken
                return steps;
            }
            foreach (int index in swappableIndices)
            {//Check each square adjecent to the blank square
                if (index == previousIndex)
                {//Ignore the previous blank square space to prevent repeating the same move over and over
                    continue;
                }
                //Create a new board by swapping the blank square
                string[] newBoard = Swap(currentBoard, currentIndex, index);
                //Calculate the new indices that can be swapped with
                int[] newSwappableIndices = GetSwappableIndices(index, width, newBoard.Length - 1);
                //Score the new board for how close to complete it is
                int score = ScoreAnswer(newBoard, answer, width, blank);
                if (score <= previousScore)
                {//If closer to completion
                    //Solve recursively
                    int result = SwapRecursive(steps + 1, index, currentIndex, newSwappableIndices, width, newBoard, answer, score, blank);
                    if (result != -1)
                    {//If not a dead end
                        //Puzzle finished, so return number of steps
                        return result;
                    }
                }
            }
            return -1;//Hit if no progress could be made
        }

        private StateNode BreadthFirstSearch(int[] currentBoard, int[] answer, int emptyIndex, int width)
        {
            Queue<StateNode> frontier = new Queue<StateNode>();
            HashSet<StateNode> explored = new HashSet<StateNode>();

            StateNode currentNode = new StateNode(currentBoard, null, 0, 0, emptyIndex);

            if(CompareArray(currentNode.board, answer))//O(n)
            {
                return currentNode;
            }

            frontier.Enqueue(currentNode);//O(1)

            while(frontier.Count > 0)
            {
                currentNode = frontier.Dequeue();//O(1)
                bool unique = explored.Add(currentNode);//O(1)
                if (unique == false)
                {
                    continue;
                }    

                if (CompareArray(currentNode.board, answer))
                {//O(n)
                    return currentNode;
                }

                int[] swappableIndices = GetSwappableIndices(currentNode.EmptyIndex, width, currentBoard.Length - 1);//O(1)
                foreach (int index in swappableIndices)
                {//O(1)
                    StateNode successor = new StateNode(Swap(currentNode.board, currentNode.EmptyIndex, index), currentNode, currentNode.depth + 1, 1, index);//O(n)
                    frontier.Enqueue(successor);
                }
            }
            return null;
        }

        private class StateNode
        {
            internal int[] board;
            StateNode parent;
            internal int depth;
            internal int cost;
            int emptyIndex;
            int identifier;

            internal int TotalCost
            {
                get
                {
                    if(parent == null)
                    {
                        return cost;
                    }
                    else
                    {
                        return parent.TotalCost + cost;
                    }
                }
            }

            internal int EmptyIndex
            {
                get
                {
                    return emptyIndex;
                }
            }

            internal StateNode(int[] currentBoard, StateNode parent, int depth, int cost, int emptyIndex)
            {
                this.board = currentBoard;
                this.parent = parent;
                this.depth = depth;
                this.cost = cost;
                this.emptyIndex = emptyIndex;
                int offset = 3;
                for (int i = 0; i < currentBoard.Length; i++)
                {
                    identifier += i * (offset++) * currentBoard[i];
                }
            }

            public override bool Equals(object obj)
            {
                return CompareArray((obj as StateNode).board, board);
            }

            public override int GetHashCode()
            {
                return identifier;
            }
        }

        /// <summary>
        /// Scores how close to completion the board is. Score == 0 is a completed board.
        /// </summary>
        /// <param name="currentBoard">The board to score.</param>
        /// <param name="answer">The board in its completed form.</param>
        /// <param name="width">The width of the board.</param>
        /// <param name="blank">The value of the blank square.</param>
        /// <returns>How close to completion the board is, with closer to 0 == closer to complete.</returns>
        private int ScoreAnswer(string[] currentBoard, string[] answer, int width, string blank)
        {
            int score = 0;
            int blankIndex = Array.FindIndex(currentBoard, s => s == blank);
            for (int i = 0; i < currentBoard.Length; i++)
            {//For every space pm the board
                if (currentBoard[i] == blank)
                {//Ignore the blank space to prevent moving away from the solution by attempting to place it in the right place
                    continue;
                }
                //Take the difference between the square's current position and solved position
                int difference = Math.Abs(Array.FindIndex(answer, s => s == currentBoard[i]) - i);
                //Since the square can be moved vertically, it can be moved width indices at once
                //That means that it is the difference / width + remainder away from desired position
                difference = (difference / width) + (difference % width);
                int baseScore = (int)Math.Pow(difference, 2f);

                int blankDifference = Math.Abs(blankIndex - i);
                blankDifference = (blankDifference / width) + (blankDifference % width);
                blankDifference = blankDifference * difference;
                baseScore = (baseScore + blankDifference + 1) / 2;

                score += baseScore;
            }
            return score;
        }

        /// <summary>
        /// Swaps indices a and b on the given board and returns a new board with the result.
        /// </summary>
        /// <param name="currentBoard">The board to perform the swap on.</param>
        /// <param name="a">The first index to swap.</param>
        /// <param name="b">The second index to swap.</param>
        /// <returns>A new board with the indices swapped.</returns>
        private string[] Swap(string[] currentBoard, int a, int b)
        {
            string[] newBoard = new string[currentBoard.Length];
            Array.Copy(currentBoard, newBoard, currentBoard.Length);
            newBoard[a] = currentBoard[b];
            newBoard[b] = currentBoard[a];
            return newBoard;
        }

        /// <summary>
        /// Swaps indices a and b on the given board and returns a new board with the result.
        /// </summary>
        /// <param name="currentBoard">The board to perform the swap on.</param>
        /// <param name="a">The first index to swap.</param>
        /// <param name="b">The second index to swap.</param>
        /// <returns>A new board with the indices swapped.</returns>
        private int[] Swap(int[] currentBoard, int a, int b)
        {
            int[] newBoard = new int[currentBoard.Length];
            Array.Copy(currentBoard, newBoard, currentBoard.Length);
            newBoard[a] = currentBoard[b];
            newBoard[b] = currentBoard[a];
            return newBoard;
        }

        /// <summary>
        /// Returns the indices next to current index from a board with the given width.
        /// </summary>
        /// <param name="currentIndex">The space to return indices next to.</param>
        /// <param name="width">The width of the board. Used to calculate vertical indices.</param>
        /// <param name="maxIndex">The max index of the board.</param>
        /// <returns>An array of indices next to currentIndex.</returns>
        private int[] GetSwappableIndices(int currentIndex, int width, int maxIndex)
        {
            List<int> indices = new List<int>();
            if (currentIndex - 1 >= 0 && currentIndex % width != 0)
            {
                indices.Add(currentIndex - 1);
            }
            if (currentIndex + 1 <= maxIndex)
            {
                bool canAdd = true;
                for(int i = width - 1; i < maxIndex; i += width)
                {
                    if(currentIndex == i)
                    {
                        canAdd = false;
                    }
                }
                if(canAdd)
                {
                    indices.Add(currentIndex + 1);
                }
            }
            if (currentIndex - width >= 0)
            {
                indices.Add(currentIndex - width);
            }
            if (currentIndex + width <= maxIndex)
            {
                indices.Add(currentIndex + width);
            }
            return indices.ToArray();
        }
    }
}
