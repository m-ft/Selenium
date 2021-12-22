using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace SeleniumApp {

    public class Reservation {
       // Login Credentials
        private static readonly string _user = "";
        private static readonly string _pass = "";
      // Properties 
        public string SeatID { get; init; } = "301667";
        public bool IsMorning { get; init;} = false;
        public string SeatNum {get; init;} = "592";
       // Reservation Date, Start time and Endtime hours
        private static DateTime _localDate = DateTime.Now;
        private static string _fourDaysLater = _localDate.AddDays(0).ToString("yyyy-MM-dd");
        private static string _fiveDaysLater = _localDate.AddDays(1).ToString("yyyy-MM-dd");
       // start date and end date
        private static string _startDate = _fourDaysLater;
        private string _endDate;
        private string _startTime;
        private string _endTime;
        private string _resourceUrl;
        private string _xpath;
       // Constructor 
       public Reservation()
        {
          _endDate = IsMorning ? _fourDaysLater : _fiveDaysLater;
          // StartTime and EndTime are in 24H format
          _startTime = IsMorning ? "09" : "19";
          _endTime = IsMorning ? "16" : "00";
          // Create Url
          _resourceUrl = $"https://www-sso.groupware.kuleuven.be/sites/KURT/Pages/NEW-Reservation.aspx?StartDateTime={_startDate}T{_startTime}:00:00&EndDateTime={_endDate}T{_endTime}:00:00&ID={SeatID}";
          //Console.WriteLine(_resourceUrl); 
           _xpath = $"//span[contains(text(), {SeatNum})]";
        }
        
        // Login User
        public static void Login(IWebDriver driver, WebDriverWait wait)
        {
           driver.Navigate().GoToUrl("https://www-sso.groupware.kuleuven.be/sites/KURT/Pages/default.aspx");
         try 
         { 
           IWebElement username = wait.Until(drv => drv.FindElement(By.CssSelector("input[name='username']")));
           IWebElement password = wait.Until(drv => drv.FindElement(By.CssSelector("input[name='password']")));
           IWebElement login_button = wait.Until(drv => drv.FindElement(By.Id("pwdLoginBtn")));
           username.SendKeys(_user);
           password.SendKeys(_pass);
           login_button.Click();
          }
         catch (Exception ex)
         {
            Console.WriteLine("Catching the {0} exception in the Login method.", ex.GetType());
            Screenshot loginEx = (driver as ITakesScreenshot).GetScreenshot();
            loginEx.SaveAsFile("login.png", ScreenshotImageFormat.Png);
         }

        }

       // Navigate to url 
        public int ReserveSeat(IWebDriver driver, WebDriverWait wait)
        { 
          driver.Navigate().GoToUrl(_resourceUrl);
          IWebElement resourceDisplayName = wait.Until(drv => drv.FindElement(By.XPath(_xpath))); 
           //string resourceDisplayNameTxt = resourceDisplayName.Text(); 
           //string resourceDisplayNum = rgx.Match(resourceDisplayNameTxt);
           return  SubmitRequest(driver, wait);
        } 

        public int SubmitRequest(IWebDriver driver, WebDriverWait wait)
        {
         try 
         {
          IWebElement checkbox = wait.Until(drv => drv.FindElement(By.Id("complyConditionsCheckbox")));
          IWebElement reserve_button = wait.Until(drv => drv.FindElement(By.CssSelector("input[onclick='createReservation();']")));
          Console.WriteLine(reserve_button);
          IJavaScriptExecutor js = (IJavaScriptExecutor) driver;
          js.ExecuteAsyncScript("arguments[0].click();", checkbox);
          try 
          {
                reserve_button.Submit();     
             
          }
          catch (Exception ex) 
          {
                js.ExecuteAsyncScript("arguments[0].click();", reserve_button);
          }
          //driver.FindElement(By.Id("formbuttons")).Click();
          return DetectError(driver, wait);
         }
         catch (Exception ex)
         {
            Console.WriteLine("Caught the {0} exception in the SubmitRequest method.", ex.GetType());
            Screenshot submitRequestEx = (driver as ITakesScreenshot).GetScreenshot();
            submitRequestEx.SaveAsFile("submitRequest.png", ScreenshotImageFormat.Png);
            return 0;
         }

        }
            
        public int DetectError(IWebDriver driver, WebDriverWait wait)
        {
               // Screenshot exit = (driver as ITakesScreenshot).GetScreenshot();
               // exit.SaveAsFile("err.png", ScreenshotImageFormat.Png);
                IWebElement errors = wait.Until(drv => drv.FindElement(By.Id("errorLabel")));
                try
                {        
                   // Actions actionProvider = new Actions(driver);
                   // actionProvider.MoveToElement(errors).Click().Perform();
                    IWebElement error = errors.FindElement(By.TagName("div"));
                    Screenshot errorScreenshot = (error as ITakesScreenshot).GetScreenshot();
                    errorScreenshot.SaveAsFile("screenshot_of_error.png");
                    return 0;
                }
                catch (NoSuchElementException)
                {
                    return WaitForCompletion(driver, wait);
                }
        
        }

        public int WaitForCompletion(IWebDriver driver, WebDriverWait wait)
        {
         try
         {
            // Screenshot exit = (driver as ITakesScreenshot).GetScreenshot();
            // exit.SaveAsFile("exit.png", ScreenshotImageFormat.Png);
             IWebElement confirmation = wait.Until(drv => drv.FindElement(By.Id("form-confirmation-message")));
             bool IsDisplayed = confirmation.Displayed;
             return IsDisplayed ? 1 : 0;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Caught the {0} exception in the WaitForCompletion method.", ex.GetType());
            Screenshot waitForCompletionEx = (driver as ITakesScreenshot).GetScreenshot();
            waitForCompletionEx.SaveAsFile("waitForCompletion.png", ScreenshotImageFormat.Png);
            return 0;
         }

        } 
    }
    public class Program {


        public static void IterateOverReservations(Dictionary <int, Reservation> reservations, IWebDriver driver, WebDriverWait wait) 
        {
         // Login   
         Reservation.Login(driver, wait);
         // Count items in reservations 
         int count = reservations.Count;
         // Iterate over reservation options
          foreach (var index in Enumerable.Range(1, count))
            {
                   int response =  reservations[index].ReserveSeat(driver, wait);
                    if (response == 0) 
                    {
                       continue;
                    } 
                    if (response == 1)
                    {
                        Console.WriteLine($"Successfuly made reservation for seat {reservations[index].SeatNum}", index);
                        break;
                    }
            }
        } 

        static void Main() 
        {

                //string[] seats = new string[] {"301667", "301549", "301662", "301668", "301697"};
                ChromeOptions options = new ChromeOptions();
                 string [] args = new string[] {"--headless", "--no-sandbox"};
                //  string [] args = new string[] {"--no-sandbox"};
                options.AddArguments(args); 
                // WebDriver
                IWebDriver driver = new ChromeDriver(@"C:\Users\u0130068a\Downloads\chromedriver", options);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
               // System.Drawing.Size size = driver.Manage().Window.Size;
               // size = new Size(1024, 768);
                var morning_reservations = new Dictionary<int, Reservation>() 
                {
                    {1, new Reservation {SeatID="301667", IsMorning=true, SeatNum="592"}}, // tulp1 592
                    {2, new Reservation {SeatID="301549", IsMorning=true, SeatNum="474"}}, // zolder 474
                    {3, new Reservation {SeatID="301662", IsMorning=true, SeatNum="587"}}, // tulp1 587
                    {4, new Reservation {SeatID="301668", IsMorning=true, SeatNum="593"}}, // tulp1 593
                    {5, new Reservation {SeatID="301697", IsMorning=true, SeatNum="622"}}  // tulp1 622
                };

                var evening_reservations = new Dictionary<int, Reservation>() 
                {
                    {1, new Reservation {SeatID="301667", IsMorning=false, SeatNum="592"}},
                    {2, new Reservation {SeatID="301549", IsMorning=false, SeatNum="474"}},
                    {3, new Reservation {SeatID="301662", IsMorning=false, SeatNum="587"}},
                    {4, new Reservation {SeatID="301668", IsMorning=false, SeatNum="593"}},
                    {5, new Reservation {SeatID="301697", IsMorning=false, SeatNum="622"}}
                };

                try
                {
                    IterateOverReservations(evening_reservations, driver, wait);
                   // IterateOverReservations(morning_reservations, driver, wait);
                }
                finally 
                {

                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
                    driver.Quit();
                }
        }
    
    }
}




