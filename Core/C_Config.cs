using System.Collections.Generic;

namespace Core
{
    public class C_Config
    {
        public static string KeyJWTAPI = "A252BEC4-3CA5-4D00-8F44-28E621C6A4CD";
        public static List<string> BlackList = new List<string> { "admin", "mod", "login", "logout", "register", "signin", "signup", "dangky", "dangnhap", "unidi", "company", "user" };
        public static List<string> Units = new List<string> { "Set", "Piece", "Stick", "Service", "Basket","Box","Kilogram","Liter","Jar","Meter","Software","Carton","Can","Tube","Pair","Pack","Dozen"};
        public static string DefaultSponsor = "Unidi";
    }
}
