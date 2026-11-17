using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using MapsterMapper;

namespace DatabaseORM.Service;

public class GreetingService(
    IGreetingRepository greetingRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IGreetingService
{
    private readonly IGreetingRepository greetingRepository = greetingRepository;

    public async Task<List<GreetingResource>> GetAllGreetingAsync()
    {
        List<GreetingResource> result = [];
        try
        {
            List<Greeting> greetings = await greetingRepository.GetAllAsync();
            result = mapper.Map<List<Greeting>, List<GreetingResource>>(greetings);
        }
        catch (Exception ex)
        {
            logger.Error("GreetingService.cs GetAllGreetingAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> AddGreetingAsync(string url)
    {
        try
        {
            Greeting greeting = new()
            {
                GreetingId = 0,
                Url = url
            };
            _ = await greetingRepository.AddAsync(greeting);

            logger.Log("Greeting added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("GreetingService.cs AddGreetingAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> RemoveGreetingAsync(int id)
    {
        try
        {
            Greeting? greeting = await greetingRepository.FindByIdAsync(id);
            if (greeting != null)
            {
                _ = await greetingRepository.RemoveAsync(greeting);

                logger.Log($"Greeting with ID: {id} removed successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log($"Greeting with ID: {id} not found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("GreetingService.cs RemoveGreetingAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
