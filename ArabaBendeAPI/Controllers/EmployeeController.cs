using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ArabaBendeAPI.Models;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Text;

namespace ArabaBendeAPI.Controllers
{
    public class EmployeeController : ApiController
    {
          private arabaDBEntities14 db = new arabaDBEntities14();  
  
        // GET: api/Employees  
        public IQueryable<Person> GetEmployees()  
        {  
            return db.Persons;  
        }  
  
          
          
        // PUT: api/Employees/5  
          
        public HttpResponseMessage PutEmployee(Person employee)  
        {  
            if (ModelState.IsValid)  
            {  
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);  
            }  
  
  
            db.Entry(employee).State = EntityState.Modified;  
  
            try  
            {  
                db.SaveChanges();  
            }  
            catch (DbUpdateConcurrencyException ex)  
            {  
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);  
            }  
  
            return Request.CreateResponse(HttpStatusCode.OK);  
        }  
  
        // POST: api/Employees  
          
        public IHttpActionResult PostEmployee([FromBody]  User user)  
        {
            try {
                Person employee = new Person { PersonID = 0, FirstName = user.givenName, LastName = user.familyName, PhotoUrl = user.photoUrl, Email = user.email, TelephoneNumber = "" };

                
                    db.Persons.Add(employee);
                    db.SaveChanges();
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, employee);
                    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = employee.PersonID }));
                    return Json(employee);
              
                  
               
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {


                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }

           
              
        }

        // DELETE: api/Employees/5  

        public HttpResponseMessage DeleteEmployee(Person employee)  
        {  
            Person remove_employee = db.Persons.Find(employee.PersonID);  
            if (remove_employee == null)  
            {  
                return Request.CreateResponse(HttpStatusCode.NotFound);  
            }  
  
            db.Persons.Remove(remove_employee);  
            try  
            {  
                db.SaveChanges();  
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
                db.Dispose();  
            }  
            base.Dispose(disposing);  
        }

        [HttpGet]
        [Route("api/EmployeeExists")]
        public IHttpActionResult EmployeeExists(string email)  
        {  
            return Json(db.Persons.Count(e => e.Email == email) > 0);  
        }

        [HttpGet]
        [Route("api/PhoneExists")]
        public IHttpActionResult PhoneExists(int empId)
        {
            Person emp = db.Persons.Where(e => (e.PersonID == empId)).ToList()[0];
            return Json(emp.TelephoneNumber);
        }


        [HttpGet]
        [Route("api/AddPhone")]
        public IHttpActionResult AddPhone(string phone,int empId)
        {
            Person person = db.Persons.Where(e => (e.PersonID == empId)).ToList()[0];
            person.TelephoneNumber = phone;
            db.SaveChanges();
            return Json(person.TelephoneNumber);

        }

        [HttpGet]
        [Route("api/GetUserInfo")]
        public IHttpActionResult GetUserInfo(int empId)
        {
            Person person = db.Persons.Where(e => (e.PersonID == empId)).ToList()[0];
            return Json(person);

        }

        [HttpGet]
        [Route("api/FindPersonIdFromEmail")]
        public IHttpActionResult FindPersonIdFromEmail(string email)
        {
            int personId = 0;
            if (db.Persons.Where(e => (e.Email == email)).ToList().Count>0)
            {
                personId = db.Persons.Where(e => (e.Email == email)).ToList()[0].PersonID;
            }
          
            return Json(personId);
        }
    }
}
