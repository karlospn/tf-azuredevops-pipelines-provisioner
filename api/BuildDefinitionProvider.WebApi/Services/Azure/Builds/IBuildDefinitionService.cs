using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public interface IBuildDefinitionService
    {
        Task<CustomBuildDefinitionPayload> GetAsync(
            string id);

        Task<string> PostAsync(
            CustomBuildDefinitionPayload payload);


        Task PutAsync(
            string id,
            CustomBuildDefinitionPayload payload);


        Task DeleteAsync(
            string id);
    }
}