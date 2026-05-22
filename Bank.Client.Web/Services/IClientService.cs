using ClassLibrary.Models;
using ClassLibrary.Models.RDto;

namespace Bank.Client.Web.Services;

public interface IClientService
{
    Task<Message> PostAsync(ClientRequestDto request);
}