﻿// Decompiled with JetBrains decompiler
// Type: ClientManager.Models.Register
// Assembly: ClientManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A31CD02-2A37-4A80-A7EA-942AEB12790F
// Assembly location: C:\Users\kanim\Downloads\Websiteapp\httpdocs\bin\ClientManager.dll

namespace ClientManager.Models
{
  public class Register
  {
    public string FullName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string ConfirmPassword { get; set; }

    public bool IsAgreeTerms { get; set; }
  }
}
