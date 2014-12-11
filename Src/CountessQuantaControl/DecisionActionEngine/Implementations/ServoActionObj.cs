using CountessQuantaControl.DecisionActionEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountessQuantaControl.DecisionActionEngine.Implementations
{
   public class ServoActionObj : IAction
    {
      
       ServoActionObj(int actionID, decimal acceleration, decimal positionSec, decimal duration)
       {
           //Insert initialization code here
            ActionID = actionID;
           Acceleration = acceleration;
           PositionSec = positionSec;
           Duration = duration;
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

       private decimal Acceleration{get;set;}
       private decimal PositionSec{get;set;}
       private decimal Duration {get;set;}

        public void ExecuteAction()
        {
            //Insert code to execute action here
            throw new NotImplementedException();
        }

     
    }
}
