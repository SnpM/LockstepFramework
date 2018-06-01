
namespace Lockstep
{
	public interface ILockstepEventsHandler
	{
		ushort ListenInput { get;}
		void FirstInitialize();
		void Initialize();
		void EarlyInitialize();
		void LateInitialize();
		void Simulate();
		void LateSimulate();
		void Visualize();
		void LateVisualize();
		void GlobalExecute(Command com);
		void RawExecute(Command com);
		void GameStart();
		void Deactivate();
	}
}