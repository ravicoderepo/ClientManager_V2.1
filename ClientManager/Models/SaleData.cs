﻿using System;

namespace ClientManager.Models
{
  public class SaleData
  {
    public int Id { get; set; }

    public string SaleDate { get; set; }

    public int Status { get; set; }

    public int SalesRepresentativeId { get; set; }

    public string ClientName { get; set; }

    public string ClientEmail { get; set; }

    public string ClientPhoneNo { get; set; }

    public string ProductName { get; set; }

    public string Capacity { get; set; }

    public string Unit { get; set; }

    public string RecentCallDate { get; set; }

    public string AnticipatedClosingDate { get; set; }

    public string InvoiceNo { get; set; }

    public Decimal InvoiceAmount { get; set; }

    public string DateOfClosing { get; set; }

    public int NoOfFollowUps { get; set; }

    public string Remarks { get; set; }
  }
}
