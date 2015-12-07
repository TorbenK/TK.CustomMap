using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api
{
    public interface IPlaceResult
    {
        string Description { get; set; }
        string PlaceId { get; set; }
    }
}
