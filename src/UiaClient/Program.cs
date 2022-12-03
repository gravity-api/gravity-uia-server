using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Uia;
using System;

namespace UiaClient
{
    internal static class Program
    {
        private static void Main()
        {
            //var chrome = new OpenQA.Selenium.Chrome.ChromeDriver(@"C:\AutomationEnv\WebDrivers");
            //chrome.Navigate().GoToUrl("https://www.google.com");
            //chrome.FindElement(By.XPath("//input[@name='q']")).Click();
            //chrome.FindElement(By.XPath("//input[@name='q']")).SendKeys("Foo Bar");
            //chrome.Dispose();

            // cache sample
            /*
            var a = new CUIAutomation8();
            var root = a.GetRootElement();

            var c = a.CreatePropertyCondition(UIA_PropertyIds.UIA_ClassNamePropertyId, "Notepad");
            var app = root.FindFirst(UIAutomationClient.TreeScope.TreeScope_Descendants, c);
            c = a.CreatePropertyCondition(UIA_PropertyIds.UIA_ClassNamePropertyId, "Edit");
            var doc = app.FindFirst(UIAutomationClient.TreeScope.TreeScope_Descendants, c);

            var r = a.CreateCacheRequest();
            r.AddPattern(UIA_PatternIds.UIA_TextChildPatternId);
            r.AddPattern(UIA_PatternIds.UIA_TextEditPatternId);
            r.AddPattern(UIA_PatternIds.UIA_TextPattern2Id);
            r.AddPattern(UIA_PatternIds.UIA_TextPatternId);

            // add properties
            r.AddProperty(UIA_PropertyIds.UIA_AcceleratorKeyPropertyId);
            r.AddProperty(UIA_PropertyIds.UIA_AccessKeyPropertyId);
            r.AddProperty(UIA_PropertyIds.UIA_AutomationIdPropertyId);

            doc = doc.BuildUpdatedCache(r);
            var aa = doc.GetCachedPattern(UIA_PatternIds.UIA_TextPatternId);
            var bb = doc.CachedAccessKey;
            var cc = doc.CachedAcceleratorKey;
            var dd = doc.CachedAutomationId;
            var ee = doc.CurrentClassName;
            */
            var options = new UiaOptions();
            //{
            //    Application = @"C:\Program Files (x86)\MyHeritage\Bin\MyHeritage.exe",
            //    DriverPath = @"C:\Users\roei.sabag\Desktop\stage\gravity-uia-server\UiaDriverServer\UiaDriverServer\bin\Debug\net47"
            //};
            //options.AddAdditionalCapability("gUser", "test@gravity.api");
            //options.AddAdditionalCapability("gPassword", "Aa123456!");


            /*
             // SCENARIO #1            
             var driver = new UiaDriver(options);
             driver.Dispose();

             Console.WriteLine("SCENARIO #1: completed");

             // SCENARIO #2
             driver = new UiaDriver(options);
             var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
             var element = wait.Until(d => d.FindElement(By.XPath("//button[@automationId='2']")));
             element.Click();
             element = wait.Until(d => d.FindElement(By.XPath("//menuItem[@name='Edit']")));
             element.Click();
             wait.Until(d => d.FindElement(By.XPath("//menu[@name='Edit']//menuItem[8]"))).Click();
             wait.Until(d => d.FindElement(By.XPath("//menuItem[@name='File']"))).Click();
             wait.Until(d => d.FindElement(By.XPath("//menu[@name='File']//menuItem[1]"))).Click();
             Thread.Sleep(3000);
             driver.Dispose();

             Console.WriteLine("SCENARIO #2: completed");

             // SCENARIO #3
             driver = new UiaDriver(options);
             wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
             wait.Until(d => d.FindElement(By.XPath("//button[@automationId='2']"))).Click();
             wait.Until(d => d.FindElement(By.XPath("//edit[@name='First Name:']"))).SendKeys("משה");
             wait.Until(d => d.FindElement(By.XPath("//edit[@name='Last Name:']"))).SendKeys("כהן");
             wait.Until(d => d.FindElement(By.XPath("//edit[@name='First Name:']/parent::pane/button[1]"))).Click();
             Thread.Sleep(3000);
             driver.Dispose();

             Console.WriteLine("SCENARIO #3: completed");

             // SCENARIO #4
             driver = new UiaDriver(options);
             wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
             wait.Until(d => d.FindElement(By.XPath("//button[@automationId='2']"))).Click();
             wait.Until(d => d.FindElement(By.XPath("//edit[@name='First Name:']"))).SendKeys("משה");
             wait.Until(d => d.FindElement(By.XPath("//edit[@name='Last Name:']"))).SendKeys("כהן");
             wait.Until(d => d.FindElement(By.XPath("//edit[@name='First Name:']/parent::pane/button[1]"))).Click();
             wait.Until(d => d.FindElement(By.XPath("//*[@name='Advanced ']"))).Click();
             wait.Until(d => d.FindElement(By.XPath("//edit[@isEnabled='1']"))).SendKeys("Moshe Cohen");
             Thread.Sleep(3000);
             driver.Dispose();

             Console.WriteLine("SCENARIO #4: completed");
             */

            // initialize new driver based on options (new selenium standard)
            // opens "notepad.exe"
            options.Application = "notepad.exe";
            var driver = new UiaDriver(new Uri("http://localhost:4444/wd/hub"), options);

            // waiter instance to avoid element-not-found exception
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            // 1. type "hello world" into document area
            var element = driver.FindElement(By.XPath("//*[@className='Edit']"));
            element.SendKeys("hello world");
            Console.WriteLine(element.Text);

            // 2. click on "file" menu
            driver.FindElement(By.XPath("//menuItem[@name='File']")).Click();

            // 3. click on "save as..." menu-item
            wait.Until(d => d.FindElement(By.XPath("//menu[@name='File']//menuItem[@name='Save As...']"))).Click();

            // 4. type a random file name into file name text-box
            wait.Until(d => d.FindElement(By.XPath("//window[@name='Save As']//comboBox[@name='File name:']"))).SendKeys($@"D:\garbage\{Guid.NewGuid()}.txt");

            // 5. click on "save" button
            wait.Until(d => d.FindElement(By.XPath("//button[@name='Save']"))).Click();

            // 6. close application
            driver.Dispose();
        }
    }
}