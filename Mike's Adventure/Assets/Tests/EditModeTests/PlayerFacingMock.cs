using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Characters.Player;

namespace Assets.Tests.EditModeTests
{
    public class PlayerFacingMock : IPlayerFacing
    {
        public bool IsFacingRight { get; set; } = true;
    }
}
