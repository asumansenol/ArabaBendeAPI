using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ArabaBendeAPI.Models;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;

namespace ArabaBendeAPI.Controllers
{
    public class ReservationController : ApiController
    {
        arabaDBEntities12 dbRes = new arabaDBEntities12();
        arabaDBEntities14 dbEmp = new arabaDBEntities14();
        arabaDBEntities10 dbVeh = new arabaDBEntities10();

        // GET: api/Employees  
        public IHttpActionResult GetReservations()
        {
            var req = new List<Reservation>(dbRes.Reservations);
            var veh = new List<Vehicle>(dbVeh.Vehicles);
            var emp = new List<Person>(dbEmp.Persons);

            var entryPoint = (from ep in emp
                              join e in req on ep.PersonID equals e.PersonID
                              join t in veh on e.VehicleID equals t.VehicleID
                              where t.ReservationFlag == 0 && e.BeginDate > DateTime.Now
                              select new
                              {
                                  Name = ep.FirstName + " " + ep.LastName,
                                  BeginDate = e.BeginDate.ToString(),
                                  PhotoUrl = ep.PhotoUrl,
                                  Plaka = t.Plaka,
                                  TelephoneNumber = ep.TelephoneNumber,
                                  EndDate = e.EndDate.ToString(),
                                  Aim = e.Aim
                              }).ToList();

            return Json(entryPoint);
        }

        [HttpGet]
        [Route("api/GetReservationsAccToEmpId")]
        public IHttpActionResult GetReservationsAccToEmpId(int empId)
        {
            var req = new List<Reservation>(dbRes.Reservations);
            var veh = new List<Vehicle>(dbVeh.Vehicles);
            var emp = new List<Person>(dbEmp.Persons);

            var entryPoint = (from ep in emp
                              join e in req on ep.PersonID equals e.PersonID
                              join t in veh on e.VehicleID equals t.VehicleID
                              where e.PersonID == empId
                              select new
                              {
                                  Name = ep.FirstName + ep.LastName,
                                  BeginDate = e.BeginDate,
                                  PhotoUrl = ep.PhotoUrl,
                                  Plaka = t.Plaka,
                                  TelephoneNumber = ep.TelephoneNumber,
                                  EndDate = e.EndDate,
                                  Aim = e.Aim
                              }).ToList();

            return Json(entryPoint);
        }

        // PUT: api/Employees/5  


        public HttpResponseMessage PutReservation(Reservation res)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }


            dbRes.Entry(res).State = EntityState.Modified;

            try
            {
                dbRes.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST: api/Employees  
        public static bool Between(DateTime input1, DateTime date1, DateTime date2)
        {
            return (input1 > date1 && input1 < date2);
        }
        public IHttpActionResult PostReservation(ReservationStringDatetime res)
        {
            DateTime startdate = DateTime.ParseExact(res.BeginDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime enddate = DateTime.ParseExact(res.EndDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);


            Reservation reservation = new Reservation
            {
                ReservationID = res.ReservationID,
                Aim = res.Aim,
                CreationDate = DateTime.Now,
                PersonID = res.PersonID,
                VehicleID = res.VehicleID,
                BeginDate = startdate,
                EndDate = enddate
            };
            bool result1, result2, result3, result4,result5, result6, result = false;
            var reservations = new List<Reservation>(dbRes.Reservations).Where(x=>x.VehicleID==res.VehicleID).ToList();
            for (int i = 0; i < reservations.Count; i++)
            {
                result1 = Between(reservation.EndDate, reservations[i].BeginDate, reservations[i].EndDate);
                result2 = Between(reservation.BeginDate, reservations[i].BeginDate, reservations[i].EndDate);
                result3 = Between(reservations[i].EndDate,reservation.BeginDate, reservation.EndDate);
                result4 = Between(reservations[i].BeginDate, reservation.BeginDate, reservation.EndDate);
                result5 = (reservation.BeginDate == reservations[i].BeginDate) && (reservation.EndDate==  reservations[i].EndDate);
                result6 = result1 || result2 || result3 || result4 || result5;
                result = result6 || result;
            }

            if (result == false)
            {
                dbRes.Reservations.Add(reservation);
                dbRes.SaveChanges();
                return Json(false);
            }
            else
            {
                return Json(true);
            }




        }
        // DELETE: api/Employees/5  

        public HttpResponseMessage DeleteReservation(Reservation res)
        {
            Reservation remove_req = dbRes.Reservations.Find(res.ReservationID);
            if (remove_req == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            dbRes.Reservations.Remove(remove_req);
            try
            {
                dbRes.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbRes.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        [Route("api/ReservationExists")]
        private bool ReservationExists(int id)
        {
            return dbRes.Reservations.Count(e => e.ReservationID == id) > 0;
        }
    }
}
