using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain;
using WebUI.Models;
using Domain.Abstract;
using Domain.Concrete;
using WebUI.Filters;


namespace WebUI.Controllers
{
    // [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
    public class TaskController : Controller
    {
      private churchdatabaseEntities db = new churchdatabaseEntities();
        private IConstantRepository ConstantRepository;
        private IMinistryRepository MinistryRepository;
        private IGoalRepository  GoalRepository;
        private ITaskRepository TaskRepository;
        private IMinistryMemberRepository MinistryMemberRepository;
        private IMemberRepository MemberRepository;
        private IActionItemRepository ActionItemRepository;
        private IMeetingAgendaRepository MeetingAgendaRepository;
        private IMeetingRepository MeetingRepository;
        private EMembershipProviderRepository MembershipRepositroy = new EMembershipProviderRepository();

        public TaskController(IConstantRepository ConstantParam, IMinistryRepository MinistryParam, IGoalRepository GoalParam, ITaskRepository TaskParam,
            IMinistryMemberRepository MinistryMemberParam, IMemberRepository MemberParam, IActionItemRepository ActionItemParam, IMeetingAgendaRepository MeetingAgendaParam,
            IMeetingRepository MeetingParam)
        {
            ConstantRepository = ConstantParam;
            MinistryRepository = MinistryParam;
            GoalRepository = GoalParam;
            TaskRepository = TaskParam;
            MinistryMemberRepository = MinistryMemberParam;
            MemberRepository = MemberParam;
            ActionItemRepository = ActionItemParam;
            MeetingAgendaRepository = MeetingAgendaParam;
            MeetingRepository = MeetingParam;

            //security
            ViewBag.Supervisor = false;
            int memberID = Convert.ToInt16(System.Web.HttpContext.Current.Session["personID"]);
            if (memberID > 0)
            {
                if (MembershipRepositroy.IsUser(memberID))
                {
                    user user = MembershipRepositroy.GetUserByID(memberID);
                    if ((user.role.Name == "WebMaster") || (user.role.Name == "Pastor") || (user.role.Name == "Admin") || (user.role.Name == "Admin2")) //creator access
                    {
                        ViewBag.Supervisor = true;
                    }
                    if (user.role.Name == "WebMaster") //creator access
                    {
                        ViewBag.WebMaster = true;
                    }

                    if (user.role.Name == "Officer") //creator access
                    {
                        ViewBag.Supervisor2 = true;
                    }
                }
            }
        }
        //
        // GET: /Pledge/
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Index(int goalID = 0)
        {
            ViewBag.GoalID = goalID;
            GetData(goalID);
            return PartialView();
        }

        //
        // GET: /Pledge/Details/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Details(int goalID = 0)
        {
            GetData(goalID);
            ViewBag.BeginDate = DateTime.Now.ToShortDateString();
            ViewBag.EndDate = DateTime.Now.ToShortDateString();
            return PartialView();
        }

        //
        // GET: /Pledge/Create
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Create(int goalID = 0, int parentTaskID = 0,string ActionItemCode = "",string Code = "Goal")
        {
            if (Code == "Goal")
            {
                goal goal = GoalRepository.GetGoalByID(goalID);
                ViewBag.Heading = string.Format("Add Tasks to Goal ({0})", goal.Title);
                ViewBag.SubHeading = "Add New Task..";
                GetData(goal.ministryID);
            }
            else if (Code == "ActionItem")
            {
                task task = TaskRepository.GetTaskByID(parentTaskID);
                goal gol = GoalRepository.GetGoalByID(goalID);
                ViewBag.Heading = string.Format("Add Action Item to Task ({0} - {1})", gol.Title, task.Title);
                ViewBag.SubHeading = "Add New Action Item..";
                GetData(gol.ministryID);
            }
            else
            {
                meetingagenda agenda = MeetingAgendaRepository.GetAgendaByID(goalID);
                ViewBag.GoalTitle = agenda.Description;
                int ministryID = MeetingRepository.GetMeetingByID(agenda.meetingID).ministryID;
                GetData(ministryID);
            }
            
            ViewBag.ParentTaskID = 0;
            
            return PartialView(new task { DateEntered = System.DateTime.Today, EnteredBy = User.Identity.Name.ToString(), Status = "Active", goalID = goalID, CompletionRatio = 0,parentTaskID=parentTaskID,ActionItemTypeCode=ActionItemCode });
        } 


