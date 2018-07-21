using System;

namespace Infogare.Classes.Presenters
{
    public class TutorialElementPresenter
    {
        public Uri TutorialImage { get; private set; }
        public String Caption { get; private set; }

        public TutorialElementPresenter(Uri tutorialImage, string caption)
        {
            TutorialImage = tutorialImage;
            Caption = caption;
        }
    }
}
