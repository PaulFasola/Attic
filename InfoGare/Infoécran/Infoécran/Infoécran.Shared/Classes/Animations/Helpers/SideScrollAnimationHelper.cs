using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media.Animation;

namespace Infoécran.Classes.Animations.Helpers
{
    public static class SideScrollAnimationHelper
    {
        public static bool IsRunning(this SideScrollAnimation animation)
        {
            return (animation.Running);
        }

        public static void Start(this SideScrollAnimation animation)
        { 
            animation.AttachedStoryboard.Begin();
            animation.AttachedStoryboard.Completed += delegate { animation.Running = false; };
        }
        public static void Stop(this SideScrollAnimation animation)
        {
            animation.AttachedStoryboard.Stop();
            animation.Running = false;
        }

        public static void Attach(this SideScrollAnimation animation, Storyboard sb)
        {
            if (!animation.Running) animation.AttachedStoryboard = sb;
        } 
    } 
}
