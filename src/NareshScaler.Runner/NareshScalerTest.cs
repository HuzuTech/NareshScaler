using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

namespace NareshScaler.Runner
{

    public abstract class NareshScalerTest
    {
        public FirefoxDriver FirefoxDriver;
        public InternetExplorerDriver IEDriver;
        public ChromeDriver ChromeDriver;


        public TimeSpan DefaultTimeOutValue;

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

            //Set the lib dir for the running solution
            var currentDir = Directory.GetCurrentDirectory();

            var packagesDir = LocateDir(currentDir, "packages");

            // TODO - Should pick up build number from Assembly
            var driverDir = new DirectoryInfo(packagesDir).FullName + "\\NareshScaler.1.0.0.21\\bin\\";

            try
            {
                ChromeDriver = new ChromeDriver(driverDir);
            }
            catch (Exception)
            {
                // Only for master build
                var masterLibDir = LocateDir(currentDir, "lib");
                ChromeDriver = new ChromeDriver(masterLibDir);
            }
            
            ChromeDriver.Manage().Timeouts().ImplicitlyWait(DefaultTimeOutValue);

            // Moved out of try catch block to trigger error on build server.
            RunSeleniumTests(ChromeDriver);
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

            RunSeleniumTests(FirefoxDriver);
        }

        /// <summary>
        /// Runs the test using the IE driver
        /// </summary>
        [Test]
        public virtual void IETest()
        {
            if (!NareshScalerSettings.Default.IEEnabled)
                return;

            IEDriver = new InternetExplorerDriver();
            IEDriver.Manage().Timeouts().ImplicitlyWait(DefaultTimeOutValue);
            RunSeleniumTests(IEDriver);
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
    }
}
