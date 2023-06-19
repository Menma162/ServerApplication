using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System.Data;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "MainAdmin")]
    public class HouseController : Controller
    {
        public async Task<IActionResult> IndexAsync(string search)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultHouses = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl);
            List<House>? houses = resultHouses.Item as List<House>;
            if (resultHouses.StatusCode == 200 && houses != null)
            {
                if (search != null) houses = houses.Where(it => it.name.ToLower().Contains(search.ToLower())).ToList();
                return View("Index", houses);
            }
            return new NotFoundResult();
        }

        public async Task<ActionResult> CreateAsync()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("name")] House house)
        {
            if (ModelState.IsValid)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiRequests.PostToApiAsync(house, Urles.HouseUrl, token);
                if (result.StatusCode == 201) return RedirectToAction("Index");
                else if(result.StatusCode == 500 && result.Message != null)
                    ViewBag.Message = result.Message;
                else ViewBag.Message = "Невозможно отправить данные";
            }
            return View(house);
        }

        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id == null) return new NotFoundResult();

            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var result = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{id}");

            if (result.StatusCode != 200) return new NotFoundResult();

            House? house = result.Item as House;
            return View(house);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, name")] House house)
        {
            if (ModelState.IsValid)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiRequests.PutToApiAsync(house, Urles.HouseUrl + $"/{house.id}", token);
                if (result.StatusCode == 201)
                    return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null)
                    ViewBag.Message = result.Message;
                else ViewBag.Message = "Невозможно отправить данные";

            }
            return View(house);
        }

        public async Task<ActionResult> DeleteAsync(int? id)
        {
            if (id == null) return new NotFoundResult();

            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var result = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{id}");

            if (result.StatusCode != 200) return new NotFoundResult();

            House? house = result.Item as House;
            return View(house);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var result = await ApiRequests.DeleteToApiAsync(Urles.HouseUrl + $"/{id}", token);
            if (result.StatusCode == 204) return RedirectToAction("Index");
            if (result.StatusCode == 500 && result.Message != null) ViewBag.Message = result.Message;

            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{id}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House? house = resultHouse.Item as House;
            return View(house);

        }
    }
}