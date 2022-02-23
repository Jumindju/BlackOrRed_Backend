using Cosmonaut;
using Microsoft.Azure.Documents.Client;

namespace WebAPI.Helper.Cosmos;

public static class CosmosHelper
{
    public static CosmosStoreSettings GetCosmosStoreSettings(IConfiguration configuration)
    {
        var configSection = configuration.GetSection("Cosmos");
        
        var dbName = configSection.GetValue<string>("DbName");
        if (string.IsNullOrEmpty(dbName))
        {
            throw new ArgumentException("Cosmos db name not provided");
        }
        
        var uri = configSection.GetValue<string>("Uri");
        if (string.IsNullOrEmpty(uri))
        {
            throw new ArgumentException("Cosmos uri not provided");
        }
        
        var key = configSection.GetValue<string>("Key");
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Cosmos key not provided");
        }

        return new CosmosStoreSettings(dbName, uri, key);
    }
}