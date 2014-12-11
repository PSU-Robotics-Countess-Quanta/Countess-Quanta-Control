using CountessQuantaControl.DecisionActionEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountessQuantaControl.DecisionActionEngine.Implementations
{
    public class ParentActionObj: IAction
    {
       
        public ParentActionObj(List<IAction> childActionList)
        {
            ChildActionList = childActionList;
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

        private List<IAction> ChildActionList { get; set; }

        public void ExecuteAction()
        {
            //Execute action here
            throw new NotImplementedException();
        }
    }
}
