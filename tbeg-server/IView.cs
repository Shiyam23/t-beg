using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBeg
{

    //interface for event handling, based on delegeates
    public interface IView
    {
        event ViewHandler_MatrixName<IView> CheckMatrixName;
        event ViewHandler_Matrix<IView> InitandSaveMatrix;
        event ViewHandler_Matrix<IView> SaveToCSV;
        event ViewHandler_Matrix<IView> LoadFromCSV;
        event ViewHandler_Matrix<IView> InitandSaveGame;
        event ViewHandler_Game<IView> InitGame;
        event ViewHandler_Game<IView> CheckConsistenceOfGameGraph;// check if user did not change the graph view
        event ViewHandler_Game<IView> StepBack;
        event ViewEvent_GameStepUser<IView> SendStep;
        event ViewHandler_Game<IView> ExitGame;
        event ViewEvent_GameGraph<IView> ResetGraph;
        event ViewHandler_Validator<IView> GetValidator;
        event ViewHandler_Graph<IView> AddGraph;
        
        void SetController(IController controller);
      
    }


}
