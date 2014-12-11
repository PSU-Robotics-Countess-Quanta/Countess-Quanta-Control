using CountessQuantaControl.DecisionActionEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountessQuantaControl.DecisionActionEngine.Implementations
{
    public class ScreenActionObj:IAction
    {
        public ScreenActionObj(int actionID, List<ScreenAction> buttonList )
        {
            ButtonList = buttonList;
        }
        public int ActionID
        {
            get;
            set;
        }

        public decimal TimeOffset
        {
            get;
            set;
        }

        public bool IsParallel
        {
            get;
            set;
        }

        private List<ScreenAction> ButtonList;
        public void ExecuteAction()
        {
            throw new NotImplementedException();
        }
    }
}
