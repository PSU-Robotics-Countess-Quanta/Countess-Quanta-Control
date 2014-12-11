using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountessQuantaControl.DecisionActionEngine.Interfaces
{
    public interface IAction
    {
        int ActionID { get; set; }
        decimal TimeOffset { get; set; }
        bool IsParallel { get; set; }

        void ExecuteAction();


    }
}
