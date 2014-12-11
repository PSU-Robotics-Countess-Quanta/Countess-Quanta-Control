using CountessQuantaControl.DecisionActionEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountessQuantaControl.DecisionActionEngine.Implementations
{
    public class SpeechActionObj:IAction
    {
       public SpeechActionObj(int actionID, List<SpeechAction>phraseList )
        {
            ActionID = actionID;
            PhraseList = phraseList;
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

        private List<SpeechAction> PhraseList{get;set;}
        public void ExecuteAction()
        {
            throw new NotImplementedException();
        }
    }
}
