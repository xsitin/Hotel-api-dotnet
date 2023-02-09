using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

public class SimpleController:ApiControllerBase
{


    [HttpGet("/")]
    public ActionResult Index()
    {
        return new ContentResult(){Content = "hello!"};
    }

}
