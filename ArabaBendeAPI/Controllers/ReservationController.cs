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
        arabaDBEntities11 dbReq = new arabaDBEntities11();
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

            if (string.IsNullOrEmpty(res.Aim))
            {
                Tuple<bool, string> tuple = new Tuple<bool, string>(false, "Lütfen bir gerekçe giriniz.");
                return Json(tuple);
            }


            if (startdate>enddate)
            {
                Tuple<bool, string> tuple = new Tuple<bool, string>(false, "Başlangıç tarihi bitiş tarihinden sonra gelemez."); 
                return Json(tuple);
            }

            if (enddate < DateTime.Now)
            {
                Tuple<bool, string> tuple = new Tuple<bool, string>(false, "Geçmiş tarihe rezervasyon yapılamaz.");
                return Json(tuple);
            }

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
                Tuple<bool, string> tuple = new Tuple<bool, string>(true, "Rezervasyonunuz kaydedilmiştir.");
                return Json(tuple);
            }
            else
            {
                Tuple<bool, string> tuple = new Tuple<bool, string>(false, "Bu tarihleri kapsayan başka bir rezervasyon bulunmaktadır. Lütfen tarihleri kontrol ediniz.");
                return Json(tuple);
            }


        }
        // DELETE: api/Employees/5  

        public IHttpActionResult DeleteReservation(int resId)
        {
            Reservation remove_req = dbRes.Reservations.Find(resId);
         

            dbRes.Reservations.Remove(remove_req);
            try
            {
                dbRes.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
               
            }

            return getReq();
        }

        protected IHttpActionResult getReq()
        {
            var res = new List<Reservation>(dbRes.Reservations);
            var req = new List<Request>(dbReq.Requests);
            var veh = new List<Vehicle>(dbVeh.Vehicles);
            var emp = new List<Person>(dbEmp.Persons);
            var request = new List<SelectRequest>();
            IList<object> sourceList = new List<object>();
            string sqlString = @"select TerritorySummary.isim, 
	                                    TerritorySummary.PhotoUrl,
	                                    TerritorySummary.TelephoneNumber,
	                                    TerritorySummary.Plaka,
	                                    TerritorySummary.DurationCheck,
	                                TerritorySummary.CreationDate
                                from 
                                (select per.FirstName + ' ' + per.LastName isim ,
	                                per.TelephoneNumber,
	                                per.PhotoUrl,
	                                veh.Plaka,
	                                req.DurationCheck,
	                                req.CreationDate,
	                                ROW_NUMBER() OVER(Partition by veh.plaka ORDER BY req.CreationDate desc) xs
	                                from dbo.Persons per,
	                                dbo.Requests req,
	                                dbo.Vehicles veh
	                                where 
		                                per.PersonID = req.PersonID
	                                and req.VehicleID = veh.VehicleID
	                                and veh.EnabledFlag = 0
                                )AS TerritorySummary
                                where TerritorySummary.xs = 1";
            using (var context = new arabaDBEntities10())
            {
                request = context.Database.SqlQuery<SelectRequest>(sqlString).ToList();
            }
            foreach (SelectRequest requ in request)
            {
                DateTimeOffset localTime, otherTime, universalTime;

                // Define local time in local time zone
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                DateTime UpdatedTime = requ.CreationDate ?? DateTime.Now;
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(UpdatedTime, cstZone);


                requ.CreationDateStr = cstTime.ToString().Split(' ')[1].Split(':')[0] + ":" + cstTime.ToString().Split(' ')[1].Split(':')[1];
            }
            sourceList.Add(request);
            var reservations = (from ep in emp
                                join e in res on ep.PersonID equals e.PersonID
                                join t in veh on e.VehicleID equals t.VehicleID
                                where e.BeginDate >= TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Turkey Standard Time")
                                || (e.BeginDate < TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Turkey Standard Time")
                                && TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Turkey Standard Time") < e.EndDate)
                                select new
                                {
                                    Name = ep.FirstName + " " + ep.LastName,
                                    BeginDate = e.BeginDate.ToString(),
                                    PhotoUrl = ep.PhotoUrl,
                                    Plaka = t.Plaka,
                                    TelephoneNumber = ep.TelephoneNumber,
                                    EndDate = e.EndDate.ToString(),
                                    Aim = e.Aim,
                                    ReservationId = e.ReservationID,
                                    PersonId = e.PersonID
                                }).ToList();

            sourceList.Add(reservations);
            return Json(sourceList);
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
