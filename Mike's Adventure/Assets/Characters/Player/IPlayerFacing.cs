using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Characters.Player
{
    public interface IPlayerFacing
    {
        public bool IsFacingRight { get; set; }
    }
}
