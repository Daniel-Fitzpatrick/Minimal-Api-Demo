using System.Data;

namespace Minimal.Api.Common.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnection();
}
