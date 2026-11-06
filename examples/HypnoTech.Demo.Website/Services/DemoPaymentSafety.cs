using System;
using Microsoft.AspNetCore.Http;

namespace HypnoTech.Demo.Website.Services;

public static class DemoPaymentSafety
{
    public static bool IsRealPaymentPath(PathString path)
    {
        return path.StartsWithSegments("/Checkout/Pay", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWithSegments("/Checkout/Complete", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWithSegments("/Checkout/Cancelled", StringComparison.OrdinalIgnoreCase);
    }
}
