using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g
{
    public interface IDrawnEffect : IDrawnObject
    {
        bool IsFinished();
    }
}
