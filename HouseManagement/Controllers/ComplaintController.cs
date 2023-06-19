using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.Data;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin, FlatOwner")]
    public class ComplaintController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            HttpContext context = HttpContext;
            var role = UserProperties.GetRole(ref context);
            var token = UserProperties.GetToken(ref context);

            var url = "";
            if (role == "HouseAdmin") url = $"/admin/{UserProperties.GetIdUser(ref context)}";
            if (role == "FlatOwner") url = $"/flatowner/{UserProperties.GetIdFlatOwner(ref context)}";

            var resultComplaint = await ApiGetRequests.LoadListComplaintsFromAPIAsync(token, Urles.ComplaintUrl + url);
            List<Complaint>? complaints = resultComplaint.Item as List<Complaint>;

            //var resultComplaintPhoto = await ApiGetRequests.LoadListStringPhotosAPIAsync(token, Urles.ComplaintUrl + "/getPhotos" + url);
            //List<string>? photos = resultComplaintPhoto.Item as List<string>;
            List<string>? photos = new();

            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + url);

            if (resultComplaint.StatusCode == 200 && resultFlat.StatusCode == 200)
            {
                var resultHouse = new ResultsRequest();
                List<House> houses = new();
                if (role == "HouseAdmin")
                {
                    url = $"/byuser/{UserProperties.GetIdUser(ref context)}";
                    resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + url);
                    houses = resultHouse.Item as List<House>;
                }
                if (role == "FlatOwner")
                {
                    url = $"/{UserProperties.GetIdHouseFlatOwner(ref context)}";
                    resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + url);
                    houses.Add(resultHouse.Item as House);
                }

                if (resultHouse.StatusCode != 200 || houses == null) return new NotFoundResult();

                List<string> names = new();

                var flats = resultFlat.Item as List<Flat>;
                List<string> numbers = new();
                complaints = complaints.OrderByDescending(it => it.date).ToList();
                foreach (var complaint in complaints)
                {
                    var resultComplaintPhoto = await ApiGetRequests.LoadStringPhotoAPIAsync(token, Urles.ComplaintUrl + $"/getPhoto/{complaint.id}");
                    if(resultComplaintPhoto.StatusCode != 200) return new NotFoundResult();
                    photos.Add(resultComplaintPhoto.Item as string);
                    numbers.Add(flats.First(it => it.id == complaint.idFlat).flatNumber);
                    names.Add(houses.First(it => it.id == (flats.First(a => a.id == complaint.idFlat).idHouse)).name);
                }
                ViewBag.NumbersFlats = numbers;
                ViewBag.Photos = photos;
                ViewBag.Names = names;
                return View("Index", complaints);
            }
            return new NotFoundResult();
        }
        [Authorize(Roles = "FlatOwner")]
        public async Task<ActionResult> CreateAsync()
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var idFlatOwner = UserProperties.GetIdFlatOwner(ref context);
            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/flatowner/{idFlatOwner}");
            List<Flat>? flats = resultFlat.Item as List<Flat>;
            if (flats == null || resultFlat.StatusCode != 200) return new NotFoundResult();
            var numbers = flats.Select(it => it.flatNumber).ToList();
            ViewBag.FlatsNumbers = new SelectList(numbers);
            ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
            return View();
        }
        [Authorize(Roles = "FlatOwner")]
        [HttpPost]
        public async Task<ActionResult> CreateAsync([Bind("description")] Complaint complaint, string number, IFormFile? photo)
        {
            var context = HttpContext;
            ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
            var token = UserProperties.GetToken(ref context);
            var idFlatOwner = UserProperties.GetIdFlatOwner(ref context);
            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/flatowner/{idFlatOwner}");
            List<Flat>? flats = resultFlat.Item as List<Flat>;
            if (flats == null || resultFlat.StatusCode != 200) return new NotFoundResult();
            if (ModelState.IsValid)
            {
                complaint.idFlat = flats.First(it => it.flatNumber == number).id;
                complaint.date = DateTime.Parse(DateTime.UtcNow.ToShortDateString());
                complaint.status = "Отправлена";
                var resultComplaint = await ApiRequests.PostToApiAsync(complaint, Urles.ComplaintUrl + "/mvc", token);
                if (resultComplaint.StatusCode == 201)
                {
                    if (photo == null) return RedirectToAction("Index");
                    var resultPhoto = await ApiRequests.PostComplaintPhotoToApiAsync(photo, Urles.ComplaintUrl + $"/photo/{resultComplaint.Message}", token);
                    if (resultPhoto.StatusCode == 201) return RedirectToAction("Index");
                    else ViewBag.Message = "Заявка отправлена, но фото не добавлено";
                }
                else
                    ViewBag.Message = "Невозможно отправить данные";
            }
            var numbers = flats.Select(it => it.flatNumber).ToList();
            ViewBag.FlatsNumbers = new SelectList(numbers);
            return View(complaint);
        }
        [Authorize(Roles = "FlatOwner")]
        public async Task<ActionResult> EditFlatOwnerAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiGetRequests.LoadComplaintFromAPIAsync(token, Urles.ComplaintUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    Complaint? complaint = result.Item as Complaint;
                    var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{complaint.idFlat}");
                    var flats = UserProperties.GetIdFlats(ref context);
                    if (resultFlat.StatusCode == 200 && complaint.status == "Отправлена" && flats.Contains((int)complaint.idFlat))
                    {
                        Flat? flat = resultFlat.Item as Flat;
                        {
                            var resultPhoto = await ApiGetRequests.LoadStringPhotoAPIAsync(token, Urles.ComplaintUrl + $"/getPhoto/{complaint.id}");
                            if (resultPhoto.StatusCode == 200) ViewBag.Photo = resultPhoto.Item as string;
                            ViewBag.Date = complaint.date.ToShortDateString();
                            ViewBag.FlatNumber = flat.flatNumber;
                            return View(complaint);
                        }
                    }

                }
            }
            return new NotFoundResult();
        }
        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> EditAdminAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiGetRequests.LoadComplaintFromAPIAsync(token, Urles.ComplaintUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    Complaint? complaint = result.Item as Complaint;
                    var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{complaint.idFlat}");
                    if (resultFlat.StatusCode == 200)
                    {
                        Flat? flat = resultFlat.Item as Flat;
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            var resultPhoto = await ApiGetRequests.LoadStringPhotoAPIAsync(token, Urles.ComplaintUrl + $"/getPhoto/{complaint.id}");
                            if (resultPhoto.StatusCode == 200) ViewBag.Photo = resultPhoto.Item as string;
                            ViewBag.Date = complaint.date.ToShortDateString();
                            ViewBag.FlatNumber = flat.flatNumber;
                            ViewBag.Statuses = new SelectList(new List<string> { "Отправлена", "Принята", "Завершена" });
                            return View(complaint);
                        }
                    }

                }
            }
            return new NotFoundResult();
        }
        [Authorize(Roles = "FlatOwner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditFlatOwnerAsync([Bind("id, date, idFlat, description, status, photo")] Complaint complaint, IFormFile? newPhoto, string? delete)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var role = UserProperties.GetRole(ref context);
            ViewBag.Date = complaint.date.ToShortDateString();
            if (ModelState.IsValid)
            {
                var photo = complaint.photo;
                if (delete == "delete") complaint.photo = null;
                var result = await ApiRequests.PutToApiAsync(complaint, Urles.ComplaintUrl + $"/{complaint.id}", token);
                if (result.StatusCode == 201)
                {
                    if ((newPhoto == null && delete == "not") || newPhoto == null) return RedirectToAction("Index");
                    var resultPhoto = await ApiRequests.PostComplaintPhotoToApiAsync(newPhoto, Urles.ComplaintUrl + $"/photo/{complaint.id}", token);
                    if (resultPhoto.StatusCode == 201) return RedirectToAction("Index");
                    else ViewBag.Message = "Заявка изменена, но фото не добавлено";
                }
                else
                    ViewBag.Message = "Невозможно отправить данные";
            }
            var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{complaint.idFlat}");
            if (resultFlat.StatusCode == 200)
            {
                Flat? flat = resultFlat.Item as Flat;
                {
                    var resultPhoto = await ApiGetRequests.LoadStringPhotoAPIAsync(token, Urles.ComplaintUrl + $"/getPhoto/{complaint.id}");
                    if (resultPhoto.StatusCode == 200) ViewBag.Photo = resultPhoto.Item as string;
                    ViewBag.Date = complaint.date.ToShortDateString();
                    ViewBag.FlatNumber = flat.flatNumber;
                    ViewBag.Statuses = new SelectList(new List<string> { "Отправлена", "Принята", "Завершена" });
                    return View(complaint);
                }
            }
            return View(complaint);
        }
        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAdminAsync([Bind("id, date, idFlat, description, photo")] Complaint complaint, string newStatus)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var role = UserProperties.GetRole(ref context);
            ViewBag.Date = complaint.date.ToShortDateString();
            if (ModelState.IsValid)
            {
                complaint.status = newStatus;
                var result = await ApiRequests.PutToApiAsync(complaint, Urles.ComplaintUrl + $"/{complaint.id}", token);
                if (result.StatusCode == 201)
                {
                    return RedirectToAction("Index");
                }
                else
                    ViewBag.Message = "Невозможно отправить данные";
            }
            var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{complaint.idFlat}");
            if (resultFlat.StatusCode == 200)
            {
                Flat? flat = resultFlat.Item as Flat;
                {
                    var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                    if (resultHouse.StatusCode != 200) return new NotFoundResult();
                    House house = resultHouse.Item as House;
                    ViewBag.NameHouse = house.name;
                    var resultPhoto = await ApiGetRequests.LoadStringPhotoAPIAsync(token, Urles.ComplaintUrl + $"/getPhoto/{complaint.id}");
                    if (resultPhoto.StatusCode == 200) ViewBag.Photo = resultPhoto.Item as string;
                    ViewBag.Date = complaint.date.ToShortDateString();
                    ViewBag.FlatNumber = flat.flatNumber;
                    ViewBag.Statuses = new SelectList(new List<string> { "Отправлена", "Принята", "Завершена" });
                    return View(complaint);
                }
            }
            return View(complaint);
        }

        public async Task<ActionResult> DeleteAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var role = UserProperties.GetRole(ref context);
                var result = await ApiGetRequests.LoadComplaintFromAPIAsync(token, Urles.ComplaintUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    Complaint? complaint = result.Item as Complaint;
                    var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{complaint.idFlat}");
                    if (resultFlat.StatusCode == 200)
                    {
                        Flat? flat = resultFlat.Item as Flat;
                        var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                        if (resultHouse.StatusCode != 200) return new NotFoundResult();
                        House house = resultHouse.Item as House;
                        ViewBag.NameHouse = house.name;
                        var flats = UserProperties.GetIdFlats(ref context);
                        if (role == "FlatOwner" && complaint.status != "Отправлена" && !flats.Contains((int)complaint.idFlat)) return new NotFoundResult();
                        if (role == "HouseAdmin" && !UserProperties.GetIdHouses(ref context).Contains(flat.idHouse)) return new NotFoundResult();
                        var resultPhoto = await ApiGetRequests.LoadStringPhotoAPIAsync(token, Urles.ComplaintUrl + $"/getPhoto/{complaint.id}");
                        if (resultPhoto.StatusCode == 200) ViewBag.Photo = resultPhoto.Item as string;
                        ViewBag.Date = complaint.date.ToShortDateString();
                        ViewBag.FlatNumber = flat.flatNumber;
                        return View(complaint);
                    }

                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var resultComplaint = await ApiGetRequests.LoadComplaintFromAPIAsync(token, Urles.ComplaintUrl + $"/{id}");
            Complaint? complaint = resultComplaint.Item as Complaint;
            string? name = complaint.photo;
            if (resultComplaint.StatusCode == 200)
            {
                var result = await ApiRequests.DeleteToApiAsync(Urles.ComplaintUrl + $"/{id}", token);
                if (result.StatusCode == 204)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{complaint.idFlat}");
                    if (resultFlat.StatusCode == 200)
                    {
                        Flat? flat = resultFlat.Item as Flat;
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            var resultPhoto = await ApiGetRequests.LoadStringPhotoAPIAsync(token, Urles.ComplaintUrl + $"/getPhoto/{complaint.id}");
                            if (resultPhoto.StatusCode == 200) ViewBag.Photo = resultPhoto.Item as string;
                            ViewBag.Date = complaint.date.ToShortDateString();
                            ViewBag.FlatNumber = flat.flatNumber;
                            return View(complaint);
                        }
                    }

                }
            }
            return new NotFoundResult();
        }
    }
}