        //
        // POST: /Task/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(task task)
        {
            int ministryID = GoalRepository.GetGoalByID(task.goalID).ministryID;
            try
            {
                if (ModelState.IsValid)
                {
                    db.tasks.Add(task);
                    db.SaveChanges();

                    //Action Items are tasks listed in the action item table
                    if (task.parentTaskID > 0)
                    {
                        if (task.ActionItemTypeCode == null) { task.ActionItemTypeCode = ""; }
                        actionitem a = new actionitem();
                        a.ParentItemID = task.parentTaskID;
                        a.childItemID = task.taskID;
                        a.ParentType = task.ActionItemTypeCode;
                        db.actionitems.Add(a);
                        db.SaveChanges();

                        TaskRepository.AddRecord(task);
                        TempData["Message2"] = "Action Item added successfully.";
                        GetData(ministryID);
                        return RedirectToAction("Create", new { goalID = task.goalID, parentTaskID = task.parentTaskID, ActionItemCode = task.ActionItemTypeCode, Code = "ActionItem" });
                    }
                    TaskRepository.AddRecord(task);
                    TempData["Message2"] = "Task added successfully.";
                    GetData(ministryID);
                    return RedirectToAction("Create", new { bDate = DateTime.Today, eDate = DateTime.Today, SearchType = "GoalSearch", codeID = task.goalID });

                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = "Error adding Task";
            }
            GetData(ministryID);
            return PartialView(task);
        }
        
        //
        // GET: /Task/Edit/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Edit(int TaskID, string Code = "Task")
        {
            task task = TaskRepository.GetTaskByID(TaskID);
            goal goal = GoalRepository.GetGoalByID((int)task.goalID);
            ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
            if (Code == "ActionItem")
            {
                ViewBag.SubHeading = "Edit ActionItem here...";
               ViewBag.Heading = string.Format("Edit Action Item - {0} Goal ({1})", ministry.MinistryName, goal.Title);
            }
            else
            {
                ViewBag.SubHeading = "Edit Task here...";
                ViewBag.Heading = string.Format("Edit Task - {0} Goal ({1})", ministry.MinistryName, goal.Title);
            }
            GetData(ministry.ministryID);
            return PartialView(task);
        }

        //
        // POST: /Task/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(task task)
        {
            int ministryID = GoalRepository.GetGoalByID(task.goalID).ministryID;
                
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message2"] = string.Format("Task update successfully.");
                    GetData(ministryID);
                    actionitem aItem = ActionItemRepository.GetActionItemParent(task.taskID);
                    if (aItem != null)
                    {
                        return RedirectToAction("ActionItemIndex", new { TaskID = aItem.ParentItemID });
                    }
                    else
                    {
                      return RedirectToAction("List", new { bDate = DateTime.Today, eDate = DateTime.Today, SearchType = "GoalSearch", codeID = task.goalID});
                    }
                    
                }
            }
            catch (Exception ex)
            {
                TempData["Message2"] = string.Format("Error editing task.");
            }
            GetData(ministryID);
            return PartialView(task);
        }

        //
        // GET: /Task/Delete/5
         [RoleAuthentication(Roles = "WebMaster,Admin,Pastor,Officer,Admin2")]
        public ActionResult Delete(int TaskID)
        {
            ViewBag.TaskID = TaskID;
            task task = TaskRepository.GetTaskByID(TaskID);
            return PartialView(task);
        }

