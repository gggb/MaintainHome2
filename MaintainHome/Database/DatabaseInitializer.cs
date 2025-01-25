using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using MaintainHome.Helper;
//using AndroidX.Annotations;
using Plugin.LocalNotification;
using System.Diagnostics;
//using static Android.Graphics.ImageDecoder;
//using Android.Gms.Tasks ;
//using Android.App;
//using Android.App;
//using static Android.Graphics.ImageDecoder;
//using Android.Views;
//using Android.App;

namespace MaintainHome.Database
{
    public partial class DatabaseInitializer
    {
        private static PlugInNotificationRepository _plugInNotificationRepository;
        private static TasksRepository? _tasksRepository;
        public static async Task InitializeAsync(SQLiteAsyncConnection database)
        {
            try
            {
                await database.DropTableAsync<User>();
                await database.DropTableAsync<PartInfo>();
                await database.DropTableAsync<Tasks>();
                await database.DropTableAsync<TaskActivity>();
                await database.DropTableAsync<MaintainHome.Models.Notification>();
                await database.DropTableAsync<Category>();
                await database.DropTableAsync<TaskHelp>();
                await database.DropTableAsync<TaskNote>();
                await database.DropTableAsync<PlugInNotification>();

                await database.CreateTableAsync<User>();
                await database.CreateTableAsync<PartInfo>();
                await database.CreateTableAsync<Tasks>();
                await database.CreateTableAsync<TaskActivity>();
                await database.CreateTableAsync<MaintainHome.Models.Notification>();
                await database.CreateTableAsync<Category>();
                await database.CreateTableAsync<TaskHelp>();
                await database.CreateTableAsync<TaskNote>();
                await database.CreateTableAsync<PlugInNotification>();

                // Reset the auto-increment sequence
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='User'");
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Tasks'");
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='TaskActivity'");
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Notification'");
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Category'");
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='TaskHelp'");
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='TaskNote'");
                await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='PlugInNotification'");

                await LoadInitialUser(database);
                await LoadInitialCategories(database);
                await LoadInitialTasks(database);
                await LoadInitialPartInfo(database);
                await LoadInitialTaskActivity(database);
                await LoadInitialNotification(database);
                await LoadInitialTaskHelp(database);
                await LoadInitialTaskNote(database);
                await WarmUpDatabase();

                //await EvaluateLoadedTaskForNotifications();
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred during initialization: {ex.Message}", "OK");
            }
        }

