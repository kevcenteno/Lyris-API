Lyris-API
=========

.NET / C# API for Lyris / EmailLabs

This API subscribes, resubscribes, unsubscribes, adds demographics, and updates demographics.  
It returns a success or error response via ``` lyrisResponse ```.  If there is an error, the error message is stored in ``` lyrisMessage ```.

Documentation
=========

In lyris.cs, change lyrisSiteID and lyrisPassword as appropriate.  

```c#
private string lyrisSiteID = "123456"; // Lyris Site ID here
private string lyrisPassword = "password"; // Lyris API password here
```

An example of usage on a page:

```c#
<script type="text/C#" runat="server">
  protected void btnSubmit_Click(object sender, EventArgs e)
  {
    var lyrisService = new LyrisService();
    var MLID = 12345; // MLID here
    var email = "email@email.com"; // Email you want to submit
    
    var demographics = new Dictionary<int, string>();
    {
        // Format is {demographic ID, demographic value}
        // The ID is setup in Lyris / EmailLabs.  The value is taken from the form
        {1, this.firstName.Text},
        {2, this.lastName.Text}
    };
    
    lyrisService.MailingListSignUp(
        MLID,
        email,
        demographics
    );
    
    // Redirect based on success or failure
    if (lyrisService.lyrisResponse == "success")
    {
      Response.Redirect("/success.html");
    }
    else 
    {
      // pass in the error message as a query string for page to display
      Response.Redirect("/error.html?response=" + HttpUtility.UrlEncode(lyrisService.lyrisMessage));
    }
  }   
</script>

```