        //
        // POST: /Task/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int TaskID)
        {
            task task = TaskRepository.GetTaskByID(TaskID);
            actionitem aItem = ActionItemRepository.GetActionItemParent(task.taskID);
            if (aItem != null)
            {
                TaskRepository.DeleteRecord(task);
                ActionItemRepository.DeleteRecord(aItem);
                return RedirectToAction("ActionItemIndex", new { TaskID = aItem.ParentItemID });
            }
            else
            {
                TaskRepository.DeleteRecord(task);
                return RedirectToAction("List", new { bDate = DateTime.Today, eDate = DateTime.Today, SearchType = "GoalSearch", codeID = task.goalID });
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public void GetData(int ministryID = 0)
        {
            int id = 0;
            Dictionary<string, string> StatusList;
            StatusList = ConstantRepository.GetConstantByCategory("Status");
            ViewBag.StatusList = new SelectList(StatusList, "Key", "Value", id);

            Dictionary<int, string> GoalList;
            GoalList = GoalRepository.GetGoalList(ministryID);
            ViewBag.GoalList = new SelectList(GoalList, "Key", "Value", id);

            if (ministryID > 0)
            {
                Dictionary<int, string> MinistryMemberList;
                MinistryMemberList = MinistryMemberRepository.GetMinistryMemberList(ministryID);
                ViewBag.MinistryMemberList = new SelectList(MinistryMemberList, "Key", "Value", id);
            }

            string ratio;
            Dictionary<string, string> CompletionRatioList = new Dictionary<string, string>();
            for (int i = 0; i <= 100; i += 5)
            {
                ratio = string.Format("{0}", i);
                CompletionRatioList.Add(ratio, ratio);
            }
            ViewBag.CompletionRatioList = new SelectList(CompletionRatioList, "Key", "Value", id);

            Dictionary<string, string> CompletionRatioList2 = new Dictionary<string, string>();
            string Quarter = "Less than Quarter";
            string Half = "Less than Half";
            string Thirds = "Less than Third";
            string Full = "Less than Full";
            string Complete = "Fully Complete";

            CompletionRatioList2.Add(Quarter, Quarter);
            CompletionRatioList2.Add(Half, Half);
            CompletionRatioList2.Add(Thirds, Thirds);
            CompletionRatioList2.Add(Full, Full);
            CompletionRatioList2.Add(Complete, Complete);
            ViewBag.CompletionRatioList2 = new SelectList(CompletionRatioList2, "Key", "Value", id);

        }

        public ActionResult List(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0, string codeName = "")
        {
            IEnumerable<task> TaskList;

            ViewBag.MinistryID = 0;
            ViewBag.Heading2 = false;
            if (SearchType == "GoalSearch")
            {
                ViewBag.Heading2 = true;
                TaskList = TaskRepository.GetTaskByGoal(codeID);
                ViewBag.GoalID = codeID;
                goal goal = GoalRepository.GetGoalByID(codeID);
                ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
                ViewBag.Heading = string.Format("{0} {1} Task List", ministry.MinistryName, goal.Title);
                ViewBag.MinistryName = ministry.MinistryName;
                ViewBag.TaskTitle = goal.Title;

                ViewBag.MinistryID = goal.ministryID;
            }
            else if (SearchType == "StatusSearch")
            {
                TaskList = TaskRepository.GetTaskByStatus(code);
                ViewBag.Heading = "Task List by Status";
            }
            else if (SearchType == "Due Date Search")
            {
                TaskList = TaskRepository.GetTaskByDueDateRange(bDate,eDate);
                ViewBag.Heading = "Task List by Date Range";
            }
            else if (SearchType == "Assigned Date Search")
            {
                TaskList = TaskRepository.GetTaskByAssignDateRange(bDate,eDate);
                ViewBag.Heading = "Task List by Assigned Date";
            }
            else if (SearchType == "CompletionRatioSearch")
            {
                TaskList = TaskRepository.GetTaskByRatio(codeName);
                ViewBag.Heading = "Task List by Percentage Completed";
            }
            else if (SearchType == "MinistrySearch")
            {
                ViewBag.Heading2 = true;
                TaskList = TaskRepository.GetTaskByMinistryDateRange(codeID, bDate, eDate);
                ViewBag.GoalID = codeID;
                goal goal = GoalRepository.GetGoalByID(codeID);
                ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
                ViewBag.Heading = string.Format("{0} {1} Task List", ministry.MinistryName, goal.Title);
                ViewBag.MinistryName = ministry.MinistryName;
                ViewBag.TaskTitle = goal.Title;
                ViewBag.MinistryID = goal.ministryID;
            }
            else
            {
                TaskList = TaskRepository.GetTaskByAssignDateRange(bDate, eDate);
            }

            ViewBag.RecordCount = TaskList.Count();

            foreach (var i in TaskList)
            {
                i.member = MemberRepository.GetMemberByID(i.AssignTo);
            }

                return PartialView(TaskList);
          
        }

        public ActionResult ListTwo(DateTime bDate, DateTime eDate, string SearchType = "", int codeID = 0, string code = "", int codeID2 = 0, string codeName = "")
        {
            IEnumerable<task> TaskList;

            if (SearchType == "GoalSearch")
            {
                TaskList = TaskRepository.GetTaskByGoal(codeID);
                ViewBag.GoalID = codeID;
                goal goal = GoalRepository.GetGoalByID(codeID);
                ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
                ViewBag.Heading = string.Format("{0} {1} Task List", ministry.MinistryName, goal.Title);
            }
            else if (SearchType == "StatusSearch")
            {
                TaskList = TaskRepository.GetTaskByStatus(code);
                ViewBag.Heading = "Task List by Status";
            }
            else if (SearchType == "Due Date Search")
            {
                TaskList = TaskRepository.GetTaskByDueDateRange(bDate, eDate);
                ViewBag.Heading = "Task List by Date Range";
            }
            else if (SearchType == "Assigned Date Search")
            {
                TaskList = TaskRepository.GetTaskByAssignDateRange(bDate, eDate);
                ViewBag.Heading = "Task List by Assigned Date";
            }
            else if (SearchType == "CompletionRatioSearch")
            {
                TaskList = TaskRepository.GetTaskByRatio(codeName);
                ViewBag.Heading = "Task List by Percentage Completed";
            }
            else if (SearchType == "MinistrySearch")
            {
                TaskList = TaskRepository.GetTaskByMinistryDateRange(codeID, bDate, eDate);
                ViewBag.GoalID = codeID;
                goal goal = GoalRepository.GetGoalByID(codeID);
                ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
                ViewBag.Heading = string.Format("{0} {1} Task List", ministry.MinistryName, goal.Title);
            }
            else
            {
                TaskList = TaskRepository.GetTaskByAssignDateRange(bDate, eDate);
            }
            ViewBag.RecordCount = TaskList.Count();

            foreach (var i in TaskList)
            {
                i.member = MemberRepository.GetMemberByID(i.AssignTo);
            }

            return PartialView(TaskList);

        }

        public JsonResult GetTaskList(int GoalID)
        {

            List<SelectListItem> listItem = new List<SelectListItem>();
            GoalTaskListModel listBox = new GoalTaskListModel();

            Dictionary<int, string> tasklist = TaskRepository.GetTaskList(GoalID);

            foreach (var GoalTasks in tasklist)
            {
                listBox.key = GoalTasks.Key;
                listBox.value = GoalTasks.Value;
                listItem.Add(new SelectListItem() { Text = listBox.value, Value = listBox.key.ToString() });
            }

            return Json(listItem, JsonRequestBehavior.AllowGet); 
        }

        public ActionResult ActionItemView(int TaskID = 0)
        {
            task task = TaskRepository.GetTaskByID(TaskID);
            goal goal = GoalRepository.GetGoalByID(task.goalID);
            ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
            ViewBag.Heading = string.Format("{0} {1} ({2}) Action Items List", ministry.MinistryName,goal.Title, task.Title);
            ViewBag.MinistryName = ministry.MinistryName;
            ViewBag.GoalTitle = goal.Title;
            ViewBag.TaskTitle = task.Title;

            ViewBag.GoalID = task.goalID;
            ViewBag.ParentTaskID = TaskID;
            IEnumerable<task> ActionItems = ActionItemRepository.GetActionItemTaskList(TaskID);
            foreach (task a in ActionItems)
            {
                a.member = MemberRepository.GetMemberByID(a.AssignTo);
            }
            ViewBag.RecordCount = ActionItems.Count();
            return PartialView(ActionItems);
        }

        public ActionResult ActionItemIndex(int TaskID = 0)
        {
            task task = TaskRepository.GetTaskByID(TaskID);
            goal goal = GoalRepository.GetGoalByID(task.goalID);
            ministry ministry = MinistryRepository.GetMinistryByID(goal.ministryID);
            ViewBag.GoalID = task.goalID;
            ViewBag.ParentTaskID = TaskID;
            ViewBag.TaskID = TaskID;
            ViewBag.MinistryID = goal.ministryID;
            return PartialView();
        }

    }
}