        public static async Task WarmUpDatabase()
        {
            var notificationRepository = new PlugInNotificationRepository();
            var dummyCheck = await notificationRepository.GetPlugInNotificationByTaskIdAsync(-1);   // Perform a dummy read
            Debug.WriteLine($"Warm-up check result: {dummyCheck}");
        }
        private static async Task LoadInitialTasks(SQLiteAsyncConnection database)
        {
            var tasks = new List<Tasks>
            {
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Change A/C filter", Description = "Change the air conditioner filter", Status = "Unscheduled", FrequencyDays = 30, DueDate = DateTime.Today.AddDays(10), Priority = "Medium", UserId = 1, CategoryId = 2},
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Drain hot-water heater sediment", Description = "Drain the sediment from the hot-water heater", Status = "Unscheduled", FrequencyDays = 180, DueDate = DateTime.Today.AddDays(15), Priority = "Low", UserId = 1, CategoryId = 3},
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Backwash pool filter", Description = "Backwash the pool filter if the PSI is over 20", Status = "Unscheduled", FrequencyDays = 60, DueDate = DateTime.Today.AddDays(20), Priority = "Med", UserId = 1, CategoryId = 4},
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Replace washing machine hoses", Description = "Replace washing machine hoses with stainless steel braided hoses", Status = "Unscheduled", FrequencyDays = 365, DueDate = DateTime.Today.AddDays(25), Priority = "Medium", UserId = 1, CategoryId = 3},
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Oil/inspect garage door", Description = "Oil the wheels of the garage door side reel", Status = "Unscheduled", FrequencyDays = 90, DueDate = DateTime.Today.AddDays(30), Priority = "Low", UserId = 1, CategoryId = 5},
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Check bathroom faucets for leaks", Description = "Replace the seal of the bathroom faucet", Status = "Unscheduled", FrequencyDays = 180, DueDate = DateTime.Today.AddDays(35), Priority = "Low", UserId = 1, CategoryId = 3},
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Check Gutters and ensure downspouts aren't blocked", Description = "Inspect the gutters and ensure the downspouts are not blocked", Status = "Unscheduled", FrequencyDays = 30, DueDate = DateTime.Today.AddDays(45), Priority = "Low", UserId = 1, CategoryId = 6},
                new Tasks { ModifiedDate = DateTime.Now, CreatedDate = DateTime.Now, Title = "Check sprinkler system for broken heads or stuck valves", Description = "Inspect the sprinkler system for any broken heads or stuck valves", Status = "Unscheduled", FrequencyDays = 90, DueDate = DateTime.Today.AddDays(60), Priority = "Urgent", UserId = 1, CategoryId = 7 },
            };

            var tasksRepository = new TasksRepository();

            foreach (var task in tasks)
            {
                var result = await tasksRepository.AddTaskAsync(task);
                if (!result)
                {
                    Console.WriteLine($"Failed to add task: {task.Title}");
                }
            }
            //Validate
            var tsks = await database.Table<Tasks>().ToListAsync();
            Console.WriteLine($"************************Task Load");
            foreach (var tsk in tsks)
            {
                Console.WriteLine($"***********Id: {tsk.Id}, CreateDate: {tsk.CreatedDate}, ModifiedDate: {tsk.ModifiedDate}, Title: {tsk.Title}, Descr: {tsk.Description}, Status: {tsk.Status}, Freq: {tsk.FrequencyDays}, DueDate: {tsk.DueDate}, Pri: {tsk.Priority}, User: {tsk.UserId}, Category: {tsk.CategoryId},");
            }
        }
        private static async Task LoadInitialTaskActivity(SQLiteAsyncConnection database)
        {
            try
            {
                var taskActivities = new List<TaskActivity>
        {
            new TaskActivity {TaskId = 1, Status = "Pending", Condition = "Fair", Action = "Filter replacement failed!", TimeSpent = .5m, Notes = "Filter was the wrong size, 20 x 20 x 4. Should have been 20 x 25 x 4."},
            new TaskActivity {TaskId = 1, Status = "On-Hold", Condition = "Fair", Action = "Replaced Filter", TimeSpent = .5m, Notes = "Not too dirty but needed changing. Everything is in order."},
            new TaskActivity {TaskId = 2, Status = "Canceled", Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Lots of sediment. May want to perform check sooner."},
            new TaskActivity {TaskId = 3, Status = "Completed", Condition = "Good", Action = "Inspection", TimeSpent = 2, Notes = "PSI was under 20. No back-wash performed"},
            new TaskActivity {TaskId = 4, Status = "Completed", Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Hose was bulging, days from breaking!!"},
            new TaskActivity {TaskId = 5, Status = "Completed", Condition = "Good", Action = "Inspection", TimeSpent = 2, Notes = "Rollers were in perfect shape."},
            new TaskActivity {TaskId = 6, Status = "Completed", Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Seals were leaking"},
            new TaskActivity {TaskId = 7, Status = "Pending", Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Leaves were starting to accumulate..."},
            new TaskActivity {TaskId = 8, Status = "Pending", Condition = "Poor", Action = "Inspection", TimeSpent = 1, Notes = ""}
        };

                var taskActivityRepository = new TaskActivityRepository();
                foreach (var taskActivity in taskActivities)
                {
                    await taskActivityRepository.AddTaskActivity(taskActivity);
                }

                // Validate
                Console.WriteLine($"************************TaskActivity Load");
                var tas = await database.Table<TaskActivity>().ToListAsync();
                foreach (var ta in tas)
                {
                    Console.WriteLine($"***********Id: {ta.Id}, TaskId: {ta.TaskId}, Status: {ta.Status}, Condition: {ta.Condition}, Action: {ta.Action}, Duration: {ta.TimeSpent}, Notes: {ta.Notes}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private static async Task LoadInitialPartInfo(SQLiteAsyncConnection database)
        {
            try
            {
                var partinfos = new List<PartInfo>
        {
            new PartInfo {TaskId = 1, Name = "20x25x4 A/C Filter ", Description = "Filterbuy 20x25x4 Air Filter MERV 8 Dust Defense (1-Pack)", Price = 9.99, Source = "Home Depot" },
            new PartInfo {TaskId = 4, Name = "Stainless Steel Washing Machine Hoses", Description = "Premium Proof Stainless Steel Washing Machine Hoses (6 FT)", Price = 17.99, Source = "Home Depot" },
            new PartInfo {TaskId = 5, Name = "Nylon Garage Door Rollers", Description = "10 Pack 2'' Ultra-Quiet Nylon Garage Door Rollers 7\" Stem", Price = 29.99, Source = "Home Depot"},
            new PartInfo {TaskId = 7, Name = "EasyOn Gutter Guard System- 24 ft", Description = "Stainless Steel Micro-Mesh Gutters for Maximum Durability and Protection From Fine Debris", Price = 39.99, Source = "CostCo"},
        };

                var partInfoRepository = new PartInfoRepository();

                foreach (var partInfo in partinfos)
                {
                    var resultPartInfo = await partInfoRepository.GetPartsInfoAsync(partInfo.PartInfoId);
                    if (resultPartInfo == null)
                    {
                        await partInfoRepository.AddPartsInfoAsync(partInfo);
                    }
                }

                // Validate  !!!!!!!! Don't forget to comment out after validation.
                var parts = await database.Table<PartInfo>().ToListAsync();
                Console.WriteLine($"************************PartInfo Load");
                foreach (var part in parts)
                {
                    Console.WriteLine($"***********Part Id: {part.PartInfoId}, Task Id: {part.TaskId}, Part name: {part.Name}, Descr: {part.Description}, Price: {part.Price}, Source: {part.Source}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private static async Task LoadInitialTaskHelp(SQLiteAsyncConnection database)
        {
            try
            {
                var taskHelps = new List<TaskHelp>
        {
            new TaskHelp {TaskId = 1, Description = "How to change an A/C filter.", Type = "Video", URL = "https://www.youtube.com/watch?v=sOcfx5o9-B4"},
            new TaskHelp {TaskId = 1, Description = "Changing A/C Filters.", Type = "Pictorial", URL = "https://www.hometips.com/repair-fix/heat-pump-maintenance.html"},
            new TaskHelp {TaskId = 1, Description = "A/C Filter Changing Discussion.", Type = "Textual", URL = "https://hvac-talk.com/vbb/threads/2278461-Teach-me-about-AC-filters-please"},
            new TaskHelp {TaskId = 4, Description = "The best type of washing machine hoses.", Type = "Pictorial", URL = "https://www.youtube.com/watch?v=cLvZIGCv36Q"},
            new TaskHelp {TaskId = 5, Description = "Garage door Maintainance tips.", Type = "Textual", URL = "https://www.huxleyandco.co.uk/common-problems-with-electric-roller-garage-doors-and-how-to-fix-them/"},
            new TaskHelp {TaskId = 7, Description = "Best and Worst Gutter Guards.", Type = "Video", URL = "https://www.youtube.com/watch?v=Yc79p-kfTvo"},
            new TaskHelp {TaskId = 8, Description = "Common Sprinkler problems.", Type = "Video", URL = "https://www.youtube.com/watch?v=m6t35voD7v0"}
        };

                var taskHelpRepository = new TaskHelpRepository();

                foreach (var taskhHelp in taskHelps)
                {
                    var resultTaskhelp = await taskHelpRepository.GetTaskHelpAsync(taskhHelp.TaskHelpsId);
                    if (resultTaskhelp == null)
                    {
                        await taskHelpRepository.AddTaskHelpAsync(taskhHelp);
                    }
                }

                // Validate  !!!!!!!! Don't forget to comment out after validation.
                var helps = await database.Table<TaskHelp>().ToListAsync();
                Console.WriteLine($"************************Task Help Load");
                foreach (var help in helps)
                {
                    Console.WriteLine($"***********Help Id: {help.TaskHelpsId}, Task Id: {help.TaskId}, Descr: {help.Description}, Type: {help.Type}, Link: {help.URL}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private static async Task LoadInitialTaskNote(SQLiteAsyncConnection database)
        {
            try
            {
                var taskNotes = new List<TaskNote>
        {
            new TaskNote {TaskId = 1, Type = "Complaint", NoteContent = "Home Depot Filters are poor quality.", TimeStamp = DateTime.Now},
            new TaskNote {TaskId = 4, Type = "Improvement", NoteContent = "Use Stainless steel braided Hoses only.", TimeStamp = DateTime.Now},
            new TaskNote {TaskId = 5, Type = "Observation", NoteContent = "Use nylon rollers as they are quiet.", TimeStamp = DateTime.Now},
            new TaskNote {TaskId = 7, Type = "Other", NoteContent = "Consider installing gutter leaf guards.", TimeStamp = DateTime.Now},
            new TaskNote {TaskId = 8, Type = "Complaint", NoteContent = "Rain Byrd heads are the least durable!.", TimeStamp = DateTime.Now},
        };

                var taskNoteRepository = new TaskNoteRepository();

                foreach (var tasknote in taskNotes)
                {
                    //var resultTaskNote = await taskNoteRepository.GetTaskNoteAsync(tasknote.TaskId);
                    //if (resultTaskNote == null) 
                    {
                        await taskNoteRepository.AddTaskNoteAsync(tasknote);
                    }
                }

                // Validate  !!!!!!!! Don't forget to comment out after validation.
                var notes = await database.Table<TaskNote>().ToListAsync();
                Console.WriteLine($"************************Task Notes Load");
                foreach (var note in notes)
                {
                    Console.WriteLine($"***********NoteId: {note.NoteId}, Type: {note.Type}, Content: {note.NoteContent}, TimeStamp: {note.TimeStamp}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private static async Task LoadInitialUser(SQLiteAsyncConnection database)
        {
            try
            {
                var initialUser = new User // this login is a Default admin login to allow login if the user account becomes corrupted.
                {
                    UserName = "Admin",
                    Password = "gggb0211",
                    Email = "Admin@swbell.net",
                    Phone = "2149955649",
                };

                //await Application.Current.MainPage.DisplayAlert("Starting LoadInitialUser method", " ", "OK");
                var existingUser2 = await database.Table<User>().FirstOrDefaultAsync(u => u.UserName == initialUser.UserName);
                if (existingUser2 == null)
                {
                    // If the user does not exist, hash the password
                    var hashedPassword = PasswordHelper.HashPassword("gggb0211");
                    //await Application.Current.MainPage.DisplayAlert("Hashed Password", hashedPassword, "OK");

                    // Store the hashed password in SecureStorage
                    await SecureStorage.SetAsync(initialUser.UserName, hashedPassword);

                    // Store hashed password in the database for debugging. Insert user into the database
                    initialUser.Password = hashedPassword;
                    await database.InsertAsync(initialUser);

                    // Display an alert confirming the insertion
                    //await Application.Current.MainPage.DisplayAlert("User Insertion", "Initial user inserted.", "OK");
                }
                else
                {
                    // Display an alert if the user already exists
                    await Application.Current.MainPage.DisplayAlert("Initialization", "User already exists in the database.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private static async Task LoadInitialNotification(SQLiteAsyncConnection database)
        {
            var notifications = new List<MaintainHome.Models.Notification>
            {
                new MaintainHome.Models.Notification {TaskId = 1, TargetName = "Jim Kramer", TargetEmail = "JKramerHVAC@swbell.net", TargetPhone = "6174455640", Message="HVAC Technician/Electrician"},
                new MaintainHome.Models.Notification {TaskId = 1, TargetName = "John Boswell", TargetEmail = "JBMcNeil@Gmail.com", TargetPhone = "8174536722", Message="Our HVAC installer/Maintainer"},
                new MaintainHome.Models.Notification {TaskId = 1, TargetName = "Keith Grimson", TargetEmail = "KG.DuctsAreUS@DuctWorks.com", TargetPhone = "4695635614", Message="He Replaced/Installed Ducts"},
                new MaintainHome.Models.Notification {TaskId = 4, TargetName = "Carl Smithson", TargetEmail = "C.Smithson@Pipes.Com", TargetPhone = "24698779155", Message="Highly recommended Master Plumber"},
                new MaintainHome.Models.Notification {TaskId = 5, TargetName = "Kevin Jackson", TargetEmail = "KJohnson9988@swbell.net", TargetPhone = "2149959834", Message="Expert gararage door DIYer"},
                new MaintainHome.Models.Notification {TaskId = 6, TargetName = "Carl Smithson", TargetEmail = "C.Smithson@Pipes.Com", TargetPhone = "4698779155", Message="Highly recommended Master Plumber"},
                new MaintainHome.Models.Notification {TaskId = 7, TargetName = "Roscoe Jenkins", TargetEmail = "RascalRoscoe@AOL.com", TargetPhone = "8172567753", Message="All-around handy man, specializing in gutters and roofing."},
                new MaintainHome.Models.Notification {TaskId = 8, TargetName = "William Byrd", TargetEmail = "wm-byrd@swbell.net", TargetPhone = "2149955647", Message="Chris Maintains my sprikler"}
            };

            var notificationRepository = new NotificationRepository();
            foreach (var notification in notifications)
            {
                var existingnotifications = await notificationRepository.GetNotificationAsync(notification.NotificationId);
                if (existingnotifications == null)
                {
                    await notificationRepository.AddNotificationAsync(notification);
                }
            }
            //Validate
            Console.WriteLine($"************************Notification Load");
            var nots = await database.Table<MaintainHome.Models.Notification>().ToListAsync();
            foreach (var not in nots)
            {
                Console.WriteLine($"***********CategoryId: {not.NotificationId}, Name: {not.TargetName}, Email: {not.TargetEmail}, {not.TargetPhone}");
            }

        }
        private static async Task LoadInitialCategories(SQLiteAsyncConnection database) // used in edit form category pick list
        {
            try
            {
                var categories = new List<Category>
                {
                    new Category {Title = "None", Priority = 2},
                    new Category {Title = "HVAC", Priority = 2},
                    new Category {Title = "Plumbing", Priority = 2},
                    new Category {Title = "Pool", Priority = 2},
                    new Category {Title = "Garage", Priority = 2},
                    new Category {Title = "Gutters", Priority = 2},
                    new Category {Title = "Sprinklers", Priority = 1}
                };

                var categoryRepository = new CategoryRepository();

                foreach (var category in categories)
                {
                    await categoryRepository.AddCategoryAsync(category);
                }

                // Validate
                Console.WriteLine($"************************Category Load");
                var cats = await categoryRepository.GetAllCategoriesAsync();
                foreach (var cat in cats)
                {
                    Console.WriteLine($"***********CategoryId: {cat.CategoryId}, Title: {cat.Title}, Priority: {cat.Priority}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

    }
}
