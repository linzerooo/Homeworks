using MyHttpServer.Attributes;
using MyHttpServer.Configuration;
using MyHttpServer.Models;
using MyHttpServer.MyORM;
using MyHttpServer.Services;

namespace MyHttpServer.Controllers;

[Controller("Account")]
public class AccountController
{
    private readonly MyDataContext _db = new("Server=(localdb)\\MSSQLLocalDB;Database=BattleDB");

    [Post("SendToEmail")]
    public static void SendToEmail(string email, string password)
    {
        new EmailSenderService(AppSettingsLoader.Instance()?.Configuration).SendEmail(email, password);
        Console.WriteLine("Email was sent successfully!");
    }

    // [Post("Add")] потому что в адресной строке всегда Get-запрос
    [Get("Add")]
    public string Add(string login, string password)
    {
        _db.Add(new Account
        {
            Login = login,
            Password = password
        });

        return $"User {login} was added";
    }


    // [Post("Delete")] потому что в адресной строке всегда Get-запрос
    [Get("Delete")]
    public string Delete(string id)
    {
        var account = _db.SelectById<Account>(int.Parse(id));
        _db.Delete<Account>(int.Parse(id));

        return $"User {account.Login} was deleted";
    }

    // [Post("Update")] потому что в адресной строке всегда Get-запрос
    [Get("Update")]
    public string Update(string id, string login, string password)
    {
        var account = _db.SelectById<Account>(int.Parse(id));

        var oldLogin = account.Login;

        account.Login = login;
        account.Password = password;

        _db.Update(account);

        return $"User {oldLogin} was updated";
    }

    [Get("GetAll")]
    public List<Account> GetAll() => _db.Select<Account>();

    [Get("GetById")]
    public Account GetById(string id) => _db.SelectById<Account>(int.Parse(id));
}