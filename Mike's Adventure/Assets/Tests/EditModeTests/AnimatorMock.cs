using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;

namespace Assets.Tests.EditModeTests
{
    public class AnimatorMock : IAnimator
    {
        public string CurrentPlayingAnimationClip { get; private set; } = "<UNSET>";

        public void Play(string stateName)
        {
            CurrentPlayingAnimationClip = stateName;
        }
    }
}
