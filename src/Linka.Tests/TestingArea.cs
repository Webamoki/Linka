using LinkaTests.Models;
using NUnit.Framework;
using Webamoki.Linka;

namespace LinkaTests;


public class TestingArea
{
    [Test]
    public void TestMethod()
    {
        Linka.AddConnection("localhost", "u62560199300users", "u62560199300users", "Thb9jbCcgpxRBvBMtfQb");
        Linka.Configure<UserDbSchema>();
        using var db = new DbService<UserDbSchema>(true);

    
        
        var memoryBefore = GC.GetTotalMemory(forceFullCollection: true);
        var ip = db.Include<IpAddressModel>(u => u.User).First(u=> u.UserID == "30120320SU");
        var ip = new IpAddressModel();
        Console.WriteLine(ip.User.Name.Value());
        var user = db.Include<UserModel>(u => u.IpAddresses).First(u=> u.ID == "30120320SU");
        Console.WriteLine("asd1: " + user.IpAddresses);
        Console.WriteLine("asd12 " + user.IpAddresses[0].IP.Value());
        Console.WriteLine("asd12 " + user.IpAddresses[1].IP.Value());
        // var user = db.First<UserModel>(u=> u.ID == "30120320SU");
        
        // 4. Measure memory after
        var memoryAfter = GC.GetTotalMemory(forceFullCollection: true);
        // 5. Calculate difference
        var memoryUsed = memoryAfter - memoryBefore;
        Console.WriteLine($"Memory used: {memoryUsed} bytes");
    }
}

