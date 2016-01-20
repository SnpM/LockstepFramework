using UnityEngine;
using System.Collections;
namespace Lockstep
{
    public interface IBehaviourHelper
    {
        void Initialize();
        void LateInitialize();
        void GameStart();

        void Simulate();
        void LateSimulate();

        void Visualize();
        void Deactivate();

        bool CanExecuteOnCommand(Command com);
        void Execute(Command com);
        void RawExecute(Command com);

        int Priority { get; set; }
    }
}
