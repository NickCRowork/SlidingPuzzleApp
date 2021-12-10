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
        public Command SendGoBack { get; }
        public Command SwapImage { get; }
        public Command SendSolveRecursive { get; }

        private string currentEmpty;
        private List<string> swappable;
        private string[] answer;
        private int movesTaken = 0;

        public SlidePuzzleViewModel()
        {
            SendGoBack = new Command(GoBack);
            SwapImage = new Command<string>(PerformImageSwap);
            SendSolveRecursive = new Command(CountRecursiveStepsToSolve);
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
                if (score < previousScore)
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
                score += (difference / width) + (difference % width);
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
        /// Returns the indices next to current index from a board with the given width.
        /// </summary>
        /// <param name="currentIndex">The space to return indices next to.</param>
        /// <param name="width">The width of the board. Used to calculate vertical indices.</param>
        /// <param name="maxIndex">The max index of the board.</param>
        /// <returns>An array of indices next to currentIndex.</returns>
        private int[] GetSwappableIndices(int currentIndex, int width, int maxIndex)
        {
            List<int> indices = new List<int>();
            if (currentIndex - 1 >= 0)
            {
                indices.Add(currentIndex - 1);
            }
            if (currentIndex + 1 <= maxIndex)
            {
                indices.Add(currentIndex + 1);
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
