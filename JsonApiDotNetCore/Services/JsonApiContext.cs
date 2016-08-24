using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiContext
  {
    public HttpContext HttpContext { get; set; }
    public Route Route { get; set; }

    private readonly object _dbContext;

    public JsonApiContext(HttpContext httpContext, Route route, object dbContext)
    {
      HttpContext = httpContext;
      Route = route;
      _dbContext = dbContext;
    }

    public List<object> Get()
    {
      return (GetDbSetFromContext(Route.RouteDefinition.ContextPropertyName) as IEnumerable<object>)?.ToList();
    }

    public object Get(string id)
    {
      // HACK: I _believe_ by casting to IEnumerable, we are loading all records into memory there has to be a better way...
      //        Also, we are making a BIG assumption that the resource has an attribute Id and not ResourceId which is allowed by EF
      return (GetDbSetFromContext(Route.RouteDefinition.ContextPropertyName) as IEnumerable<dynamic>)?.FirstOrDefault(x => x.Id.ToString() == id);
    }

    private object GetDbSetFromContext(string propName)
    {
      return _dbContext.GetType().GetProperties().FirstOrDefault(pI => pI.Name == propName)?.GetValue(_dbContext, null);
    }
  }
}