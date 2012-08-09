using System;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;

namespace NareshScaler.Runner.Samples
{
	[TestFixture]
	public class NugetOrgTest : NareshScalerTest
	{
		private IWebDriver driver;
		private StringBuilder verificationErrors;
		private string baseURL;
		
		[SetUp]
		public void SetupTest()
		{
			baseURL = "http://nuget.org";
			verificationErrors = new StringBuilder();

			// You can disable a particular browser for a particular class, by setting below values to false
			//NareshScalerSettings.Default.ChromeEnabled = false;
			// NareshScalerSettings.Default.FirefoxEnabled = false;
			// NareshScalerSettings.Default.IEEnabled = false;

			// Enable logging by setting this value to true
			//NareshScalerSettings.Default.LoggingEnabled = true;
		}
		
		[TearDown]
		public void TeardownTest()
		{
			try
			{
				driver.Quit();
			}
			catch (Exception)
			{
				// Ignore errors if unable to close the browser
			}
			Assert.AreEqual("", verificationErrors.ToString());
		}
		
		public void Test_That_NareshScaler_Exists_On_NugetOrg()
		{
			driver.Navigate().GoToUrl(baseURL + "/");
			driver.FindElement(By.Id("searchBoxInput")).Clear();
			driver.FindElement(By.Id("searchBoxInput")).SendKeys("NareshScaler");
			driver.FindElement(By.Id("searchBoxSubmit")).Click();
			driver.FindElement(By.LinkText("Naresh Scaler")).Click();
		}

		public void Test_That_Nant_Builder_Exists_On_NugetOrg()
		{
			driver.Navigate().GoToUrl(baseURL + "/");
			driver.FindElement(By.Id("searchBoxInput")).Clear();
			driver.FindElement(By.Id("searchBoxInput")).SendKeys("nant.builder");
			driver.FindElement(By.Id("searchBoxSubmit")).Click();
			driver.FindElement(By.LinkText("Nant.Builder")).Click();
		}



		private bool IsElementPresent(By by)
		{
			try
			{
				driver.FindElement(by);
				return true;
			}
			catch (NoSuchElementException)
			{
				return false;
			}
		}

		public override void RunSeleniumTests(IWebDriver webDriver)
		{
			driver = webDriver;

			// Log running test
			CurrentlyRunningTest = "Test_That_NareshScaler_Exists_On_NugetOrg()";

			Test_That_NareshScaler_Exists_On_NugetOrg();

			// Log running test
			CurrentlyRunningTest = "Test_That_Nant_Builder_Exists_On_NugetOrg()";

			Test_That_Nant_Builder_Exists_On_NugetOrg();
		}
	}
}
