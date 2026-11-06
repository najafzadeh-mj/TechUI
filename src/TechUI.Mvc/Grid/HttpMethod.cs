using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// HTTP method used by grid datasource requests.
    /// </summary>
    [Obsolete("Use TechUI.Mvc.Infrastructure.TechHttpMethod instead.")]
    public enum HttpMethod
    {
        Get,
        Post
    }
}
