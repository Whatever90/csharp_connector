using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using connectingToDBTESTING.Models;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace connectingToDBTESTING.Controllers
{
    public class DashboardController : Controller
    {
        private DashboardContext _context;

        public DashboardController(DashboardContext context)
        {
            _context = context;
        }
        // GET: /Home/
        [HttpGet]
        [Route("users/lastscoreshow")]
        // public JsonResult lastscoreshow(){
        //     List<User> users = _context.Users.ToList();
        //     return Json(users);
        // }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            HttpContext.Session.SetObjectAsJson("reg_errors", null);
            HttpContext.Session.SetObjectAsJson("exists", null);
            return View();
        }
        [HttpGet]
        [Route("signin")]
        public IActionResult Signin()
        {
            string login_err = HttpContext.Session.GetObjectFromJson<string>("login_errors");
            HttpContext.Session.SetObjectAsJson("login_errors", null); 
            ViewBag.err = login_err;
            HttpContext.Session.SetObjectAsJson("exists", null);
            return View();
        }
        [HttpPost]
        [Route("signing")]
        public IActionResult Signing(string email, string password)
        {
            User RUser = _context.Users.SingleOrDefault(user => user.Email == email);
            if(RUser==null){
                string errors = "Invalid email or password";
                HttpContext.Session.SetObjectAsJson("login_errors", errors); 
                return RedirectToAction("Signin");
            }
            Console.WriteLine("++++++++++++++++++");
            Console.WriteLine(RUser.Password);
            Console.WriteLine(password);
            Console.WriteLine("++++++++++++++++++");
            if(RUser.Password==password){
                HttpContext.Session.SetObjectAsJson("cur_user", RUser);
                return RedirectToAction("Showuser", new{id = RUser.UserId});
            }
            string errors2 = "Invalid email or password";
            HttpContext.Session.SetObjectAsJson("login_errors", errors2); 
            return RedirectToAction("Signin");
        }
        
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            List<Dictionary<string, object>> err = HttpContext.Session.GetObjectFromJson<List<Dictionary<string, object>>>("reg_errors"); 
            ViewBag.errors = err;
            String exists = HttpContext.Session.GetObjectFromJson<String>("exists");
            ViewBag.exists = exists;
            return View();
        }
        [HttpPost]
        [Route("Registrating")]
        public IActionResult Registrating(User model)
        {
            if(ModelState.IsValid){
                User CurrentUser = new User(){
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password,
                    Email = model.Email,
                    ConPassword = model.ConPassword,
                    Photo = "https://ssl.prcdn.com/uk/people/default-profile.png?1406639312",
                    Level = "user"
                };
                User ret2 = _context.Users.SingleOrDefault(user => user.Email == CurrentUser.Email);
                if(ret2==null){
                    _context.Add(CurrentUser);
                    _context.SaveChanges();
                    HttpContext.Session.SetObjectAsJson("cur_user", CurrentUser);
                    HttpContext.Session.SetObjectAsJson("exists", null);
                    User ret = _context.Users.SingleOrDefault(user => user.Email == CurrentUser.Email);
                    return RedirectToAction("Showuser", new{id = ret.UserId});
                }else{
                    string exists = "Such email already in use";
                    HttpContext.Session.SetObjectAsJson("exists", exists);
                    return RedirectToAction("Register");
                }
                
            }else{
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Console.WriteLine(messages);
                HttpContext.Session.SetObjectAsJson("exists", null);
                HttpContext.Session.SetObjectAsJson("reg_errors", ModelState.Values);
                return RedirectToAction("Register");
            }
            
            //List<Dictionary<string, object>> Allq = _dbConnector.Query("SELECT * FROM quotes ORDER BY created_at Desc");
            
        }
        [HttpGet]
        [Route("logout")]
        public IActionResult Logout(){
            HttpContext.Session.SetObjectAsJson("cur_user", null);
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            User RetrievedUser = _context.Users.SingleOrDefault(user => user.UserId == cur_user.UserId);
            if(cur_user.UserId==1 && cur_user.Level!="admin"){
                RetrievedUser.Level = "admin";
                _context.SaveChanges();
                cur_user = RetrievedUser;
                HttpContext.Session.SetObjectAsJson("cur_user", RetrievedUser);
            }else if(cur_user.UserId!=1 && cur_user.Level!="admin"){
                RetrievedUser.Level = "user";
                _context.SaveChanges();
                cur_user = RetrievedUser;
                HttpContext.Session.SetObjectAsJson("cur_user", RetrievedUser);
            }
            User cur_user2 = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            if(cur_user2.Level == "admin"){
                return RedirectToAction("Dashboardadmin");
            }
            List<User> AllU = _context.Users.ToList();
            ViewBag.user = cur_user2;
            ViewBag.AllU = AllU;
            return View();
        }
        [HttpGet]
        [Route("dashboard/admin")]
        public IActionResult Dashboardadmin()
        {   
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            if(cur_user.Level != "admin"){
                return RedirectToAction("Dashboard");
            }
            List<User> AllU = _context.Users.ToList();
            ViewBag.user = cur_user;
            ViewBag.AllU = AllU;
            return View();
            
        }
        [HttpGet]
        [Route("users/new")]
        public IActionResult Newuser()
        {   
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            if(cur_user.Level != "admin"){
                return RedirectToAction("Dashboard");
            }
            ViewBag.user = cur_user;
            List<Dictionary<string, object>> err = HttpContext.Session.GetObjectFromJson<List<Dictionary<string, object>>>("Newuser_errors"); 
            ViewBag.errors = err;
            //check current user in session. If it's not admin, redirect to dashboard
            return View();
        }
        [HttpPost]
        [Route("adding")]
        public IActionResult Adding(User model)
        {
            if(ModelState.IsValid){
                User CurrentUser = new User(){
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password,
                    Email = model.Email,
                    ConPassword = model.ConPassword,
                    Level = "user",
                    Photo = "https://ssl.prcdn.com/uk/people/default-profile.png?1406639312"
                };
                 _context.Add(CurrentUser);
                _context.SaveChanges();
                return RedirectToAction("Dashboardadmin");
            }else{
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Console.WriteLine(messages);
                HttpContext.Session.SetObjectAsJson("Newuser_errors", ModelState.Values);
                return RedirectToAction("Newuser");
            }
             
        }
        [HttpGet]
        [Route("users/edit")]
        public IActionResult Edituser()
        {   
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            ViewBag.user = cur_user;
            List<Dictionary<string, object>> err = HttpContext.Session.GetObjectFromJson<List<Dictionary<string, object>>>("create_errors"); 
            ViewBag.errors = err;
            string login_err = HttpContext.Session.GetObjectFromJson<string>("login_errors"); 
            ViewBag.err = login_err;
            return View();
        }
        [HttpPost]
        [Route("editing")]
        public IActionResult Editing(string fname, string lname, string email, string description, int id, string level, string photo)
        {   
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            User RetrievedUser = new User();
            String lvl;
            if(cur_user.Level != "admin"){
                 RetrievedUser = _context.Users.SingleOrDefault(user => user.UserId == cur_user.UserId);
                 lvl = "user";
            }else{
                 RetrievedUser = _context.Users.SingleOrDefault(user => user.UserId == id);
                 lvl = level;
            }
            User NewUser = new User(){
                    FirstName = fname,
                    LastName = lname,
                    Password = cur_user.ConPassword,
                    Email = email,
                    ConPassword = cur_user.ConPassword,
                    Description = description,
                    Level = lvl,
                    Photo = photo
                };
            if(ModelState.IsValid){
                RetrievedUser.FirstName = fname;
                RetrievedUser.LastName = lname;
                RetrievedUser.Email = email;
                RetrievedUser.Description = description;
                RetrievedUser.Level = lvl;
                RetrievedUser.Photo = photo;
                _context.SaveChanges();
                if(cur_user.Level != "admin"){
                    HttpContext.Session.SetObjectAsJson("cur_user", RetrievedUser);
                    return RedirectToAction("Showuser", new{id = id});   
                }else{
                    return RedirectToAction("Dashboardadmin");
                }  
            }else{
                HttpContext.Session.SetObjectAsJson("create_errors", ModelState.Values);
                return RedirectToAction("Edituser");
            }
        }
        [HttpPost]
        [Route("passwordupd")]
        public IActionResult PasswordUpd(string Password, string ConPassword, int id)
        {   
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            User RetrievedUser = new User();
            if(cur_user.Level!="admin"){
                 RetrievedUser = _context.Users.SingleOrDefault(user => user.UserId == cur_user.UserId);
            }else{
                 RetrievedUser = _context.Users.SingleOrDefault(user => user.UserId == id);
            }
            User NewUser = new User(){
                    FirstName = RetrievedUser.FirstName,
                    LastName = RetrievedUser.LastName,
                    Password = Password,
                    Email = RetrievedUser.Email,
                    ConPassword = ConPassword,
                    Description = RetrievedUser.Description,
                    Photo = RetrievedUser.Photo
                };
            if(ModelState.IsValid){
                RetrievedUser.Password = NewUser.Password;
                RetrievedUser.ConPassword = NewUser.Password;
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("cur_user", RetrievedUser);
                cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
                if(cur_user.Level != "admin"){
                    return RedirectToAction("Dashboard");
                }else{
                    return RedirectToAction("Dashboardadmin");
                }  
            }else{
                HttpContext.Session.SetObjectAsJson("create_errors", ModelState.Values);
                if(cur_user.Level != "admin"){
                    return RedirectToAction("Edituser");
                }else{
                    return RedirectToAction("Edituseradmin", new{id = id});
                } 
            }
        }
        [HttpGet]
        [Route("users/edit/{id}")]
        public IActionResult Edituseradmin(int id)
        {   
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            if(cur_user.Level != "admin"){
                    return RedirectToAction("Dashboard");
                }
            User RetrievedU = _context.Users.SingleOrDefault(c => c.UserId == id);
            ViewBag.user = RetrievedU;
            ViewBag.user2 = cur_user;
            //check current user in session. If it's not admin, redirect to dashboard
            return View();
        }
        
        
        
        [HttpGet]
        [Route("delete/{id}")]
        public IActionResult Delete(int id)
        {
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            User RetrievedU = _context.Users.SingleOrDefault(c => c.UserId == id);
            _context.Users.Remove(RetrievedU);
            _context.SaveChanges();
            if(cur_user.Level != "admin"){
                    return RedirectToAction("Dashboard");
                }else{
                    return RedirectToAction("Dashboardadmin");
                } 
        }
        [HttpGet]
        [Route("users/show/{id}")]
        public IActionResult Showuser(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            User RetrievedU = _context.Users.SingleOrDefault(c => c.UserId == id);
            List<Message> RetrievedM = _context.Messages.Where(m => m.PostedToId == id).Include(mg => mg.User).ToList();
            foreach(Message m in RetrievedM){
                m.Comments = _context.Comments.Where(c => c.MessageId == m.MessageId).Include(c => c.User).ToList();
            }
            if(RetrievedU.Photo==null){
                RetrievedU.Photo = "https://ssl.prcdn.com/uk/people/default-profile.png?1406639312";
                _context.SaveChanges();
                RetrievedU = _context.Users.SingleOrDefault(c => c.UserId == id);
            }
            List<Activity> TA = _context.Activities.Include(a => a.Guests).ThenInclude(g => g.User).ToList();
            foreach(var act in TA){
                foreach(var user in act.Guests){
                    if(user.UserId==RetrievedU.UserId){
                        RetrievedU.Activities.Add(act);
                    }
                }
            }
            
            ViewBag.cur_user = cur_user;
            ViewBag.user = RetrievedU;
            ViewBag.messages = RetrievedM;
            return View();
        }
        [HttpPost]
        [Route("newmessage")]
        public IActionResult Newmessage(Message model)
        {
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            if(ModelState.IsValid){
                Message newMessage = new Message(){
                    Text = model.Text,
                    PostedToId = model.PostedToId,
                    UserId = cur_user.UserId,
                };
                _context.Add(newMessage);
                _context.SaveChanges();

                return RedirectToAction("Showuser", new{id = model.PostedToId});
            }else{
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Console.WriteLine(messages);
                HttpContext.Session.SetObjectAsJson("reg_errors", ModelState.Values);
                return RedirectToAction("Showuser", new{id = model.PostedToId});
            }
        }
        [HttpPost]
        [Route("newamessage")]
        public IActionResult Newamessage(string text, int PostedToId)
        {
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            
                Amessage newMessage = new Amessage(){
                    Text = text,
                    PostedToId = PostedToId,
                    UserId = cur_user.UserId,
                };
            if(ModelState.IsValid){
                
                _context.Add(newMessage);
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("amerrors", null);
                return RedirectToAction("Activity", new{id = PostedToId});
            }else{
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Console.WriteLine(messages);
                HttpContext.Session.SetObjectAsJson("amerrors", messages);
                return RedirectToAction("Activity", new{id = PostedToId});
            }
        }
        [HttpPost]
        [Route("newcomment")]
        public IActionResult Newcomment(string text, int mid, int uid, int pid)
        {
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            if(cur_user.UserId != uid){
                Console.WriteLine("HACKER HAS BEEN DETECTED!");
                return RedirectToAction("Showuser", new{id = pid});
            }
            if(ModelState.IsValid){
                Comment newC = new Comment(){
                    Text = text,
                    MessageId = mid,
                    UserId = uid,
                };
                _context.Add(newC);
                _context.SaveChanges();

                return RedirectToAction("Showuser", new{id = pid});
            }else{
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Console.WriteLine(messages);
                HttpContext.Session.SetObjectAsJson("reg_errors", ModelState.Values);
                return RedirectToAction("Showuser", new{id = pid});
            }
        }
        [HttpPost]
        [Route("newacomment")]
        public IActionResult Newacomment(string text, int mid, int uid, int pid)
        {
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            if(cur_user.UserId != uid){
                Console.WriteLine("HACKER HAS BEEN DETECTED!");
                return RedirectToAction("Showuser", new{id = pid});
            }
            
                Acomment newC = new Acomment(){
                    Text = text,
                    AmessageId = mid,
                    UserId = uid,
                };
                _context.Add(newC);
                _context.SaveChanges();
                if(ModelState.IsValid){
                    HttpContext.Session.SetObjectAsJson("acerrors", null);
                    return RedirectToAction("Activity", new{id = pid});
                } else{
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Console.WriteLine(messages);
                HttpContext.Session.SetObjectAsJson("acerrors", messages);
                return RedirectToAction("Activity", new{id = pid});
            }
        }
        [HttpGet]
        [Route("home")]
        public IActionResult Home()
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            User RetrievedUser2 = _context.Users.SingleOrDefault(user => user.UserId == cur_user.UserId);
            List<Activity> filter = _context.Activities.Where(act => act.Date < DateTime.Now).ToList();
            foreach(Activity act in filter){
                Console.WriteLine(act.Name);
                _context.Activities.Remove(act);
                _context.SaveChanges();
            }
            Console.WriteLine(RetrievedUser2.UserId);
            List<Activity> AllA = _context.Activities.Include(ac=>ac.User).ToList();
            List<Guest> AllG = _context.Guests.ToList();
            ViewBag.AllGuests = AllG;
            ViewBag.cur_user = RetrievedUser2;
            ViewBag.AllA = AllA;
            string act_err = HttpContext.Session.GetObjectFromJson<string>("act_errors"); 
            ViewBag.act_err = act_err;
            HttpContext.Session.SetObjectAsJson("act_errors", null);
            HttpContext.Session.SetObjectAsJson("act2_errors", null);
            // string[] address = new string[AllA.Count];
            // int counter = 0;
            // foreach(var x in AllA){
            //     address[counter]
            // }
            return View();
        }
        [HttpGet]
        [Route("createactivity")]
        public IActionResult Createactivity()
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            ViewBag.user = cur_user;
            String act2_err = HttpContext.Session.GetObjectFromJson<String>("act2_errors"); 
            ViewBag.act2_errors = act2_err;
            List<Dictionary<string, object>> act_err = HttpContext.Session.GetObjectFromJson<List<Dictionary<string, object>>>("act_errors"); 
            ViewBag.act_errors = act_err;
            return View();
        }
        [HttpPost]
        [Route("newact")]
        public IActionResult Newact(string Name, DateTime Date, int duration1, string duration2, string description, string address)
        {   
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            Console.WriteLine("+++++++++++++++++++++++++++++");
            DateTime endDate;
            if(duration2 == "min"){
                endDate = Date.AddMinutes(duration1);
            }else if(duration2 == "hours"){
                endDate = Date.AddHours(duration1);
            }else{
                endDate = Date.AddDays(duration1);
            }
            string Duration = duration1.ToString() + " " + duration2;
            
            //TryValidateModel(model);
            Activity act = new Activity(){
                    Name = Name,
                    Date = Date,
                    UserId = cur_user.UserId,
                    Duration = Duration,
                    End = endDate,
                    GuestsAmount = 0,
                    Description = description,
                    Address = address

                };
            
            if(ModelState.IsValid){
                Console.WriteLine(act.Name);
                Console.WriteLine(act.Date);
                Console.WriteLine(act.Duration);
                Console.WriteLine(act.End);
                Console.WriteLine(act.Description);
                Console.WriteLine(act.UserId);
                Console.WriteLine(act.GuestsAmount);
                Console.WriteLine(act.Address);
                Console.WriteLine("+++++++++++++++++++++++++++++");
            List<Activity> AllA = _context.Activities.OrderBy(actc => act.Date).Include(ac=>ac.User).Include(a=>a.Guests).ThenInclude(g => g.User).ToList();
            int check = 0;
            foreach(Activity actc in AllA){
                foreach(Guest g in actc.Guests){
                    if(g.UserId == cur_user.UserId){
                        if(act.Date<actc.End && act.End > actc.Date){
                            check = 1;
                        } else if(act.Date>actc.Date && act.Date < actc.End){
                            check = 1;
                        } else if(act.End>actc.Date && act.End < actc.End){
                            check = 1;
                        }
                    }
                }
            }
                if(check == 0){
                    _context.Add(act);
                    _context.SaveChanges();
                    HttpContext.Session.SetObjectAsJson("act2_errors", null);
                    HttpContext.Session.SetObjectAsJson("act_errors", null);
                    return RedirectToAction("Home");
                }else{

                }

                
                string errors2 = "Oops, You can't create an activity. You will be in another place at that time!";
                HttpContext.Session.SetObjectAsJson("act2_errors", errors2); 
                return RedirectToAction("Createactivity");
            }else{
                string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                Console.WriteLine(messages);
                HttpContext.Session.SetObjectAsJson("act2_errors", null);
                HttpContext.Session.SetObjectAsJson("act_errors", ModelState.Values);
                return RedirectToAction("Createactivity");
            
            }
        }
        [HttpGet]
        [Route("deleteact/{id}")]
        public IActionResult Deleteact(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            Activity RA = _context.Activities.SingleOrDefault(act => act.ActivityId == id);
            if(RA.UserId==cur_user.UserId){
                _context.Activities.Remove(RA);
                _context.SaveChanges();
            }else{
                Console.WriteLine("WARNING! HACKER IS DETECTED! SEARCH AND DESTROY!");
            }
            return RedirectToAction("Home");
        }
        [HttpGet]
        [Route("destroy/{id}")]
        public IActionResult Destroy(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            Activity RA = _context.Activities.SingleOrDefault(act => act.ActivityId == id);
            if(RA.UserId==cur_user.UserId){
                _context.Activities.Remove(RA);
                _context.SaveChanges();
            }else{
                Console.WriteLine("WARNING! HACKER IS DETECTED! SEARCH AND DESTROY!");
            }
            return RedirectToAction("Activity", new{id = id});
        }
        [HttpGet]
        [Route("attend/{id}")]
        public IActionResult Attend(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            List<Activity> AllA = _context.Activities.OrderBy(act => act.Date).Include(ac=>ac.User).Include(a=>a.Guests).ThenInclude(g => g.User).ToList();
            Activity RA = _context.Activities.SingleOrDefault(act => act.ActivityId == id);
            int check = 0;
            foreach(Activity act in AllA){
                foreach(Guest g in act.Guests){
                    if(g.UserId == cur_user.UserId){
                        if(RA.Date<act.End && RA.End > act.Date){
                            check = 1;
                        } else if(RA.Date>act.Date && RA.Date < act.End){
                            check = 1;
                        } else if(RA.End>act.Date && RA.End < act.End){
                            check = 1;
                        }
                    }
                }
            }
            if(check == 0){
                RA.GuestsAmount++;
                _context.SaveChanges();
                Guest NewGuest = new Guest{
                    UserId = cur_user.UserId,
                    ActivityId = id
                };
                _context.Add(NewGuest);
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("act_errors", null);
                return RedirectToAction("Home");
            }else{
                string errors2 = "Oops, You can't go there";
                HttpContext.Session.SetObjectAsJson("act_errors", errors2); 
                return RedirectToAction("Home");
            }
            
        }
        [HttpGet]
        [Route("join/{id}")]
        public IActionResult Join(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            List<Activity> AllA = _context.Activities.OrderBy(act => act.Date).Include(ac=>ac.User).Include(a=>a.Guests).ThenInclude(g => g.User).ToList();
            Activity RA = _context.Activities.SingleOrDefault(act => act.ActivityId == id);
            int check = 0;
            foreach(Activity act in AllA){
                foreach(Guest g in act.Guests){
                    if(g.UserId == cur_user.UserId){
                        if(RA.Date<act.End && RA.End > act.Date){
                            check = 1;
                        } else if(RA.Date>act.Date && RA.Date < act.End){
                            check = 1;
                        } else if(RA.End>act.Date && RA.End < act.End){
                            check = 1;
                        }
                    }
                }
            }
            if(check == 0){
                RA.GuestsAmount++;
                _context.SaveChanges();
                Guest NewGuest = new Guest{
                    UserId = cur_user.UserId,
                    ActivityId = id
                };
                _context.Add(NewGuest);
                _context.SaveChanges();
                HttpContext.Session.SetObjectAsJson("act2_errors", null);
                return RedirectToAction("Activity", new{id = id});
            }else{
                string errors2 = "Oops, You can't go there";
                HttpContext.Session.SetObjectAsJson("act2_errors", errors2); 
                return RedirectToAction("Activity", new{id = id});
            }
            
        }
        [HttpGet]
        [Route("changeyourmind/{id}")]
        public IActionResult Changeyourmind(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            Guest RetrievedGuest = _context.Guests.Where(act => act.ActivityId == id).SingleOrDefault(user=> user.UserId == cur_user.UserId);
            _context.Guests.Remove(RetrievedGuest);
            _context.SaveChanges();
            Activity RA = _context.Activities.SingleOrDefault(act => act.ActivityId == id);
            RA.GuestsAmount--;
            _context.SaveChanges();
            
            return RedirectToAction("Home");
        }
        [HttpGet]
        [Route("cancel/{id}")]
        public IActionResult Cancel(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            Guest RetrievedGuest = _context.Guests.Where(act => act.ActivityId == id).SingleOrDefault(user=> user.UserId == cur_user.UserId);
            _context.Guests.Remove(RetrievedGuest);
            _context.SaveChanges();
            Activity RA = _context.Activities.SingleOrDefault(act => act.ActivityId == id);
            RA.GuestsAmount--;
            _context.SaveChanges();
            
            return RedirectToAction("Activity", new{id = id});
        }
        [HttpGet]
        [Route("activity/{id}")]
        public IActionResult Activity(int id)
        {
            if(HttpContext.Session.GetObjectFromJson<User>("cur_user")==null){
                return RedirectToAction("Index");
            }
            Console.WriteLine(id);
            User cur_user = HttpContext.Session.GetObjectFromJson<User>("cur_user");
            Activity RA = _context.Activities.Include(ac=>ac.User).Include(a=>a.Guests).ThenInclude(g => g.User).SingleOrDefault(act => act.ActivityId == id);
            List<Amessage> RetrievedAM = _context.Amessages.Where(m => m.PostedToId == id).Include(mg => mg.User).ToList();
            foreach(Amessage m in RetrievedAM){
                m.Acomments = _context.Acomments.Where(c => c.AmessageId == m.AmessageId).Include(c => c.User).ToList();
            }
            ViewBag.act = RA;
            ViewBag.cur_user = cur_user;
            string act2_err = HttpContext.Session.GetObjectFromJson<string>("act2_errors");
            string amerrors = HttpContext.Session.GetObjectFromJson<string>("amerrors");  
            string acerrors = HttpContext.Session.GetObjectFromJson<string>("acerrors");
            ViewBag.act2_err = act2_err;
            ViewBag.messages = RetrievedAM;
            return View();
        }
        
            
    }


public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }
    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
        string value = session.GetString(key);
        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }
}
}
