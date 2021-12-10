using SlidingPuzzleApp.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SlidingPuzzleApp.ViewModels
{
    class MainPageViewModel : BaseViewModel
    {
        public Command OpenSlidePuzzle { get; }
        public MainPageViewModel()
        {
            OpenSlidePuzzle = new Command(GotoSlidePuzzle);
        }
        private async void GotoSlidePuzzle()
        {
            var navpage = new NavigationPage(new SlidePuzzlePage());
            await Application.Current.MainPage.Navigation.PushModalAsync(navpage);
        }
    }
}
