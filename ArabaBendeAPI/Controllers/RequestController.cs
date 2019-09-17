using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ArabaBendeAPI.Models;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace ArabaBendeAPI.Controllers
{
    public class RequestController : ApiController
    {
        arabaDBEntities12 dbRes = new arabaDBEntities12();
        arabaDBEntities11 dbReq = new arabaDBEntities11();
        arabaDBEntities14 dbEmp = new arabaDBEntities14();
        arabaDBEntities10 dbVeh = new arabaDBEntities10();
        private IObjectContextAdapter ctx;

        // GET: api/Employees  
        public IHttpActionResult GetRequests()
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
                                   Aim = e.Aim
                               }).ToList();

            sourceList.Add(reservations);
            return Json(sourceList);
        }

        [HttpGet]
        [Route("api/GetRequestsAccToEmpId")]
        public IHttpActionResult GetRequestsAccToEmpId(int empId)
        {
            var req = new List<Request>(dbReq.Requests);
            var veh = new List<Vehicle>(dbVeh.Vehicles);
            var emp = new List<Person>(dbEmp.Persons);

            var entryPoint = (from ep in emp
                              join e in req on ep.PersonID equals e.PersonID
                              join t in veh on e.VehicleID equals t.VehicleID
                              where e.PersonID == empId
                              select new
                              {
                                  Plaka = t.Plaka,
                                  Duration = e.DurationCheck
                              }).ToList();

            return Json(entryPoint);
        }

        // PUT: api/Employees/5  

        public HttpResponseMessage PutRequest(Request req)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }


            dbReq.Entry(req).State = EntityState.Modified;

            try
            {
                dbReq.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST: api/Employees  

        public IQueryable<Vehicle> PostRequest(RequestWithoutDate request)
        {
            Request req = new Request
            {
                RequestID = request.RequestID,
                PersonID = request.PersonID,
                VehicleID = request.VehicleID,
                CreationDate = DateTime.Now,
                DurationCheck = request.DurationCheck
            };
            dbReq.Requests.Add(req);
            dbReq.SaveChanges();
            Vehicle result = dbVeh.Vehicles.SingleOrDefault(b => b.VehicleID == req.VehicleID);
            if (result != null && result.EnabledFlag == 0)
            {
                result.EnabledFlag = 1;
                dbVeh.SaveChanges();
            }
            else if (result != null && result.EnabledFlag == 1)
            {
                result.EnabledFlag = 0;
                dbVeh.SaveChanges();
            }
            VehicleController a = new VehicleController();
            a.GetVehicles();
            return a.GetVehicles();
        }

        // DELETE: api/Employees/5  

        public HttpResponseMessage DeleteRequest(Request req)
        {
            Request remove_req = dbReq.Requests.Find(req.RequestID);
            if (remove_req == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            dbReq.Requests.Remove(remove_req);
            try
            {
                dbReq.SaveChanges();
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
                dbReq.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        [Route("api/RequestExists")]
        private bool RequestExists(int id)
        {
            return dbReq.Requests.Count(e => e.RequestID == id) > 0;
        }
    }
}
