using hwMarch11.Data;
using hwMarch11.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace hwMarch11.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=ShareImage; Integrated Security=true;";
        private static List<int> _ids = new();

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upload(IFormFile imageFile, string password)
        {
            var repo = new ImageRepository(_connectionString);
            var fileName = $"{Guid.NewGuid()}-{imageFile.FileName}";
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            using FileStream fs = new FileStream(imagePath, FileMode.Create);
            imageFile.CopyTo(fs);
            var image = new Image()
            {
                ImagePath = fileName,
                Password = password
            };

            repo.AddImage(image);
            var vm = new UploadViewModel()
            {
                Password = password,
                Link = $"https://localhost:7243/home/viewimage?id={image.Id}"
            };
            return View(vm);
        }

        public IActionResult ViewImage(int id)
        {

            if (TempData["message"] != null)
            {
                ViewBag.message = (string)TempData["message"];
            }

            _ids = HttpContext.Session.Get<List<int>>("id");
            var show = false;

            if(_ids != null)
            {
                show = _ids.Any(i => i == id);
            }
            else
            {
                _ids = new();
            }

            var repo = new ImageRepository(_connectionString);
            var image = repo.GetImageById(id);
            if (image == null)
            {
                return Redirect($"/");
            }
            repo.AddView(id);
            var vm = new ViewImageViewModel { Id = id, ShowImage = show, ImageName = image.ImagePath, Views = image.Views };
            return View(vm);
        }

        [HttpPost]
        public IActionResult ViewImage(int id, string password)
        {
            var repo = new ImageRepository(_connectionString);
            var image = repo.GetImageById(id);
            if(image == null)
            {
                return Redirect($"/");
            }    
            if (image.Password != password)
            {
                TempData["message"] = "Invalid Password!";

                return Redirect($"/home/viewimage?id={id}");
            }
            _ids.Add(id);
            HttpContext.Session.Set<List<int>>("id", _ids);
            repo.AddView(id);
            var vm = new ViewImageViewModel()
            {
                ShowImage = true,
                Id = id,
                ImageName = image.ImagePath,
                Views = image.Views
            };
            return View(vm);
        }
    }
}
