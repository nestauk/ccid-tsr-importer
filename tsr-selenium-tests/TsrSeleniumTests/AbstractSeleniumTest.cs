using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

public abstract class AbstractSeleniumTest : IDisposable
{
    protected readonly IWebDriver driver;
    protected WebDriverWait wait;

    protected AbstractSeleniumTest()
    {
        driver = new ChromeDriver();
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
        // wait.IgnoreExceptionTypes(typeof(StaleReferenceException));
    }

    public void Dispose()
    {
        driver.Quit();
        driver.Dispose();
    }

    protected IJavaScriptExecutor Jsx => (IJavaScriptExecutor)driver;

    protected void ScrollToBottom()
    {
        ScrollToElement(By.PartialLinkText("project pages"));
    }

    protected void MoveToElement(By by, int offset = 0)
    {
        var element = driver.FindElement(by);
        var actions = new Actions(driver);
        actions.MoveToElement(element, 0, offset);
        actions.Perform();
    }
    protected void ScrollToElement(By by, int offset = 0)
    {
        var element = driver.FindElement(by);
        ScrollToElement(element, offset);
    }

    protected void ScrollToElement(IWebElement element, int offset = 0)
    {
        ScrollToY(element.Location.Y + offset);
    }

    protected void ScrollToY(int y)
    {
        Jsx.ExecuteScript($"window.scrollTo({{ top: {y}, behavior: 'smooth' }})");
    }

    protected void ScrollToTop() => ScrollToY(0);

    private static bool DEFINED_slowScrollTo = false;
    protected void SlowScrollTo(int y, int factor = 8)
    {
        if (!DEFINED_slowScrollTo)
        {
            var fn_js = @$"
            const slowScrollTo = (y) => {{
                let c = document.documentElement.scrollTop || document.body.scrollTop;
                if (Math.round(c) != Math.round(y)) {{
                    let next = y-c > 0 ? {factor} : -{factor};
                    window.scrollTo(0, next);
                    window.requestAnimationFrame((timestamp) => {{ slowScrollTo({y}); }});
                }}
            }};";
            Jsx.ExecuteScript(fn_js);
            DEFINED_slowScrollTo = true;
        }
        var js = $"slowScrollTo({y});";
        Jsx.ExecuteScript(js);
    }
}
