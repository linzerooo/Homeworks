namespace Shablonizator;


public class PhrasesToStudents
{
    public static string Ex1(string? name) => "Здравствуйте, @{name}, вы отчислены".Replace("@{name}", name);
    
    public static string Ex2(object? obj)
    {
        const string template = "Здравствуйте, @{name} вы прописаны по адресу @{address}";

        return template.Substitute(obj);
    }

    public static string Ex3(object? obj)
    {
        const string template = "Здравствуйте, @{if(temperature >= 37)} @then{Выздоравливайте} @else{Прогульщица}";

        return template.Substitute(obj);
    }

    public static string Ex4(object? obj)
    {
        const string template = "Здравствуйте, студенты группы @{group}.\nВаши баллы по ОРИС:\n@for(item in students) {@{item.FIO} - @{item.grade}}";

        return template.Substitute(obj);
    }
}