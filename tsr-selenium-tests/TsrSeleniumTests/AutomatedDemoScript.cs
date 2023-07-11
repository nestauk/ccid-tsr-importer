namespace TsrSeleniumTests;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

public class AutomatedDemoScript : AbstractSeleniumTest
{
    public static int PAUSE = 3000;
    [Fact]
    public void StepThroughDemoScript()
    {
        driver.Manage().Window.Position = new System.Drawing.Point(50, 50);
        driver.Manage().Window.Size = new System.Drawing.Size(1229, 970);
        driver.Navigate().GoToUrl("http://strategyroom.uk/");
        Thread.Sleep(10000);  // pause to start video recording

        // scroll through page
        ScrollToBottom();
        Thread.Sleep(PAUSE * 2);

        // visit explore data page
        ScrollToTop();
        Thread.Sleep(PAUSE);
        var exploreButton = driver.FindElement(By.XPath("//button[contains(text(),'Explore data')]"));
        exploreButton.Click();
        wait.Until(d => d.FindElement(By.XPath("//h2[contains(text(),'Most popular policies')]")));
        Thread.Sleep(PAUSE);

        // scroll through page
        ScrollThroughAllPolicies();
        Thread.Sleep(PAUSE);

        var policyCoBenefitsHeading = driver.FindElement(By.XPath("//h2[contains(text(),'Policy co-benefits')]"));
        ScrollToElement(policyCoBenefitsHeading);
        Thread.Sleep(PAUSE);

        var outcomesHeading = driver.FindElement(By.XPath("//h2[contains(text(),'Outcome and experience')]"));
        ScrollToElement(outcomesHeading);
        Thread.Sleep(PAUSE);

        ScrollToTop();
        Thread.Sleep(PAUSE);

        // select lambeth
        var councilDropdown = GetFilterSelect(0);
        councilDropdown.Click();
        Thread.Sleep(PAUSE);
        var allCouncils = councilDropdown.FindElement(By.XPath("//li[contains(text(), 'All')]"));
        var lambethCouncil = councilDropdown.FindElement(By.XPath("//li[contains(text(), 'London Borough of Lambeth')]"));
        lambethCouncil.Click();
        wait.Until(d => d.FindElement(By.XPath("//p[contains(text(),'101')]")).Text.Contains("participants"));
        Thread.Sleep(PAUSE);

        // select female
        var genderDropdown = GetFilterSelect(1);
        genderDropdown.Click();
        Thread.Sleep(PAUSE);
        var femaleGender = genderDropdown.FindElement(By.XPath("//li[contains(text(), 'Female')]"));
        femaleGender.Click();
        wait.Until(d => d.FindElement(By.XPath("//p[contains(text(),'69')]")).Text.Contains("participants"));
        Thread.Sleep(PAUSE);

        // show age filter
        var ageDropdown = GetFilterSelect(2);
        ageDropdown.Click();
        Thread.Sleep(PAUSE);
        var allAges = ageDropdown.FindElement(By.XPath("//li[contains(text(), 'All')]"));
        allAges.Click();
        Thread.Sleep(PAUSE);

        // show ethnicity filter
        var ethnicityDropdown = GetFilterSelect(3);
        ethnicityDropdown.Click();
        Thread.Sleep(PAUSE);
        var allEthnicities = ethnicityDropdown.FindElement(By.XPath("//li[contains(text(), 'All')]"));
        allEthnicities.Click();
        Thread.Sleep(PAUSE);

        // reset gender
        var genderDropdown2 = GetFilterSelect(1);
        genderDropdown2.Click();
        var allGenders = genderDropdown2.FindElement(By.XPath("//li[contains(text(), 'All')]"));
        allGenders.Click();
        wait.Until(d => d.FindElement(By.XPath("//p[contains(text(),'101')]")).Text.Contains("participants"));
        Thread.Sleep(PAUSE);

        // go to travel module
        var travelModuleButton = driver.FindElement(By.XPath("//button[contains(text(),'Travel policy')]"));
        travelModuleButton.Click();
        Thread.Sleep(PAUSE);

        // scroll to feasibility section
        var feasibilityHeading = driver.FindElement(By.XPath("//h2[contains(text(),'Feasibility of policies')]"));
        ScrollToElement(feasibilityHeading);
        Thread.Sleep(PAUSE);

        // scroll to fairness section
        var fairnessHeading = driver.FindElement(By.XPath("//h2[contains(text(),'Fairness of policies')]"));
        ScrollToElement(fairnessHeading);
        Thread.Sleep(PAUSE);

        // back to top
        ScrollToTop();
        Thread.Sleep(PAUSE);

        // go to health module
        var heatModuleButton = driver.FindElement(By.XPath("//button[contains(text(),'Energy and heating policy')]"));
        heatModuleButton.Click();
        Thread.Sleep(PAUSE);

        // scroll to housing situation section
        var housingHeading = driver.FindElement(By.XPath("//h2[contains(text(),'Housing situation')]"));
        ScrollToElement(housingHeading);
        Thread.Sleep(PAUSE);

        // back to top
        ScrollToTop();
        Thread.Sleep(PAUSE);

        // finish on sign up page
        var signUpButton = driver.FindElement(By.XPath("//button[contains(text(),'Sign up')]"));
        signUpButton.Click();
        Thread.Sleep(PAUSE * 5);
    }

    private IWebElement GetFilterSelect(int index)
    {
        var filterSelects = driver.FindElements(By.XPath("//div[@id='filter-select']")).ToList();
        Assert.Equal(4, filterSelects.Count);
        return filterSelects.ElementAt(index);
    }

    private void ScrollThroughAllPolicies()
    {
        var policyElements = new List<IWebElement>();

        for (int i = 1; i <= 5; i++)
            policyElements.Add(driver.FindElement(By.XPath($"//p[contains(text(),'{i}') and @element='h3']")));

        ScrollToElement(policyElements[4], -400);
        Thread.Sleep(2000);

        var seeAllBtn = driver.FindElement(By.XPath("//button[contains(text(),'See all policies')]"));
        seeAllBtn.Click();
        Thread.Sleep(2000);

        for (int i = 6; i <= 10; i++)
            policyElements.Add(driver.FindElement(By.XPath($"//p[contains(text(),'{i}') and @element='h3']")));

        ScrollToElement(policyElements[9]);
    }


}
