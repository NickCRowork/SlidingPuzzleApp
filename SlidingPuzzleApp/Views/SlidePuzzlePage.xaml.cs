using SlidingPuzzleApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SlidingPuzzleApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SlidePuzzlePage : ContentPage
    {
        public SlidePuzzlePage()
        {
            InitializeComponent();
            this.BindingContext = new SlidePuzzleViewModel();
        }
    }
}