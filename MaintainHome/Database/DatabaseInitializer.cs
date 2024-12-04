﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using MaintainHome.Helper;
//using Android.Gms.Tasks ;
//using Android.App;
//using Android.App;
//using static Android.Graphics.ImageDecoder;
//using Android.Views;
//using Android.App;

namespace MaintainHome.Database
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(SQLiteAsyncConnection database)
        {
            await database.DropTableAsync<User>();
            await database.DropTableAsync<PartBuy>();
            await database.DropTableAsync<PartInfo>();
            await database.DropTableAsync<Tasks>();
            await database.DropTableAsync<TaskActivity>();
            await database.DropTableAsync<MaintainHome.Models.Notification>();
            await database.DropTableAsync<Category>();
            await database.DropTableAsync<TaskHelp>();
            await database.DropTableAsync<TaskNote>();


            await database.CreateTableAsync<User>();
            await database.CreateTableAsync<PartBuy>();
            await database.CreateTableAsync<PartInfo>();
            await database.CreateTableAsync<Tasks>();  
            await database.CreateTableAsync<TaskActivity>();
            await database.CreateTableAsync<MaintainHome.Models.Notification>();
            await database.CreateTableAsync<Category>();
            await database.CreateTableAsync<TaskHelp>();
            await database.CreateTableAsync<TaskNote>();


            // Reset the auto-increment sequence
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='User'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='PartBuy'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Tasks'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='TaskActivity'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Notification'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Category'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='TaskHelp'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Category'");
            await database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='TaskNote'");

            await LoadInitialUser(database);
            await LoadInitialCategories(database);
            await LoadInitialTasks(database);
            await LoadInitialPartInfo(database);
            await LoadInitialTaskActivity(database);
            await LoadInitialPartBuy(database);
            await LoadInitialNotification(database);
            await LoadInitialTaskHelp(database);
            await LoadInitialTaskNote(database);
        }


        private static async Task LoadInitialTasks(SQLiteAsyncConnection database)
        {
            var tasks = new List<Tasks>
            {
                new Tasks
                {
                    Title = "Change A/C filter",
                    Description = "Change the air conditioner filter",
                    Status = "Pending",
                    FrequencyDays = 30,
                    DueDate = DateTime.Today.AddDays(-5),
                    Priority = "Medium",
                    UserId = 1,
                    CategoryId = 1,
                    //TaskHelpsId = 1
                },
                new Tasks
                {
                    Title = "Drain hot-water heater sediment",
                    Description = "Drain the sediment from the hot-water heater",
                    Status = "Pending",
                    FrequencyDays = 180,
                    DueDate = DateTime.Today.AddDays(-3),
                    Priority = "Low",
                    UserId = 1,
                    CategoryId = 2,
                    //TaskHelpsId = 2
                },
                new Tasks
                {
                    Title = "Backwash pool filter",
                    Description = "Backwash the pool filter if the PSI is over 20",
                    Status = "Pending",
                    FrequencyDays = 60,
                    DueDate = DateTime.Today.AddDays(-1),
                    Priority = "Med",
                    UserId = 1,
                    CategoryId = 3,
                    //TaskHelpsId = 3
                },
                new Tasks
                {
                    Title = "Replace washing machine hoses",
                    Description = "",
                    Status = "Pending",
                    FrequencyDays = 365,
                    DueDate = DateTime.Today.AddDays(+7),
                    Priority = "Medium",
                    UserId = 1,
                    CategoryId = 2,
                    //TaskHelpsId = 4
                },
                new Tasks
                {
                    Title = "Oil/inspect garage door",
                    Description = "Oil the wheels of the garage door side reel",
                    Status = "Pending",
                    FrequencyDays = 90,
                    DueDate = DateTime.Today.AddDays(+14),
                    Priority = "Low",
                    UserId = 1,
                    CategoryId = 4,
                    //TaskHelpsId = 5
                },
                new Tasks
                {
                    Title = "Check bathroom faucets for leaks",
                    Description = "Replace the seal of the bathroom faucet",
                    Status = "Pending",
                    FrequencyDays = 180,
                    DueDate = DateTime.Today.AddDays(+30),
                    Priority = "Low",
                    UserId = 1,
                    CategoryId = 2,
                    //TaskHelpsId = 6
                },
                new Tasks
                {
                    Title = "Check Gutters and ensure downspouts aren't blocked",
                    Description = "Inspect the gutters and ensure the downspouts are not blocked",
                    Status = "Pending",
                    FrequencyDays = 30,
                    DueDate = DateTime.Today.AddDays(+45),
                    Priority = "Low",
                    UserId = 1,
                    CategoryId = 5,
                    //TaskHelpsId = 7
                },
                new Tasks
                {
                    Title = "Check sprinkler system for broken heads or stuck valves",
                    Description = "Inspect the sprinkler system for any broken heads or stuck valves",
                    Status = "Pending",
                    FrequencyDays = 90,
                    DueDate = DateTime.Today.AddDays(+60),
                    Priority = "Urgent",
                    UserId = 1,
                    CategoryId = 6,
                    //TaskHelpsId = 8
                }
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
                Console.WriteLine($"***********Id: {tsk.Id}, Title: {tsk.Title}, Descr: {tsk.Description}, Status: {tsk.Status}, Freq: {tsk.FrequencyDays}, DueDate: {tsk.DueDate}, Pri: {tsk.Priority}, User: {tsk.UserId}, Category: {tsk.CategoryId},");
            }
        }

        private static async Task LoadInitialTaskActivity(SQLiteAsyncConnection database)
        {
            var taskActivities = new List<TaskActivity>
            {
                new TaskActivity {TaskId = 1, Status = "Pending", Condition = "Fair", Action = "Filter replacement failed!", TimeSpent = .5m, Notes = "Filter was the wrong size, 20 x 20 x 4. Should have been 20 x 25 x 4."},
                new TaskActivity {TaskId = 1, Status = "On-Hold", Condition = "Fair", Action = "Replaced Filter", TimeSpent = .5m, Notes = "Not too dirty but needed changing. Everything is in order."},
                new TaskActivity {TaskId = 1, Status = "Canceled", Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Lots of sediment. May want to peform check sooner."},
                new TaskActivity {TaskId = 3, Status = "Completed", Condition = "Good", Action = "Inspection", TimeSpent = 2, Notes = "PSI was under 20. No back-wash performed"},
                new TaskActivity {TaskId = 4, Status = "Completed", Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Hose was bulging, days from breaking!!"},
                new TaskActivity {TaskId = 5, Status = "Completed", Condition = "Good", Action = "Inspection", TimeSpent = 2, Notes = "Rollers were in perfect shape."},
                new TaskActivity {TaskId = 6, Status = "Completed", Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Seals were leaking"},
                new TaskActivity {TaskId = 7, Status = "Pending",   Condition = "Poor", Action = "Inspection", TimeSpent = 2, Notes = "Leaves were starting to accumulate..."},
                new TaskActivity {TaskId = 8, Status = "Pending", Condition = "Poor", Action = "Inspection", TimeSpent = 1, Notes = ""}
            };

            var taskActivityRepository = new TaskActivityRepository();
            foreach (var taskActivity in taskActivities)
            {               
                    await taskActivityRepository.AddTaskActivity(taskActivity);               
            }
            //Validate
            Console.WriteLine($"************************TaskActivity Load");
            var tas = await database.Table<TaskActivity>().ToListAsync();
            foreach (var ta in tas)
            {
                Console.WriteLine($"***********Id: {ta.Id}, TaskId: {ta.TaskId}, Status: {ta.Status}, Condition: {ta.Condition}, Action: {ta.Action}, Duration: {ta.TimeSpent}, Notes: {ta.Notes}");
            }
        }

        private static async Task LoadInitialPartBuy(SQLiteAsyncConnection database)
        {
            var partBuys = new List<PartBuy>
            {
                new PartBuy {PartInfoId = 1, SourceName = "Home Depot", SourceURL = "https://www.homedepot.com/s/20%20x%2025%20x%204?NCNI-5", Price = 30.88, Availability = true},
                new PartBuy {PartInfoId = 1, SourceName = "Amazon", SourceURL = "https://www.amazon.com/Filterbuy-20x25x4-Defense-Pleated-Replacement/dp/B0B7LMD6J9/ref=sr_1_22?crid=UTZV5VVBA801&dib=eyJ2IjoiMSJ9.QpwoBFjFG1k9C0YlLiuTZoNZY1TODKfh_NBaILAVmKUKR3Y7qmi_XuyFzQ9BXTOd1L5AC7VOa91w9Rc8C4-VEBARPwVjsRdKL64WfCeoGRAFa9lYIISwYTBcGh1lTMiW60pz-4D_WkWrYtzi7wFTXXRgSKo9IARx8t7yU2eqVRF3Py70MkMNhckc7Ozg_Bq5xJnWrUsioCT8rPtKkzzdVSV8hW6rVK_KQi3iTmwOlYn9j8j0R9E6w6mes4OXd6wRhLoJzfOMMKEjKDg5t8FQb4amHXnswAsfAyNY0xqHUjo.L5ZRb-ea20s0vOzYZ4SjP_LdtP8_sf2nMlM3fM8rDEU&dib_tag=se&keywords=20x25x4+air+filter&qid=1732750550&sprefix=20x25x4%2Caps%2C274&sr=8-22", Price = 30.15, Availability = true},
                new PartBuy {PartInfoId = 3, SourceName = "Amazon", SourceURL = "https://www.amazon.com/Ultra-Quiet-Rollers-Bearing-Durable-Cycles/dp/B07YZ8WRT7/ref=sr_1_5_pp?crid=105ETIPY4XP7O&dib=eyJ2IjoiMSJ9.c7GmkJeHTuDsbWj5EwTAf3hby8F6H-7ZiNBEQDH3zKJl7WM3iTLEyGedGHawNjuEAoZKAblHAB6tpqBxunDOJhlQFr9vt4Kq7oSCuJWn0rqWJghrdHImQ7PO0CJ2wiVvbfhAtrccjXoQDz3eGJZRGGFHW6K7_v5hXMNlwbRKLHXqMR4_pESVmjIuQBIqkdbwTEHXuaPrYQUDQICcB5uEVYHbN4uogm7YE7-Pv3gcFjjeT_pxjz39U96mOY-lzJJqdhjioL7BuZorZBrjFWQ06fraRdHLRP2QG3Qf1fgLYvk.skPKC8aZPP2k7EUSFOiC3JY_sxH3mrdjDI1GEf89WAE&dib_tag=se&keywords=Garage+door+roller&qid=1732752020&sprefix=garage+door+roller%2Caps%2C164&sr=8-5", Price = 21.99, Availability = true},
                new PartBuy {PartInfoId = 2, SourceName = "Home Depot", SourceURL = "https://www.homedepot.com/p/CERTIFIED-APPLIANCE-ACCESSORIES-4-ft-Braided-Stainless-Steel-Washing-Machine-Hoses-2-Pack-WM48SS2PK/309375807", Price = 30.88, Availability = true},
                new PartBuy {PartInfoId = 4, SourceName = "Costco", SourceURL = "https://www.costco.ca/easyon-gutter-guard-gutter-system.product.100240616.html", Price = 30.88, Availability = true},
            };

            var PartBuyRepository = new PartBuyRepository();

            foreach (var partBuy in partBuys)
            {
                var resultPartBuy = await PartBuyRepository.GetPartBuyAsync(partBuy.PartBuyId);
                if (resultPartBuy == null)
                {
                    await PartBuyRepository.AddPartBuyAsync(partBuy);
                }
            }

            // Validate  !!!!!!!! Don't forget to comment out after validation.
            var parts = await database.Table<PartBuy>().ToListAsync();
            Console.WriteLine($"************************PartBuy Load");
            foreach (var part in parts)
            {
                Console.WriteLine($"***********Help Id: {part.PartBuyId}, PartInfo Id: {part.PartInfoId}, Source name: {part.SourceName}, URL: {part.SourceURL}, Price: {part.Price}, Availability: {part.Availability}");
            }
        }
        
        private static async Task LoadInitialPartInfo(SQLiteAsyncConnection database)
        {
            var partinfos = new List<PartInfo>
            {
                new PartInfo {TaskId = 1, PartName = "20x25x4 A/C Filter ", PartDescription = "Filterbuy 20x25x4 Air Filter MERV 8 Dust Defense (1-Pack)", ImagePath = "" },
                new PartInfo {TaskId = 4, PartName = "Stainless Steel Washing Machine Hoses", PartDescription = "Premium Proof Stainless Steel Washing Machine Hoses (6 FT)", ImagePath = ""},
                new PartInfo {TaskId = 5, PartName = "Nylon Garage Door Rollers", PartDescription = "10 Pack 2'' Ultra-Quiet Nylon Garage Door Rollers 7\" Stem", ImagePath = ""},
                new PartInfo {TaskId = 7, PartName = "EasyOn Gutter Guard System", PartDescription = "", ImagePath = ""},                                                                            // Type 1 = video, 2 = written instructions. 3 = pictoral instructions
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
                Console.WriteLine($"***********Help Id: {part.PartInfoId}, Task Id: {part.TaskId}, Part name: {part.PartName}, Descr: {part.PartDescription}, Img location: {part.ImagePath}");
            }
        }
        
        private static async Task LoadInitialTaskHelp(SQLiteAsyncConnection database)
        {
            var taskHelps = new List<TaskHelp>
            {
                new TaskHelp {TaskId = 1, Description = "How to change an A/C filter.", Type = 1, URL = "https://www.youtube.com/watch?v=sOcfx5o9-B4" },
                new TaskHelp {TaskId = 4, Description = "The best type of washing machine hoses.", Type = 2, URL = "https://www.youtube.com/watch?v=cLvZIGCv36Q"},
                new TaskHelp {TaskId = 5, Description = "Garage door Maintainance tips.", Type = 1, URL = "https://www.huxleyandco.co.uk/common-problems-with-electric-roller-garage-doors-and-how-to-fix-them/"},
                new TaskHelp {TaskId = 7, Description = "Best and Worst Gutter Guards.", Type = 1, URL = "https://www.youtube.com/watch?v=Yc79p-kfTvo"},
                new TaskHelp {TaskId = 8, Description = "Common Sprinkler problems.", Type = 1, URL = "https://www.youtube.com/watch?v=m6t35voD7v0" },                                                                               // Type 1 = video, 2 = written instructions. 3 = pictoral instructions
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

        private static async Task LoadInitialTaskNote(SQLiteAsyncConnection database)
        {
            var taskNotes = new List<TaskNote>
            {
                new TaskNote {TaskId = 1, Type = "Comment", NoteContent = "Home Depot Filters are poor quality.", TimeStamp = DateTime.Now},
                new TaskNote {TaskId = 4, Type = "Comment", NoteContent = "Use Stainless steel braided Hoses only.", TimeStamp = DateTime.Now},
                new TaskNote {TaskId = 5, Type = "Comment", NoteContent = "Use nylon rollers as they are quiet.", TimeStamp = DateTime.Now},
                new TaskNote {TaskId = 7, Type = "Comment", NoteContent = "Consider installing gutter leaf guards.", TimeStamp = DateTime.Now},
                new TaskNote {TaskId = 8, Type = "Comment", NoteContent = "Rain Byrd heads are the least durable!.", TimeStamp = DateTime.Now},
            };

            var taskNoteRepository = new TaskNoteRepository();

            foreach (var tasknote in taskNotes)
            {
                var resultTaskNote = await taskNoteRepository.GetTaskNoteAsync(tasknote.TaskId);
                if (resultTaskNote == null) 
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

        private static async Task LoadInitialUser(SQLiteAsyncConnection database)
        {
            
            var initialUser = new User
            {
                UserName = "billbyrd",
                Password = "gggb0211",
                Email = "wm-byrd@swbell.net",
                Phone = "2149955647",
            };
            //await Application.Current.MainPage.DisplayAlert("Starting LoadInitialUser method", " ", "OK");
            var existingUser = await database.Table<User>().FirstOrDefaultAsync(u => u.UserName == initialUser.UserName);
            if (existingUser == null)
            {
                // If the user does not exist, hash the password
                var hashedPassword = PasswordHelper.HashPassword("gggb0211");
                //await Application.Current.MainPage.DisplayAlert("Hashed Password", hashedPassword, "OK");

                // Store the hashed password in SecureStorage
                await SecureStorage.SetAsync(initialUser.UserName, hashedPassword);


                // Store hashed password in the database for debugging. Insert user into the database
                initialUser.Password = hashedPassword;
                await database.InsertAsync(initialUser);

                //Display an alert confirming the insertion
                //await Application.Current.MainPage.DisplayAlert("User Insertion", "Initial user inserted.", "OK");
            }
            else
            {
                // Display an alert if the user already exists
                //await Application.Current.MainPage.DisplayAlert("Initialization", "User already exists in the database.", "OK");
            }
          
        }

        
        private static async Task LoadInitialNotification(SQLiteAsyncConnection database)
        {
            var notifications = new List<MaintainHome.Models.Notification>
            {
                new MaintainHome.Models.Notification {TaskId = 1, TargetName = "Chris Byrd", TargetEmail = "Chris-byrd@swbell.net", TargetPhone = "2149955625", Message="Task is due Soon"},
                new MaintainHome.Models.Notification {TaskId = 1, TargetName = "Alexandria Byrd", TargetEmail = "Ally-byrd@swbell.net", TargetPhone = "2149955645", Message="Task is due Soon"},
                new MaintainHome.Models.Notification {TaskId = 1, TargetName = "William Byrd", TargetEmail = "wm-byrd@swbell.net", TargetPhone = "2149955695", Message="Task is due Soon"},
                new MaintainHome.Models.Notification {TaskId = 4, TargetName = "William Byrd", TargetEmail = "wm-byrd@swbell.net", TargetPhone = "2149955647", Message="Task is due Soon"},
                new MaintainHome.Models.Notification {TaskId = 5, TargetName = "William Byrd", TargetEmail = "wm-byrd@swbell.net", TargetPhone = "2149955647", Message="Task is due Soon"},
                new MaintainHome.Models.Notification {TaskId = 6, TargetName = "William Byrd", TargetEmail = "wm-byrd@swbell.net", TargetPhone = "2149955647", Message="Task is due Soon"},
                new MaintainHome.Models.Notification {TaskId = 7, TargetName = "William Byrd", TargetEmail = "wm-byrd@swbell.net", TargetPhone = "2149955647", Message="Task is due Soon"},
                new MaintainHome.Models.Notification {TaskId = 8, TargetName = "William Byrd", TargetEmail = "wm-byrd@swbell.net", TargetPhone = "2149955647", Message="Task is due Soon"}
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

        private static async Task LoadInitialCategories(SQLiteAsyncConnection database)
        {
            var categories = new List<Category>
            {
                new Category {Title = "HVAC", Priority = 2},
                new Category {Title = "Plumbing", Priority = 2},
                new Category {Title = "Pool", Priority = 2},
                new Category {Title = "Garrage", Priority = 2},
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








    }
}
