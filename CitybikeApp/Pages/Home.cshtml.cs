using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1591

namespace CitybikeApp.Pages
{
  public class HomeModel: PageModel
  {
    private readonly ILogger<HomeModel> _logger;

    public HomeModel(ILogger<HomeModel> logger)
    {
      _logger = logger;
    }

    public void OnGet()
    {

    }
  }
}