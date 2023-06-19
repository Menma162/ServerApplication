using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin")]
    public class ServiceController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var resultSettingsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{UserProperties.GetIdHouses(ref context)[0]}");
            List<SettingsService>? settingsServices = resultSettingsService.Item as List<SettingsService>;

            if (resultService.StatusCode == 200 && resultSettingsService.StatusCode == 200 && services != null && settingsServices != null)
            {
                List<string> names = new();
                List<string> periods = new();
                settingsServices = settingsServices.Where(it => it.paymentStatus == true).ToList();

                foreach (var settingsService in settingsServices)
                    names.Add(services.First(it => it.id == settingsService.idService).nameService);

                int c = 0;
                foreach (var settings in settingsServices)
                {
                    if (settings.paymentPeriod.Length > 0)
                    {
                        string[] months = settings.paymentPeriod[1..].Split(',');
                        periods.Add("Месяца: ");
                        for (int i = 0; i < months.Length; i++)
                        {
                            periods[c] += CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(months[i])) + ", ";
                        }
                        periods[c] = periods[c][..^2];
                        c++;
                    }
                    else
                    {
                        periods.Add("Месяца: -");
                        c++;
                    }
                }

                ViewBag.Periods = periods;
                ViewBag.Names = names;

                return View("Index", settingsServices);
            }
            return new NotFoundResult();
        }


        public async Task<ActionResult> DetailsAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);

                var resultSettingService = await ApiGetRequests.LoadSettingsServiceFromAPIAsync(token, Urles.SettingsServiceUrl + $"/{id}");
                var settingsService = resultSettingService.Item as SettingsService;

                if (resultSettingService.StatusCode == 200 && settingsService != null)
                {
                    var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{settingsService.idService}");
                    var service = resultService.Item as Service;

                    if (resultService.StatusCode == 200 && service != null)
                    {
                        ViewBag.Id = id;
                        ViewBag.NameService = service.nameService;
                        if (!(service.nameService == "Вывоз мусора" || service.nameService == "Кап. ремонт" ||
                        service.nameService == "Лифт" || service.nameService == "Домофон")) ViewBag.Normative = true;
                        else ViewBag.Normative = false;
                        if (service.nameService == "Вывоз мусора" || service.nameService == "Кап. ремонт" ||
                            service.nameService == "Лифт" || service.nameService == "Домофон" ||
                            service.nameService == "Водоотведение") ViewBag.CanHave = false;
                        else ViewBag.CanHave = true;
                        if (settingsService.haveCounter == true)
                        {
                            ViewBag.NameCounter = service.nameCounter;
                            ViewBag.Deadlines = $"c {settingsService.startDateTransfer} по {settingsService.endDateTransfer} число";
                            if (settingsService.typeIMD == true) ViewBag.TypeCounter = "Индивидуальный прибор учета";
                            else ViewBag.TypeCounter = "Общедомовой прибор учета";
                        }


                        string periods = "";
                        if (settingsService.paymentPeriod.Length > 0)
                        {
                            string[] months = settingsService.paymentPeriod[1..].Split(',');
                            for (int i = 0; i < months.Length; i++)
                            {
                                periods += CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(months[i])) + ", ";
                            }
                        }
                        else periods = "Месяца: -  ";

                        ViewBag.Periods = periods[..^2];
                        return View(settingsService);
                    }
                }
            }
            return new NotFoundResult();
        }
        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);

                var resultSettingService = await ApiGetRequests.LoadSettingsServiceFromAPIAsync(token, Urles.SettingsServiceUrl + $"/{id}");
                var settingsService = resultSettingService.Item as SettingsService;

                if (resultSettingService.StatusCode == 200 && settingsService != null)
                {
                    var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{settingsService.idService}");
                    var service = resultService.Item as Service;

                    if (resultService.StatusCode == 200 && service != null)
                    {
                        ViewBag.NameService = service.nameService;
                        ViewData["type"] = settingsService.typeIMD;
                        if (settingsService.haveCounter == true) ViewBag.Display = "block";
                        else ViewBag.Display = "none";
                        if (service.nameService == "Вывоз мусора" || service.nameService == "Кап. ремонт" ||
                            service.nameService == "Лифт" || service.nameService == "Домофон" ||
                            service.nameService == "Водоотведение") ViewBag.HaveCounter = false;
                        else ViewBag.HaveCounter = true;
                        if (service.nameService == "Вывоз мусора" || service.nameService == "Кап. ремонт" ||
                            service.nameService == "Лифт" || service.nameService == "Домофон") ViewBag.HaveNormative = false;
                        else ViewBag.HaveNormative = true;
                        if (settingsService.paymentPeriod != null)
                        {
                            string[] months = settingsService.paymentPeriod.Split(',');
                            for (int i = 1; i < 13; i++)
                            {
                                if (months.FirstOrDefault(it => it == i.ToString()) != null) ViewData["Check" + i.ToString()] = true;
                                else ViewData["Check" + i.ToString()] = false;
                            }
                        }
                        return View(settingsService);
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, idHouse, idService, paymentEndDate, paymentStatus, valueRate, valueNormative, haveCounter, startDateTransfer, endDateTransfer")] SettingsService settingsService,
            bool typeIMDCounter, bool January, bool February, bool March, bool April, bool May, bool June, bool July,
            bool August, bool September, bool October, bool November, bool December)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            string paymentPeriod = "";
            if (January == true) paymentPeriod += ",1";
            if (February == true) paymentPeriod += ",2";
            if (March == true) paymentPeriod += ",3";
            if (April == true) paymentPeriod += ",4";
            if (May == true) paymentPeriod += ",5";
            if (June == true) paymentPeriod += ",6";
            if (July == true) paymentPeriod += ",7";
            if (August == true) paymentPeriod += ",8";
            if (September == true) paymentPeriod += ",9";
            if (October == true) paymentPeriod += ",10";
            if (November == true) paymentPeriod += ",11";
            if (December == true) paymentPeriod += ",12";

            settingsService.paymentPeriod = paymentPeriod;
            settingsService.typeIMD = typeIMDCounter;

            var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{settingsService.idService}");
            var service = resultService.Item as Service;

            if (resultService.StatusCode == 200 && service != null)
            {
                if (ModelState.IsValid)
                {
                    var hasError = false;
                    if (!(service.nameService == "Вывоз мусора" || service.nameService == "Кап. ремонт" ||
                    service.nameService == "Лифт" || service.nameService == "Домофон"))
                    {
                        if (settingsService.valueNormative == null)
                        {
                            ModelState.AddModelError("valueNormative", "Необходимо заполнить поле");
                            hasError = true;
                        }
                    }
                    if (hasError == false)
                    {
                        if (settingsService.haveCounter)
                        {
                            if (settingsService.startDateTransfer == null) ModelState.AddModelError("startDateTransfer", "Необходимо заполнить поле");
                            else if (settingsService.endDateTransfer == null) ModelState.AddModelError("endDateTransfer", "Необходимо заполнить поле");
                            else if (settingsService.startDateTransfer >= settingsService.endDateTransfer)
                            {
                                ModelState.AddModelError("endDateTransfer", "Дата окончания передачи показаний должна быть больше даты начала");
                            }
                            else
                            {
                                var result = await ApiRequests.PutToApiAsync(settingsService, Urles.SettingsServiceUrl + $"/{settingsService.id}/{UserProperties.GetIdUser(ref context)}", token);
                                if (result.StatusCode == 201)
                                    return RedirectToAction("Index");
                                if (result.StatusCode == 500 && result.Message != null)
                                {
                                    ViewBag.Message = result.Message;
                                }
                                else ViewBag.Message = "Невозможно отправить данные";
                            }
                        }
                        else
                        {
                            var result = await ApiRequests.PutToApiAsync(settingsService, Urles.SettingsServiceUrl + $"/{settingsService.id}/{UserProperties.GetIdUser(ref context)}", token);
                            if (result.StatusCode == 201)
                                return RedirectToAction("Index");
                            if (result.StatusCode == 500 && result.Message != null)
                            {
                                ViewBag.Message = result.Message;
                            }
                            else ViewBag.Message = "Невозможно отправить данные";
                        }
                    }
                }
            }

            ViewBag.NameService = service.nameService;
            ViewData["type"] = settingsService.typeIMD;
            if (settingsService.haveCounter == true) ViewBag.Display = "block";
            else ViewBag.Display = "none";
            if (service.nameService == "Вывоз мусора" || service.nameService == "Кап. ремонт" ||
                service.nameService == "Лифт" || service.nameService == "Домофон" ||
                service.nameService == "Водоотведение") ViewBag.HaveCounter = false;
            else ViewBag.HaveCounter = true;
            if (service.nameService == "Вывоз мусора" || service.nameService == "Кап. ремонт" ||
                service.nameService == "Лифт" || service.nameService == "Домофон") ViewBag.HaveNormative = false;
            else ViewBag.HaveNormative = true;
            if (settingsService.paymentPeriod != null)
            {
                string[] months = settingsService.paymentPeriod.Split(',');
                for (int i = 1; i < 13; i++)
                {
                    if (months.FirstOrDefault(it => it == i.ToString()) != null) ViewData["Check" + i.ToString()] = true;
                    else ViewData["Check" + i.ToString()] = false;
                }
                return View(settingsService);
            }
            return new NotFoundResult();
        }

        public async Task<IActionResult> Setting()
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var resultSettingsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{UserProperties.GetIdHouses(ref context)[0]}");
            List<SettingsService>? settingsServices = resultSettingsService.Item as List<SettingsService>;

            if (resultService.StatusCode == 200 && resultSettingsService.StatusCode == 200 && services != null && settingsServices != null)
            {
                ViewBag.ColdWaterName = "Холодное водоснабжение";
                ViewBag.ColdWater = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Холодное водоснабжение").id)).paymentStatus;

                ViewBag.HotWaterName = "Горячее водоснабжение";
                ViewBag.HotWater = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Горячее водоснабжение").id)).paymentStatus;

                ViewBag.GasName = "Газ";
                ViewBag.Gas = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Газ").id)).paymentStatus;

                ViewBag.ElectricityName = "Электроэнергия";
                ViewBag.Electricity = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Электроэнергия").id)).paymentStatus;

                ViewBag.HeatingName = "Отопление";
                ViewBag.Heating = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Отопление").id)).paymentStatus;

                ViewBag.CapitalRepairsName = "Кап. ремонт";
                ViewBag.CapitalRepairs = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Кап. ремонт").id)).paymentStatus;

                ViewBag.IntercomName = "Домофон";
                ViewBag.Intercom = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Домофон").id)).paymentStatus;

                ViewBag.GarbageDisposalName = "Вывоз мусора";
                ViewBag.GarbageDisposal = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Вывоз мусора").id)).paymentStatus;

                ViewBag.ElevatorName = "Лифт";
                ViewBag.Elevator = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Лифт").id)).paymentStatus;

                ViewBag.WaterDisposalName = "Водоотведение";
                ViewBag.WaterDisposal = settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Водоотведение").id)).paymentStatus;

                return View("Setting", settingsServices);
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setting(bool ColdWater, bool HotWater, bool Gas, bool Electricity, bool Heating,
            bool CapitalRepairs, bool Intercom, bool GarbageDisposal, bool Elevator, bool WaterDisposal)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var resultSettingsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{UserProperties.GetIdHouses(ref context)[0]}");
            List<SettingsService>? settingsServices = resultSettingsService.Item as List<SettingsService>;

            if (resultService.StatusCode == 200 && resultSettingsService.StatusCode == 200 && services != null && settingsServices != null)
            {

                ViewBag.ColdWaterName = "Холодное водоснабжение";
                ViewBag.ColdWater = ColdWater;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Холодное водоснабжение").id)).paymentStatus = ColdWater; ;

                ViewBag.HotWaterName = "Горячее водоснабжение";
                ViewBag.HotWater = HotWater;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Горячее водоснабжение").id)).paymentStatus = HotWater;

                ViewBag.GasName = "Газ";
                ViewBag.Gas = Gas;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Газ").id)).paymentStatus = Gas;

                ViewBag.ElectricityName = "Электроэнергия";
                ViewBag.Electricity = Electricity;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Электроэнергия").id)).paymentStatus = Electricity;

                ViewBag.HeatingName = "Отопление";
                ViewBag.Heating = Heating;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Отопление").id)).paymentStatus = Heating;

                ViewBag.CapitalRepairsName = "Кап. ремонт";
                ViewBag.CapitalRepairs = CapitalRepairs;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Кап. ремонт").id)).paymentStatus = CapitalRepairs;

                ViewBag.IntercomName = "Домофон";
                ViewBag.Intercom = Intercom;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Домофон").id)).paymentStatus = Intercom;

                ViewBag.GarbageDisposalName = "Вывоз мусора";
                ViewBag.GarbageDisposal = GarbageDisposal;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Вывоз мусора").id)).paymentStatus = GarbageDisposal;

                ViewBag.ElevatorName = "Лифт";
                ViewBag.Elevator = Elevator;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Лифт").id)).paymentStatus = Elevator;

                ViewBag.WaterDisposalName = "Водоотведение";
                ViewBag.WaterDisposal = WaterDisposal;
                settingsServices.Find(it => it.idService == (services.Find(it => it.nameService == "Водоотведение").id)).paymentStatus = WaterDisposal;

                var result = await ApiRequests.PutToApiAsync(settingsServices, Urles.SettingsServiceUrl + $"/admin/{UserProperties.GetIdUser(ref context)}", token);
                if (result.StatusCode == 200)
                    return RedirectToAction("Index");
                if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                }
                else ViewBag.Message = "Невозможно отправить данные";

                return View("Setting", settingsServices);
            }
            return new NotFoundResult();
        }
    }
}