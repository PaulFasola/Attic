using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;

namespace Infoécran.Classes
{
    public class RateReminder
    {
        public int NextRemindCount { get; set; }

        public bool ShouldRemind()
        {
            return NextRemindCount == 0;
        }

        public void DecreaseCountTime()
        {
            if (NextRemindCount <= 0) return;
            NextRemindCount--;
        }

        private  void StartReminder()
        {
            //var response = await new MessageDialog("");
        }

    }
}
