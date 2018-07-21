using System; 
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Infoécran.AppStates;
using Infoécran.Classes.Presenters;
using Infoécran.Helpers;

namespace Infoécran.Classes
{
    public static class AppStateManager
    {
        public static async Task SecondaryTileOnNavigatedTo(string e, Frame rootFrame)
        {
            var stack = e.Split('_');
            var line = stack[1];
            var logo = stack[2];
            var trigramme = stack[3] ?? null;
            var infogare = new InfoGare();

            await infogare.Init();

            infogare = await new Mission().GetBoard(trigramme, line);
            var gsp = new GareSuggestionPresenter(MissionHelper.DetermineLineType(line), line, trigramme);
            if (!rootFrame.Navigate(typeof(InfoScreenState), new Tuple<InfoGare, string, GareSuggestionPresenter>(infogare, line, gsp)))
            {
                throw new Exception("Failed to acess SecondaryTile");
            }
        }
    }
}
