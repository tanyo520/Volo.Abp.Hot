using DemoPlugin2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DemoPlugin2.Controllers
{
    [Area("DemoPlugin2")]
    public class Plugin1Controller : Controller
    {

        public Plugin1Controller()
        {
        }

        [HttpGet]
        public IActionResult HelloWorld()
        {
            TestClass testClass = new TestClass
            {
                Message = "Hello World"
            };

            ViewBag.Books = new List<BookViewModel>();

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return Content("OK");
        }
        
    }

    public interface IHandler
    {
        string Work();
    }

    public class MyHandler : IHandler
    {
        public string Work()
        {
            return "My Handler Work";
        }
    }

    public class LoadHelloWorldEvent
    {
        public string Str { get; set; }
    }
}
