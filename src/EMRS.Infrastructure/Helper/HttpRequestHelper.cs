using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Helper;

public static class HttpRequestHelper
{
    public static Dictionary<string, string> GetQueryParams(IHttpContextAccessor accessor)
    {
        var query = accessor.HttpContext?.Request?.Query;
        if (query == null || !query.Any())
            return new Dictionary<string, string>();

        return query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
    }
}
