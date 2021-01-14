using System.Threading.Tasks;

namespace Gtt.CodeWorks.StateMachines
{
    public interface IStateRepository
    {
        Task<long> StoreStateData(StateDto data, long currentSequenceNumber);
        Task<StateDto> RetrieveStateData(string identifier, string machineName);
    }
}