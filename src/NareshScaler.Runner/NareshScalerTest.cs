using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Drawing.Imaging;

namespace NareshScaler.Runner
{

	public abstract class NareshScalerTest
	{
		public FirefoxDriver FirefoxDriver;
		public InternetExplorerDriver IEDriver;
		public ChromeDriver ChromeDriver;

		public TimeSpan DefaultTimeOutValue;

		public string CurrentlyRunningTest = "Not specified, set CurrentRunningTest var prior to running each test";

		public string LogFileDirectory, LogFileName, ErrorRowFormat;
		public Dictionary<string, dynamic> ErrorList;

		protected NareshScalerTest()
		{
			// Set default timeout to 5s
			DefaultTimeOutValue = new TimeSpan(0, 0, 10);
		}

		/// <summary>
		/// Runs test using ChromeDriver
		/// </summary>
		[Test]
		public virtual void ChromeTest()
		{
			if (!NareshScalerSettings.Default.ChromeEnabled)
				return;

			var driverDir = GetDriverDirectory();

			try
			{
				ChromeDriver = new ChromeDriver(driverDir);
			}
			catch (Exception)
			{
				// Only for master build
				var masterLibDir = LocateDir(Directory.GetCurrentDirectory(), "lib");
				ChromeDriver = new ChromeDriver(masterLibDir);
			}
			
			ChromeDriver.Manage().Timeouts().ImplicitlyWait(DefaultTimeOutValue);


			try
			{
				RunSeleniumTests(ChromeDriver);
			}
			catch (Exception e)
			{

				RecordError(ChromeDriver, CurrentlyRunningTest, e);
				throw;
			}
			
		}

		private static string GetDriverDirectory()
		{
			//Set the lib dir for the running solution
			var currentDir = Directory.GetCurrentDirectory();

			var packagesDir = LocateDir(currentDir, "packages");

			var assemblyVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			return new DirectoryInfo(packagesDir).FullName + "\\NareshScaler." + assemblyVer + "\\bin\\";
		}

		/// <summary>
		/// Runs the test using the Firefox driver
		/// </summary>
		[Test]
		public virtual void FirefoxTest()
		{
			if (!NareshScalerSettings.Default.FirefoxEnabled)
				return;

			FirefoxDriver = new FirefoxDriver();
			FirefoxDriver.Manage().Timeouts().ImplicitlyWait(DefaultTimeOutValue);

			try
			{
				RunSeleniumTests(FirefoxDriver);
			}
			catch (Exception e)
			{
				RecordError(FirefoxDriver, CurrentlyRunningTest, e);
				throw;
			}
		}

		/// <summary>
		/// Runs the test using the IE driver
		/// </summary>
		[Test]
		public virtual void IETest()
		{
			if (!NareshScalerSettings.Default.IEEnabled)
				return;

			var driverDir = GetDriverDirectory();

			try
			{
				IEDriver = new InternetExplorerDriver(driverDir);
			}
			catch (Exception)
			{
				// Only for master build
				var masterLibDir = LocateDir(Directory.GetCurrentDirectory(), "lib");
				IEDriver = new InternetExplorerDriver(masterLibDir);
			}

			IEDriver.Manage().Timeouts().ImplicitlyWait(DefaultTimeOutValue);

			try
			{
				RunSeleniumTests(IEDriver);
			}
			catch (Exception e)
			{
				RecordError(IEDriver, CurrentlyRunningTest, e);
				
				throw;
			}
			
		}

		/// <summary>
		/// Helper method to look discover where the ChromeDriver executeable lives
		/// </summary>
		private static string LocateDir(string currentDir, string dirToFind)
		{
			// Locate the packages dir in the running solution
			if (currentDir.ToLower().Contains(dirToFind))
				return currentDir;

			var dirPath = new DirectoryInfo(currentDir).Parent.FullName;
			string[] dirs = Directory.GetDirectories(dirPath);

			foreach (string dir in dirs)
			{
				if (dir.ToLower().Contains(dirToFind))
					return dir;
			}

			// recurse up the tree until we find the packages dir
			return LocateDir(dirPath, dirToFind);
		}

		/// <summary>
		/// Override this method and use it to run any selenium methods created by the
		/// Selenium Webdriver.
		/// </summary>
		/// <param name="webDriver"></param>
		public abstract void RunSeleniumTests(IWebDriver webDriver);

		/// <summary>
		/// Setup method called prior to any tests being ran
		/// </summary>
		[TestFixtureSetUp]
		public virtual void FixtureSetup()
		{
			// define the output directory for the build reports
			LogFileDirectory = NareshScalerSettings.Default.LogfilePath;
			LogFileName = LogFileDirectory + "\\build-report" + DateTime.Now.ToString("-MMdd-HHmm") + ".html";
			ErrorList = new Dictionary<string, dynamic>();
			ErrorRowFormat = "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td><a href='{3}'>screenshot</a></td></tr>";
		}

		/// <summary>
		/// TearDown method called after all tests have ran
		/// </summary>
		[TestFixtureTearDown]
		public virtual void FixtureTearDown()
		{
            // if logging is disabled, dont write out the log file
            if (!NareshScalerSettings.Default.LoggingEnabled)
                return;


			// get the log template file
			var html = Properties.Resources.log_template;

			// write each error into a stringbuilder
			var htmlErorrs = new StringBuilder();
			foreach (var error in ErrorList)
			{
				htmlErorrs.AppendFormat(ErrorRowFormat, error.Value.Browser, error.Value.TestName, error.Value.Description, error.Value.Screenshot);
			}

			// Replace your placeholder in the log file with the list of errors that occurred during testing
			html = html.Replace("<!--error-placeholder-->", htmlErorrs.ToString());

			using (var sw = new StreamWriter(LogFileName))
			{
				sw.WriteLine(html);
			}
		}

		/// <summary>
		/// Record details of an error that was detected during testing
		/// </summary>
		protected void RecordError(IWebDriver driver, string failingTest, Exception ex)
		{
			if (!NareshScalerSettings.Default.LoggingEnabled)
				return;

			if (!Directory.Exists(LogFileDirectory))
				Directory.CreateDirectory(LogFileDirectory);

			var screenshotsDir = LogFileDirectory + @"\screenshots";

			if (!Directory.Exists(screenshotsDir))
				Directory.CreateDirectory(screenshotsDir);

			var screenshotFilename = string.Format(screenshotsDir + @"\{0}-{1}.png", failingTest, DateTime.Now.ToString("MMdd-HHmm"));
			
			dynamic error = new ExpandoObject();
			error.Browser = driver.GetType().Name;
			error.TestName = failingTest;
			error.Description = ex.Message;
			error.Screenshot = screenshotFilename;
			TakeScreenshot(driver, screenshotFilename);
			ErrorList.Add(driver.GetType().Name + "_" + failingTest, error);

			// once we've logged the error, throw it on to allow nunit etc to handle it
			throw ex;
		}

		/// <summary>
		/// Take a screenshot of the current webpage, save to filename provided
		/// </summary>
		protected void TakeScreenshot(IWebDriver driver, string filename)
		{
			ITakesScreenshot screenshotDriver = driver as ITakesScreenshot;
			Screenshot screenshot = screenshotDriver.GetScreenshot();
			screenshot.SaveAsFile(filename, ImageFormat.Png);
		}
	}
}


