using System.Threading.Tasks;
using biodisel.domain.Models;

namespace biodisel.domain.Interfaces
{
    public interface IReactorService
    {
        Task Fill(ResidueMessage residueMessage);
        bool IsResting { get; }
    }
}