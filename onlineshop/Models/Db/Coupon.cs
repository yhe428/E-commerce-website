using System;
using System.Collections.Generic;

namespace onlineshop.Models.Db;

public partial class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal Discount { get; set; }
}
