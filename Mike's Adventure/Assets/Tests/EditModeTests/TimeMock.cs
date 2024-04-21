using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;

namespace Assets.Tests.EditModeTests
{
    public class TimeMock : ITime
    {
        public float DeltaTime { get; set; } = 1.0f/60f;
    }
}
