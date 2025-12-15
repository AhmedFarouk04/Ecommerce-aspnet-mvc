namespace ECommerce.Core.Enums
{
    public enum PaymentStatus
    {
        Unpaid = 1,     // Order created but not paid yet
        Paid = 2,       // Payment completed successfully
        Failed = 3,     // Payment failed
        Refunded = 4    // Payment refunded
    }
}
