using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin, FlatOwner")]
    public class AdvertisementController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            HttpContext context = HttpContext;
            var role = UserProperties.GetRole(ref context);
            var token = UserProperties.GetToken(ref context);

            if (role == "FlatOwner")
            {
                var idHouse = UserProperties.GetIdHouseFlatOwner(ref context);
                var resultAdvertisement = await ApiGetRequests.LoadListAdvertisementsFromAPIAsync(token, Urles.AdvertisementUrl + $"/house/{idHouse}");
                List<Advertisement>? advertisements = resultAdvertisement.Item as List<Advertisement>;
                if (resultAdvertisement.StatusCode == 200)
                {
                    var idFlatOwner = UserProperties.GetIdFlatOwner(ref context);
                    var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
                    var resultSettingsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{idHouse}");
                    var resultPayment = await ApiGetRequests.LoadListPaymentsFromAPIAsync(token, Urles.PaymentUrl + $"/flatowner/{idFlatOwner}");
                    var resultIndication = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + $"/flatowner/{idFlatOwner}");
                    var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + $"/flatowner/{idFlatOwner}");
                    if (resultService.StatusCode == 200 && resultSettingsService.StatusCode == 200 && resultPayment.StatusCode == 200
                        && resultIndication.StatusCode == 200 && resultCounter.StatusCode == 200)
                    {
                        var news = GetNews(advertisements, resultService.Item as List<Service>, resultSettingsService.Item as List<SettingsService>,
                            resultCounter.Item as List<Counter>, resultPayment.Item as List<Payment>, resultIndication.Item as List<Indication>);
                        return View("IndexForOwner", news);
                    }
                }
                else return new NotFoundResult();
            }
            if (role == "HouseAdmin")
            {
                var resultAdvertisement = await ApiGetRequests.LoadListAdvertisementsFromAPIAsync(token, Urles.AdvertisementUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
                List<Advertisement>? advertisements = resultAdvertisement.Item as List<Advertisement>;
                if (resultAdvertisement.StatusCode != 200 || advertisements == null) return new NotFoundResult();
                advertisements = advertisements.OrderByDescending(it => it.date).ToList();

                var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
                var houses = resultHouse.Item as List<House>;
                if (resultHouse.StatusCode != 200 || houses == null) return new NotFoundResult();

                List<string> names = new();
                foreach (Advertisement advertisement in advertisements)
                    names.Add(houses.First(it => it.id == advertisement.idHouse).name);

                ViewBag.Names = names;

                return View("Index", advertisements);
            }
            return new NotFoundResult();
        }

        private static List<NewModel> GetNews(List<Advertisement>? advertisements, List<Service>? services, List<SettingsService>? settingsServices,
            List<Counter>? counters, List<Payment>? payments, List<Indication>? indications)
        {

            var news = new List<NewModel>();
            var textReminderIndication = "Внимание, необходимо передать показания по услугам: ";
            var textReminderPayment = "Внимание, необходимо оплатить начисления по услугам: ";
            var dateNow = DateTime.Now;

            var haveNoIndications = false;
            var haveNoPayments = false;

            var index = 0;

            if (services != null && settingsServices != null)
                foreach (var service in services)
                {
                    var settings = settingsServices.Where(it => it.idService == service.id).First();
                    //-начало проверки показаний
                    if (settings.haveCounter == true && dateNow.Day >= settings.startDateTransfer && dateNow.Day <= settings.endDateTransfer)
                    {
                        if (counters != null)
                        {
                            var countersService = counters.Where(it => it.idService == service.id);
                            foreach (var counter in countersService)
                            {
                                if (indications != null)
                                {
                                    var indication = indications.FirstOrDefault(it =>
                                   (it.dateTransfer.Year == dateNow.Year) &&
                                   (it.dateTransfer.Month == dateNow.Month) &&
                                   it.idCounter == counter.id);
                                    if (indication == null)
                                    {
                                        textReminderIndication += service.nameService + ", ";
                                        haveNoIndications = true;
                                    }
                                }
                            }
                        }
                    }
                }

            if (haveNoIndications)
            {
                index++;
                textReminderIndication = textReminderIndication[..^2];
                textReminderIndication += " до истечения сроков!";
                news.Add(
                new NewModel(index, "Напоминание о показаниях", textReminderIndication));
            }
            //-конец проверки показаний
            if (advertisements != null)
            {
                advertisements = advertisements.OrderByDescending(it => it.date).ToList();
                foreach (var advertisement in advertisements)
                {
                    index++;
                    news.Add(new NewModel(index, "Объявление от " + advertisement.date.ToShortDateString(), advertisement.description));
                }
            }

            return news;
        }

        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> SelectHouse()
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            List<House>? houses = resultHouse.Item as List<House>;
            ViewBag.Houses = houses;
            List<ModelCheckBox> modelCheckBox = new();
            foreach (var house in houses)
                modelCheckBox.Add(new ModelCheckBox(house.id, false));
            return View(modelCheckBox);

        }

        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelectHouse(List<ModelCheckBox> ModelCheckBox)
        {
            if (ModelCheckBox.Count == 0 || ModelCheckBox.Where(it => it.selected == true).Count() == 0)
            {
                ViewBag.Message = "Необходимо выбрать дома";
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
                if (resultHouse.StatusCode == 200)
                {
                    List<House>? houses = resultHouse.Item as List<House>;
                    ViewBag.Houses = houses;
                    List<ModelCheckBox> modelCheckBox = new();
                    foreach (var house in houses)
                        modelCheckBox.Add(new ModelCheckBox(house.id, false));
                    return View(modelCheckBox);
                }
            }
            else
            {
                List<int> houses = ModelCheckBox.Where(it => it.selected == true).Select(it => it.id).ToList();
                return RedirectToAction("Create", new { @houses = houses });
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> CreateAsync(List<int> houses)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var names = new List<string>();
            foreach (var house in houses)
            {
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{house}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House? houseItem = resultHouse.Item as House;
                names.Add(houseItem.name);
            }

            ViewBag.NameHouse = names;
            ViewBag.Houses = houses;
            ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
            return View();
        }

        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("description")] Advertisement advertisement, List<int>? Houses)
        {
            var context = HttpContext;
            ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                advertisement.date = DateTime.Parse(DateTime.UtcNow.ToShortDateString());

                AdvertisementCreateModel advertisementCreateModel = new(advertisement.date, advertisement.description, Houses);

                var result = await ApiRequests.PostToApiAsync(advertisementCreateModel, Urles.AdvertisementUrl, token);
                if (result.StatusCode == 201) return RedirectToAction("Index");
                else
                    ViewBag.Message = "Невозможно отправить данные";
            }
            var names = new List<string>();
            foreach (var house in Houses)
            {
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{house}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House? houseItem = resultHouse.Item as House;
                names.Add(houseItem.name);
            }

            ViewBag.Houses = Houses;
            ViewBag.NameHouse = names;
            return View(advertisement);
        }

        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiGetRequests.LoadAdvertisementFromAPIAsync(token, Urles.AdvertisementUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    Advertisement? advertisement = result.Item as Advertisement;
                    {
                        if (advertisement != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{advertisement.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            ViewBag.Date = advertisement.date.ToShortDateString();
                            return View(advertisement);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, date, idHouse, description")] Advertisement advertisement)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            ViewBag.Date = advertisement.date.ToShortDateString();
            if (ModelState.IsValid)
            {
                var result = await ApiRequests.PutToApiAsync(advertisement, Urles.AdvertisementUrl + $"/{advertisement.id}", token);
                if (result.StatusCode == 201)
                    return RedirectToAction("Index");
                else
                    ViewBag.Message = "Невозможно сохранить данные";

            }
            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{advertisement.idHouse}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House house = resultHouse.Item as House;
            ViewBag.NameHouse = house.name;
            return View(advertisement);
        }

        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> DeleteAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiGetRequests.LoadAdvertisementFromAPIAsync(token, Urles.AdvertisementUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    Advertisement? advertisement = result.Item as Advertisement;
                    {
                        if (advertisement != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{advertisement.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            advertisement.date = DateTime.Parse(advertisement.date.ToShortDateString());
                            return View(advertisement);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var result = await ApiRequests.DeleteToApiAsync(Urles.AdvertisementUrl + $"/{id}", token);
            if (result.StatusCode == 204) return RedirectToAction("Index");
            else
            {
                Advertisement? advertisement = new Advertisement();
                var resultGet = await ApiGetRequests.LoadAdvertisementFromAPIAsync(token, Urles.AdvertisementUrl + $"/{id}");
                if (resultGet.StatusCode == 200)
                {
                    advertisement = resultGet.Item as Advertisement;
                }
                ViewBag.Message = "Невозможно удалить данные";
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{advertisement.idHouse}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House house = resultHouse.Item as House;
                ViewBag.NameHouse = house.name;
                return View(advertisement);
            }
        }
    }
}