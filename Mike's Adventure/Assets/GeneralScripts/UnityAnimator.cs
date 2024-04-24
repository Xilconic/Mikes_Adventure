using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GeneralScripts
{
    public interface IAnimator
    {
        void Play(string stateName);
    }

    public class UnityAnimator : IAnimator
    {
        private readonly Animator _animator;
        private string _currentStateName;

        public UnityAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void Play(string stateName)
        {
            // Avoid repeatedly setting the same animation over-and-over again:
            if(_currentStateName != stateName ) 
            {
                _animator.Play(stateName);
                _currentStateName = stateName;
            }
        }
    }
}
